using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Interface;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Providers
{
    public class Youtube : IVideoProvider
    {
        public string Name => "Youtube";

        public bool Available => !string.IsNullOrEmpty(Key);

        public bool ShowLogo => false;

        public bool IsSupportBackground => false;
        public string Logo => Url.ResolveUrl("~/DesktopModules/Vanjaro/Core/Library/Resources/Images/youtube.png");

        public string Link => "https://www.youtube.com";

        private string Key
        {
            get
            {
                string YouTube_key = SettingManager.GetPortalSetting("Vanjaro.Integration.YouTube", true);
                return string.IsNullOrEmpty(YouTube_key) ? SettingManager.GetHostSetting("Vanjaro.Integration.YouTube", true) : YouTube_key;
            }
        }

        public static bool IsValid(string Key)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(Key))
                {
                    WebClient client = new WebClient();
                    Uri Link = new Uri("https://www.googleapis.com/youtube/v3/search?q=youtube&part=snippet&type=video&key=" + Key);
                    string jsonString = client.DownloadString(Link);
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            return result;
        }

        public Task<string> GetVideos(string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData)
        {
            return GetVideos(Key, Keyword, PageNo, PageSize, AdditionalData);
        }

        public static async Task<string> GetVideos(string ApiKey, string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData)
        {
            return await Get(ApiKey, Keyword, PageNo, PageSize, "video", AdditionalData);
        }

        private static async Task<string> Get(string ApiKey, string Keyword, int PageNo, int PageSize, string Type, Dictionary<string, object> AdditionalData)
        {
            try
            {
                if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Keyword) && !string.IsNullOrEmpty(Type) && PageNo > 0 && PageSize > 0)
                {
                    List<Entities.VideoInfo> ytVideoList = GetYoutubeVideoList(ApiKey, Keyword, PageNo, PageSize, AdditionalData);
                    WebClient client = new WebClient();
                    string result = new JavaScriptSerializer().Serialize(ytVideoList);
                    return await Task.FromResult(result);
                }
            }
            catch (Exception) { }
            return null;
        }

        private static List<Entities.VideoInfo> GetYoutubeVideoList(string ApiKey, string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData)
        {
            List<Entities.VideoInfo> ytVideoList = new List<Entities.VideoInfo>();
            dynamic searchListResponse = null;
            string nextPageToken = string.Empty;
            if (AdditionalData != null && AdditionalData.ContainsKey("nextPageToken"))
            {
                nextPageToken = AdditionalData["nextPageToken"].ToString();
            }

            dynamic AllVideoResponse = null;
            string queryString = string.Empty;
            queryString = Keyword + "&key=" + ApiKey + "&part=snippet";
            queryString += "&safeSearch=" + "None";
            searchListResponse = GetResponse("https://www.googleapis.com/youtube/v3/search?q=" + queryString + "&maxResults=" + PageSize + "&pageToken=" + nextPageToken, searchListResponse);
            if (searchListResponse != null)
            {
                nextPageToken = Convert.ToString(searchListResponse["nextPageToken"]);
                if (searchListResponse["items"] != null)
                {
                    string videoIdString = string.Empty;
                    foreach (dynamic responseItem in searchListResponse["items"])
                    {
                        if (responseItem["id"]["kind"].Value == "youtube#video")
                        {
                            videoIdString += Convert.ToString(responseItem["id"]["videoId"]) + ",";
                        }
                    }
                    if (!string.IsNullOrEmpty(videoIdString))
                    {
                        AllVideoResponse = GetResponse("https://www.googleapis.com/youtube/v3/videos?id=" + videoIdString.TrimEnd(',') + "&key=" + ApiKey + "&part=snippet,statistics,contentDetails", AllVideoResponse);
                    }

                    if (AllVideoResponse != null)
                    {
                        foreach (dynamic VideoResponse in AllVideoResponse["items"])
                        {
                            Entities.YoutubeVideo ytVideo = new Entities.YoutubeVideo();
                            FetchVideoDetail(ytVideo, ApiKey, VideoResponse);          // get video details  by Id  
                            Entities.VideoInfo ytVideoinfo = new Entities.VideoInfo()
                            {
                                ID = ytVideo.videoId,
                                Thumbnail = ytVideo.thumbnailUrl,
                                Url = ytVideo.videoUrl,
                                Title = ytVideo.title,
                                Duration = ytVideo.duration
                            };
                            ytVideoList.Add(ytVideoinfo);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(nextPageToken) && ytVideoList.Count > 0)
            {
                ytVideoList[0].AdditionalData = new Dictionary<string, object>
                {
                    { "nextPageToken", nextPageToken }
                };
            }
            return ytVideoList;
        }

        private static dynamic GetResponse(string url, dynamic searchListResponse)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                searchListResponse = JObject.Parse(reader.ReadToEnd());
                response.Close();
                reader.Close();
            }
            catch (WebException wex)
            {
                StreamReader reader = new StreamReader(wex.Response.GetResponseStream());
                dynamic errorListResponse = JObject.Parse(reader.ReadToEnd());
                string errorTemplate = string.Empty;
                foreach (dynamic err in errorListResponse["error"]["errors"])
                {
                    errorTemplate += string.Format("domain: {0}, reason: {1}, message: {2}, extendedHelp: {3}", err["domain"].Value, err["reason"].Value, err["message"].Value, err["extendedHelp"].Value);
                    errorTemplate += Environment.NewLine + Environment.NewLine;
                }
                Exceptions.LogException(new Exception(errorTemplate)); throw wex;
            }
            return searchListResponse;
        }

        private static void FetchVideoDetail(Entities.YoutubeVideo ytvideo, string apiKey, dynamic responseItem)
        {
            if (responseItem != null)
            {
                if (responseItem["id"] != null)
                {
                    ytvideo.videoId = Convert.ToString(responseItem["id"]);
                    ytvideo.videoUrl = Convert.ToString("https://www.youtube.com/watch?v=" + responseItem["id"]);
                }
                if (responseItem["snippet"]["thumbnails"]["medium"]["url"] != null)
                {
                    ytvideo.thumbnailUrl = Convert.ToString(responseItem["snippet"]["thumbnails"]["medium"]["url"]);
                }

                if (responseItem["snippet"]["title"] != null)
                {
                    ytvideo.title = Convert.ToString(responseItem["snippet"]["title"]);
                }

                if (responseItem["contentDetails"]["duration"] != null)
                {
                    TimeSpan youTubeDuration = XmlConvert.ToTimeSpan(Convert.ToString(responseItem["contentDetails"]["duration"]));
                    if (youTubeDuration.Hours > 0)
                    {
                        ytvideo.duration = youTubeDuration.Hours.ToString() + ":";
                    }

                    if (youTubeDuration.Minutes > 0)
                    {
                        ytvideo.duration = ytvideo.duration + (youTubeDuration.Minutes < 10 ? (youTubeDuration.Hours > 0 ? ("0" + youTubeDuration.Minutes.ToString() + ":") : youTubeDuration.Minutes.ToString() + ":") : youTubeDuration.Minutes.ToString() + ":");
                    }
                    else
                    {
                        ytvideo.duration = ytvideo.duration + "0:";
                    }

                    if (youTubeDuration.Seconds > 0)
                    {
                        ytvideo.duration = ytvideo.duration + (youTubeDuration.Seconds < 10 ? ("0" + youTubeDuration.Seconds.ToString()) : youTubeDuration.Seconds.ToString());
                    }
                    else
                    {
                        ytvideo.duration = ytvideo.duration + "00";
                    }
                }
            }
        }
    }
}
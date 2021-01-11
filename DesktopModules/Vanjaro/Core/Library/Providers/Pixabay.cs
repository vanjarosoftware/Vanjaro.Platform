using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities;
using Vanjaro.Core.Entities.Interface;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Providers
{
    public class Pixabay : IImageProvider, IVideoProvider
    {
        public string Name => "Pixabay";

        public bool Available => !string.IsNullOrEmpty(Key);

        public bool ShowLogo => true;

        public string Logo => Url.ResolveUrl("~/DesktopModules/Vanjaro/Core/Library/Resources/Images/pixabay.svg");

        public string Link => "https://pixabay.com";
        public bool IsSupportBackground => true;

        private string Key
        {
            get
            {
                string Pixabay_Key = SettingManager.GetPortalSetting("Vanjaro.Integration.Pixabay", true);
                return string.IsNullOrEmpty(Pixabay_Key) ? SettingManager.GetHostSetting("Vanjaro.Integration.Pixabay", true) : Pixabay_Key;
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
                    Uri Link = new Uri("https://pixabay.com/api/?key=" + Key);
                    string jsonString = client.DownloadString(Link);
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Managers.ExceptionManage.LogException(ex);
            }
            return result;
        }

        public Task<string> GetImages(string Keyword, int PageNo, int PageSize)
        {
            return GetImages(Key, Keyword, PageNo, PageSize);
        }

        public Task<string> GetVideos(string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData)
        {
            return GetVideos(Key, Keyword, PageNo, PageSize, AdditionalData);
        }

        public static async Task<string> GetImages(string ApiKey, string Keyword)
        {
            return await GetImages(ApiKey, Keyword, 1, 10);
        }

        public static async Task<string> GetImages(string ApiKey, string Keyword, int PageNo, int PageSize)
        {
            return await Get(ApiKey, Keyword, PageNo, PageSize, "photo");
        }

        public static async Task<string> GetVideos(string ApiKey, string Keyword, Dictionary<string, object> AdditionalData)
        {
            return await GetVideos(ApiKey, Keyword, 1, 10, AdditionalData);
        }

        public static async Task<string> GetVideos(string ApiKey, string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData)
        {
            return await Get(ApiKey, Keyword, PageNo, PageSize, "video", AdditionalData);
        }

        private static async Task<string> Get(string ApiKey, string Keyword, int PageNo, int PageSize, string Type, Dictionary<string, object> AdditionalData = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Keyword) && !string.IsNullOrEmpty(Type) && PageNo > 0 && PageSize > 0)
                {
                    Uri Link;
                    WebClient client = new WebClient();
                    string result = string.Empty;
                    if (Type == "photo")
                    {
                        Link = new Uri("https://pixabay.com/api/?key=" + ApiKey + "&q=" + Keyword + "&image_type=photo" + "&page=" + PageNo + "&per_page=" + PageSize + "&pretty=true");
                        result = await client.DownloadStringTaskAsync(Link);
                    }
                    else
                    {
                        Link = new Uri("https://pixabay.com/api/videos/?key=" + ApiKey + "&q=" + Keyword + "&video_type=all" + "&page=" + PageNo + "&per_page=" + PageSize + "&pretty=true");
                        result = await client.DownloadStringTaskAsync(Link);
                        PixabayVideo videos = new JavaScriptSerializer().Deserialize<PixabayVideo>(result);
                        List<Entities.VideoInfo> VideoList = new List<Entities.VideoInfo>();
                        foreach (VideoSearchHit item in videos.Hits)
                        {
                            TimeSpan t = TimeSpan.FromSeconds(item.Duration);
                            string PixabayDuration = string.Empty;
                            if (t.Hours > 0)
                            {
                                PixabayDuration = t.Hours.ToString() + ":";
                            }

                            if (t.Minutes > 0)
                            {
                                PixabayDuration = PixabayDuration + (t.Minutes < 10 ? (t.Hours > 0 ? ("0" + t.Minutes.ToString() + ":") : t.Minutes.ToString() + ":") : t.Minutes.ToString() + ":");
                            }
                            else
                            {
                                PixabayDuration = PixabayDuration + "0:";
                            }

                            if (t.Seconds > 0)
                            {
                                PixabayDuration = PixabayDuration + (t.Seconds < 10 ? ("0" + t.Seconds.ToString()) : t.Seconds.ToString());
                            }
                            else
                            {
                                PixabayDuration = PixabayDuration + "00";
                            }

                            VideoList.Add(new Entities.VideoInfo() { Thumbnail = "https://i.vimeocdn.com/video/" + item.picture_id + "_295x166.jpg", Url = item.Videos.Large.url, Title = item.User, Duration = PixabayDuration });
                        }
                        result = new JavaScriptSerializer().Serialize(VideoList);
                    }
                    return result;
                }
            }
            catch (Exception) { }
            return null;
        }
    }
}
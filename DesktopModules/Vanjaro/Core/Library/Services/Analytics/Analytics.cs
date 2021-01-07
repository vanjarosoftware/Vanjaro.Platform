using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Services
{
    public class Analytics
    {


#if DEBUG
        private const string TrackUrl = "https://www.google-analytics.com/";
        private const string SecretKey = "V9HPVrLWQBu4amOgbWDl9g";
        public const string MeasurementID = "G-WJTPTNNMHN";
#else
        private const string TrackUrl = "https://www.google-analytics.com/";
        private const string SecretKey = "V9HPVrLWQBu4amOgbWDl9g";
        public const string MeasurementID = "G-WJTPTNNMHN";
#endif


        public static void TrackEvent(Content.Event Event)
        {
            Content content = new Content()
            {
                events = Event
            };

            TrackEvent("mp/collect", content);
        }
        public static void TrackEvent(Content content)
        {
            TrackEvent("mp/collect", content);
        }
        public static void TrackEvent(string Action, Content.Event Event)
        {
            Content content = new Content()
            {
                events = Event
            };
            TrackEvent(Action, content);
        }
        public static void TrackEvent(string Action, Content content)
        {
            if (string.IsNullOrEmpty(Action) || string.IsNullOrEmpty(content.clientId))
                return;

            if (HostController.Instance.GetBoolean("VJImprovementProgram", true))
                PostWebResponse(TrackUrl + Action + "?measurement_id=" + MeasurementID + "&api_secret=" + SecretKey, content);
        }
        private static void PostWebResponse(string Url, Content content)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                string Json = JsonConvert.SerializeObject(content, Formatting.Indented);
                var stringContent = new StringContent(Json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("user-agent", HttpContext.Current.Request.UserAgent);
                HttpResponseMessage Res = client.PostAsync("", stringContent).Result;
            }
        }


        #region Post Content Class
        public class Content
        {
            string _ClientID = null;
            public string clientId
            {
                get
                {
                    if (_ClientID == null)

                        if (!string.IsNullOrEmpty(CookieManager.GetValue("vj_AnalyticsClientID")))
                            _ClientID = CookieManager.GetValue("vj_AnalyticsClientID");
                        else
                        {
                            Exceptions.LogException(new Exception("Analytics ClientID is null"));
                            _ClientID = null;
                        }
                    return _ClientID;
                }
            }
            public string userId { get { return HostController.Instance.GetString("GUID"); } }
            public bool nonPersonalizedAds { get; set; }
            public Event events { get; set; }

            public class Event
            {
                public string name { get; set; }

                [JsonProperty(PropertyName = "params")]
                public Dictionary<string, string> parameter { get; set; }
            }
        }
        #endregion

    }






}

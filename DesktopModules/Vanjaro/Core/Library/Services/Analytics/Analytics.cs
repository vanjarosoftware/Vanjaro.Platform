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

        public static void TrackEvent(Event Event)
        {
            if (Event != null)
            {
                if (HttpContext.Current.Application["AnalyticEvents"] == null)
                    HttpContext.Current.Application["AnalyticEvents"] = new List<Event>();

                if (HttpContext.Current.Application["AnalyticEvents"] != null)
                {
                    Event.parameter.Add("send_to", MeasurementID);
                    List<Event> e = new List<Event>();
                    e = (List<Event>)HttpContext.Current.Application["AnalyticEvents"];
                    e.Add(Event);
                    HttpContext.Current.Application["AnalyticEvents"] = e;
                }
            }           
        }

        public static string PostEvent()
        {
            StringBuilder sb = new StringBuilder();
            if (HttpContext.Current.Application["AnalyticEvents"] != null && ((List<Event>)HttpContext.Current.Application["AnalyticEvents"]).Count > 0)
            {
                foreach (Event e in (List<Event>)HttpContext.Current.Application["AnalyticEvents"])
                {
                    if (e.parameter.Count > 0)
                    {
                        StringBuilder s = new StringBuilder();
                        s.Append("gtag('event','" + e.name + "',{");
                        foreach (var p in e.parameter)
                        {
                            s.Append("'" + p.Key + "': '" + p.Value + "',");
                        }
                        sb.Append(s.ToString().TrimEnd(',') + "});");
                    }
                }
            }
            HttpContext.Current.Application["AnalyticEvents"] = null;
            return sb.ToString();
        }

        public static bool RegisterScript { get { return (HttpContext.Current.Application["AnalyticEvents"] != null && ((List<Event>)HttpContext.Current.Application["AnalyticEvents"]).Count > 0); } }

        #region Post Content Class

        public class Event
        {
            public string name { get; set; }
            public Dictionary<string, string> parameter { get; set; }
        }
        #endregion
    }
}

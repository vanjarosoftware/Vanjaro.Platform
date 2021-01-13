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

#if RELEASE
        public const string MeasurementID = "G-W7S4EZFGWW";
#else        
        public const string MeasurementID = "G-WJTPTNNMHN";
#endif

        public static void TrackEvent(Event Event)
        {
            if (Event != null)
            {
                if (HttpContext.Current.Application["AnalyticsEvents"] == null)
                    HttpContext.Current.Application["AnalyticsEvents"] = new List<Event>();

                if (HttpContext.Current.Application["AnalyticsEvents"] != null)
                {
                    Event.parameter.Add("send_to", MeasurementID);
                    Event.parameter.Add("non_interaction", "true");
                    List<Event> e = new List<Event>();
                    e = (List<Event>)HttpContext.Current.Application["AnalyticsEvents"];
                    e.Add(Event);
                    HttpContext.Current.Application["AnalyticsEvents"] = e;
                }
            }
        }

        public static string PostEvent()
        {
            StringBuilder sb = new StringBuilder();
            if (HttpContext.Current.Application["AnalyticsEvents"] != null && ((List<Event>)HttpContext.Current.Application["AnalyticsEvents"]).Count > 0)
            {
                foreach (Event e in (List<Event>)HttpContext.Current.Application["AnalyticsEvents"])
                {
                    if (!string.IsNullOrEmpty(e.name) && e.parameter.Count > 0)
                    {
                        StringBuilder s = new StringBuilder();
                        s.Append("gtag(\"event\",\"" + ((e.name.Length <= 40) ? e.name : e.name.Substring(0, 40)) + "\",{");
                        foreach (var p in e.parameter)
                        {
                            if (!string.IsNullOrEmpty(p.Value))
                            {
                                string Value = p.Value.Trim(' ').TrimEnd(new[] { '/', '\\' });
                                s.Append("\"" + ((p.Key.Length <= 25) ? p.Key : p.Key.Substring(0, 25)) + "\": \"" + ((Value.Length <= 100) ? Value : Value.Substring(0, 100)) + "\",");
                            }
                        }
                        sb.Append(s.ToString().TrimEnd(',') + "});");
                    }
                }
            }
            HttpContext.Current.Application["AnalyticsEvents"] = null;
            return sb.ToString();
        }

        public static bool RegisterScript { get { return (HttpContext.Current.Application["AnalyticsEvents"] != null && ((List<Event>)HttpContext.Current.Application["AnalyticsEvents"]).Count > 0); } }

        #region Post Content Class

        public class Event
        {
            public string name { get; set; }
            public Dictionary<string, string> parameter { get; set; }
        }
        #endregion
    }
}

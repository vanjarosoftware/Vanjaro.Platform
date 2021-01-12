using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Vanjaro.Core.Services;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class AnalyticsManager
        {
            public static void TrackEvent(string Name, string Parameter, string Value)
            {
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add(Parameter, Value);
                TrackEvent(Name, p);
            }

            protected static void TrackEvent(string Name, Dictionary<string, string> Parameters)
            {
                Analytics.Event e = new Analytics.Event()
                {
                    name = Name,
                    parameter = Parameters
                };
                Analytics.TrackEvent(e);
            }

            internal static void TrackException(Exception ex)
            {
                if (ex != null && !string.IsNullOrEmpty(ex.Message))
                {
                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                    parameter.Add("description", ex.Message);
                    if (!string.IsNullOrEmpty(ex.StackTrace))
                        parameter.Add("stacktrace", ex.StackTrace);
                    parameter.Add("fatal", "false");
                    TrackEvent("exception", parameter);
                }
            }

            public static void Post()
            {
                string AnalyticsUpdate = SettingManager.GetHostSetting("AnalyticsUpdate", false, "");

                if (!string.IsNullOrEmpty(AnalyticsUpdate))
                {
                    bool extension = SettingManager.IsVanjaroExtensionInstalled();
                    if (AnalyticsUpdate == "install")
                    {
                        Dictionary<string, string> parameter = new Dictionary<string, string>();
                        parameter.Add(extension ? "extension" : "platform", Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.'));
                        TrackEvent("install", parameter);
                        SettingManager.UpdateHostSetting("AnalyticsUpdate", "", false);

                    }
                    else if (AnalyticsUpdate == "upgrade")
                    {
                        Dictionary<string, string> parameter = new Dictionary<string, string>();
                        parameter.Add(extension ? "extension" : "platform", Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.'));
                        TrackEvent("upgrade", parameter);
                        SettingManager.UpdateHostSetting("AnalyticsUpdate", "", false);
                    }
                }

                if (HttpContext.Current != null && HttpContext.Current.Application["PingAnalytics"] != null && HttpContext.Current.Application["PingAnalytics"].ToString().ToLower() == "true")
                {
                    bool extension = SettingManager.IsVanjaroExtensionInstalled();
                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                    parameter.Add(extension ? "extension" : "platform", Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.'));
                    TrackEvent("ping", parameter);
                    HttpContext.Current.Application["PingAnalytics"] = null;
                }
            }
        }
    }
}
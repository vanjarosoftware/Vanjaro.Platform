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
            internal static void TrackEvent(string Version)
            {
                if (Version == "01.00.00" && !SettingManager.IsVanjaroInstalled())
                    SettingManager.UpdateHostSetting("AnalyticsUpdate", "install", false);
                else
                {
                    if (SettingManager.GetHostSetting("AnalyticsUpdate", false, string.Empty) == "install")
                        return;
                    SettingManager.UpdateHostSetting("AnalyticsUpdate", "upgrade", false);
                }
            }

            public static void Post()
            {
                string AnalyticsUpdate = SettingManager.GetHostSetting("AnalyticsUpdate", false, "");

                if (!string.IsNullOrEmpty(AnalyticsUpdate))
                {
                    bool extension = SettingManager.IsInstalledVanjaroExtension();
                    if (AnalyticsUpdate == "install")
                    {
                        Dictionary<string, string> parameter = new Dictionary<string, string>();
                        parameter.Add(extension ? "extension" : "platform", Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.'));
                        Analytics.Event e = new Analytics.Event()
                        {
                            name = "install",
                            parameter = parameter
                        };
                        Analytics.TrackEvent(e);
                        SettingManager.UpdateHostSetting("AnalyticsUpdate", "", false);

                    }
                    else if (AnalyticsUpdate == "upgrade")
                    {
                        Dictionary<string, string> parameter = new Dictionary<string, string>();
                        parameter.Add(extension ? "extension" : "platform", Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.'));
                        Analytics.Event e = new Analytics.Event()
                        {
                            name = "upgrade",
                            parameter = parameter
                        };
                        Analytics.TrackEvent(e);
                        SettingManager.UpdateHostSetting("AnalyticsUpdate", "", false);
                    }
                }
            }

            private static void Ping()
            {
                if (HttpContext.Current != null && HttpContext.Current.Application["PingAnalytics"] != null && HttpContext.Current.Application["PingAnalytics"].ToString().ToLower() == "true")
                {
                    bool extension = SettingManager.IsInstalledVanjaroExtension();
                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                    parameter.Add(extension ? "extension" : "platform", Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.'));
                    Analytics.Event e = new Analytics.Event()
                    {
                        name = "ping",
                        parameter = parameter
                    };
                    Analytics.TrackEvent(e);
                    HttpContext.Current.Application["PingAnalytics"] = null;
                }
            }

            public static void AnalyticsPost()
            {
                Ping();
                Post();
            }
        }
    }
}
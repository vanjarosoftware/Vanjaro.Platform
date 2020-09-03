using DotNetNuke.Services.Analytics.Config;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Components;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Managers
{
    public class ConnectorManager
    {
        public static IDictionary<string, dynamic> GetConfig()
        {
            AnalyticsConfiguration config = AnalyticsConfiguration.GetConfig("GoogleAnalytics");
            string empty = string.Empty;
            string settingValue = string.Empty;
            bool trackforadmin = false;
            bool anonymizeip = false;
            bool trackuserid = false;
            if (config != null)
            {
                foreach (AnalyticsSetting setting in config.Settings)
                {
                    string lower = setting.SettingName.ToLower();
                    if (lower == "trackingid")
                    {
                        empty = setting.SettingValue;
                    }
                    else if (lower == "urlparameter")
                    {
                        settingValue = setting.SettingValue;
                    }
                    else if (lower == "trackforadmin")
                    {
                        if (bool.TryParse(setting.SettingValue, out trackforadmin))
                        {
                            continue;
                        }
                        trackforadmin = true;
                    }
                    else if (lower == "anonymizeip")
                    {
                        if (bool.TryParse(setting.SettingValue, out anonymizeip))
                        {
                            continue;
                        }
                        anonymizeip = true;
                    }
                    else if (lower == "trackuserid")
                    {
                        if (bool.TryParse(setting.SettingValue, out trackuserid))
                        {
                            continue;
                        }
                        trackuserid = true;
                    }
                }

            }
            return new Dictionary<string, dynamic>()
            {
                { "TrackingID", empty },
                { "UrlParameter", settingValue },
                { "TrackAdministrators", trackforadmin },
                { "AnonymizeIp", anonymizeip },
                { "TrackUserId", trackuserid },
                { "isDeactivating", false.ToString() }
            };
        }

        public static bool HasConfig()
        {
            IDictionary<string, dynamic> config = GetConfig();
            if (!config.ContainsKey("TrackingID"))
            {
                return false;
            }
            return !string.IsNullOrEmpty(config["TrackingID"]);
        }
        public static void DeleteConfig()
        {
            AnalyticsConfiguration analyticsConfiguration = new AnalyticsConfiguration()
            {
                Settings = new AnalyticsSettingCollection()
            };
            AnalyticsSetting analyticsSetting = new AnalyticsSetting()
            {
                SettingName = "TrackingId",
                SettingValue = ""
            };
            analyticsConfiguration.Settings.Add(analyticsSetting);
            AnalyticsSetting analyticsSetting1 = new AnalyticsSetting()
            {
                SettingName = "UrlParameter",
                SettingValue = ""
            };
            analyticsConfiguration.Settings.Add(analyticsSetting1);
            AnalyticsSetting analyticsSetting2 = new AnalyticsSetting()
            {
                SettingName = "TrackForAdmin",
                SettingValue = "false"
            };
            analyticsConfiguration.Settings.Add(analyticsSetting2);
            AnalyticsSetting analyticsSetting3 = new AnalyticsSetting()
            {
                SettingName = "AnonymizeIp",
                SettingValue = "false"
            };
            analyticsConfiguration.Settings.Add(analyticsSetting3);
            AnalyticsSetting analyticsSetting4 = new AnalyticsSetting()
            {
                SettingName = "TrackUserId",
                SettingValue = "false"
            };
            analyticsConfiguration.Settings.Add(analyticsSetting4);
            AnalyticsConfiguration.SaveConfig("GoogleAnalytics", analyticsConfiguration);
        }

        public static ActionResult SaveConfig(IDictionary<string, dynamic> values)
        {
            ActionResult actionResult = new ActionResult();
            string str;
            string str1;
            bool str2;
            bool str3;
            bool str4;
            object empty;
            string customErrorMessage = string.Empty;
            try
            {
                bool.TryParse(values["isDeactivating"].ToLowerInvariant(), out bool flag);
                bool flag1 = true;
                if (!flag)
                {
                    str = (values["TrackingID"] != null ? values["TrackingID"].ToLowerInvariant().Trim() : string.Empty);
                    string item = values["UrlParameter"];
                    if (item != null)
                    {
                        empty = item.Trim();
                    }
                    else
                    {
                        empty = null;
                    }
                    if (empty == null)
                    {
                        empty = string.Empty;
                    }
                    str1 = (string)empty;
                    str2 = (values["TrackAdministrators"] != null ? values["TrackAdministrators"] : false);
                    str3 = (values["AnonymizeIp"] != null ? values["AnonymizeIp"] : false);
                    str4 = (values["TrackUserId"] != null ? values["TrackUserId"] : false);
                    if (string.IsNullOrEmpty(str))
                    {
                        flag1 = false;
                        customErrorMessage = Localization.Get("TrackingCodeFormat", "ErrorMessage", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);
                    }
                }
                else
                {
                    str = string.Empty;
                    str1 = string.Empty;
                    str2 = false;
                    str3 = false;
                    str4 = false;
                }
                if (flag1)
                {
                    AnalyticsConfiguration analyticsConfiguration = new AnalyticsConfiguration()
                    {
                        Settings = new AnalyticsSettingCollection()
                    };
                    analyticsConfiguration.Settings.Add(new AnalyticsSetting()
                    {
                        SettingName = "TrackingId",
                        SettingValue = str
                    });
                    analyticsConfiguration.Settings.Add(new AnalyticsSetting()
                    {
                        SettingName = "UrlParameter",
                        SettingValue = str1
                    });
                    analyticsConfiguration.Settings.Add(new AnalyticsSetting()
                    {
                        SettingName = "TrackForAdmin",
                        SettingValue = str2.ToString()
                    });
                    analyticsConfiguration.Settings.Add(new AnalyticsSetting()
                    {
                        SettingName = "AnonymizeIp",
                        SettingValue = str3.ToString()
                    });
                    analyticsConfiguration.Settings.Add(new AnalyticsSetting()
                    {
                        SettingName = "TrackUserId",
                        SettingValue = str4.ToString()
                    });
                    AnalyticsConfiguration.SaveConfig("GoogleAnalytics", analyticsConfiguration);
                }

                if (flag1)
                {
                    actionResult.IsSuccess = true;
                }
                else
                {
                    string message = string.IsNullOrEmpty(customErrorMessage)
                                 ? Localization.Get("ErrSavingConnectorSettings", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix)
                                 : customErrorMessage;
                    actionResult.AddError("ErrSavingConnectorSettings", message);
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
            }
            return actionResult;
        }

    }
}
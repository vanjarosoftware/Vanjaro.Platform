using Dnn.PersonaBar.AdminLogs.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Vanjaro.UXManager.Extensions.Apps.LogsSettings.Entities;

namespace Vanjaro.UXManager.Extensions.Apps.LogsSettings
{
    public static partial class Managers
    {
        public class LogsManager
        {
            public static Dnn.PersonaBar.AdminLogs.Components.AdminLogsController _controller = new Dnn.PersonaBar.AdminLogs.Components.AdminLogsController();

            public static dynamic GetLogSettings(int pageSize, int pageIndex, string search, UserInfo UserInfo)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    IEnumerable<LogTypeConfigInfo> logTypes = LogController.Instance.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>();
                    int portalId;
                    var types = logTypes
                        .Where(x => UserInfo.IsSuperUser || (int.TryParse(x.LogTypePortalID, out portalId) && portalId == UserInfo.PortalID))
                        .Select(v => new
                        {
                            v.LogTypeFriendlyName,
                            v.LogTypeKey,
                            v.LogTypePortalID,
                            LogTypePortalName =
                                int.TryParse(v.LogTypePortalID, out portalId)
                                    ? PortalController.Instance.GetPortal(portalId).PortalName
                                    : "*",
                            v.LoggingIsActive,
                            v.EmailNotificationIsActive,
                            v.MailFromAddress,
                            v.MailToAddress,
                            v.ID,
                            v.KeepMostRecent,
                            v.NotificationThreshold,
                            v.NotificationThresholdTime,
                            v.NotificationThresholdTimeType
                        }).ToList();

                    if (!string.IsNullOrEmpty(search))
                    {
                        types = types.Where(s => s.LogTypeFriendlyName.ToLower().Contains(search) || s.LogTypeKey.ToLower().Contains(search)).ToList();
                    }

                    Result.Types = types.Skip(pageIndex * pageSize).Take(pageSize);
                    double NumberOfPages = (double)types.Count / pageSize;
                    if ((int)NumberOfPages > 0)
                    {
                        NumberOfPages = Math.Ceiling(NumberOfPages);
                    }

                    Result.NumberOfPages = NumberOfPages;
                    Result.Status = "Success";
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message;
                    Core.Managers.ExceptionManage.LogException(ex);
                }
                return Result;
            }

            public static List<LogTypeInfo> GetLogTypes()
            {
                List<LogTypeInfo> LogTypeInfos = new List<LogTypeInfo>();
                try
                {
                    LogTypeInfos = LogController.Instance.GetLogTypeInfoDictionary()
                                   .Values.OrderBy(t => t.LogTypeFriendlyName)
                                   .ToList();

                    LogTypeInfos.Insert(0, new LogTypeInfo
                    {
                        LogTypeFriendlyName = Localization.GetString("AllTypes", Dnn.PersonaBar.AdminLogs.Components.Constants.LocalResourcesFile),
                        LogTypeKey = "*"
                    });

                    var types = LogTypeInfos.Select(v => new
                    {
                        v.LogTypeFriendlyName,
                        v.LogTypeKey
                    }).ToList();
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                }
                return LogTypeInfos;
            }

            public static dynamic GetSingleLogSetting(UserInfo UserInfo, string logTypeConfigId)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    LogTypeConfigInfo configInfo = _controller.GetLogTypeConfig(logTypeConfigId);
                    if (!UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out int portalId) || portalId != UserInfo.PortalID))
                    {
                        Result.Status = "Unauthorized";
                        return Result;
                    }

                    Result.ID = configInfo.ID;
                    Result.LoggingIsActive = configInfo.LoggingIsActive;
                    Result.LogTypeFriendlyName = configInfo.LogTypeFriendlyName;
                    Result.LogTypeKey = configInfo.LogTypeKey;
                    Result.LogTypePortalID =
                                int.TryParse(configInfo.LogTypePortalID, out portalId) ? portalId.ToString() : "*";
                    Result.LogTypePortalName =
                            int.TryParse(configInfo.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*";
                    Result.KeepMostRecent = configInfo.KeepMostRecent;
                    Result.EmailNotificationIsActive = configInfo.EmailNotificationIsActive;
                    Result.NotificationThreshold = configInfo.NotificationThreshold;
                    Result.NotificationThresholdTime = configInfo.NotificationThresholdTime;
                    Result.NotificationThresholdTimeType = configInfo.NotificationThresholdTimeType;
                    Result.MailFromAddress = configInfo.MailFromAddress;
                    Result.MailToAddress = configInfo.MailToAddress;
                    Result.Status = "Success";

                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message;
                    Core.Managers.ExceptionManage.LogException(ex);
                }
                return Result;
            }

            public static dynamic DeleteLogSetting(UserInfo UserInfo, string LogTypeConfigId)
            {
                dynamic Data = new ExpandoObject();
                try
                {
                    LogTypeConfigInfo configInfo = _controller.GetLogTypeConfig(LogTypeConfigId);
                    if (!UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out int portalId) || portalId != UserInfo.PortalID))
                    {
                        Data.Status = "Unauthorized";
                        return Data;
                    }
                    _controller.DeleteLogTypeConfig(LogTypeConfigId);
                    Data.Status = "Success";
                }
                catch (Exception ex)
                {
                    Data.Status = ex.Message;
                    Core.Managers.ExceptionManage.LogException(ex);
                }
                return Data;
            }

            public static dynamic AddLogSetting(PortalSettings PortalSettings, UserInfo UserInfo, UpdateLogSettingsRequest request)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    bool isAdmin = UserInfo.Roles.Contains(PortalSettings.AdministratorRoleName);
                    if (isAdmin)
                    {
                        Result.Status = "Unauthorized";
                        return Result;
                    }
                    request.LogTypePortalID = UserInfo.IsSuperUser ? request.LogTypePortalID : PortalSettings.PortalId.ToString();
                    LogTypeConfigInfo logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                    _controller.AddLogTypeConfig(logTypeConfigInfo);
                    Result.Status = "Success";
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message;
                    Core.Managers.ExceptionManage.LogException(ex);
                }
                return Result;
            }

            public static dynamic UpdateLogSetting(UpdateLogSettingsRequest request, UserInfo UserInfo)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    request.LogTypePortalID = UserInfo.IsSuperUser ? request.LogTypePortalID : UserInfo.PortalID.ToString();
                    LogTypeConfigInfo configInfo = _controller.GetLogTypeConfig(request.ID);
                    if (!UserInfo.IsSuperUser &&
                        (!int.TryParse(configInfo.LogTypePortalID, out int settingPortalId) ||
                         !int.TryParse(request.LogTypePortalID, out int requestPortalId) || requestPortalId != settingPortalId))
                    {
                        Result.Status = "Unauthorized";
                        return Result;
                    }
                    LogTypeConfigInfo logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                    _controller.UpdateLogTypeConfig(logTypeConfigInfo);
                    return GetSingleLogSetting(UserInfo, request.ID);
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message;
                    Core.Managers.ExceptionManage.LogException(ex);
                }
                return Result;
            }

            public static dynamic GetKeepMostRecentOptions()
            {
                return _controller.GetKeepMostRecentOptions().ToList();
            }

            public static dynamic GetNotificationThresholds()
            {
                List<StringText> result = new List<StringText>();
                foreach (KeyValuePair<string, string> item in _controller.GetOccurenceThresholds().ToList())
                {
                    result.Add(new StringText { Text = item.Key, Value = item.Value });
                }
                return result;
            }

            public static dynamic GetNotificationTimes()
            {
                return _controller.GetOccurenceThresholdNotificationTimes().ToList();
            }

            public static dynamic GetNotificationTimeTypes()
            {
                List<StringText> result = new List<StringText>();
                foreach (KeyValuePair<string, string> item in _controller.GetOccurenceThresholdNotificationTimeTypes().ToList())
                {
                    result.Add(new StringText { Text = item.Key, Value = item.Value });
                }
                return result;
            }
        }
    }
}
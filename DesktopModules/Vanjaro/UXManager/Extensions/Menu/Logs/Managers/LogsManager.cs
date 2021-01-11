using Dnn.PersonaBar.AdminLogs.Services.Dto;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using Vanjaro.UXManager.Extensions.Menu.Logs.Data.Scripts;

namespace Vanjaro.UXManager.Extensions.Menu.Logs
{
    public static partial class Managers
    {
        public class LogsManager
        {
            public static Dnn.PersonaBar.AdminLogs.Components.AdminLogsController _controller = new Dnn.PersonaBar.AdminLogs.Components.AdminLogsController();

            public static dynamic GetLogTypes(int PortalID, UserInfo UserInfo)
            {
                try
                {
                    List<string> distinctLogTypes = new List<string>
                    {
                        "*"
                    };

                    string cmdText = "select distinct LogTypeKey from " + CommonScript.DnnTablePrefix + "EventLog where LogPortalID=" + PortalID;
                    if (UserInfo.IsSuperUser)
                    {
                        cmdText += " or LogPortalID is null";
                    }

                    IDataReader reader = DataProvider.Instance().ExecuteSQL(cmdText);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            distinctLogTypes.Add(reader.GetString(0));
                        }
                    }

                    List<LogTypeInfo> logTypes = LogController.Instance.GetLogTypeInfoDictionary()
                                    .Values.OrderBy(t => t.LogTypeFriendlyName)
                                    .ToList();

                    logTypes.Insert(0, new LogTypeInfo
                    {
                        LogTypeFriendlyName = Localization.GetString("AllTypes", Dnn.PersonaBar.AdminLogs.Components.Constants.LocalResourcesFile),
                        LogTypeKey = "*"
                    });

                    var types = logTypes.Select(v => new
                    {
                        v.LogTypeFriendlyName,
                        v.LogTypeKey
                    }).ToList();

                    return types.Where(t => distinctLogTypes.Contains(t.LogTypeKey)).ToList();
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    return ex.ToString();
                }
            }

            public static dynamic GetLogItems(int PortalID, string logType, int pageSize, int pageIndex, UserInfo UserInfo)
            {
                dynamic Result = new ExpandoObject();

                if (UserInfo.IsSuperUser)
                {
                    PortalID = -1;
                }
                else
                {
                    PortalID = UserInfo.PortalID;
                }

                int totalRecords = 0;
                List<LogInfo> logItems = LogController.Instance.GetLogs(PortalID,
                        logType == "*" ? string.Empty : logType,
                        pageSize, pageIndex, ref totalRecords);

                var items = logItems.Select(v => new
                {
                    v.LogGUID,
                    v.LogFileID,
                    _controller.GetMyLogType(v.LogTypeKey).LogTypeCSSClass,
                    _controller.GetMyLogType(v.LogTypeKey).LogTypeFriendlyName,
                    v.LogUserName,
                    v.LogPortalName,
                    LogCreateDate = v.LogCreateDate.ToString("G", CultureInfo.InvariantCulture),
                    Summary = GetSummary(v.LogProperties.Summary),
                    LogProperties = _controller.GetPropertiesText(v),
                    UserImage = GetUserPhoto(v.LogPortalID, v.LogUserID, v.LogUserName)
                });

                Result.Items = items;
                double NumberOfPages = (double)totalRecords / pageSize;
                if ((int)NumberOfPages > 0)
                {
                    NumberOfPages = Math.Ceiling(NumberOfPages);
                }

                Result.NumberOfPages = NumberOfPages;
                return Result;
            }

            public static string ClearLog()
            {
                try
                {
                    _controller.ClearLog();
                    return "Success";
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    return ex.ToString();
                }
            }

            public static string DeleteLogItems(List<string> logIds)
            {
                try
                {
                    foreach (string logId in logIds)
                    {
                        LogInfo objLogInfo = new LogInfo { LogGUID = logId };
                        LogController.Instance.DeleteLog(objLogInfo);
                    }
                    return "success";
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    return ex.ToString();
                }
            }

            public static dynamic EmailLogItems(EmailLogItemsRequest request, UserInfo UserInfo, PortalSettings PortalSettings)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    if (!UserInfo.IsSuperUser && request.LogIds.Any(
                        x =>
                            ((LogInfo)
                                LogController.Instance.GetSingleLog(new LogInfo { LogGUID = x },
                                    LoggingProvider.ReturnType.LogInfoObjects))?.LogPortalID != PortalSettings.PortalId))
                    {
                        Result.Status = Localization.GetString("UnAuthorizedToSendLog", Dnn.PersonaBar.AdminLogs.Components.Constants.LocalResourcesFile);
                        return Result;
                    }
                    string subject = request.Subject;
                    string strFromEmailAddress = !string.IsNullOrEmpty(UserInfo.Email) ? UserInfo.Email : PortalSettings.Email;
                    if (string.IsNullOrEmpty(subject))
                    {
                        subject = PortalSettings.PortalName + @" Exceptions";
                    }
                    string returnMsg = _controller.EmailLogItems(subject, strFromEmailAddress, request.Email,
                        request.Message, request.LogIds, out string error);
                    Result.Success = string.IsNullOrEmpty(returnMsg) ? true : false;
                    Result.ErrorMessage = error;
                    Result.Status = returnMsg;
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    Result.Status = ex.ToString();
                }
                return Result;
            }

            private static object GetSummary(string Summary)
            {
                if (!string.IsNullOrEmpty(Summary) && Summary.Length > 70)
                {
                    return Summary.ToString().Substring(0, 70);
                }
                else
                {
                    return Summary;
                }
            }

            private static string GetUserPhoto(int PortalID, int UserID, string UserName)
            {
                UserInfo userInfo = UserController.GetUserById(PortalID, UserID);
                if (userInfo != null)
                {
                    return Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalID, userInfo.UserID, userInfo.Email);
                }

                return Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalID, UserID, UserName);
            }
        }
    }
}
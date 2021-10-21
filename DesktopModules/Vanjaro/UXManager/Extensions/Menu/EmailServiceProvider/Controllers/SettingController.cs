using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.EmailServiceProvider.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings)
        {
            bool IsSuperUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            string mode = IsSuperUser ? "h" : "p";
            string HostServer = SettingManager.GetHostSetting("SMTPServer", false);
            string HostPort = "587";
            if (!string.IsNullOrEmpty(HostServer) && HostServer.Contains(":"))
            {
                try
                {
                    HostPort = HostServer.Split(':')[1];
                    HostServer = HostServer.Split(':')[0];
                }
                catch { }
            }
            string PortalServer = SettingManager.GetPortalSetting("SMTPServer", false);
            string PortalPort = "587";
            if (!string.IsNullOrEmpty(PortalServer) && PortalServer.Contains(":"))
            {
                try
                {
                    PortalPort = PortalServer.Split(':')[1];
                    PortalServer = PortalServer.Split(':')[0];
                }
                catch { }
            }
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "IsSuperUser", new UIData { Name = "IsSuperUser", Options = IsSuperUser } },
                { "SMTPmode", new UIData { Name = "SMTPmode", Options = IsSuperUser? SettingManager.GetPortalSetting("SMTPmode", false, mode) == mode:false } },
                { "Host_Server", new UIData { Name = "Host_Server", Value = HostServer } },
                { "Host_Port", new UIData { Name = "Host_Port", Value = HostPort } },
                { "Host_Username", new UIData { Name = "Host_Username", Value = SettingManager.GetHostSetting("SMTPUsername",false) } },
                { "Host_Password", new UIData { Name = "Host_Password", Value = SettingManager.GetHostSetting("SMTPPassword", true) } },
                { "Host_Email", new UIData { Name = "Host_Email", Value = SettingManager.GetHostSetting("SMTPEmail",false,PortalSettings.Email) } },
                { "Host_EnableSSL", new UIData { Name = "Host_EnableSSL", Options = SettingManager.GetHostSettingAsBoolean("SMTPEnableSSL", false) } },
                { "Host_PurgeLogsAfter", new UIData { Name = "Host_PurgeLogsAfter", Value = SettingManager.GetHostSetting("SMTPPurgeLogsAfter", false,"60") } },
                { "Portal_Server", new UIData { Name = "Portal_Server", Value = PortalServer } },
                { "Portal_Port", new UIData { Name = "Portal_Port", Value = PortalPort } },
                { "Portal_Username", new UIData { Name = "Portal_Username", Value = SettingManager.GetPortalSetting("SMTPUsername", false) } },
                { "Portal_Password", new UIData { Name = "Portal_Password", Value = SettingManager.GetPortalSetting("SMTPPassword", true) } },
                { "Portal_EnableSSL", new UIData { Name = "Portal_EnableSSL", Options = SettingManager.GetPortalSetting("SMTPEnableSSL", false) == "Y" } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Update(dynamic Data)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                if (bool.Parse(Data.SMTPmode.ToString()))
                {
                    SettingManager.UpdatePortalSetting("SMTPmode", "h", false);
                    SettingManager.UpdateHostSetting("SMTPServer", Data.Host_Server.ToString() + ":" + Data.Host_Port.ToString(), false);
                    SettingManager.UpdateHostSetting("SMTPAuthentication", "1", false);
                    SettingManager.UpdateHostSetting("SMTPUsername", Data.Host_Username.ToString(), false);
                    SettingManager.UpdateHostSetting("SMTPEmail", Data.Host_Email.ToString(), false);
                    SettingManager.UpdateHostSetting("SMTPPassword", Data.Host_Password.ToString(), true);
                    SettingManager.UpdateHostSetting("SMTPEnableSSL", bool.Parse(Data.Host_EnableSSL.ToString()) ? "Y" : "N", false);
                    SettingManager.UpdateHostSetting("SMTPPurgeLogsAfter", Data.Host_PurgeLogsAfter.ToString(), false);
                }
                else
                {
                    SettingManager.UpdatePortalSetting("SMTPmode", "p", false);
                    SettingManager.UpdatePortalSetting("SMTPAuthentication", "1", false);
                    SettingManager.UpdatePortalSetting("SMTPServer", Data.Portal_Server.ToString() + ":" + Data.Portal_Port.ToString(), false);
                    SettingManager.UpdatePortalSetting("SMTPUsername", Data.Portal_Username.ToString(), false);
                    SettingManager.UpdatePortalSetting("SMTPPassword", Data.Portal_Password.ToString(), true);
                    SettingManager.UpdatePortalSetting("SMTPEnableSSL", bool.Parse(Data.Portal_EnableSSL.ToString()) ? "Y" : "N", false);
                }

                DataCache.ClearCache();
                actionResult.IsSuccess = true;
            }
            catch (Exception ex)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        public ActionResult SendTestEmail(dynamic Data)
        {
            string LocalResourcesFile = "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/Setting/App_LocalResources/Setting.resx";
            ActionResult actionResult = new ActionResult();
            try
            {
                SmtpClient client = NotificationFactory.Connect(Data.Server.ToString(), int.Parse(Data.Port.ToString()), "1", Data.Username.ToString(), Data.Password.ToString(), bool.Parse(Data.EnableSSL.ToString()));
                if (client != null)
                {
                    MailQueue mail = new MailQueue
                    {
                        PortalID = PortalController.Instance.GetCurrentSettings().PortalId,
                        ModuleID = 0,
                        Subject = HtmlUtils.StripWhiteSpace(Localization.GetSystemMessage(PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"), true),
                        Content = HtmlUtils.StripWhiteSpace(Localization.GetSystemMessage(PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"), true),
                        SmtpUniqueId = -1,
                        Status = "Queue",
                        RetryAttempt = 0,
                        RetryDateTime = DateTime.UtcNow,
                        FromName = PortalSettings.UserInfo.DisplayName,
                        FromEmail = PortalSettings.UserInfo.Email,
                        ReplyEmail = PortalSettings.UserInfo.Email,
                        ToEmail = PortalSettings.UserInfo.Email,
                        Attachment = ""
                    };
                    NotificationFactory.SendMail(client, mail);
                    client.Dispose();
                    actionResult.IsSuccess = true;
                    actionResult.Data = Localization.GetString("EmailSentMessage", LocalResourcesFile) + PortalSettings.UserInfo.Email + Localization.GetString("to", LocalResourcesFile) + PortalSettings.UserInfo.Email;
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", ex.Message);
            }
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
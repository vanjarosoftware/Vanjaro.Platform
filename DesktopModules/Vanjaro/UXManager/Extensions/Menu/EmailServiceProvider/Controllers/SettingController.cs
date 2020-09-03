using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.EmailServiceProvider.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalId)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Server", new UIData { Name = "Server", Value = PortalController.GetPortalSetting("SMTPServer", PortalId, string.Empty) } },
                { "Username", new UIData { Name = "Username", Value = PortalController.GetPortalSetting("SMTPUsername", PortalId, string.Empty) } },
                { "Password", new UIData { Name = "Password", Value = PortalController.GetEncryptedString("SMTPPassword", PortalId, Config.GetDecryptionkey()) } },
                { "EnableSSL", new UIData { Name = "EnableSSL", Options = PortalController.GetPortalSetting("SMTPEnableSSL", PortalId, string.Empty) == "Y" } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Update(dynamic Data)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SMTPAuthentication", "1");
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SMTPmode", "p");
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SMTPServer", Data.Server.ToString());
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SMTPUsername", Data.Username.ToString());
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "SMTPPassword", Data.Password.ToString(), Config.GetDecryptionkey());
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SMTPEnableSSL", bool.Parse(Data.EnableSSL.ToString()) ? "Y" : "N");

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
                dynamic errMessage = Mail.SendMail(PortalSettings.UserInfo.Email,
                    PortalSettings.UserInfo.Email,
                    "",
                    "",
                    MailPriority.Normal,
                    Localization.GetSystemMessage(PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
                    MailFormat.Text,
                    Encoding.UTF8,
                    "",
                    "",
                    Data.Server.ToString(),
                    "1",
                    Data.Username.ToString(),
                    Data.Password.ToString(),
                    bool.Parse(Data.EnableSSL.ToString()));
                if (string.IsNullOrEmpty(errMessage))
                {
                    actionResult.IsSuccess = true;
                    actionResult.Data = Localization.GetString("EmailSentMessage", LocalResourcesFile) + PortalSettings.UserInfo.Email + Localization.GetString("to", LocalResourcesFile) + PortalSettings.UserInfo.Email;
                }
                else
                {
                    actionResult.AddError("errMessage", errMessage);
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleReCaptcha.Controllers
{
    [ValidateAntiForgeryToken]    
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            HostController hostController = new HostController();
            string Host_SiteKey = hostController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SiteKey", Config.GetDecryptionkey());
            string Host_SecretKey = hostController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SecretKey", Config.GetDecryptionkey());
            string Site_SiteKey = PortalController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SiteKey", portalId, Config.GetDecryptionkey());
            string Site_SecretKey = PortalController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SecretKey", portalId, Config.GetDecryptionkey());       

            Settings.Add("IsSuperUser", new UIData { Name = "IsSuperUser", Options = UserController.Instance.GetCurrentUserInfo().IsSuperUser });
            Settings.Add("ApplyTo", new UIData { Name = "ApplyTo", Options = false });
            Settings.Add("Host_SiteKey", new UIData { Name = "Host_SiteKey", Value = Host_SiteKey });
            Settings.Add("Host_SecretKey", new UIData { Name = "Host_SecretKey", Value = Host_SecretKey });
            Settings.Add("Host_HasSiteKey", new UIData { Name = "Host_HasSiteKey", Options = string.IsNullOrEmpty(Host_SiteKey) ? false : true });
            Settings.Add("Site_SiteKey", new UIData { Name = "Site_SiteKey", Value = Site_SiteKey });
            Settings.Add("Site_SecretKey", new UIData { Name = "Site_SecretKey", Value = Site_SecretKey });
            Settings.Add("Site_HasSiteKey", new UIData { Name = "Site_HasSiteKey", Options = string.IsNullOrEmpty(Site_SiteKey) ? false : true });
            return Settings.Values.ToList();
        }

        [AuthorizeAccessRoles(AccessRoles = "admin")]
        [HttpPost]
        public bool Save(dynamic Data)
        {
            if (bool.Parse(Data.ApplyTo.ToString()))
            {
                if (!string.IsNullOrEmpty(Data.Host_SiteKey.ToString()) && !string.IsNullOrEmpty(Data.Host_SecretKey.ToString()))
                {
                    HostController hostController = new HostController();
                    hostController.UpdateEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SiteKey", Data.Host_SiteKey.ToString(), Config.GetDecryptionkey());
                    hostController.UpdateEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SecretKey", Data.Host_SecretKey.ToString(), Config.GetDecryptionkey());
                }
                else
                    return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(Data.Site_SiteKey.ToString()) && !string.IsNullOrEmpty(Data.Site_SecretKey.ToString()))
                {
                    PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.GoogleReCaptcha.SiteKey", Data.Site_SiteKey.ToString(), Config.GetDecryptionkey());
                    PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.GoogleReCaptcha.SecretKey", Data.Site_SecretKey.ToString(), Config.GetDecryptionkey());
                }
                else
                    return false;
            }
            return true;
        }

        [AuthorizeAccessRoles(AccessRoles = "admin")]
        [HttpPost]
        public string Delete(dynamic Data)
        {
            if (bool.Parse(Data.ToString()))
            {
                HostController hostController = new HostController();
                hostController.UpdateEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SiteKey", string.Empty, Config.GetDecryptionkey());
                hostController.UpdateEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SecretKey", string.Empty, Config.GetDecryptionkey());
            }
            else
            {
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.GoogleReCaptcha.SiteKey", string.Empty, Config.GetDecryptionkey());
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.GoogleReCaptcha.SecretKey", string.Empty, Config.GetDecryptionkey());
            }

            return string.Empty;
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public string SiteKey()
        {
            HostController hostController = new HostController();
            string SiteKey = PortalController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SiteKey", PortalSettings.PortalId, Config.GetDecryptionkey());
            if (string.IsNullOrEmpty(SiteKey))
                SiteKey = hostController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SiteKey", Config.GetDecryptionkey());
            return SiteKey;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
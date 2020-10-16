using DotNetNuke.Common.Utilities;
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
            string SiteKey = PortalController.GetEncryptedString("Vanjaro.Integration.SiteKey", portalId, Config.GetDecryptionkey());
            string SecretKey = PortalController.GetEncryptedString("Vanjaro.Integration.SecretKey", portalId, Config.GetDecryptionkey());
            bool HasSiteKey = false;
            if (!string.IsNullOrEmpty(SiteKey))
            {
                HasSiteKey = true;
            }

            Settings.Add("SiteKey", new UIData { Name = "SiteKey", Value = SecretKey });
            Settings.Add("SecretKey", new UIData { Name = "SecretKey", Value = SecretKey });
            Settings.Add("HasSiteKey", new UIData { Name = "HasSiteKey", Options = HasSiteKey });
            return Settings.Values.ToList();
        }

        [AuthorizeAccessRoles(AccessRoles = "admin")]
        [HttpPost]
        public bool Save(dynamic Data)
        {
            if (!string.IsNullOrEmpty(Data.SiteKey.ToString())&& !string.IsNullOrEmpty(Data.SecretKey.ToString()))
            {
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.SiteKey", Data.SiteKey.ToString(), Config.GetDecryptionkey());
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.SecretKey", Data.SecretKey.ToString(), Config.GetDecryptionkey());
                return true;
            }
            return false;
        }

        [AuthorizeAccessRoles(AccessRoles = "admin")]
        [HttpGet]
        public string Delete()
        {
            PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.SiteKey", string.Empty, Config.GetDecryptionkey());
            PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.SecretKey", string.Empty, Config.GetDecryptionkey());
            return string.Empty;
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public string SiteKey()
        {
            string SiteKey = PortalController.GetEncryptedString("Vanjaro.Integration.SiteKey", PortalSettings.PortalId, Config.GetDecryptionkey());
            return SiteKey;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
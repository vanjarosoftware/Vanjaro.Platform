using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Menu.YouTube.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string Site_ApiKey = PortalController.GetEncryptedString("Vanjaro.Integration.YouTube", portalId, Config.GetDecryptionkey());
            string Host_ApiKey = HostController.Instance.GetEncryptedString("Vanjaro.Integration.YouTube", Config.GetDecryptionkey());

            Settings.Add("IsSuperUser", new UIData { Name = "IsSuperUser", Options = UserController.Instance.GetCurrentUserInfo().IsSuperUser });
            Settings.Add("ApplyTo", new UIData { Name = "ApplyTo", Options = false });
            Settings.Add("Host_ApiKey", new UIData { Name = "Host_ApiKey", Value = Host_ApiKey });
            Settings.Add("Host_HasApiKey", new UIData { Name = "Host_HasApiKey", Options = string.IsNullOrEmpty(Host_ApiKey) ? false : true });
            Settings.Add("Site_ApiKey", new UIData { Name = "Site_ApiKey", Value = Site_ApiKey });
            Settings.Add("Site_HasApiKey", new UIData { Name = "Site_HasApiKey", Options = string.IsNullOrEmpty(Site_ApiKey) ? false : true });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public bool Save(dynamic Data)
        {
            if (bool.Parse(Data.ApplyTo.ToString()))
            {
                if (Core.Providers.Youtube.IsValid(Data.Host_ApiKey.ToString()))
                {
                    HostController.Instance.UpdateEncryptedString("Vanjaro.Integration.YouTube", Data.Host_ApiKey.ToString(), Config.GetDecryptionkey());
                }
                else
                    return false;
            }
            else
            {
                if (Core.Providers.Youtube.IsValid(Data.Site_ApiKey.ToString()))
                {
                    PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.YouTube", Data.Site_ApiKey.ToString(), Config.GetDecryptionkey());
                }
                else
                    return false;
            }
            return true;
        }

        [HttpPost]
        public string Delete(dynamic Data)
        {
            if (bool.Parse(Data.ToString()))
                HostController.Instance.UpdateEncryptedString("Vanjaro.Integration.YouTube", string.Empty, Config.GetDecryptionkey());
            else
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.YouTube", string.Empty, Config.GetDecryptionkey());
            return string.Empty;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
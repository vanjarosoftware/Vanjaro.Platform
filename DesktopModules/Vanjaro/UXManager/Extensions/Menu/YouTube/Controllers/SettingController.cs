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
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.YouTube.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string Site_ApiKey = SettingManager.GetPortalSetting("Vanjaro.Integration.YouTube", true);
            string Host_ApiKey = SettingManager.GetHostSetting("Vanjaro.Integration.YouTube", true);

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
                    SettingManager.UpdateHostSetting("Vanjaro.Integration.YouTube", Data.Host_ApiKey.ToString(), true);
                }
                else
                    return false;
            }
            else
            {
                if (Core.Providers.Youtube.IsValid(Data.Site_ApiKey.ToString()))
                {
                    SettingManager.UpdatePortalSetting("Vanjaro.Integration.YouTube", Data.Site_ApiKey.ToString(), true);
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
                SettingManager.UpdateHostSetting("Vanjaro.Integration.YouTube", string.Empty, true);
            else
                SettingManager.UpdatePortalSetting("Vanjaro.Integration.YouTube", string.Empty, true);
            return string.Empty;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
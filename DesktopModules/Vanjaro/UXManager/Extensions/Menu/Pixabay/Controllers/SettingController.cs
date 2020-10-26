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

namespace Vanjaro.UXManager.Extensions.Menu.Pixabay.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string Site_ApiKey = SettingManager.GetPortalSetting("Vanjaro.Integration.Pixabay", true);
            string Host_ApiKey = SettingManager.GetHostSetting("Vanjaro.Integration.Pixabay", true);
            
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
                if (Vanjaro.Core.Providers.Pixabay.IsValid(Data.Host_ApiKey.ToString()))
                {
                    SettingManager.UpdateHostSetting("Vanjaro.Integration.Pixabay", Data.Host_ApiKey.ToString(), true);
                }
                else
                    return false;
            }
            else
            {
                if (Vanjaro.Core.Providers.Pixabay.IsValid(Data.Site_ApiKey.ToString()))
                {
                    SettingManager.UpdatePortalSetting("Vanjaro.Integration.Pixabay", Data.Site_ApiKey.ToString(), true);
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
                SettingManager.UpdateHostSetting("Vanjaro.Integration.Pixabay", string.Empty, true);
            else
                SettingManager.UpdatePortalSetting("Vanjaro.Integration.Pixabay", string.Empty, true);
            return string.Empty;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
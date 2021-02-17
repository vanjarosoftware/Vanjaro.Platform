using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleTagManager.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData()
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            Settings.Add("IsSuperUser", new UIData { Name = "IsSuperUser", Options = UserController.Instance.GetCurrentUserInfo().IsSuperUser });
            Settings.Add("ApplyTo", new UIData { Name = "ApplyTo", Options = false });
            Settings.Add("Host_Tag", new UIData { Name = "Host_Tag", Value = SettingManager.GetHostSetting("Vanjaro.Integration.GoogleTagManager", true) });
            Settings.Add("Site_Tag", new UIData { Name = "Site_Tag", Value = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleTagManager", true) });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public bool Save(dynamic Data)
        {
            if (bool.Parse(Data.ApplyTo.ToString()))
                SettingManager.UpdateHostSetting("Vanjaro.Integration.GoogleTagManager", Data.Host_Tag.ToString(), true);
            else
                SettingManager.UpdatePortalSetting("Vanjaro.Integration.GoogleTagManager", Data.Site_Tag.ToString(), true);
            
            return true;

        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}

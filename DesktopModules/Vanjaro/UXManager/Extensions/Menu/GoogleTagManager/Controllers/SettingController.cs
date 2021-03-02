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
            Settings.Add("Host_Head", new UIData { Name = "Host_Head", Value = SettingManager.GetHostSetting("Vanjaro.Integration.GoogleTagManager.Host_Head", true) });
            Settings.Add("Host_Body", new UIData { Name = "Host_Body", Value = SettingManager.GetHostSetting("Vanjaro.Integration.GoogleTagManager.Host_Body", true) });
            Settings.Add("Site_Head", new UIData { Name = "Site_Head", Value = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleTagManager.Site_Head", true) });
            Settings.Add("Site_Body", new UIData { Name = "Site_Body", Value = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleTagManager.Site_Body", true) });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public bool Save(dynamic Data)
        {
            if (bool.Parse(Data.ApplyTo.ToString()))
            {
                SettingManager.UpdateHostSetting("Vanjaro.Integration.GoogleTagManager.Host_Head", Data.Host_Head.ToString(), true);
                SettingManager.UpdateHostSetting("Vanjaro.Integration.GoogleTagManager.Host_Body", Data.Host_Body.ToString(), true);
            }
            else
            {
                SettingManager.UpdatePortalSetting("Vanjaro.Integration.GoogleTagManager.Site_Head", Data.Site_Head.ToString(), true);
                SettingManager.UpdatePortalSetting("Vanjaro.Integration.GoogleTagManager.Site_Body", Data.Site_Body.ToString(), true);
            }
            return true;

        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}

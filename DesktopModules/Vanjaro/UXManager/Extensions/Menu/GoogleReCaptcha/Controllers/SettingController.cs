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
using Vanjaro.Core.Services;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleReCaptcha.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]    
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string Host_SiteKey = SettingManager.GetHostSetting(Captcha.SiteKey, true);
            string Host_SecretKey = SettingManager.GetHostSetting(Captcha.SecretKey, true);
            bool Host_Enabled = SettingManager.GetHostSettingAsBoolean(Captcha.Enabled, false);
            string Site_SiteKey = SettingManager.GetPortalSetting(Captcha.SiteKey, true);
            string Site_SecretKey = SettingManager.GetPortalSetting(Captcha.SecretKey, true);
            bool Site_Enabled = SettingManager.GetPortalSettingAsBoolean(Captcha.Enabled);

            Settings.Add("IsSuperUser", new UIData { Name = "IsSuperUser", Options = UserController.Instance.GetCurrentUserInfo().IsSuperUser });
            Settings.Add("ApplyTo", new UIData { Name = "ApplyTo", Options = false });
            Settings.Add("Host_SiteKey", new UIData { Name = "Host_SiteKey", Value = Host_SiteKey });
            Settings.Add("Host_SecretKey", new UIData { Name = "Host_SecretKey", Value = Host_SecretKey });
            Settings.Add("Host_Enabled", new UIData { Name = "Host_Enabled", Options = Host_Enabled });
            Settings.Add("Site_SiteKey", new UIData { Name = "Site_SiteKey", Value = Site_SiteKey });
            Settings.Add("Site_SecretKey", new UIData { Name = "Site_SecretKey", Value = Site_SecretKey });
            Settings.Add("Site_Enabled", new UIData { Name = "Site_Enabled", Options = Site_Enabled });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public void Save(dynamic Data)
        {
            if (bool.Parse(Data.ApplyTo.ToString()))
            {
                SettingManager.UpdateHostSetting(Captcha.SiteKey, Data.Host_SiteKey.ToString(), true);
                SettingManager.UpdateHostSetting(Captcha.SecretKey, Data.Host_SecretKey.ToString(), true);
                SettingManager.UpdateHostSetting(Captcha.Enabled, Data.Host_Enabled.ToString(), false);

            }
            else
            {
                SettingManager.UpdatePortalSetting(Captcha.SiteKey, Data.Site_SiteKey.ToString(), true);
                SettingManager.UpdatePortalSetting(Captcha.SecretKey, Data.Site_SecretKey.ToString(), true);
                SettingManager.UpdatePortalSetting(Captcha.Enabled, Data.Site_Enabled.ToString(), false);
            }
        }
        
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
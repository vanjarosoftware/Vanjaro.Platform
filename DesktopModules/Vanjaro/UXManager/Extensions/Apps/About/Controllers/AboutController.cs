using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Apps.About.Managers;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Apps.About.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class AboutController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings portalSettings, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            bool isHost = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            Settings.Add("ProductName", new UIData() { Name = "ProductName", Value = DotNetNukeContext.Current.Application.Description });
            Settings.Add("ProductVersion", new UIData() { Name = "ProductVersion", Value = Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, true) });
            Settings.Add("FrameworkVersion", new UIData() { Name = "FrameworkVersion", Value = isHost ? Globals.NETFrameworkVersion.ToString(2) : "" });
            Settings.Add("ServerName", new UIData() { Name = "ServerName", Value = isHost ? Globals.ServerName : "" });
            Settings.Add("LogoutUrl", new UIData() { Name = "LogoutUrl", Value = Core.Managers.LoginManager.Logoff() });
            Settings.Add("RedirectAfterLogout", new UIData() { Name = "RedirectAfterLogout", Value = GetRedirectAfterLogout(portalSettings) });
            // Settings.Add("IncrementVersionCount", new UIData() { Name = "IncrementVersionCount", Value = GetPortalVersion(portalSettings.PortalId) });
            Settings.Add("EnableMode", new UIData() { Name = "EnableMode", Options = bool.Parse(HostController.Instance.GetString(ClientResourceSettings.EnableCompositeFilesKey, "false")) });


            //Get Vanjaro Version            
            Settings.Add("VanjaroVersion", new UIData() { Name = "VanjaroVersion", Value = Core.Managers.SettingManager.GetVersion().TrimEnd('0').TrimEnd('.') });
            Settings.Add("SKU", new UIData() { Name = "SKU", Value = Core.Components.Product.SKU });

            return Settings.Values.ToList();
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "host")]
        public void RestartApplication()
        {
            try
            {
                string LocalResourceFile = Path.Combine("~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + AboutInfo.Name + "/Views/App_LocalResources/Shared.resx");
                LogInfo log = new LogInfo { BypassBuffering = true, LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("Message", Localization.GetString("UserRestart", LocalResourceFile));
                LogController.Instance.AddLog(log);
                Config.Touch();
            }
            catch (Exception exc)
            {
                ExceptionManager.LogException(exc);
            }
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "host")]
        public void ClearCache()
        {
            try
            {
                DataCache.ClearCache();
                ClientResourceManager.ClearCache();
            }
            catch (Exception exc)
            {
                ExceptionManager.LogException(exc);
            }
        }

        [HttpPost]
        public void IncrementCRMVersion()
        {
            HostController.Instance.IncrementCrmVersion(false);
        }

        [HttpGet]
        public bool EnableMode(bool IsEnabled)
        {
            HostController.Instance.Update(ClientResourceSettings.EnableCompositeFilesKey, IsEnabled.ToString());
            HostController.Instance.Update(ClientResourceSettings.MinifyCssKey, IsEnabled.ToString());
            HostController.Instance.Update(ClientResourceSettings.MinifyJsKey, IsEnabled.ToString());
            return IsEnabled;
        }

        private static string GetRedirectAfterLogout(PortalSettings portalSettings)
        {
            string result = "-1";
            if (portalSettings != null && portalSettings.Registration != null)
            {
                result = portalSettings.Registration.RedirectAfterLogout.ToString();
            }

            return result;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
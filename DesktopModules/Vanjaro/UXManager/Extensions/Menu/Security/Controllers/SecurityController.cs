using Dnn.PersonaBar.Security.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Security.Factories;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Extensions.Menu.Security.Factories.AppFactory;
using static Vanjaro.UXManager.Extensions.Menu.Security.Managers;
using DataCache = DotNetNuke.Common.Utilities.DataCache;

namespace Vanjaro.UXManager.Extensions.Menu.Security.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SecurityController : UIEngineController
    {
        public static List<IUIData> GetData(PortalSettings portalSettings, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();

            Settings.Add("UpdateSslSettingsRequest", new UIData { Name = "UpdateSslSettingsRequest", Options = Managers.SecurityManager.GetSslSettings(portalSettings, userInfo) });

            List<TreeView> DefaultFolders = new List<TreeView>
            {
                new TreeView() { Text = "Default", Value = -1 }
            };
            DefaultFolders.AddRange(BrowseUploadFactory.GetFolders(portalSettings.PortalId).Where(f => !f.Text.Contains(".versions")));
            Settings.Add("Picture_DefaultFolder", new UIData { Name = "Picture_DefaultFolder", Options = DefaultFolders, Value = Core.Managers.SettingManager.GetValue(portalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Picture_DefaultFolder", AppFactory.GetViews()) });
            Settings.Add("Video_DefaultFolder", new UIData { Name = "Video_DefaultFolder", Options = DefaultFolders, Value = Core.Managers.SettingManager.GetValue(portalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Video_DefaultFolder", AppFactory.GetViews()) });
            Settings.Add("Picture_MaxUploadSize", new UIData { Name = "Picture_MaxUploadSize", Value = Core.Managers.SettingManager.GetValue(portalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Picture_MaxUploadSize", AppFactory.GetViews()) });
            Settings.Add("Video_MaxUploadSize", new UIData { Name = "Video_MaxUploadSize", Value = Core.Managers.SettingManager.GetValue(portalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Video_MaxUploadSize", AppFactory.GetViews()) });
            Settings.Add("Picture_AllowableFileExtensions", new UIData { Name = "Picture_AllowableFileExtensions", Value = Core.Managers.SettingManager.GetValue(portalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Picture_AllowableFileExtensions", AppFactory.GetViews()) });
            Settings.Add("Video_AllowableFileExtensions", new UIData { Name = "Video_AllowableFileExtensions", Value = Core.Managers.SettingManager.GetValue(portalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Video_AllowableFileExtensions", AppFactory.GetViews()) });
            if (userInfo.IsSuperUser)
            {
                Settings.Add("AutoAccountUnlockDuration", new UIData { Name = "AutoAccountUnlockDuration", Value = Host.AutoAccountUnlockDuration.ToString() });
                Settings.Add("AsyncTimeout", new UIData { Name = "AsyncTimeout", Value = Host.AsyncTimeout.ToString() });
                Settings.Add("FileExtensions", new UIData { Name = "FileExtensions", Value = Host.AllowedExtensionWhitelist.ToStorageString() });
                Settings.Add("MaxUploadSize", new UIData { Name = "MaxUploadSize", Value = (Config.GetMaxUploadSize() / (1024 * 1024)).ToString() });
                Settings.Add("DefaultEndUserExtensionWhitelist", new UIData { Name = "DefaultEndUserExtensionWhitelist", Value = Host.DefaultEndUserExtensionWhitelist.ToStorageString() });
            }
            Settings.Add("IsSuperUser", new UIData { Name = "IsSuperUser", Value = userInfo.IsSuperUser.ToString() });
            return Settings.Values.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSettings(dynamic settingData)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Entities.UpdateSslSettingsRequest UpdateSslSettingsRequest = JsonConvert.DeserializeObject<Entities.UpdateSslSettingsRequest>(settingData.UpdateSslSettingsRequest.ToString());

                actionResult = UpdateMediaSettings(settingData);
                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

                if (UserInfo.IsSuperUser)
                {
                    actionResult = UpdateGeneralSettings(settingData);
                }

                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

                actionResult = UpdateSslSettings(PortalSettings, UpdateSslSettingsRequest);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                actionResult.AddError("", ex.Message);
            }
            return actionResult;
        }

        private ActionResult UpdateGeneralSettings(dynamic request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                HostController.Instance.Update("AutoAccountUnlockDuration", request.AutoAccountUnlockDuration.Value.ToString(), false);
                HostController.Instance.Update("AsyncTimeout", request.AsyncTimeout.Value.ToString(), false);
                HostController.Instance.Update("FileExtensions", SecurityManager.ValidateFileExtension(request.FileExtensions.Value.ToString()), false);
                HostController.Instance.Update("DefaultEndUserExtensionWhitelist", SecurityManager.ValidateFileExtension(request.DefaultEndUserExtensionWhitelist.Value.ToString()), false);

                long maxCurrentRequest = Config.GetMaxUploadSize();
                dynamic maxUploadByMb = request.MaxUploadSize.Value * 1024 * 1024;
                if (maxCurrentRequest != maxUploadByMb)
                {
                    Config.SetMaxUploadSize(maxUploadByMb);
                }

                DataCache.ClearCache();
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }
        private ActionResult UpdateMediaSettings(dynamic request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Picture_DefaultFolder", request.Picture_DefaultFolder.Value.ToString());
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Video_DefaultFolder", request.Video_DefaultFolder.Value.ToString());
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Picture_MaxUploadSize", request.Picture_MaxUploadSize.Value.ToString());
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Video_MaxUploadSize", request.Video_MaxUploadSize.Value.ToString());
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Picture_AllowableFileExtensions", SecurityManager.ValidateFileExtension(request.Picture_AllowableFileExtensions.Value.ToString()));
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.security_settings.ToString(), "Video_AllowableFileExtensions", request.Video_AllowableFileExtensions.Value.ToString());
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        public ActionResult UpdateSslSettings(PortalSettings PortalSettings, Entities.UpdateSslSettingsRequest request)
        {
            ActionResult ActionResult = new ActionResult();
            try
            {
                bool PreviousValue_SSLEnabled = PortalController.GetPortalSettingAsBoolean("SSLEnabled", PortalSettings.PortalId, false);
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SSLEnabled", request.SSLEnabled.ToString(), false);
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SSLEnforced", request.SSLEnforced.ToString(), false);
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SSLURL", AddPortalAlias(request.SSLURL, PortalSettings.PortalId), false);
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "STDURL", AddPortalAlias(request.STDURL, PortalSettings.PortalId), false);
                if (UserInfo.IsSuperUser)
                {
                    HostController.Instance.Update("SSLOffloadHeader", request.SSLOffloadHeader);
                }

                if (PreviousValue_SSLEnabled != request.SSLEnabled)
                {
                    foreach (KeyValuePair<int, TabInfo> t in TabController.Instance.GetTabsByPortal(PortalSettings.PortalId))
                    {
                        t.Value.IsSecure = request.SSLEnabled;
                        TabController.Instance.UpdateTab(t.Value);
                    }

                    if (PortalSettings != null && PortalSettings.ActiveTab != null && !string.IsNullOrEmpty(PortalSettings.ActiveTab.FullUrl))
                    {
                        ActionResult.RedirectURL = PortalSettings.ActiveTab.FullUrl;
                    }
                    else
                    {
                        ActionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL();
                    }
                }
                DataCache.ClearPortalCache(PortalSettings.PortalId, false);
            }
            catch (Exception ex)
            {
                ActionResult.AddError("UpdateSslSettings", ex.Message);
            }
            return ActionResult;
        }
        private string AddPortalAlias(string portalAlias, int portalId)
        {
            if (!string.IsNullOrEmpty(portalAlias))
            {
                portalAlias = portalAlias.ToLowerInvariant().Trim('/');
                if (portalAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    portalAlias = portalAlias.Remove(0, portalAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
                PortalAliasInfo alias = PortalAliasController.Instance.GetPortalAlias(portalAlias, portalId);
                if (alias == null)
                {
                    alias = new PortalAliasInfo { PortalID = portalId, HTTPAlias = portalAlias };
                    PortalAliasController.Instance.AddPortalAlias(alias);
                }
            }
            return portalAlias;
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
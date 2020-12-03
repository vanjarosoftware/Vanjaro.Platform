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

            var PageList = Library.Managers.PageManager.GetParentPages(PortalSettings.Current).Select(a => new { a.TabID, a.TabName, a.DisableLink });
            dynamic Results = Managers.SecurityManager.GetRegistrationSettings(portalSettings);
            dynamic basicLoginSettings = Managers.SecurityManager.GetBasicLoginSettings(portalSettings);
            if (basicLoginSettings != null)
            {
                Settings.Add("UpdateBasicLoginSettingsRequest", new UIData { Name = "UpdateBasicLoginSettingsRequest", Options = basicLoginSettings.UpdateBasicLoginSettingsRequest });
                Settings.Add("DefaultAuthProvider", new UIData { Name = "DefaultAuthProvider", Options = basicLoginSettings.AuthProviders, OptionsText = "Name", OptionsValue = "Value", Value = basicLoginSettings.Settings.DefaultAuthProvider });
                Settings.Add("RedirectAfterLogin", new UIData { Name = "RedirectAfterLogin", Options = PageList, OptionsText = "TabName", OptionsValue = "TabID", Value = basicLoginSettings.Settings.RedirectAfterLoginTabId.ToString() });
                Settings.Add("RedirectAfterLogout", new UIData { Name = "RedirectAfterLogout", Options = PageList, OptionsText = "TabName", OptionsValue = "TabID", Value = basicLoginSettings.Settings.RedirectAfterLogoutTabId.ToString() });
            }
            Settings.Add("UserRegistration", new UIData { Name = "UserRegistration", Options = Results });
            Settings.Add("RedirectAfterRegistration", new UIData { Name = "RedirectAfterRegistration", Options = PageList, OptionsText = "TabName", OptionsValue = "TabID", Value = Results.Settings.RedirectAfterRegistrationTabId.ToString() });
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
                dynamic UpdateBasicLoginSettingsRequest = JsonConvert.DeserializeObject<dynamic>(settingData.UpdateBasicLoginSettingsRequest.ToString());
                dynamic UpdateRegistrationSettingsRequest = JsonConvert.DeserializeObject<dynamic>(settingData.UpdateRegistrationSettingsRequest.ToString());
                Entities.UpdateSslSettingsRequest UpdateSslSettingsRequest = JsonConvert.DeserializeObject<Entities.UpdateSslSettingsRequest>(settingData.UpdateSslSettingsRequest.ToString());
                actionResult = UpdateBasicLoginSettings(UpdateBasicLoginSettingsRequest);
                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

                actionResult = UpdateRegistrationSettings(UpdateRegistrationSettingsRequest);
                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

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

                actionResult = UpdateSslSettings(UpdateSslSettingsRequest);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                actionResult.AddError("", ex.Message);
            }
            return actionResult;
        }
        private ActionResult UpdateBasicLoginSettings(dynamic request)
        {
            ActionResult actionResult = new ActionResult();
            if (!ModelState.IsValid)
            {
                foreach (System.Web.Http.ModelBinding.ModelState val in ModelState.Values)
                {
                    foreach (System.Web.Http.ModelBinding.ModelError err in val.Errors)
                    {
                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), err.ErrorMessage);
                    }
                }
            }
            try
            {
                if (actionResult.IsSuccess)
                {
                    int PortalId = PortalSettings.PortalId;
                    string cultureCode = string.IsNullOrEmpty(request.CultureCode.ToString()) ? LocaleController.Instance.GetCurrentLocale(PortalId).Code : request.CultureCode.ToString();
                    PortalInfo portalInfo = PortalController.Instance.GetPortal(PortalId);
                    portalInfo.AdministratorId = Convert.ToInt32(request.PrimaryAdministratorId);
                    PortalController.Instance.UpdatePortalInfo(portalInfo);
                    PortalController.UpdatePortalSetting(PortalId, "DefaultAuthProvider", request.DefaultAuthProvider.ToString());
                    PortalController.UpdatePortalSetting(PortalId, "Redirect_AfterLogin", request.RedirectAfterLoginTabId.ToString(), false, cultureCode, false);
                    PortalController.UpdatePortalSetting(PortalId, "Redirect_AfterLogout", request.RedirectAfterLogoutTabId.ToString(), false, cultureCode, false);
                    PortalController.UpdatePortalSetting(PortalId, "Security_RequireValidProfileAtLogin", request.RequireValidProfileAtLogin.ToString(), false);
                    PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaLogin", request.CaptchaLogin.ToString(), false);
                    PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaRetrivePassword", request.CaptchaRetrivePassword.ToString(), false);
                    PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaChangePassword", request.CaptchaChangePassword.ToString(), false);
                    PortalController.UpdatePortalSetting(PortalId, "HideLoginControl", request.HideLoginControl.ToString(), false);
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
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

        private ActionResult UpdateRegistrationSettings(dynamic request)
        {
            ActionResult ActionResult = new ActionResult();
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            try
            {
                bool.TryParse(request.UseEmailAsUsername.ToString(), out bool userEmailAsUsername);
                if (userEmailAsUsername && UserController.GetDuplicateEmailCount() > 0)
                {
                    ActionResult.AddError("userEmailAsUsername", DotNetNuke.Services.Localization.Localization.GetString(Constants.ContainsDuplicateAddresses, Constants.LocalResourcesFile));
                    return ActionResult;
                }

                string setting = request.RegistrationFields;
                PortalInfo portalInfo = PortalController.Instance.GetPortal(portalSettings.PortalId);
                portalInfo.UserRegistration = Convert.ToInt32(request.UserRegistration);
                PortalController.Instance.UpdatePortalInfo(portalInfo);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_RegistrationFields", setting);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_RegistrationFormType", request.RegistrationFormType.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_UseEmailAsUserName", request.UseEmailAsUsername.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "EnableRegisterNotification", request.EnableRegisterNotification.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_UseAuthProviders", request.UseAuthenticationProviders.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_ExcludeTerms", request.ExcludedTerms.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_UseProfanityFilter", request.UseProfanityFilter.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_RequireUniqueDisplayName", request.RequireUniqueDisplayName.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Security_DisplayNameFormat", request.DisplayNameFormat.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Security_UserNameValidation", request.UserNameValidation.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Security_EmailValidation", request.EmailAddressValidation.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_RandomPassword", request.UseRandomPassword.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Registration_RequireConfirmPassword", request.RequirePasswordConfirmation.ToString(), true);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Security_RequireValidProfile", request.RequireValidProfile.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Security_CaptchaRegister", request.UseCaptchaRegister.ToString(), false);
                PortalController.UpdatePortalSetting(portalSettings.PortalId, "Redirect_AfterRegistration", request.RedirectAfterRegistrationTabId.ToString(), false, LocaleController.Instance.GetCurrentLocale(portalSettings.PortalId).Code, false);
                                
                Core.Managers.SettingManager.UpdateConfig("system.web/membership", "requiresUniqueEmail", userEmailAsUsername.ToString());
            }
            catch (Exception ex)
            {
                ActionResult.AddError("UpdateRegistrationSettings", ex.Message);
            }
            return ActionResult;
        }
        private ActionResult UpdateSslSettings(Entities.UpdateSslSettingsRequest request)
        {
            ActionResult ActionResult = new ActionResult();
            try
            {
                PortalSettings PortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
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
                    foreach (KeyValuePair<int, TabInfo> t in TabController.Instance.GetTabsByPortal(PortalSettings.Current.PortalId))
                    {
                        t.Value.IsSecure = request.SSLEnabled;
                        TabController.Instance.UpdateTab(t.Value);
                    }

                    if (PortalSettings.Current != null && PortalSettings.Current.ActiveTab != null && !string.IsNullOrEmpty(PortalSettings.Current.ActiveTab.FullUrl))
                    {
                        ActionResult.RedirectURL = PortalSettings.Current.ActiveTab.FullUrl;
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
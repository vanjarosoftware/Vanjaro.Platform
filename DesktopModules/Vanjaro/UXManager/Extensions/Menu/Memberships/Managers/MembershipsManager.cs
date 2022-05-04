using Dnn.PersonaBar.Security.Components;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Memberships
{
    public static partial class Managers
    {
        public class MembershipsManager
        {                       
            #region Get Login Settings
            public static dynamic GetBasicLoginSettings(PortalSettings portalSettings)
            {
                dynamic response = new ExpandoObject();
                try
                {
                    int PortalId = portalSettings.PortalId;
                    string cultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
                    dynamic settings = new ExpandoObject();
                    settings.DefaultAuthProvider = PortalController.GetPortalSetting("DefaultAuthProvider", PortalId, "DNN");
                    settings.PrimaryAdministratorId = PortalSettings.Current.AdministratorId;
                    settings.RedirectAfterLoginTabId = ValidateTabId(portalSettings.Registration.RedirectAfterLogin);
                    settings.RedirectAfterLoginTabName = GetTabName(portalSettings.Registration.RedirectAfterLogin);
                    settings.RedirectAfterLoginTabPath = GetTabPath(portalSettings.Registration.RedirectAfterLogin);
                    settings.RedirectAfterLogoutTabId = ValidateTabId(portalSettings.Registration.RedirectAfterLogout);
                    settings.RedirectAfterLogoutTabName = GetTabName(portalSettings.Registration.RedirectAfterLogout);
                    settings.RedirectAfterLogoutTabPath = GetTabPath(portalSettings.Registration.RedirectAfterLogout);
                    settings.RequireValidProfileAtLogin = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfileAtLogin", PortalId, true);
                    settings.CaptchaLogin = PortalController.GetPortalSettingAsBoolean("Security_CaptchaLogin", PortalId, false);
                    settings.CaptchaRetrivePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRetrivePassword", PortalId, false);
                    settings.CaptchaChangePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaChangePassword", PortalId, false);
                    settings.HideLoginControl = PortalSettings.Current.HideLoginControl;
                    settings.CultureCode = cultureCode;
                    SecurityController securityController = new SecurityController();
                    var authProviders = securityController.GetAuthenticationProviders().Select(v => new
                    {
                        Name = v,
                        Value = v
                    }).ToList();

                    var adminUsers = securityController.GetAdminUsers(PortalId).Select(v => new
                    {
                        v.UserID,
                        v.FullName
                    }).ToList();

                    response = new
                    {
                        Success = true,
                        Settings = settings,
                        AuthProviders = authProviders,
                        Administrators = adminUsers,
                        UpdateBasicLoginSettingsRequest = settings
                    };
                }
                catch (Exception exc)
                {
                    ExceptionManager.LogException(exc);
                }

                return response;
            }
            private static int ValidateTabId(int tabId)
            {
                TabInfo tab = TabController.Instance.GetTab(tabId, PortalSettings.Current.PortalId);
                return tab?.TabID ?? Null.NullInteger;
            }
            private static string GetTabName(int tabId)
            {
                if (tabId == Null.NullInteger)
                {
                    return "";
                }
                else
                {
                    TabInfo tab = TabController.Instance.GetTab(tabId, PortalSettings.Current.PortalId);
                    return tab != null ? tab.TabName : "";
                }
            }
            private static string GetTabPath(int tabId)
            {
                if (tabId == Null.NullInteger)
                {
                    return "";
                }
                else
                {
                    TabInfo tab = TabController.Instance.GetTab(tabId, PortalSettings.Current.PortalId);
                    return tab != null ? tab.TabPath : "";
                }
            }
            #endregion
            #region Get Registration Settings
            public static dynamic GetRegistrationSettings(PortalSettings portalSettings)
            {
                List<KeyValuePair<string, int>> userRegistrationOptions = Managers.MembershipsManager.GetUserRegistrationOptions();
                List<KeyValuePair<string, int>> registrationFormTypeOptions = Managers.MembershipsManager.GetRegistrationFormOptions();
                var Results = new
                {
                    Settings = new
                    {
                        UserRegistration = portalSettings.UserRegistration.ToString(),
                        EnableRegisterNotification = PortalController.GetPortalSettingAsBoolean("EnableRegisterNotification", portalSettings.PortalId, true),
                        UseAuthenticationProviders = PortalController.GetPortalSettingAsBoolean("Registration_UseAuthProviders", portalSettings.PortalId, false),
                        ExcludedTerms = PortalController.GetPortalSetting("Registration_ExcludeTerms", portalSettings.PortalId, string.Empty),
                        UseProfanityFilter = PortalController.GetPortalSettingAsBoolean("Registration_UseProfanityFilter", portalSettings.PortalId, false),
                        portalSettings.Registration.RegistrationFormType,
                        portalSettings.Registration.RegistrationFields,
                        UseEmailAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", portalSettings.PortalId, false),
                        RequireUniqueDisplayName = PortalController.GetPortalSettingAsBoolean("Registration_RequireUniqueDisplayName", portalSettings.PortalId, false),
                        DisplayNameFormat = PortalController.GetPortalSetting("Security_DisplayNameFormat", portalSettings.PortalId, string.Empty),
                        UserNameValidation = PortalController.GetPortalSetting("Security_UserNameValidation", portalSettings.PortalId, Globals.glbUserNameRegEx),
                        EmailAddressValidation = PortalController.GetPortalSetting("Security_EmailValidation", portalSettings.PortalId, Globals.glbEmailRegEx),
                        UseRandomPassword = PortalController.GetPortalSettingAsBoolean("Registration_RandomPassword", portalSettings.PortalId, false),
                        RequirePasswordConfirmation = PortalController.GetPortalSettingAsBoolean("Registration_RequireConfirmPassword", portalSettings.PortalId, true),
                        RequireValidProfile = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfile", portalSettings.PortalId, false),
                        UseCaptchaRegister = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRegister", portalSettings.PortalId, false),
                        RedirectAfterRegistrationTabId = ValidateTabId(portalSettings.Registration.RedirectAfterRegistration, portalSettings.PortalId),
                        RedirectAfterRegistrationTabName = GetTabName(portalSettings.Registration.RedirectAfterRegistration, portalSettings.PortalId),
                        RedirectAfterRegistrationTabPath = GetTabPath(portalSettings.Registration.RedirectAfterRegistration, portalSettings.PortalId),
                        RequiresUniqueEmail = MembershipProviderConfig.RequiresUniqueEmail.ToString(CultureInfo.InvariantCulture),
                        PasswordFormat = MembershipProviderConfig.PasswordFormat.ToString(),
                        PasswordRetrievalEnabled = MembershipProviderConfig.PasswordRetrievalEnabled.ToString(CultureInfo.InvariantCulture),
                        PasswordResetEnabled = MembershipProviderConfig.PasswordResetEnabled.ToString(CultureInfo.InvariantCulture),
                        MinPasswordLength = MembershipProviderConfig.MinPasswordLength.ToString(CultureInfo.InvariantCulture),
                        MinNonAlphanumericCharacters = MembershipProviderConfig.MinNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture),
                        RequiresQuestionAndAnswer = MembershipProviderConfig.RequiresQuestionAndAnswer.ToString(CultureInfo.InvariantCulture),
                        MembershipProviderConfig.PasswordStrengthRegularExpression,
                        MaxInvalidPasswordAttempts = MembershipProviderConfig.MaxInvalidPasswordAttempts.ToString(CultureInfo.InvariantCulture),
                        PasswordAttemptWindow = MembershipProviderConfig.PasswordAttemptWindow.ToString(CultureInfo.InvariantCulture)
                    },
                    UserRegistrationOptions = userRegistrationOptions,
                    RegistrationFormTypeOptions = registrationFormTypeOptions
                };

                return Results;
            }
            internal static List<KeyValuePair<string, int>> GetUserRegistrationOptions()
            {
                List<KeyValuePair<string, int>> userRegistrationOptions = new List<KeyValuePair<string, int>>
                {
                    new KeyValuePair<string, int>(Localization.GetString("None", Components.Constants.LocalResourcesFile), 0),
                    new KeyValuePair<string, int>(Localization.GetString("Private", Components.Constants.LocalResourcesFile), 1),
                    new KeyValuePair<string, int>(Localization.GetString("Public", Components.Constants.LocalResourcesFile), 2)
                };
                return userRegistrationOptions;
            }
            internal static List<KeyValuePair<string, int>> GetRegistrationFormOptions()
            {
                List<KeyValuePair<string, int>> registrationFormTypeOptions = new List<KeyValuePair<string, int>>
                {
                    new KeyValuePair<string, int>(Localization.GetString("Standard", Constants.LocalResourcesFile), 0),
                    new KeyValuePair<string, int>(Localization.GetString("Custom", Constants.LocalResourcesFile), 1)
                };
                return registrationFormTypeOptions;
            }
            #region Private Method
            private static int ValidateTabId(int tabId, int PortalId)
            {
                TabInfo tab = TabController.Instance.GetTab(tabId, PortalId);
                return tab?.TabID ?? Null.NullInteger;
            }
            private static string GetTabName(int tabId, int PortalId)
            {
                if (tabId == Null.NullInteger)
                {
                    return "";
                }
                else
                {
                    TabInfo tab = TabController.Instance.GetTab(tabId, PortalId);
                    return tab != null ? tab.TabName : "";
                }
            }
            private static string GetTabPath(int tabId, int PortalId)
            {
                if (tabId == Null.NullInteger)
                {
                    return "";
                }
                else
                {
                    TabInfo tab = TabController.Instance.GetTab(tabId, PortalId);
                    return tab != null ? tab.TabPath : "";
                }
            }
            #endregion
            #endregion            

            internal static string ValidateFileExtension(string Value)
            {
                #region Validate webp extension 
                if (!string.IsNullOrEmpty(Value) && !System.Text.RegularExpressions.Regex.IsMatch(Value, string.Format(@"\b{0}\b", System.Text.RegularExpressions.Regex.Escape("webp"))))
                {
                    Value += ",webp";
                    string[] extn = Value.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    Value = string.Join(",", extn);
                }
                return Value;
                #endregion
            }
        }
    }
}
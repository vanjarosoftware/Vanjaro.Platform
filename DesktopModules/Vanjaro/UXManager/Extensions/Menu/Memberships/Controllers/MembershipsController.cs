using Dnn.PersonaBar.Security.Components;
using DotNetNuke.Entities.Portals;
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
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Memberships.Factories;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Memberships.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class MembershipsController : UIEngineController
    {
        public static List<IUIData> GetData(PortalSettings portalSettings, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            
            dynamic Results = Managers.MembershipsManager.GetRegistrationSettings(portalSettings);
            dynamic basicLoginSettings = Managers.MembershipsManager.GetBasicLoginSettings(portalSettings);
            if (basicLoginSettings != null)
            {
                Settings.Add("UpdateBasicLoginSettingsRequest", new UIData { Name = "UpdateBasicLoginSettingsRequest", Options = basicLoginSettings.UpdateBasicLoginSettingsRequest });
            }
            Settings.Add("UserRegistration", new UIData { Name = "UserRegistration", Options = Results });            
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
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
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
            }
            catch (Exception ex)
            {
                ActionResult.AddError("UpdateRegistrationSettings", ex.Message);
            }
            return ActionResult;
        }               

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
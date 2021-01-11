using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Block.Login.Entities;
using Vanjaro.UXManager.Library.Common;
using static DotNetNuke.Modules.Admin.Users.DataConsent;

namespace Vanjaro.UXManager.Extensions.Block.Login.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class LoginController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Global", new UIData { Name = "Global", Value = "false" } },
                { "ShowResetPassword", new UIData { Name = "ShowResetPassword", Value = "true" } },
                { "ShowRememberPassword", new UIData { Name = "ShowRememberPassword", Value = "true" } },
                { "ResetPassword", new UIData { Name = "ResetPassword", Value = "false" } },
                { "ShowLabel", new UIData { Name = "ShowLabel", Value = "false" } },
                { "ButtonAlign", new UIData { Name = "ButtonAlign", Value = "justify" } },
                { "ShowRegister", new UIData { Name = "ShowRegister", Value = "true" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "login") } },
                { "IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        [DnnPageEditor]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public void Update(Dictionary<string, string> Attributes)
        {
            Core.Managers.BlockManager.UpdateDesignElement(PortalSettings, Attributes);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public ActionResult UserLogin(UserLogin userLogin)
        {
            ActionResult actionResult = new ActionResult();

            if (Core.Services.Captcha.Validate("login"))
            {

                dynamic eventArgs = Core.Managers.LoginManager.UserLogin(userLogin);

                if (userLogin.HasAgreedToTerms)
                {
                    eventArgs.User.HasAgreedToTerms = true;
                    UserController.UserAgreedToTerms(eventArgs.User);
                    OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Consented));
                }

                actionResult = Managers.LoginManager.UserAuthenticated(eventArgs);
            }
            else
            {
                actionResult.AddError("recaptcha_error", DotNetNuke.Services.Localization.Localization.GetString("ReCaptcha_Error", Core.Components.Constants.LocalResourcesFile));
            }
            return actionResult;
        }

        /// <summary>
        /// cmdSendPassword_Click runs when the Password Reminder button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public ActionResult OnSendPasswordClick(string Email)
        {
            ActionResult actionResult = new ActionResult();

            if (Core.Services.Captcha.Validate("resetpassword"))
            {
                actionResult = Managers.LoginManager.OnSendPasswordClick(Email);
            }
            else
            {
                actionResult.AddError("recaptcha_error", DotNetNuke.Services.Localization.Localization.GetString("ReCaptcha_Error", Core.Components.Constants.LocalResourcesFile));
            }
            return actionResult;
        }


        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public ActionResult DataConsentDeleteMe(UserLogin userLogin)
        {
            ActionResult actionResult = new ActionResult();
            dynamic getuser = Core.Managers.LoginManager.UserLogin(userLogin);
            bool success = false;
            int UserId = getuser.User.UserID;
            UserInfo User = UserController.GetUserById(PortalSettings.Current.PortalId, UserId);
            switch (PortalSettings.DataConsentUserDeleteAction)
            {
                case PortalSettings.UserDeleteAction.Manual:
                    User.Membership.Approved = false;
                    UserController.UpdateUser(PortalSettings.PortalId, User);
                    UserController.UserRequestsRemoval(User, true);
                    success = true;
                    break;
                case PortalSettings.UserDeleteAction.DelayedHardDelete:
                    UserInfo user = User;
                    success = UserController.DeleteUser(ref user, true, false);
                    UserController.UserRequestsRemoval(User, true);
                    break;
                case PortalSettings.UserDeleteAction.HardDelete:
                    success = UserController.RemoveUser(User);
                    break;
            }
            if (success)
            {
                PortalSecurity.Instance.SignOut();
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.RemovedAccount));
            }
            else
            {
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.FailedToRemoveAccount));
            }
            actionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL();
            return actionResult;
        }


        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public ActionResult DataConsentCancel()
        {
            ActionResult actionResult = new ActionResult();
            OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Cancelled));
            actionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL();
            return actionResult;
        }
                
        public void OnDataConsentComplete(DataConsentEventArgs e)
        {
            DataConsentCompleted?.Invoke(this, e);
        }

        #region Delegate and event
        public delegate void DataConsentEventHandler(object sender, DataConsentEventArgs e);
        public event DataConsentEventHandler DataConsentCompleted;
        #endregion

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}

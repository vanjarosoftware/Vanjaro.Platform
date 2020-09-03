using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.UserRequest;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.Core.Providers.Authentication.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
    public class ResetPasswordController : UIEngineController
    {
        private const int RedirectTimeout = 3000;
        private string _ipAddress;
        public const string LocalResourceFile = "~/Admin/Security/App_LocalResources/PasswordReset.ascx.resx";
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public dynamic Index(Entities.ResetPassword PasswordReset)
        {
            dynamic actionResult = new ExpandoObject();
            try
            {
                PasswordReset.ResetToken = HttpContext.Current.Request.UrlReferrer.AbsoluteUri.Split('/')[HttpContext.Current.Request.UrlReferrer.AbsoluteUri.Split('/').Length - 1];
                UserInfo UserInfo = UserController.GetUserByPasswordResetToken(PortalSettings.Current.PortalId, PasswordReset.ResetToken);
                _ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
                string username = PasswordReset.Username;
                if (PasswordReset.Password != PasswordReset.ConfirmPassword)
                {
                    string failed = Localization.GetString("PasswordMismatch");
                    LogFailure(failed, UserInfo);
                    actionResult.IsSuccess = false;
                    actionResult.Message = failed;
                    return actionResult;
                }
                string newPassword = PasswordReset.Password.Trim();
                if (UserController.ValidatePassword(newPassword) == false)
                {
                    string failed = Localization.GetString("PasswordResetFailed");
                    LogFailure(failed, UserInfo);
                    actionResult.IsSuccess = false;
                    actionResult.Message = failed;
                    return actionResult;
                }

                MembershipPasswordSettings settings = new MembershipPasswordSettings(PortalSettings.Current.PortalId);
                if (settings.EnableBannedList)
                {
                    MembershipPasswordController m = new MembershipPasswordController();
                    if (m.FoundBannedPassword(newPassword) || username == newPassword)
                    {
                        string failed = Localization.GetString("PasswordResetFailed");
                        LogFailure(failed, UserInfo);
                        actionResult.IsSuccess = false;
                        actionResult.Message = failed;
                        return actionResult;
                    }
                }
                if (PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalSettings.Current.PortalId, false))
                {
                    UserInfo testUser = UserController.GetUserByEmail(PortalSettings.Current.PortalId, username); // one additonal call to db to see if an account with that email actually exists
                    if (testUser != null)
                    {
                        username = testUser.Username; //we need the username of the account in order to change the password in the next step
                    }
                }
                if (UserController.ChangePasswordByToken(PortalSettings.PortalId, username, newPassword, null, PasswordReset.ResetToken, out string errorMessage) == false)
                {
                    string failed = errorMessage;
                    LogFailure(failed, UserInfo);
                    actionResult.IsSuccess = false;
                    actionResult.Message = failed;
                    return actionResult;
                }
                else
                {
                    //check user has a valid profile
                    UserInfo user = UserController.GetUserByName(PortalSettings.PortalId, username);
                    UserValidStatus validStatus = UserController.ValidateUser(user, PortalSettings.PortalId, false);
                    if (validStatus == UserValidStatus.UPDATEPROFILE)
                    {
                        LogSuccess(UserInfo);
                    }
                    else
                    {
                        //Log user in to site
                        LogSuccess(UserInfo);
                        UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                        UserController.UserLogin(PortalSettings.PortalId, username, PasswordReset.Password, "", "", "", ref loginStatus, false);
                        actionResult.Message = Localization.GetString("ChangeSuccessful", LocalResourceFile);
                    }
                }
                actionResult.IsSuccess = true;
                actionResult.IsRedirect = true;
                actionResult.RedirectURL = Managers.ResetPasswordManager.RedirectAfterLogin();
            }
            catch (Exception ex)
            {
                actionResult.IsSuccess = false;
                actionResult.Message = ex.Message;
            }
            return actionResult;
        }
        private void LogSuccess(UserInfo UserInfo)
        {
            LogResult(string.Empty, UserInfo);
        }
        private void LogFailure(string reason, UserInfo UserInfo)
        {
            LogResult(reason, UserInfo);
        }
        private void LogResult(string message, UserInfo UserInfo)
        {
            LogInfo log = new LogInfo
            {
                LogPortalID = PortalSettings.PortalId,
                LogPortalName = PortalSettings.PortalName,
                LogUserID = UserInfo.UserID
            };

            if (string.IsNullOrEmpty(message))
            {
                log.LogTypeKey = "PASSWORD_SENT_SUCCESS";
            }
            else
            {
                log.LogTypeKey = "PASSWORD_SENT_FAILURE";
                log.LogProperties.Add(new LogDetailInfo("Cause", message));
            }
            log.AddProperty("IP", _ipAddress);
            LogController.Instance.AddLog(log);
        }
        public override string AccessRoles()
        {
            List<string> AccessRoles = new List<string>();

            if (UserInfo.UserID > 0)
            {
                AccessRoles.Add("user");
            }
            else
            {
                AccessRoles.Add("anonymous");
            }

            if (UserInfo.IsSuperUser)
            {
                AccessRoles.Add("host");
            }

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
            }
            return string.Join(",", AccessRoles.Distinct());
        }
    }
}
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Messaging.Data;
using DotNetNuke.Services.UserRequest;
using System;
using System.Collections;
using System.Web;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library.Common;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.Login
{
    public static partial class Managers
    {
        public class LoginManager
        {
            private static string AuthenticationType;

            private static UserLoginStatus LoginStatus;
            private static bool RememberMe;


            public const string LocalResourceFile = "~/DesktopModules/Admin/Authentication/App_LocalResources/Login.ascx.resx";
            public static string GetRedirectUrl(bool checkSettings = true)
            {
                string redirectUrl = "";
                int redirectAfterLogin = PortalSettings.Current.Registration.RedirectAfterLogin;
                if (checkSettings && redirectAfterLogin > 0) //redirect to after login page
                {
                    redirectUrl = ServiceProvider.NavigationManager.NavigateURL(redirectAfterLogin);
                }
                else
                {
                    if (HttpContext.Current.Request.QueryString["returnurl"] != null || HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)["returnurl"] != null)
                    {
                        //return to the url passed to login
                        if (HttpContext.Current.Request.QueryString["returnurl"] != null)
                        {
                            redirectUrl = HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString["returnurl"]);
                        }
                        else
                        {
                            redirectUrl = HttpUtility.UrlDecode(HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)["returnurl"]);
                        }

                        //clean the return url to avoid possible XSS attack.
                        redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);

                        if (redirectUrl.Contains("?returnurl"))
                        {
                            string baseURL = redirectUrl.Substring(0,
                                redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal));
                            string returnURL =
                                redirectUrl.Substring(redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                            redirectUrl = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                        }
                    }
                    if (string.IsNullOrEmpty(redirectUrl))
                    {
                        //redirect to current page 
                        redirectUrl = ServiceProvider.NavigationManager.NavigateURL();
                    }
                }

                return redirectUrl;
            }

            public static string LoginURL(string returnUrl, bool overrideSetting)
            {
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                string loginUrl;
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = string.Format("returnurl={0}", returnUrl);
                }
                string popUpParameter = "";
                if (HttpUtility.UrlDecode(returnUrl).IndexOf("popUp=true", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    popUpParameter = "popUp=true";
                }

                if (portalSettings.LoginTabId != -1 && !overrideSetting)
                {
                    //if (ValidateLoginTabID(portalSettings.LoginTabId))
                    if (portalSettings.LoginTabId != -1)
                    {
                        loginUrl = string.IsNullOrEmpty(returnUrl)
                                            ? ServiceProvider.NavigationManager.NavigateURL(portalSettings.LoginTabId, "", popUpParameter)
                                            : ServiceProvider.NavigationManager.NavigateURL(portalSettings.LoginTabId, "", returnUrl, popUpParameter);
                    }
                    else
                    {
                        string strMessage = string.Format("error={0}", Localization.GetString("NoLoginControl", Localization.GlobalResourceFile));
                        //No account module so use portal tab
                        loginUrl = string.IsNullOrEmpty(returnUrl)
                                     ? ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", strMessage, popUpParameter)
                                     : ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnUrl, strMessage, popUpParameter);
                    }
                }
                else
                {
                    //portal tab
                    loginUrl = string.IsNullOrEmpty(returnUrl)
                                    ? ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", popUpParameter)
                                    : ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnUrl, popUpParameter);
                }
                return loginUrl;
            }

            public static ActionResult OnSendPasswordClick(string Email)
            {
                UserInfo _user = null;
                int _userCount = Null.NullInteger;

                //pretty much alwasy display the same message to avoid hinting on the existance of a user name
                ActionResult actionResult = new ActionResult
                {
                    Message = Localization.GetString("PasswordSent", Components.Constants.LocalResourcesFile)
                };
                bool canSend = true;

                if (string.IsNullOrEmpty(Email))
                {
                    //No email address either (cannot retrieve password)
                    canSend = false;
                    actionResult.AddError("EnterUsernameEmail", Localization.GetString("EnterUsernameEmail", Components.Constants.LocalResourcesFile));
                }

                if (actionResult.IsSuccess)
                {
                    if (string.IsNullOrEmpty(Host.SMTPServer))
                    {
                        //SMTP Server is not configured
                        canSend = false;
                        actionResult.AddError("OptionUnavailable", Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile));
                        string logMessage = Localization.GetString("SMTPNotConfigured", Components.Constants.LocalResourcesFile);
                        actionResult.AddError("SMTPNotConfigured", logMessage);
                        LogResult(logMessage, Email);
                    }
                    if (actionResult.IsSuccess && canSend)
                    {
                        _user = GetUser(Email);
                        if (_user != null)
                        {
                            if (_user.IsDeleted)
                            {
                                canSend = false;
                            }
                            else
                            {
                                if (_user.Membership.Approved == false)
                                {
                                    Mail.SendMail(_user, MessageType.PasswordReminderUserIsNotApproved, PortalSettings.Current);
                                    canSend = false;
                                }
                                if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                                {
                                    UserController.ResetPasswordToken(_user);
                                }

                                if (canSend)
                                {
                                    if (Mail.SendMail(_user, MessageType.PasswordReminder, PortalSettings.Current) != string.Empty)
                                    {
                                        canSend = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_userCount > 1)
                            {
                                actionResult.AddError("MultipleUsers", Localization.GetString("MultipleUsers", Components.Constants.LocalResourcesFile));
                            }

                            canSend = false;
                        }
                        if (actionResult.IsSuccess && canSend)
                        {
                            if (canSend)
                            {
                                LogResult(string.Empty, Email);
                            }
                            else
                            {
                                LogFailure(Localization.GetString("MultipleUsers", Components.Constants.LocalResourcesFile), Email);
                            }


                            // don't hide panel when e-mail only in use and error occured. We must provide negative feedback to the user, in case he doesn't rember what e-mail address he has used
                            if (!canSend && _user == null && MembershipProviderConfig.RequiresUniqueEmail && PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalSettings.Current.PortalId, false))
                            {
                                actionResult.AddError("EmailNotFound", Localization.GetString("EmailNotFound", Components.Constants.LocalResourcesFile));
                            }
                        }

                    }
                }
                return actionResult;
            }
            public static ActionResult UserAuthenticated(UserAuthenticatedEventArgs e)
            {
                ActionResult actionResult = new ActionResult();
                LoginStatus = e.LoginStatus;

                //Check the Login Status
                switch (LoginStatus)
                {
                    case UserLoginStatus.LOGIN_USERNOTAPPROVED:
                        switch (e.Message)
                        {
                            case "UnverifiedUser":
                                if (e.User != null)
                                {
                                    //First update the profile (if any properties have been passed)
                                    AuthenticationType = e.AuthenticationType;
                                    //ProfileProperties = e.Profile;
                                    RememberMe = e.RememberMe;
                                    //UpdateProfile(e.User, true);
                                    actionResult = ValidateUser(e.User, false);
                                }
                                break;
                            case "EnterCode":
                                actionResult.AddError(e.Message.ToString(), Localization.GetString(e.Message, LocalResourceFile));
                                break;
                            case "InvalidCode":
                            case "UserNotAuthorized":
                                actionResult.AddError(e.Message.ToString(), Localization.GetString(e.Message, LocalResourceFile));
                                break;
                            default:
                                actionResult.AddError(e.Message.ToString(), Localization.GetString(e.Message, LocalResourceFile));
                                break;
                        }
                        break;
                    case UserLoginStatus.LOGIN_USERLOCKEDOUT:
                        if (Host.AutoAccountUnlockDuration > 0)
                        {
                            actionResult.AddError("UserLockedOut", string.Format(Localization.GetString("UserLockedOut", LocalResourceFile), Host.AutoAccountUnlockDuration));
                        }
                        else
                        {
                            actionResult.AddError("UserLockedOut_ContactAdmin", Localization.GetString("UserLockedOut_ContactAdmin", LocalResourceFile));
                        }
                        //notify administrator about account lockout ( possible hack attempt )
                        ArrayList Custom = new ArrayList { e.UserToken };

                        Message message = new Message
                        {
                            FromUserID = PortalSettings.Current.AdministratorId,
                            ToUserID = PortalSettings.Current.AdministratorId,
                            Subject = Localization.GetSystemMessage(PortalSettings.Current, "EMAIL_USER_LOCKOUT_SUBJECT", Localization.GlobalResourceFile, Custom),
                            Body = Localization.GetSystemMessage(PortalSettings.Current, "EMAIL_USER_LOCKOUT_BODY", Localization.GlobalResourceFile, Custom),
                            Status = MessageStatusType.Unread
                        };
                        //_messagingController.SaveMessage(_message);

                        Mail.SendEmail(PortalSettings.Current.Email, PortalSettings.Current.Email, message.Subject, message.Body);
                        break;
                    case UserLoginStatus.LOGIN_FAILURE:
                        //A Login Failure can mean one of two things:
                        //  1 - User was authenticated by the Authentication System but is not "affiliated" with a DNN Account
                        //  2 - User was not authenticated
                        if (string.IsNullOrEmpty(e.Message))
                        {
                            actionResult.AddError("LoginFailed", Localization.GetString("LoginFailed", LocalResourceFile));
                        }
                        else
                        {
                            actionResult.AddError(e.Message.ToString(), Localization.GetString(e.Message, LocalResourceFile));
                        }
                        break;
                    default:
                        if (e.User != null)
                        {
                            //First update the profile (if any properties have been passed)
                            AuthenticationType = e.AuthenticationType;
                            //ProfileProperties = e.Profile;
                            RememberMe = e.RememberMe;
                            //UpdateProfile(e.User, true);
                            actionResult = ValidateUser(e.User, (e.AuthenticationType != "DNN"));
                        }
                        break;
                }
                return actionResult;
            }

            private static void LogFailure(string reason, string email)
            {
                LogResult(reason, email);
            }
            private static void LogResult(string message, string email)
            {
                PortalSecurity portalSecurity = PortalSecurity.Instance;
                LogInfo log = new LogInfo
                {
                    LogPortalID = PortalSettings.Current.PortalId,
                    LogPortalName = PortalSettings.Current.PortalName,
                    //LogUserID = UserId,
                    LogUserName = portalSecurity.InputFilter(email, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup)
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
                log.AddProperty("IP", UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request)));
                LogController.Instance.AddLog(log);
            }
            private static UserInfo GetUser(string Email)
            {
                ArrayList arrUsers;
                UserInfo _user = null;
                int _userCount = Null.NullInteger;
                if (!string.IsNullOrEmpty(Email))
                {
                    arrUsers = UserController.GetUsersByEmail(PortalSettings.Current.PortalId, Email, 0, int.MaxValue, ref _userCount);
                    if (arrUsers != null && arrUsers.Count == 1)
                    {
                        _user = (UserInfo)arrUsers[0];
                    }
                }
                else
                {
                    _user = UserController.GetUserByName(PortalSettings.Current.PortalId, Email);
                }
                return _user;
            }
            //private static void UpdateProfile(UserInfo objUser, bool update)
            //{
            //    bool bUpdateUser = false;
            //    if (ProfileProperties.Count > 0)
            //    {
            //        foreach (string key in ProfileProperties)
            //        {
            //            switch (key)
            //            {
            //                case "FirstName":
            //                    if (objUser.FirstName != ProfileProperties[key])
            //                    {
            //                        objUser.FirstName = ProfileProperties[key];
            //                        bUpdateUser = true;
            //                    }
            //                    break;
            //                case "LastName":
            //                    if (objUser.LastName != ProfileProperties[key])
            //                    {
            //                        objUser.LastName = ProfileProperties[key];
            //                        bUpdateUser = true;
            //                    }
            //                    break;
            //                case "Email":
            //                    if (objUser.Email != ProfileProperties[key])
            //                    {
            //                        objUser.Email = ProfileProperties[key];
            //                        bUpdateUser = true;
            //                    }
            //                    break;
            //                case "DisplayName":
            //                    if (objUser.DisplayName != ProfileProperties[key])
            //                    {
            //                        objUser.DisplayName = ProfileProperties[key];
            //                        bUpdateUser = true;
            //                    }
            //                    break;
            //                default:
            //                    objUser.Profile.SetProfileProperty(key, ProfileProperties[key]);
            //                    break;
            //            }
            //        }
            //        if (update)
            //        {
            //            if (bUpdateUser)
            //            {
            //                UserController.UpdateUser(PortalSettings.Current.PortalId, objUser);
            //            }
            //            ProfileController.UpdateUserProfile(objUser);
            //        }
            //    }
            //}


            /// -----------------------------------------------------------------------------
            /// <summary>
            /// ValidateUser runs when the user has been authorized by the data store.  It validates for
            /// things such as an expiring password, valid profile, or missing DNN User Association
            /// </summary>
            /// <param name="objUser">The logged in User</param>
            /// <param name="ignoreExpiring">Ignore the situation where the password is expiring (but not yet expired)</param>
            /// -----------------------------------------------------------------------------
            private static ActionResult ValidateUser(UserInfo objUser, bool ignoreExpiring)
            {
                ActionResult actionResult = new ActionResult();
                UserValidStatus validStatus = UserValidStatus.VALID;
                string strMessage = Null.NullString;
                DateTime expiryDate = Null.NullDate;

                validStatus = UserController.ValidateUser(objUser, PortalSettings.Current.PortalId, ignoreExpiring);

                if (PasswordConfig.PasswordExpiry > 0)
                {
                    expiryDate = objUser.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry);
                }

                //Check if the User has valid Password/Profile
                switch (validStatus)
                {
                    case UserValidStatus.VALID:
                        //check if the user is an admin/host and validate their IP
                        if (Host.EnableIPChecking)
                        {
                            bool isAdminUser = objUser.IsSuperUser || objUser.IsInRole(PortalSettings.Current.AdministratorRoleName);
                            if (isAdminUser)
                            {
                                if (IPFilterController.Instance.IsIPBanned(HttpContext.Current.Request.UserHostAddress))
                                {
                                    PortalSecurity.Instance.SignOut();
                                    actionResult.AddError("IPAddressBanned", Localization.GetString("IPAddressBanned", LocalResourceFile));
                                    break;
                                }
                            }
                        }

                        //Set the Page Culture(Language) based on the Users Preferred Locale
                        if ((objUser.Profile != null) && (objUser.Profile.PreferredLocale != null) && LocaleEnabled(objUser.Profile.PreferredLocale))
                        {
                            Localization.SetLanguage(objUser.Profile.PreferredLocale);
                        }
                        else
                        {
                            Localization.SetLanguage(PortalSettings.Current.DefaultLanguage);
                        }

                        //Set the Authentication Type used
                        AuthenticationController.SetAuthenticationType(AuthenticationType);

                        //Complete Login
                        IUserRequestIPAddressController userRequestIpAddressController = UserRequestIPAddressController.Instance;
                        string ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
                        UserController.UserLogin(PortalSettings.Current.PortalId, objUser, PortalSettings.Current.PortalName, ipAddress, RememberMe);

                        //check whether user request comes with IPv6 and log it to make sure admin is aware of that
                        if (string.IsNullOrWhiteSpace(ipAddress))
                        {
                            string ipAddressV6 = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request), IPAddressFamily.IPv6);

                            if (!string.IsNullOrWhiteSpace(ipAddressV6))
                            {
                                AddEventLog(objUser.UserID, objUser.Username, PortalSettings.Current.PortalId, "IPv6", ipAddressV6);
                            }
                        }

                        //redirect browser
                        //var redirectUrl = RedirectURL;

                        //Clear the cookie
                        HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", "")
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                        });

                        actionResult.RedirectURL = GetRedirectUrl();
                        break;
                    case UserValidStatus.PASSWORDEXPIRED:
                        //strMessage = string.Format(Localization.GetString("PasswordExpired", LocalResourceFile), expiryDate.ToLongDateString());
                        //AddLocalizedModuleMessage(strMessage, ModuleMessage.ModuleMessageType.YellowWarning, true);
                        actionResult.AddError("PASSWORDEXPIRED", string.Format(Localization.GetString("PasswordExpired", LocalResourceFile), expiryDate.ToLongDateString()));
                        break;
                    case UserValidStatus.PASSWORDEXPIRING:
                        //strMessage = string.Format(Localization.GetString("PasswordExpiring", LocalResourceFile), expiryDate.ToLongDateString());
                        //AddLocalizedModuleMessage(strMessage, ModuleMessage.ModuleMessageType.YellowWarning, true);       
                        actionResult.AddError("PASSWORDEXPIRING", string.Format(Localization.GetString("PasswordExpiring", LocalResourceFile), expiryDate.ToLongDateString()));

                        break;
                    case UserValidStatus.UPDATEPASSWORD:
                        string portalAlias = Globals.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias);
                        if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                        {
                            UserController.ResetPasswordToken(objUser);
                            objUser = UserController.GetUserById(objUser.PortalID, objUser.UserID);
                        }
                        string redirTo = string.Format("{0}/default.aspx?ctl=PasswordReset&resetToken={1}&forced=true", portalAlias, objUser.PasswordResetToken);
                        //Response.Redirect(redirTo);
                        break;
                    case UserValidStatus.UPDATEPROFILE:
                        //Save UserID in ViewState so that can update profile later

                        //When the user need update its profile to complete login, we need clear the login status because if the logrin is from
                        //3rd party login provider, it may call UserController.UserLogin because they doesn't check this situation.
                        actionResult.Data = new { UserExtensionURL = ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3#/updateprofile?uid=" + objUser.UserID) };
                        PortalSecurity.Instance.SignOut();
                        //Admin has forced profile update
                        actionResult.AddError("ProfileUpdate", Localization.GetString("ProfileUpdate", LocalResourceFile));
                        break;
                    case UserValidStatus.MUSTAGREETOTERMS:
                        if (PortalSettings.Current.DataConsentConsentRedirect == -1)
                        {
                            //AddModuleMessage("MustConsent", ModuleMessage.ModuleMessageType.YellowWarning, true);
                            actionResult.AddError("MUSTAGREETOTERMS", string.Format(Localization.GetString("MustConsent", LocalResourceFile), expiryDate.ToLongDateString()));
                        }
                        else
                        {
                            // Use the reset password token to identify the user during the redirect
                            UserController.ResetPasswordToken(objUser);
                            objUser = UserController.GetUserById(objUser.PortalID, objUser.UserID);
                            actionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL(PortalSettings.Current.DataConsentConsentRedirect, "", string.Format("token={0}", objUser.PasswordResetToken));
                        }
                        break;
                }
                return actionResult;
            }
            private static bool LocaleEnabled(string locale)
            {
                return LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId).ContainsKey(locale);
            }
            private static void AddEventLog(int userId, string username, int portalId, string propertyName, string propertyValue)
            {
                LogInfo log = new LogInfo
                {
                    LogUserID = userId,
                    LogUserName = username,
                    LogPortalID = portalId,
                    LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString()
                };
                log.AddProperty(propertyName, propertyValue);
                LogController.Instance.AddLog(log);
            }
        }
    }
}
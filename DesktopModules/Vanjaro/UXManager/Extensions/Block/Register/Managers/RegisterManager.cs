using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.UI.Skins.Controls;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Threading;
using System.Web;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Block.Register.Entities;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.Register
{
    public static partial class Managers
    {

        public class RegisterManager
        {
            private static UserInfo User;
            public static UserCreateStatus CreateStatus { get; set; }
            public static ActionResult CreateUser(RegisterDetails RegisterDetails)
            {
                ActionResult actionResult = new ActionResult();
                User.Membership.Approved = PortalSettings.Current.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration;
                CreateStatus = UserController.CreateUser(ref User);
                DataCache.ClearPortalUserCountCache(PortalSettings.Current.PortalId);

                try
                {
                    if (CreateStatus == UserCreateStatus.Success)
                    {
                        ////Assocate alternate Login with User and proceed with Login
                        //if (!String.IsNullOrEmpty(AuthenticationType))
                        //{
                        //    AuthenticationController.AddUserAuthentication(User.UserID, AuthenticationType, UserToken);
                        //}

                        ActionResult result = CompleteUserCreation(CreateStatus, User, true, !(HttpContext.Current.Request.IsAuthenticated && PortalSecurity.IsInRole(PortalSettings.Current.AdministratorRoleName)) && !(HttpContext.Current.Request.IsAuthenticated && (User.UserID == (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserInfo.UserID)));
                        if (result.IsSuccess)
                        {
                            if (string.IsNullOrEmpty(result.Message))
                            {
                                IDictionary<string, object> dynObjects = new ExpandoObject() as IDictionary<string, object>;
                                dynObjects.Add("RedirectURL", GetRedirectUrl());
                                actionResult.Data = dynObjects;
                            }
                            actionResult.Message = result.Message;
                        }
                        else
                        {
                            string errorMessage = string.Empty;
                            foreach (KeyValuePair<string, Exception> error in result.Errors)
                            {
                                errorMessage = error.Value.ToString();
                            }
                            actionResult.AddError("RegisterManager.CompleteUserCreation", errorMessage);
                        }
                    }
                    else
                    {
                        actionResult.AddError("RegisterManager.CreateUser", UserController.GetUserCreateStatus(CreateStatus));
                    }
                }
                catch (Exception exc) //Module failed to load
                {
                    Exceptions.ProcessModuleLoadException(null, exc);
                }
                return actionResult;
            }
            public static bool Validate()
            {
                bool _IsValid = false;
                CreateStatus = UserCreateStatus.AddUser;
                PortalSecurity portalSecurity = PortalSecurity.Instance;

                string name = User.Username ?? User.Email;
                string cleanUsername = PortalSecurity.Instance.InputFilter(name,
                                                      PortalSecurity.FilterFlag.NoScripting |
                                                      PortalSecurity.FilterFlag.NoAngleBrackets |
                                                      PortalSecurity.FilterFlag.NoMarkup);
                if (!cleanUsername.Equals(name))
                {
                    CreateStatus = UserCreateStatus.InvalidUserName;
                }

                bool valid = UserController.Instance.IsValidUserName(name);

                if (!valid)
                {
                    CreateStatus = UserCreateStatus.InvalidUserName;
                }

                if (PortalSettings.Current.Registration.RegistrationFormType == 0)
                {
                    //Update UserName
                    if (PortalSettings.Current.Registration.UseEmailAsUserName)
                    {
                        User.Username = User.Email;
                        User.Email = User.Email;
                        if (string.IsNullOrEmpty(User.DisplayName))
                        {
                            User.DisplayName = User.Email.Substring(0, User.Email.IndexOf("@", StringComparison.Ordinal));
                        }
                    }

                    //Check Password is valid
                    if (!PortalSettings.Current.Registration.RandomPassword)
                    {
                        //Check Password is Valid
                        if (CreateStatus == UserCreateStatus.AddUser && !UserController.ValidatePassword(User.Membership.Password))
                        {
                            CreateStatus = UserCreateStatus.InvalidPassword;
                        }

                        //if (PortalSettings.Current.Registration.RequirePasswordConfirm && String.IsNullOrEmpty(AuthenticationType))
                        if (PortalSettings.Current.Registration.RequirePasswordConfirm)
                        {
                            if (User.Membership.Password != User.Membership.PasswordConfirm)
                            {
                                CreateStatus = UserCreateStatus.PasswordMismatch;
                            }
                        }
                    }
                    else
                    {
                        //Generate a random password for the user
                        User.Membership.Password = UserController.GeneratePassword();
                        User.Membership.PasswordConfirm = User.Membership.Password;
                    }

                }
                else
                {
                    //Set Username to Email
                    if (string.IsNullOrEmpty(User.Username))
                    {
                        User.Username = User.Email;
                    }

                    //Set DisplayName
                    if (string.IsNullOrEmpty(User.DisplayName))
                    {
                        User.DisplayName = string.IsNullOrEmpty(User.FirstName + " " + User.LastName)
                                               ? User.Email.Substring(0, User.Email.IndexOf("@", StringComparison.Ordinal))
                                               : User.FirstName + " " + User.LastName;
                    }

                    //Random Password
                    if (string.IsNullOrEmpty(User.Membership.Password))
                    {
                        //Generate a random password for the user
                        User.Membership.Password = UserController.GeneratePassword();
                    }

                    //Password Confirm
                    if (!string.IsNullOrEmpty(User.Membership.PasswordConfirm))
                    {
                        if (User.Membership.Password != User.Membership.PasswordConfirm)
                        {
                            CreateStatus = UserCreateStatus.PasswordMismatch;
                        }
                    }
                }

                //Validate banned password
                MembershipPasswordSettings settings = new MembershipPasswordSettings(User.PortalID);

                if (settings.EnableBannedList)
                {
                    MembershipPasswordController m = new MembershipPasswordController();
                    if (m.FoundBannedPassword(User.Membership.Password) || User.Username == User.Membership.Password)
                    {
                        CreateStatus = UserCreateStatus.BannedPasswordUsed;
                    }

                }
                //Validate Profanity
                if (PortalSettings.Current.Registration.UseProfanityFilter)
                {
                    if (!portalSecurity.ValidateInput(User.Username, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        CreateStatus = UserCreateStatus.InvalidUserName;
                    }
                    if (!string.IsNullOrEmpty(User.DisplayName))
                    {
                        if (!portalSecurity.ValidateInput(User.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                        {
                            CreateStatus = UserCreateStatus.InvalidDisplayName;
                        }
                    }
                }

                //Validate Unique User Name
                UserInfo user = UserController.GetUserByName(PortalSettings.Current.PortalId, User.Username);
                if (user != null)
                {
                    if (PortalSettings.Current.Registration.UseEmailAsUserName)
                    {
                        CreateStatus = UserCreateStatus.DuplicateEmail;
                    }
                    else
                    {
                        CreateStatus = UserCreateStatus.DuplicateUserName;
                        int i = 1;
                        string userName = null;
                        while (user != null)
                        {
                            userName = User.Username + "0" + i.ToString(CultureInfo.InvariantCulture);
                            user = UserController.GetUserByName(PortalSettings.Current.PortalId, userName);
                            i++;
                        }
                        User.Username = userName;
                    }
                }

                //Validate Unique Display Name
                if (CreateStatus == UserCreateStatus.AddUser && PortalSettings.Current.Registration.RequireUniqueDisplayName)
                {
                    user = UserController.Instance.GetUserByDisplayname(PortalSettings.Current.PortalId, User.DisplayName);
                    if (user != null)
                    {
                        CreateStatus = UserCreateStatus.DuplicateDisplayName;
                        int i = 1;
                        string displayName = null;
                        while (user != null)
                        {
                            displayName = User.DisplayName + " 0" + i.ToString(CultureInfo.InvariantCulture);
                            user = UserController.Instance.GetUserByDisplayname(PortalSettings.Current.PortalId, displayName);
                            i++;
                        }
                        User.DisplayName = displayName;
                    }
                }

                //Check Question/Answer
                if (CreateStatus == UserCreateStatus.AddUser && MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    if (string.IsNullOrEmpty(User.Membership.PasswordQuestion))
                    {
                        //Invalid Question
                        CreateStatus = UserCreateStatus.InvalidQuestion;
                    }
                    if (CreateStatus == UserCreateStatus.AddUser)
                    {
                        if (string.IsNullOrEmpty(User.Membership.PasswordAnswer))
                        {
                            //Invalid Question
                            CreateStatus = UserCreateStatus.InvalidAnswer;
                        }
                    }
                }

                if (CreateStatus == UserCreateStatus.AddUser)
                {
                    _IsValid = true;
                }
                return _IsValid;
            }
            #region Private Method
            //static string _UserToken;
            //protected static string UserToken
            //{
            //    get
            //    {
            //        return "";
            //    }
            //    set
            //    {
            //        _UserToken = value;
            //    }
            //}

            //static string _AuthenticationType;
            //protected static string AuthenticationType
            //{
            //    get
            //    {
            //        return "";
            //    }
            //    set
            //    {
            //        _AuthenticationType = value;
            //    }
            //}

            internal static void MapRegisterDetail(RegisterDetails Request)
            {
                UserModuleBase userModuleBase = new UserModuleBase();
                User = userModuleBase.UserInfo;
                User.Username = !string.IsNullOrEmpty(Request.UserName) ? Request.UserName : null;
                User.Email = !string.IsNullOrEmpty(Request.Email) ? Request.Email : null;
                User.DisplayName = !string.IsNullOrEmpty(Request.DisplayName) ? Request.DisplayName : null;
                User.Membership.Password = !string.IsNullOrEmpty(Request.Password) ? Request.Password : null;
                User.Membership.PasswordConfirm = !string.IsNullOrEmpty(Request.ConfirmPassword) ? Request.ConfirmPassword : null;
                User.PortalID = PortalSettings.Current.PortalId;
                User.Profile.PreferredLocale = Thread.CurrentThread.CurrentUICulture.ToString();
                //Update DisplayName to conform to Format
                if (!string.IsNullOrEmpty(PortalSettings.Current.Registration.DisplayNameFormat))
                {
                    User.UpdateDisplayName(PortalSettings.Current.Registration.DisplayNameFormat);
                }

            }

            protected static ActionResult CompleteUserCreation(UserCreateStatus createStatus, UserInfo newUser, bool notify, bool register)
            {

                ActionResult actionResult = new ActionResult();

                string strMessage = "";
                ModuleMessage.ModuleMessageType message = ModuleMessage.ModuleMessageType.RedError;
                if (register)
                {
                    //send notification to portal administrator of new user registration
                    //check the receive notification setting first, but if register type is Private, we will always send the notification email.
                    //because the user need administrators to do the approve action so that he can continue use the website.
                    if (PortalSettings.Current.EnableRegisterNotification || PortalSettings.Current.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, PortalController.Instance.GetCurrentSettings() as PortalSettings);
                        SendAdminNotification(newUser, PortalController.Instance.GetCurrentSettings() as PortalSettings);
                    }

                    UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;

                    //complete registration
                    switch (PortalSettings.Current.UserRegistration)
                    {
                        case (int)Globals.PortalRegistrationType.PrivateRegistration:
                            strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPrivate, PortalController.Instance.GetCurrentSettings() as PortalSettings);

                            //show a message that a portal administrator has to verify the user credentials
                            if (string.IsNullOrEmpty(strMessage))
                            {
                                strMessage += Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile);
                                message = ModuleMessage.ModuleMessageType.GreenSuccess;
                            }
                            break;
                        case (int)Globals.PortalRegistrationType.PublicRegistration:
                            Mail.SendMail(newUser, MessageType.UserRegistrationPublic, PortalController.Instance.GetCurrentSettings() as PortalSettings);
                            UserController.UserLogin(PortalSettings.Current.PortalId, newUser.Username, newUser.Membership.Password, "", PortalSettings.Current.PortalName, "", ref loginStatus, false);
                            break;
                        case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                            Mail.SendMail(newUser, MessageType.UserRegistrationVerified, PortalController.Instance.GetCurrentSettings() as PortalSettings);
                            UserController.UserLogin(PortalSettings.Current.PortalId, newUser.Username, newUser.Membership.Password, "", PortalSettings.Current.PortalName, "", ref loginStatus, false);
                            break;
                    }
                    //store preferredlocale in cookie
                    Localization.SetLanguage(newUser.Profile.PreferredLocale);
                    if (!(HttpContext.Current.Request.IsAuthenticated && PortalSecurity.IsInRole(PortalSettings.Current.AdministratorRoleName)) && !(HttpContext.Current.Request.IsAuthenticated && (newUser.UserID == (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserInfo.UserID)) && message == ModuleMessage.ModuleMessageType.RedError)
                    {

                        //HS Skin Messages 
                        //actionResult.AddError("SendMail_Error", string.Format(Localization.GetString("SendMail.Error", Localization.SharedResourceFile), newUser.Email));
                        ExceptionManager.LogException(new Exception("Skin Messages : " + string.Format(Localization.GetString("SendMail.Error", Localization.SharedResourceFile), newUser.Email)));
                        actionResult.Message = strMessage;
                    }
                    else
                    {
                        if (message == ModuleMessage.ModuleMessageType.RedError)
                        {
                            actionResult.AddError("CompleteUserCreation", strMessage);
                        }
                        else
                        {
                            actionResult.Message = strMessage;
                        }
                    }
                }
                else
                {
                    if (notify)
                    {
                        //Send Notification to User
                        if (PortalSettings.Current.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                        {
                            strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationVerified, PortalController.Instance.GetCurrentSettings() as PortalSettings);
                        }
                        else
                        {
                            strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPublic, PortalController.Instance.GetCurrentSettings() as PortalSettings);
                        }
                    }
                }

                actionResult.Message = strMessage;

                return actionResult;
            }

            private static string GetNotificationSubject(string locale, UserInfo newUser, PortalSettings portalSettings)
            {
                const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
                return Localization.GetSystemMessage(locale, portalSettings, text, newUser, Localization.GlobalResourceFile, null, "", portalSettings.AdministratorId);
            }

            private static string GetNotificationBody(string locale, UserInfo newUser, PortalSettings portalSettings)
            {
                const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
                return Localization.GetSystemMessage(locale, portalSettings, text, newUser, Localization.GlobalResourceFile, null, "", portalSettings.AdministratorId);
            }

            private static void SendAdminNotification(UserInfo newUser, PortalSettings portalSettings)
            {
                string notificationType = newUser.Membership.Approved ? "NewUserRegistration" : "NewUnauthorizedUserRegistration";
                string locale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId).Code;
                Notification notification = new Notification
                {
                    NotificationTypeID = NotificationsController.Instance.GetNotificationType(notificationType).NotificationTypeId,
                    IncludeDismissAction = newUser.Membership.Approved,
                    SenderUserID = portalSettings.AdministratorId,
                    Subject = GetNotificationSubject(locale, newUser, portalSettings),
                    Body = GetNotificationBody(locale, newUser, portalSettings),
                    Context = newUser.UserID.ToString(CultureInfo.InvariantCulture)
                };
                RoleInfo adminrole = RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId);
                List<RoleInfo> roles = new List<RoleInfo> { adminrole };
                NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, roles, new List<UserInfo>());
            }

            internal static string GetRedirectUrl(bool checkSetting = true)
            {
                string redirectUrl = "";
                int redirectAfterRegistration = PortalSettings.Current.Registration.RedirectAfterRegistration;
                if (checkSetting && redirectAfterRegistration > 0) //redirect to after registration page
                {
                    redirectUrl = ServiceProvider.NavigationManager.NavigateURL(redirectAfterRegistration);
                }
                else
                {
                    if (HttpContext.Current.Request.QueryString["returnurl"] != null)
                    {
                        //return to the url passed to register
                        redirectUrl = HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString["returnurl"]);

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

            #endregion

        }
    }
}
using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Social.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Vanjaro.UXManager.Extensions.Menu.Users
{
    public static partial class Managers
    {
        public class RegisteredManager
        {
            //public static ActionResult Register(RegisterationDetails registerationDetails)
            //{
            //    ActionResult actionResult = new ActionResult();
            //    var portalSettings = registerationDetails.PortalSettings;
            //    var username = registerationDetails.UserName;
            //    var email = registerationDetails.Email;

            //    Requires.NotNullOrEmpty("email", email);

            //    var disallowRegistration = !registerationDetails.IgnoreRegistrationMode && ((portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration) || (portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration));

            //    if (disallowRegistration)
            //        throw new Exception(Localization.GetString("RegistrationNotAllowed", Components.Constants.LocalResourcesFile));

            //    //initial creation of the new User object
            //    var newUser = new UserInfo
            //    {
            //        PortalID = portalSettings.PortalId,
            //        Email = email
            //    };

            //    var cleanUsername = PortalSecurity.Instance.InputFilter(username,
            //                                              PortalSecurity.FilterFlag.NoScripting |
            //                                              PortalSecurity.FilterFlag.NoAngleBrackets |
            //                                              PortalSecurity.FilterFlag.NoMarkup);

            //    if (!cleanUsername.Equals(username))
            //        throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));

            //    var valid = UserController.Instance.IsValidUserName(username);

            //    if (!valid)
            //        throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));

            //    //ensure this user doesn't exist
            //    if (!string.IsNullOrEmpty(username) && UserController.GetUserByName(portalSettings.PortalId, username) != null)
            //        throw new Exception(Localization.GetString("RegistrationUsernameAlreadyPresent", Components.Constants.LocalResourcesFile));

            //    //set username as email if not specified
            //    newUser.Username = string.IsNullOrEmpty(username) ? email : username;

            //    if (!string.IsNullOrEmpty(registerationDetails.Password) && !registerationDetails.RandomPassword)
            //        newUser.Membership.Password = registerationDetails.Password;
            //    else
            //        //Generate a random password for the user
            //        newUser.Membership.Password = UserController.GeneratePassword();

            //    newUser.Membership.PasswordConfirm = newUser.Membership.Password;

            //    //set other profile properties
            //    newUser.Profile.PreferredLocale = new Localization().CurrentUICulture;
            //    newUser.Profile.InitialiseProfile(portalSettings.PortalId);
            //    newUser.Profile.PreferredTimeZone = portalSettings.TimeZone;

            //    //derive display name from supplied firstname, lastname or from email
            //    if (!string.IsNullOrEmpty(registerationDetails.FirstName) && !string.IsNullOrEmpty(registerationDetails.LastName))
            //    {
            //        newUser.DisplayName = registerationDetails.FirstName + " " + registerationDetails.LastName;
            //        newUser.FirstName = registerationDetails.FirstName;
            //        newUser.LastName = registerationDetails.LastName;
            //    }
            //    else
            //        newUser.DisplayName = newUser.Email.Substring(0, newUser.Email.IndexOf("@", StringComparison.Ordinal));

            //    //read all the user account settings
            //    var settings = UserController.GetUserSettings(portalSettings.PortalId);

            //    //Verify Profanity filter
            //    if (GetBoolSetting(settings, "Registration_UseProfanityFilter"))
            //    {
            //        var portalSecurity = PortalSecurity.Instance;
            //        if (!portalSecurity.ValidateInput(newUser.Username, PortalSecurity.FilterFlag.NoProfanity) || !portalSecurity.ValidateInput(newUser.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
            //            throw new Exception(Localization.GetString("RegistrationProfanityNotAllowed", Components.Constants.LocalResourcesFile));
            //    }

            //    //Email Address Validation
            //    var emailValidator = GetStringSetting(settings, "Security_EmailValidation");
            //    if (!string.IsNullOrEmpty(emailValidator))
            //    {
            //        var regExp = RegexUtils.GetCachedRegex(emailValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //        var matches = regExp.Matches(newUser.Email);
            //        if (matches.Count == 0)
            //            throw new Exception(Localization.GetString("RegistrationInvalidEmailUsed", Components.Constants.LocalResourcesFile));
            //    }

            //    //Excluded Terms Verification
            //    var excludeRegex = GetExcludeTermsRegex(settings);
            //    if (!string.IsNullOrEmpty(excludeRegex))
            //    {
            //        var regExp = RegexUtils.GetCachedRegex(excludeRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //        var matches = regExp.Matches(newUser.Username);
            //        if (matches.Count > 0)
            //            throw new Exception(Localization.GetString("RegistrationExcludedTermsUsed", Components.Constants.LocalResourcesFile));
            //    }

            //    //User Name Validation
            //    var userNameValidator = GetStringSetting(settings, "Security_UserNameValidation");
            //    if (!string.IsNullOrEmpty(userNameValidator))
            //    {
            //        var regExp = RegexUtils.GetCachedRegex(userNameValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //        var matches = regExp.Matches(newUser.Username);
            //        if (matches.Count == 0)
            //            throw new Exception(Localization.GetString("RegistrationInvalidUserNameUsed", Components.Constants.LocalResourcesFile));
            //    }

            //    //ensure unique username
            //    var user = UserController.GetUserByName(portalSettings.PortalId, newUser.Username);
            //    if (user != null)
            //    {
            //        if (GetBoolSetting(settings, "Registration_UseEmailAsUserName"))
            //            throw new Exception(UserController.GetUserCreateStatus(UserCreateStatus.DuplicateEmail));

            //        var i = 1;
            //        string userName = null;
            //        while (user != null)
            //        {
            //            userName = newUser.Username + "0" + i.ToString(CultureInfo.InvariantCulture);
            //            user = UserController.GetUserByName(portalSettings.PortalId, userName);
            //            i++;
            //        }
            //        newUser.Username = userName;
            //    }

            //    //ensure unique display name
            //    if (GetBoolSetting(settings, "Registration_RequireUniqueDisplayName"))
            //    {
            //        user = UserController.Instance.GetUserByDisplayname(portalSettings.PortalId, newUser.DisplayName);
            //        if (user != null)
            //        {
            //            var i = 1;
            //            string displayName = null;
            //            while (user != null)
            //            {
            //                displayName = newUser.DisplayName + " 0" + i.ToString(CultureInfo.InvariantCulture);
            //                user = UserController.Instance.GetUserByDisplayname(portalSettings.PortalId, displayName);
            //                i++;
            //            }
            //            newUser.DisplayName = displayName;
            //        }
            //    }

            //    //Update display name format
            //    var displaynameFormat = GetStringSetting(settings, "Security_DisplayNameFormat");
            //    if (!string.IsNullOrEmpty(displaynameFormat))
            //        newUser.UpdateDisplayName(displaynameFormat);

            //    //membership is approved only for public registration
            //    newUser.Membership.Approved = (registerationDetails.IgnoreRegistrationMode || portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration) && registerationDetails.Authorize;
            //    newUser.Membership.PasswordQuestion = registerationDetails.Question;
            //    newUser.Membership.PasswordAnswer = registerationDetails.Answer;
            //    //final creation of user
            //    var createStatus = UserController.CreateUser(ref newUser, registerationDetails.Notify);

            //    //clear cache
            //    if (createStatus == UserCreateStatus.Success)
            //        CachingProvider.Instance().Remove(string.Format(DataCache.PortalUserCountCacheKey, portalSettings.PortalId));

            //    if (createStatus != UserCreateStatus.Success)
            //        throw new Exception(UserController.GetUserCreateStatus(createStatus));

            //    //            if (registerationDetails.IgnoreRegistrationMode)
            //    //            {
            //    //                Mail.SendMail(newUser, MessageType.UserRegistrationPublic, portalSettings);
            //    //                return UserBasicDto.FromUserInfo(newUser);
            //    //            }

            //    //send notification to portal administrator of new user registration
            //    //check the receive notification setting first, but if register type is Private, we will always send the notification email.
            //    //because the user need administrators to do the approve action so that he can continue use the website.
            //    if (!registerationDetails.IgnoreRegistrationMode && (portalSettings.EnableRegisterNotification || portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration))
            //    {
            //        Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, portalSettings);
            //        SendAdminNotification(newUser, portalSettings);
            //    }
            //    if (actionResult.IsSuccess)
            //        actionResult.Data = UserBasicDto.FromUserInfo(newUser);
            //    return actionResult;
            //}

            public static UserBasicDto Register(RegisterationDetails registerationDetails)
            {
                PortalSettings portalSettings = registerationDetails.PortalSettings;
                string username = registerationDetails.UserName;
                string email = registerationDetails.Email;

                Requires.NotNullOrEmpty("email", email);

                bool disallowRegistration = !registerationDetails.IgnoreRegistrationMode &&
                                   ((portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration) ||
                                   (portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration));

                if (disallowRegistration)
                {
                    throw new Exception(Localization.GetString("RegistrationNotAllowed", Components.Constants.LocalResourcesFile));
                }

                //initial creation of the new User object
                UserInfo newUser = new UserInfo
                {
                    PortalID = portalSettings.PortalId,
                    Email = email
                };

                string cleanUsername = PortalSecurity.Instance.InputFilter(username, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

                if (!cleanUsername.Equals(username))
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
                }

                bool valid = UserController.Instance.IsValidUserName(username);

                if (!valid)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
                }

                //ensure this user doesn't exist
                if (!string.IsNullOrEmpty(username) && UserController.GetUserByName(portalSettings.PortalId, username) != null)
                {
                    throw new Exception(Localization.GetString("RegistrationUsernameAlreadyPresent", Components.Constants.LocalResourcesFile));
                }

                //set username as email if not specified
                newUser.Username = string.IsNullOrEmpty(username) ? email : username;

                if (!string.IsNullOrEmpty(registerationDetails.Password) && !registerationDetails.RandomPassword)
                {
                    newUser.Membership.Password = registerationDetails.Password;
                }
                else
                {
                    //Generate a random password for the user
                    newUser.Membership.Password = UserController.GeneratePassword();
                }

                newUser.Membership.PasswordConfirm = newUser.Membership.Password;

                //set other profile properties
                newUser.Profile.PreferredLocale = new Localization().CurrentUICulture;
                newUser.Profile.InitialiseProfile(portalSettings.PortalId);
                newUser.Profile.PreferredTimeZone = portalSettings.TimeZone;

                //derive display name from supplied firstname, lastname or from email
                if (!string.IsNullOrEmpty(registerationDetails.FirstName))
                {
                    newUser.DisplayName = registerationDetails.FirstName.Trim();
                    if (registerationDetails.FirstName.Trim().Split(' ').Length > 0)
                    {
                        newUser.FirstName = registerationDetails.FirstName.Split(' ')[0].Trim();
                    }

                    if (registerationDetails.FirstName.Trim().Split(' ').Length > 1)
                    {
                        newUser.LastName = registerationDetails.FirstName.Split(' ')[1].Trim();
                    }
                }
                else
                {
                    newUser.DisplayName = newUser.Email.Substring(0, newUser.Email.IndexOf("@", StringComparison.Ordinal));
                }

                //read all the user account settings
                Hashtable settings = UserController.GetUserSettings(portalSettings.PortalId);

                //Verify Profanity filter
                if (GetBoolSetting(settings, "Registration_UseProfanityFilter"))
                {
                    PortalSecurity portalSecurity = PortalSecurity.Instance;
                    if (!portalSecurity.ValidateInput(newUser.Username, PortalSecurity.FilterFlag.NoProfanity) || !portalSecurity.ValidateInput(newUser.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        throw new Exception(Localization.GetString("RegistrationProfanityNotAllowed", Components.Constants.LocalResourcesFile));
                    }
                }

                //Email Address Validation
                string emailValidator = GetStringSetting(settings, "Security_EmailValidation");
                if (!string.IsNullOrEmpty(emailValidator))
                {
                    Regex regExp = RegexUtils.GetCachedRegex(emailValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    MatchCollection matches = regExp.Matches(newUser.Email);
                    if (matches.Count == 0)
                    {
                        throw new Exception(Localization.GetString("RegistrationInvalidEmailUsed", Components.Constants.LocalResourcesFile));
                    }
                }

                //Excluded Terms Verification
                string excludeRegex = GetExcludeTermsRegex(settings);
                if (!string.IsNullOrEmpty(excludeRegex))
                {
                    Regex regExp = RegexUtils.GetCachedRegex(excludeRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    MatchCollection matches = regExp.Matches(newUser.Username);
                    if (matches.Count > 0)
                    {
                        throw new Exception(Localization.GetString("RegistrationExcludedTermsUsed", Components.Constants.LocalResourcesFile));
                    }
                }

                //User Name Validation
                string userNameValidator = GetStringSetting(settings, "Security_UserNameValidation");
                if (!string.IsNullOrEmpty(userNameValidator))
                {
                    Regex regExp = RegexUtils.GetCachedRegex(userNameValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    MatchCollection matches = regExp.Matches(newUser.Username);
                    if (matches.Count == 0)
                    {
                        throw new Exception(Localization.GetString("RegistrationInvalidUserNameUsed", Components.Constants.LocalResourcesFile));
                    }
                }

                //ensure unique username
                UserInfo user = UserController.GetUserByName(portalSettings.PortalId, newUser.Username);
                if (user != null)
                {
                    if (GetBoolSetting(settings, "Registration_UseEmailAsUserName"))
                    {
                        throw new Exception(UserController.GetUserCreateStatus(UserCreateStatus.DuplicateEmail));
                    }

                    int i = 1;
                    string userName = null;
                    while (user != null)
                    {
                        userName = newUser.Username + "0" + i.ToString(CultureInfo.InvariantCulture);
                        user = UserController.GetUserByName(portalSettings.PortalId, userName);
                        i++;
                    }
                    newUser.Username = userName;
                }

                //ensure unique display name
                if (GetBoolSetting(settings, "Registration_RequireUniqueDisplayName"))
                {
                    user = UserController.Instance.GetUserByDisplayname(portalSettings.PortalId, newUser.DisplayName);
                    if (user != null)
                    {
                        int i = 1;
                        string displayName = null;
                        while (user != null)
                        {
                            displayName = newUser.DisplayName + " 0" + i.ToString(CultureInfo.InvariantCulture);
                            user = UserController.Instance.GetUserByDisplayname(portalSettings.PortalId, displayName);
                            i++;
                        }
                        newUser.DisplayName = displayName;
                    }
                }

                //Update display name format
                string displaynameFormat = GetStringSetting(settings, "Security_DisplayNameFormat");
                if (!string.IsNullOrEmpty(displaynameFormat))
                {
                    newUser.UpdateDisplayName(displaynameFormat);
                }

                //membership is approved only for public registration
                newUser.Membership.Approved = (registerationDetails.IgnoreRegistrationMode || portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration) && registerationDetails.Authorize;
                newUser.Membership.PasswordQuestion = registerationDetails.Question;
                newUser.Membership.PasswordAnswer = registerationDetails.Answer;
                //final creation of user
                UserCreateStatus createStatus = UserController.CreateUser(ref newUser, registerationDetails.Notify);

                //clear cache
                if (createStatus == UserCreateStatus.Success)
                {
                    CachingProvider.Instance().Remove(string.Format(DataCache.PortalUserCountCacheKey, portalSettings.PortalId));
                }

                if (createStatus != UserCreateStatus.Success)
                {
                    throw new Exception(UserController.GetUserCreateStatus(createStatus));
                }

                //            if (registerationDetails.IgnoreRegistrationMode)
                //            {
                //                Mail.SendMail(newUser, MessageType.UserRegistrationPublic, portalSettings);
                //                return UserBasicDto.FromUserInfo(newUser);
                //            }

                //send notification to portal administrator of new user registration
                //check the receive notification setting first, but if register type is Private, we will always send the notification email.
                //because the user need administrators to do the approve action so that he can continue use the website.
                if (!registerationDetails.IgnoreRegistrationMode && (portalSettings.EnableRegisterNotification || portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration))
                {
                    Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, portalSettings);
                    SendAdminNotification(newUser, portalSettings);
                }

                return UserBasicDto.FromUserInfo(newUser);
            }

            private static void SendAdminNotification(UserInfo newUser, PortalSettings portalSettings)
            {
                RoleController roleController = new RoleController();
                RoleInfo adminrole = roleController.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId);
                List<RoleInfo> roles = new List<RoleInfo> { adminrole };
                SendNewUserNotifications(newUser, portalSettings, roles);
            }

            public static void SendNewUserNotifications(UserInfo newUser, PortalSettings portalSettings, List<RoleInfo> roles)
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

                notification.Body = Utilities.FixDoublEntityEncoding(notification.Body);
                NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, roles, new List<UserInfo>());
            }

            private static string GetNotificationBody(string locale, UserInfo newUser, PortalSettings portalSettings)
            {
                const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
                return LocalizeNotificationText(text, locale, newUser, portalSettings);
            }

            private static string LocalizeNotificationText(string text, string locale, UserInfo user, PortalSettings portalSettings)
            {
                //This method could need a custom ArrayList in future notification types. Currently it is null
                return Localization.GetSystemMessage(locale, portalSettings, text, user, Localization.GlobalResourceFile, null, "", portalSettings.AdministratorId);
            }

            private static string GetNotificationSubject(string locale, UserInfo newUser, PortalSettings portalSettings)
            {
                const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
                return LocalizeNotificationText(text, locale, newUser, portalSettings);
            }

            private static bool GetBoolSetting(Hashtable settings, string settingKey)
            {
                return settings[settingKey] != null && Convert.ToBoolean(settings[settingKey]);
            }

            private static string GetStringSetting(Hashtable settings, string settingKey)
            {
                return settings[settingKey] == null ? string.Empty : settings[settingKey].ToString();
            }

            private static string GetExcludeTermsRegex(Hashtable settings)
            {
                string excludeTerms = GetStringSetting(settings, "Registration_ExcludeTerms");
                string regex = string.Empty;
                if (!string.IsNullOrEmpty(excludeTerms))
                {
                    regex = excludeTerms.Replace(" ", "").Replace(",", "|");
                }
                return regex;
            }


        }
    }
}
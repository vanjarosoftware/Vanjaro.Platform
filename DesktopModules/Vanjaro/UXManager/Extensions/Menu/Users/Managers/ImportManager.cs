using System;
using System.Web;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Exceptions;
using System.Data;
using DotNetNuke.Entities.Profile;
using System.Collections.Generic;
using System.Text;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using DotNetNuke.Services.Log.EventLog;
using System.Threading;
using DotNetNuke.Security;
using System.Web.Security;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Lists;
using System.Linq;
using DotNetNuke.Services.Localization;
using System.Globalization;
using DotNetNuke.Data;
using System.Collections;
using System.IO;
using DotNetNuke.Security.Roles;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Users
{
    public static partial class Managers
    {
        public class ImportManager
        {
            public static int MaximumLimit
            {
                get
                {
                    return 1000;
                }
            }
            public static ActionResult UserImport(int PortalID, ImportJob eJob, DataTable dT)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalSettings pS = new PortalSettings(PortalID);
                    RoleController rC = new RoleController();

                    #region User Accounts
                    DataTable ErrorLog = AddBasicColumns(dT);
                    DataRow[] rows = (from t in dT.AsEnumerable().Cast<DataRow>() select t).ToArray();
                    foreach (DataRow dR in rows)
                    {
                        if (dR.ItemArray.Length == 1 && String.IsNullOrEmpty(dR.ItemArray[0].ToString()))
                        {
                            continue;
                        }
                        UserInfo uInfo = null;

                        #region Append Only
                        if (eJob.ImportAction == ImportJob.ImportActions.Append)
                        {
                            string Error = null;
                            uInfo = GetUserInfo(PortalID, eJob, dR, ref Error);
                            if (Error == null)
                            {
                                UserCreateStatus cStatus = UserCreateStatus.InvalidUserName;
                                cStatus = CreateUser(ref uInfo);
                                if (cStatus != UserCreateStatus.Success)
                                {
                                    AddErrorLog(ErrorLog, dR, UserController.GetUserCreateStatus(cStatus));
                                }
                                else
                                {
                                    //Append Profile Fields
                                    try
                                    {
                                        uInfo = UserController.GetUserByName(PortalID, uInfo.Username);
                                        MapProfileFields(PortalID, uInfo, dR);
                                        UpdateProfile(uInfo);
                                    }
                                    catch (Exception ex)
                                    {
                                        AddErrorLog(ErrorLog, dR, "Error Creating User Profile");
                                        Exceptions.LogException(ex);
                                    }
                                    List<UserRoleInfo> Roles = GetUserRoles(PortalID, eJob, rC, dR, ref Error);

                                    foreach (UserRoleInfo uRole in Roles)
                                    {
                                        rC.AddUserRole(PortalID, uInfo.UserID, uRole.RoleID, uRole.EffectiveDate, uRole.ExpiryDate);
                                    }

                                    foreach (var objRole in rC.GetRoles(PortalID))
                                    {
                                        if (objRole.AutoAssignment == true)
                                        {
                                            rC.AddUserRole(PortalID, uInfo.UserID, objRole.RoleID, Null.NullDate, Null.NullDate);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AddErrorLog(ErrorLog, dR, Error);
                            }
                        }
                        #endregion
                    }

                    //Update Error Log
                    if (ErrorLog.Rows.Count > 0)
                    {
                        actionResult.Data = ErrorLog.Rows[0].Table;
                        actionResult.AddError("ErrorLog", "Error Creating User Profile");
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }

                return actionResult;
            }

            #region User Helpers

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// CreateUser persists a User to the Data Store
            /// </summary>
            /// <remarks>
            /// </remarks>
            /// <param name="user">The user to persist to the Data Store.</param>
            /// <returns>A UserCreateStatus enumeration indicating success or reason for failure.</returns>
            /// <history>
            /// DNN-4016 Allow OAuth authenticated user to join more than one portal
            /// DNN-4133 Prevent duplicate usernames for OAuth email address with same email prefix and different email domain.
            /// </history>
            /// -----------------------------------------------------------------------------
            private static UserCreateStatus CreateUser(ref UserInfo user)
            {
                UserCreateStatus createStatus = ValidateForProfanity(user);
                ImportManager import = new ImportManager();

                if (createStatus == UserCreateStatus.AddUser)
                {
                    ValidateForDuplicateDisplayName(user, ref createStatus);
                }

                if (createStatus == UserCreateStatus.AddUser)
                {
                    try
                    {
                        //check if username exists in database for any portal
                        UserInfo objVerifyUser = (new DotNetNuke.Security.Membership.AspNetMembershipProvider()).GetUserByUserName(Null.NullInteger, user.Username);
                        if (objVerifyUser != null)
                        {
                            //DNN-4016
                            //the username exists so we should now verify the password, DNN-4016 or check for oauth user authentication.
                            //if (isOAuthUser || System.Web.Security.Membership.ValidateUser(user.Username, user.Membership.Password))
                            if (System.Web.Security.Membership.ValidateUser(user.Username, user.Membership.Password))
                            {
                                //check if user exists for the portal specified
                                objVerifyUser = (new DotNetNuke.Security.Membership.AspNetMembershipProvider()).GetUserByUserName(user.PortalID, user.Username);
                                if (objVerifyUser != null)
                                {
                                    if (objVerifyUser.PortalID == user.PortalID && (!user.IsSuperUser || user.PortalID == Null.NullInteger))
                                    {
                                        createStatus = UserCreateStatus.UserAlreadyRegistered;
                                    }
                                    else
                                    {
                                        //SuperUser who is not part of portal
                                        createStatus = UserCreateStatus.AddUserToPortal;
                                    }
                                }
                                else
                                {
                                    createStatus = UserCreateStatus.AddUserToPortal;
                                }
                            }
                            else
                            {
                                //not the same person - prevent registration
                                createStatus = UserCreateStatus.UsernameAlreadyExists;
                            }
                        }
                        else
                        {
                            //the user does not exist
                            createStatus = UserCreateStatus.AddUser;
                        }

                        //If new user - add to aspnet membership
                        if (createStatus == UserCreateStatus.AddUser)
                        {
                            createStatus = CreateMemberhipUser(user);
                        }

                        //If asp user has been successfully created or we are adding a existing user
                        //to a new portal 
                        if (createStatus == UserCreateStatus.Success || createStatus == UserCreateStatus.AddUserToPortal)
                        {
                            //Create the DNN User Record
                            createStatus = import.CreateDNNUser(ref user);
                            if (createStatus == UserCreateStatus.Success)
                            {
                                //Persist the Profile to the Data Store
                                ProfileController.UpdateUserProfile(user);
                            }
                        }
                    }
                    catch (Exception exc) //an unexpected error occurred
                    {
                        Exceptions.LogException(exc);
                        createStatus = UserCreateStatus.UnexpectedError;
                    }
                }

                return createStatus;
            }
            private UserCreateStatus CreateDNNUser(ref UserInfo user)
            {
                var objSecurity = new PortalSecurity();
                string userName = objSecurity.InputFilter(user.Username,
                                                          PortalSecurity.FilterFlag.NoScripting |
                                                          PortalSecurity.FilterFlag.NoAngleBrackets |
                                                          PortalSecurity.FilterFlag.NoMarkup);
                string email = objSecurity.InputFilter(user.Email,
                                                       PortalSecurity.FilterFlag.NoScripting |
                                                       PortalSecurity.FilterFlag.NoAngleBrackets |
                                                       PortalSecurity.FilterFlag.NoMarkup);
                string lastName = objSecurity.InputFilter(user.LastName,
                                                          PortalSecurity.FilterFlag.NoScripting |
                                                          PortalSecurity.FilterFlag.NoAngleBrackets |
                                                          PortalSecurity.FilterFlag.NoMarkup);
                string firstName = objSecurity.InputFilter(user.FirstName,
                                                           PortalSecurity.FilterFlag.NoScripting |
                                                           PortalSecurity.FilterFlag.NoAngleBrackets |
                                                           PortalSecurity.FilterFlag.NoMarkup);
                var createStatus = UserCreateStatus.Success;
                string displayName = objSecurity.InputFilter(user.DisplayName,
                                                             PortalSecurity.FilterFlag.NoScripting |
                                                             PortalSecurity.FilterFlag.NoAngleBrackets |
                                                             PortalSecurity.FilterFlag.NoMarkup);
                if (displayName.Contains("<"))
                {
                    displayName = HttpUtility.HtmlEncode(displayName);
                }
                bool updatePassword = user.Membership.UpdatePassword;
                bool isApproved = user.Membership.Approved;
                try
                {
                    user.UserID =
                        Convert.ToInt32(_dataProvider.AddUser(user.PortalID,
                                                              userName,
                                                              firstName,
                                                              lastName,
                                                              user.AffiliateID,
                                                              user.IsSuperUser,
                                                              email,
                                                              displayName,
                                                              updatePassword,
                                                              isApproved,
                                                              UserController.Instance.GetCurrentUserInfo().UserID));
                }
                catch (Exception ex)
                {
                    //Clear User (duplicate User information)
                    Exceptions.LogException(ex);
                    user = null;
                    createStatus = UserCreateStatus.ProviderError;
                }
                return createStatus;
            }
            private static UserCreateStatus CreateMemberhipUser(UserInfo user)
            {
                var portalSecurity = new PortalSecurity();
                string userName = portalSecurity.InputFilter(user.Username,
                                                             PortalSecurity.FilterFlag.NoScripting |
                                                             PortalSecurity.FilterFlag.NoAngleBrackets |
                                                             PortalSecurity.FilterFlag.NoMarkup);
                string email = portalSecurity.InputFilter(user.Email,
                                                          PortalSecurity.FilterFlag.NoScripting |
                                                          PortalSecurity.FilterFlag.NoAngleBrackets |
                                                          PortalSecurity.FilterFlag.NoMarkup);
                MembershipCreateStatus status;
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    System.Web.Security.Membership.CreateUser(userName,
                                                              user.Membership.Password,
                                                              email,
                                                              user.Membership.PasswordQuestion,
                                                              user.Membership.PasswordAnswer,
                                                              true,
                                                              out status);
                }
                else
                {
                    System.Web.Security.Membership.CreateUser(userName,
                                                              user.Membership.Password,
                                                              email,
                                                              null,
                                                              null,
                                                              true,
                                                              out status);
                }
                var createStatus = UserCreateStatus.Success;
                switch (status)
                {
                    case MembershipCreateStatus.DuplicateEmail:
                        createStatus = UserCreateStatus.DuplicateEmail;
                        break;
                    case MembershipCreateStatus.DuplicateProviderUserKey:
                        createStatus = UserCreateStatus.DuplicateProviderUserKey;
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        createStatus = UserCreateStatus.DuplicateUserName;
                        break;
                    case MembershipCreateStatus.InvalidAnswer:
                        createStatus = UserCreateStatus.InvalidAnswer;
                        break;
                    case MembershipCreateStatus.InvalidEmail:
                        createStatus = UserCreateStatus.InvalidEmail;
                        break;
                    case MembershipCreateStatus.InvalidPassword:
                        createStatus = UserCreateStatus.InvalidPassword;
                        break;
                    case MembershipCreateStatus.InvalidProviderUserKey:
                        createStatus = UserCreateStatus.InvalidProviderUserKey;
                        break;
                    case MembershipCreateStatus.InvalidQuestion:
                        createStatus = UserCreateStatus.InvalidQuestion;
                        break;
                    case MembershipCreateStatus.InvalidUserName:
                        createStatus = UserCreateStatus.InvalidUserName;
                        break;
                    case MembershipCreateStatus.ProviderError:
                        createStatus = UserCreateStatus.ProviderError;
                        break;
                    case MembershipCreateStatus.UserRejected:
                        createStatus = UserCreateStatus.UserRejected;
                        break;
                }
                return createStatus;
            }
            private static void ValidateForDuplicateDisplayName(UserInfo user, ref UserCreateStatus createStatus)
            {
                Hashtable settings = GetUserSettings(user.PortalID, new Hashtable());
                bool requireUniqueDisplayName = Convert.ToBoolean(settings["Registration_RequireUniqueDisplayName"]);

                if (requireUniqueDisplayName)
                {
                    UserInfo duplicateUser = (new DotNetNuke.Security.Membership.AspNetMembershipProvider()).GetUserByDisplayName(Null.NullInteger, user.DisplayName);
                    if (duplicateUser != null)
                    {
                        createStatus = UserCreateStatus.DuplicateDisplayName;
                    }
                }
            }
            private static UserCreateStatus ValidateForProfanity(UserInfo user)
            {
                var portalSecurity = new PortalSecurity();
                var createStatus = UserCreateStatus.AddUser;

                Hashtable settings = GetUserSettings(user.PortalID, new Hashtable());
                bool useProfanityFilter = Convert.ToBoolean(settings["Registration_UseProfanityFilter"]);

                //Validate Profanity
                if (useProfanityFilter)
                {
                    if (!portalSecurity.ValidateInput(user.Username, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        createStatus = UserCreateStatus.InvalidUserName;
                    }
                    if (!String.IsNullOrEmpty(user.DisplayName))
                    {
                        if (!portalSecurity.ValidateInput(user.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                        {
                            createStatus = UserCreateStatus.InvalidDisplayName;
                        }
                    }
                }
                return createStatus;
            }
            static Hashtable GetUserSettings(int portalId, Hashtable settings)
            {
                portalId = PortalController.GetEffectivePortalId(portalId);

                if (settings["Column_FirstName"] == null)
                {
                    settings["Column_FirstName"] = false;
                }
                if (settings["Column_LastName"] == null)
                {
                    settings["Column_LastName"] = false;
                }
                if (settings["Column_DisplayName"] == null)
                {
                    settings["Column_DisplayName"] = true;
                }
                if (settings["Column_Address"] == null)
                {
                    settings["Column_Address"] = true;
                }
                if (settings["Column_Telephone"] == null)
                {
                    settings["Column_Telephone"] = true;
                }
                if (settings["Column_Email"] == null)
                {
                    settings["Column_Email"] = false;
                }
                if (settings["Column_CreatedDate"] == null)
                {
                    settings["Column_CreatedDate"] = true;
                }
                if (settings["Column_LastLogin"] == null)
                {
                    settings["Column_LastLogin"] = false;
                }
                if (settings["Column_Authorized"] == null)
                {
                    settings["Column_Authorized"] = true;
                }
                if (settings["Display_Mode"] == null)
                {
                    settings["Display_Mode"] = DotNetNuke.Entities.Modules.DisplayMode.All;
                }
                else
                {
                    settings["Display_Mode"] = (DotNetNuke.Entities.Modules.DisplayMode)Convert.ToInt32(settings["Display_Mode"]);
                }
                if (settings["Display_SuppressPager"] == null)
                {
                    settings["Display_SuppressPager"] = false;
                }
                if (settings["Records_PerPage"] == null)
                {
                    settings["Records_PerPage"] = 10;
                }
                if (settings["Profile_DefaultVisibility"] == null)
                {
                    settings["Profile_DefaultVisibility"] = UserVisibilityMode.AdminOnly;
                }
                else
                {
                    settings["Profile_DefaultVisibility"] = (UserVisibilityMode)Convert.ToInt32(settings["Profile_DefaultVisibility"]);
                }
                if (settings["Profile_DisplayVisibility"] == null)
                {
                    settings["Profile_DisplayVisibility"] = true;
                }
                if (settings["Profile_ManageServices"] == null)
                {
                    settings["Profile_ManageServices"] = true;
                }
                if (settings["Redirect_AfterLogin"] == null)
                {
                    settings["Redirect_AfterLogin"] = -1;
                }
                if (settings["Redirect_AfterRegistration"] == null)
                {
                    settings["Redirect_AfterRegistration"] = -1;
                }
                if (settings["Redirect_AfterLogout"] == null)
                {
                    settings["Redirect_AfterLogout"] = -1;
                }
                if (settings["Security_CaptchaLogin"] == null)
                {
                    settings["Security_CaptchaLogin"] = false;
                }
                if (settings["Security_CaptchaRegister"] == null)
                {
                    settings["Security_CaptchaRegister"] = false;
                }
                if (settings["Security_CaptchaRetrivePassword"] == null)
                {
                    settings["Security_CaptchaRetrivePassword"] = false;
                }
                if (settings["Security_EmailValidation"] == null)
                {
                    settings["Security_EmailValidation"] = DotNetNuke.Common.Globals.glbEmailRegEx;
                }
                if (settings["Security_UserNameValidation"] == null)
                {
                    settings["Security_UserNameValidation"] = DotNetNuke.Common.Globals.glbUserNameRegEx;
                }
                //Forces a valid profile on registration
                if (settings["Security_RequireValidProfile"] == null)
                {
                    settings["Security_RequireValidProfile"] = false;
                }
                //Forces a valid profile on login
                if (settings["Security_RequireValidProfileAtLogin"] == null)
                {
                    settings["Security_RequireValidProfileAtLogin"] = true;
                }
                if (settings["Security_UsersControl"] == null)
                {
                    var portal = new PortalController().GetPortal(portalId);

                    if (portal != null && portal.Users > 1000)
                    {
                        settings["Security_UsersControl"] = DotNetNuke.Entities.Modules.UsersControl.TextBox;
                    }
                    else
                    {
                        settings["Security_UsersControl"] = DotNetNuke.Entities.Modules.UsersControl.Combo;
                    }
                }
                else
                {
                    settings["Security_UsersControl"] = (DotNetNuke.Entities.Modules.UsersControl)Convert.ToInt32(settings["Security_UsersControl"]);
                }
                //Display name format
                if (settings["Security_DisplayNameFormat"] == null)
                {
                    settings["Security_DisplayNameFormat"] = "";
                }
                if (settings["Registration_RequireConfirmPassword"] == null)
                {
                    settings["Registration_RequireConfirmPassword"] = true;
                }
                if (settings["Registration_RandomPassword"] == null)
                {
                    settings["Registration_RandomPassword"] = false;
                }
                if (settings["Registration_UseEmailAsUserName"] == null)
                {
                    settings["Registration_UseEmailAsUserName"] = false;
                }
                if (settings["Registration_UseAuthProviders"] == null)
                {
                    settings["Registration_UseAuthProviders"] = false;
                }
                if (settings["Registration_UseProfanityFilter"] == null)
                {
                    settings["Registration_UseProfanityFilter"] = false;
                }
                if (settings["Registration_RegistrationFormType"] == null)
                {
                    settings["Registration_RegistrationFormType"] = 0;
                }
                if (settings["Registration_RegistrationFields"] == null)
                {
                    settings["Registration_RegistrationFields"] = String.Empty;
                }
                if (settings["Registration_ExcludeTerms"] == null)
                {
                    settings["Registration_ExcludeTerms"] = String.Empty;
                }
                if (settings["Registration_RequireUniqueDisplayName"] == null)
                {
                    settings["Registration_RequireUniqueDisplayName"] = false;
                }
                return settings;
            }

            private static void UpdateProfile(UserInfo uInfo)
            {
                UpdateUserProfile(uInfo);
            }

            #region Role Helpers           
            private static List<UserRoleInfo> GetUserRoles(int PortalID, ImportJob eJob, RoleController rC, DataRow dR, ref string Error)
            {
                List<UserRoleInfo> Roles = new List<UserRoleInfo>();
                if (dR.Table.Columns.Contains("SecurityRoles"))
                {
                    string[] RoleData = dR["SecurityRoles"].ToString().Split('|');

                    foreach (string RoleInfo in RoleData)
                    {
                        string[] rArray = RoleInfo.Split(';');

                        UserRoleInfo uRole = new UserRoleInfo();
                        RoleInfo rI = rC.GetRoleByName(PortalID, rArray[0]);

                        if (rI != null)
                        {
                            try
                            {
                                uRole.RoleID = rI.RoleID;
                                if (rI.RoleGroupID > 0)
                                    uRole.RoleGroupID = rI.RoleGroupID;
                                if (rArray.Length >= 2)
                                    uRole.EffectiveDate = DateTime.Parse(rArray[1], new System.Globalization.CultureInfo("en-US"));


                                if (rArray.Length >= 3)
                                    uRole.ExpiryDate = DateTime.Parse(rArray[2], new System.Globalization.CultureInfo("en-US"));
                                Roles.Add(uRole);
                            }
                            catch
                            {
                                Error = "Unable to parse effective or expiry date of security role: " + rArray[0];
                            }
                        }
                        else
                        {
                            Error = "Unable to find security role: " + rArray[0];
                        }
                    }
                }
                return Roles;
            }
            #endregion

            #region Update User
            #region Private Members
            private readonly DataProvider _dataProvider = DataProvider.Instance();
            #endregion

            private static void UpdateUserProfile(UserInfo user)
            {
                int portalId = PortalController.GetEffectivePortalId(user.PortalID);
                user.PortalID = portalId;
                ImportManager import = new ImportManager();
                //Update the User Profile
                if (user.Profile.IsDirty)
                {
                    import.UpdateMainUserProfile(user);
                }

                //Remove the UserInfo from the Cache, as it has been modified
                DataCache.ClearUserCache(user.PortalID, user.Username);
            }
            private void UpdateMainUserProfile(UserInfo user)
            {
                ProfilePropertyDefinitionCollection properties = user.Profile.ProfileProperties;

                //Ensure old and new TimeZone properties are in synch
                var newTimeZone = properties["PreferredTimeZone"];
                var oldTimeZone = properties["TimeZone"];
                if (oldTimeZone != null && newTimeZone != null)
                {   //preference given to new property, if new is changed then old should be updated as well.
                    if (newTimeZone.IsDirty && !string.IsNullOrEmpty(newTimeZone.PropertyValue))
                    {
                        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(newTimeZone.PropertyValue);
                        if (timeZoneInfo != null)
                            oldTimeZone.PropertyValue = timeZoneInfo.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture);
                    }
                    //however if old is changed, we need to update new as well
                    else if (oldTimeZone.IsDirty)
                    {
                        int oldOffset;
                        int.TryParse(oldTimeZone.PropertyValue, out oldOffset);
                        newTimeZone.PropertyValue = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(oldOffset).Id;
                    }
                }

                foreach (ProfilePropertyDefinition profProperty in properties)
                {
                    if ((profProperty.PropertyValue != null) && (profProperty.IsDirty))
                    {
                        var objSecurity = new PortalSecurity();
                        string propertyValue = InputFilter(user.PortalID, profProperty.PropertyValue, PortalSecurity.FilterFlag.NoScripting);
                        _dataProvider.UpdateProfileProperty(Null.NullInteger, user.UserID, profProperty.PropertyDefinitionId,
                                                    propertyValue, (int)profProperty.ProfileVisibility.VisibilityMode,
                                                    profProperty.ProfileVisibility.ExtendedVisibilityString(), DateTime.Now);
                        var objEventLog = new EventLogController();
                        objEventLog.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", "USERPROFILE_UPDATED");
                    }
                }
            }

            public string InputFilter(int PortalId, string userInput, DotNetNuke.Security.PortalSecurity.FilterFlag filterType)
            {
                if (userInput == null)
                {
                    return "";
                }
                var tempInput = userInput;
                if ((filterType & DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets) == DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets)
                {
                    var removeAngleBrackets = DotNetNuke.Common.Utilities.Config.GetSetting("RemoveAngleBrackets") != null && Boolean.Parse(DotNetNuke.Common.Utilities.Config.GetSetting("RemoveAngleBrackets"));
                    if (removeAngleBrackets)
                    {
                        tempInput = FormatAngleBrackets(tempInput);
                    }
                }
                if ((filterType & DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL) == DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL)
                {
                    tempInput = FormatRemoveSQL(tempInput);
                }
                else
                {
                    if ((filterType & DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup) == DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup && IncludesMarkup(tempInput))
                    {
                        tempInput = HttpUtility.HtmlEncode(tempInput);
                    }
                    if ((filterType & DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting) == DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)
                    {
                        tempInput = FormatDisableScripting(tempInput);
                    }
                    if ((filterType & DotNetNuke.Security.PortalSecurity.FilterFlag.MultiLine) == DotNetNuke.Security.PortalSecurity.FilterFlag.MultiLine)
                    {
                        tempInput = FormatMultiLine(tempInput);
                    }
                }
                if ((filterType & DotNetNuke.Security.PortalSecurity.FilterFlag.NoProfanity) == DotNetNuke.Security.PortalSecurity.FilterFlag.NoProfanity)
                {
                    tempInput = Replace(PortalId, tempInput, DotNetNuke.Security.PortalSecurity.ConfigType.ListController, "ProfanityFilter", DotNetNuke.Security.PortalSecurity.FilterScope.SystemAndPortalList);
                }
                return tempInput;
            }
            private string FormatMultiLine(string strInput)
            {
                string tempInput = strInput.Replace(Environment.NewLine, "<br />").Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");
                return (tempInput);
            }
            private bool IncludesMarkup(string strInput)
            {
                const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                const string pattern = "<[^<>]*>";
                return Regex.IsMatch(strInput, pattern, options);
            }
            private string FormatDisableScripting(string strInput)
            {
                string TempInput = strInput;
                TempInput = FilterStrings(TempInput);
                return TempInput;
            }
            private string FilterStrings(string strInput)
            {
                //setup up list of search terms as items may be used twice
                string TempInput = strInput;
                var listStrings = new List<string>
                                  {
                                      "<script[^>]*>.*?</script[^><]*>",
                                      "<script",
                                      "<input[^>]*>.*?</input[^><]*>",
                                      "<object[^>]*>.*?</object[^><]*>",
                                      "<embed[^>]*>.*?</embed[^><]*>",
                                      "<applet[^>]*>.*?</applet[^><]*>",
                                      "<form[^>]*>.*?</form[^><]*>",
                                      "<option[^>]*>.*?</option[^><]*>",
                                      "<select[^>]*>.*?</select[^><]*>",
                                      "<iframe[^>]*>.*?</iframe[^><]*>",
                                      "<iframe.*?<",
                                      "<iframe.*?",
                                      "<ilayer[^>]*>.*?</ilayer[^><]*>",
                                      "<form[^>]*>",
                                      "</form[^><]*>",
                                      "onerror",
                                      "onmouseover",
                                      "javascript:",
                                      "vbscript:",
                                      "unescape",
                                      "alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?"
                                  };

                const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                const string replacement = " ";

                //check if text contains encoded angle brackets, if it does it we decode it to check the plain text
                if (TempInput.Contains("&gt;") && TempInput.Contains("&lt;"))
                {
                    //text is encoded, so decode and try again
                    TempInput = HttpUtility.HtmlDecode(TempInput);
                    TempInput = listStrings.Aggregate(TempInput, (current, s) => Regex.Replace(current, s, replacement, options));

                    //Re-encode
                    TempInput = HttpUtility.HtmlEncode(TempInput);
                }
                else
                {
                    TempInput = listStrings.Aggregate(TempInput, (current, s) => Regex.Replace(current, s, replacement, options));
                }
                return TempInput;
            }
            private string Replace(int PortalId, string inputString, DotNetNuke.Security.PortalSecurity.ConfigType configType, string configSource, DotNetNuke.Security.PortalSecurity.FilterScope filterScope)
            {
                switch (configType)
                {
                    case DotNetNuke.Security.PortalSecurity.ConfigType.ListController:
                        const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                        const string listName = "ProfanityFilter";

                        var listController = new ListController();


                        IEnumerable<ListEntryInfo> listEntryHostInfos;
                        IEnumerable<ListEntryInfo> listEntryPortalInfos;

                        switch (filterScope)
                        {
                            case DotNetNuke.Security.PortalSecurity.FilterScope.SystemList:
                                listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
                                inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                                break;
                            case DotNetNuke.Security.PortalSecurity.FilterScope.SystemAndPortalList:
                                listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
                                listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + PortalId, "", PortalId);
                                inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                                inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                                break;
                            case DotNetNuke.Security.PortalSecurity.FilterScope.PortalList:
                                listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + PortalId, "", PortalId);
                                inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                                break;
                        }
                        break;
                    case DotNetNuke.Security.PortalSecurity.ConfigType.ExternalFile:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException("configType");
                }

                return inputString;
            }
            private string FormatAngleBrackets(string strInput)
            {
                string TempInput = strInput.Replace("<", "");
                TempInput = TempInput.Replace(">", "");
                return TempInput;
            }
            private string FormatRemoveSQL(string strSQL)
            {
                const string BadStatementExpression = ";|--|create|drop|select|insert|delete|update|union|sp_|xp_|exec|/\\*.*\\*/|declare|waitfor|%|&";
                return Regex.Replace(strSQL, BadStatementExpression, " ", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace("'", "''");
            }
            #endregion            

            private static UserInfo GetUserInfo(int PortalID, ImportJob job, DataRow dR, ref string Error)
            {
                return GetUserInfo(PortalID, null, job, dR, ref Error, false);
            }
            private static UserInfo GetUserInfo(int PortalID, UserInfo u, ImportJob job, DataRow dR, ref string Error, bool UpdateUserInfo)
            {
                PortalSettings pS = new PortalSettings(PortalID);

                if (u == null)
                    u = new UserInfo();

                #region Basic Properties
                if (dR.Table.Columns.Contains("Firstname"))
                    u.FirstName = dR["Firstname"].ToString();

                if (dR.Table.Columns.Contains("Lastname"))
                    u.LastName = dR["Lastname"].ToString();

                if (dR.Table.Columns.Contains("Email"))
                {
                    if (IsValidEmail(dR["Email"].ToString().TrimStart(' ').TrimEnd(' ')))
                    {
                        u.Email = dR["Email"].ToString().TrimStart(' ').TrimEnd(' ');
                    }
                    else
                    {
                        Error = "Invalid Email Address :" + dR["Email"].ToString();
                        return u;
                    }

                }
                else if (!UpdateUserInfo)
                {
                    Error = "Missing required field: Email Address";
                    return u;
                }

                if (!UpdateUserInfo)
                {
                    if (dR.Table.Columns.Contains("Username") && !string.IsNullOrEmpty(dR["Username"].ToString()))
                        u.Username = dR["Username"].ToString();
                    else if (job.Options.GenerateUsername)
                    {
                        switch (job.Options.GenerateUsernamePattern)
                        {
                            #region Generate Username
                            case "Email address":
                                {
                                    u.Username = u.Email;
                                    break;
                                }
                            case "First initial and lastname":
                                {
                                    if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                        u.Username = u.FirstName.Substring(0, 1) + u.LastName;
                                    else
                                    {
                                        Error = "Unable to generate username";
                                        return u;
                                    }
                                    break;
                                }
                            case "Lastname and first initial":
                                {
                                    if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                        u.Username = u.LastName + u.FirstName.Substring(0, 1);
                                    else
                                    {
                                        Error = "Unable to generate username";
                                        return u;
                                    }
                                    break;
                                }
                            case "Last initial and firstname":
                                {
                                    if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                        u.Username = u.LastName.Substring(0, 1) + u.FirstName;
                                    else
                                    {
                                        Error = "Unable to generate username";
                                        return u;
                                    }
                                    break;
                                }
                            case "Firstname and last initial":
                                {
                                    if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                        u.Username = u.FirstName + u.LastName.Substring(0, 1);
                                    else
                                    {
                                        Error = "Unable to generate username";
                                        return u;
                                    }
                                    break;
                                }
                            case "Firstname and lastname":
                                {
                                    if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                        u.Username = u.FirstName + " " + u.LastName;
                                    else
                                    {
                                        Error = "Unable to generate username";
                                        return u;
                                    }
                                    break;
                                }
                            case "Lastname and firstname":
                                {
                                    if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                        u.Username = u.LastName + " " + u.FirstName;
                                    else
                                    {
                                        Error = "Unable to generate username";
                                        return u;
                                    }
                                    break;
                                }
                            default:
                                {
                                    Error = "Missing required field: Username";
                                    return u;
                                }
                                #endregion
                        }

                    }
                    else
                    {
                        Error = "Missing required field: Username";
                        return u;
                    }
                }

                if (dR.Table.Columns.Contains("DisplayName") && !string.IsNullOrEmpty(dR["DisplayName"].ToString()))
                    u.DisplayName = dR["DisplayName"].ToString();
                else if (!UpdateUserInfo && job.Options.GenerateDisplayName)
                {
                    switch (job.Options.GenerateDisplayNamePattern)
                    {
                        #region Generate DisplayName
                        case "Email address":
                            {
                                u.DisplayName = u.Email;
                                break;
                            }
                        case "First initial and lastname":
                            {
                                if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                    u.DisplayName = u.FirstName.Substring(0, 1) + u.LastName;
                                else
                                {
                                    Error = "Unable to generate displayname";
                                    return u;
                                }
                                break;
                            }
                        case "Lastname and first initial":
                            {
                                if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                    u.DisplayName = u.LastName + u.FirstName.Substring(0, 1);
                                else
                                {
                                    Error = "Unable to generate displayname";
                                    return u;
                                }
                                break;
                            }
                        case "Last initial and firstname":
                            {
                                if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                    u.DisplayName = u.LastName.Substring(0, 1) + u.FirstName;
                                else
                                {
                                    Error = "Unable to generate displayname";
                                    return u;
                                }
                                break;
                            }
                        case "Firstname and last initial":
                            {
                                if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                    u.DisplayName = u.FirstName + u.LastName.Substring(0, 1);
                                else
                                {
                                    Error = "Unable to generate displayname";
                                    return u;
                                }
                                break;
                            }
                        case "Firstname and lastname":
                            {
                                if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                    u.DisplayName = u.FirstName + " " + u.LastName;
                                else
                                {
                                    Error = "Unable to generate displayname";
                                    return u;
                                }
                                break;
                            }
                        case "Lastname and firstname":
                            {
                                if (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName))
                                    u.DisplayName = u.LastName + " " + u.FirstName;
                                else
                                {
                                    Error = "Unable to generate displayname";
                                    return u;
                                }
                                break;
                            }
                        default:
                            {
                                Error = "Missing required field: displayname";
                                return u;
                            }
                            #endregion
                    }
                }
                else if (!UpdateUserInfo)
                {
                    Error = "Missing required field: Displayname";
                    return u;
                }

                if (dR.Table.Columns.Contains("Password"))
                    u.Membership.Password = dR["Password"].ToString();

                #endregion

                if (pS.Registration.UseEmailAsUserName)
                    u.Username = u.Email;

                #region New User Requirements
                if (!UpdateUserInfo)
                {
                    u.PortalID = PortalID;
                    u.IsSuperUser = false;

                    u.Profile.PreferredLocale = pS.DefaultLanguage;
                    u.Profile.PreferredTimeZone = pS.TimeZone;
                    u.Profile.FirstName = u.FirstName;
                    u.Profile.LastName = u.LastName;
                    u.Membership = new UserMembership(u);
                    u.Membership.Approved = true;
                    u.Membership.CreatedDate = DateTime.Now;
                    //u.Membership.Email = u.Email;
                    u.Membership.IsOnLine = false;
                    //u.Membership.Username = u.Username;
                    u.Membership.Password = "test";
                    DataCache.ClearUserCache(PortalID, u.Username);
                    if (dR.Table.Columns.Contains("Password") && !string.IsNullOrEmpty(dR["Password"].ToString()))
                        u.Membership.Password = dR["Password"].ToString();
                    else if (job.Options.GeneratePassword)
                        u.Membership.Password = UserController.GeneratePassword();
                    else
                    {
                        Error = "Missing required field: Password";
                        return u;
                    }

                    if (job.Options.ForceChangePassword)
                        u.Membership.UpdatePassword = true;
                }
                #endregion
                else
                    MapProfileFields(PortalID, u, dR);

                return u;
            }
            public static bool IsValidEmail(string email)
            {
                if (string.IsNullOrEmpty(email) || email.Contains(" ") || !(email.Contains("@") && email.Contains(".")))
                    return false;

                return true;
            }
            private static void MapProfileFields(int PortalID, UserInfo u, DataRow dR)
            {
                ProfilePropertyDefinitionCollection pFields = ProfileController.GetPropertyDefinitionsByPortal(PortalID);

                foreach (ProfilePropertyDefinition p in pFields)
                {
                    if (dR.Table.Columns.Contains("Profile_" + p.PropertyName))
                        u.Profile.SetProfileProperty(p.PropertyName, dR["Profile_" + p.PropertyName].ToString());
                }
            }
            #endregion

            #region Utility Functions            
            private static void AddErrorLog(DataTable ErrorLog, DataRow dataRow, string Param1)
            {
                DataRow dR = ErrorLog.NewRow();

                for (int i = 0; i < dataRow.ItemArray.Length; i++)
                {
                    dR[i] = dataRow[i];
                }
                dR[dataRow.ItemArray.Length] = Param1;

                ErrorLog.Rows.Add(dR);
            }
            private static DataTable AddBasicColumns(DataTable dT)
            {
                DataTable tableErr = new DataTable("ErrorLog");

                foreach (var d in dT.Columns)
                {
                    tableErr.Columns.Add(d.ToString());
                }

                tableErr.Columns.Add("Error");
                return tableErr;
            }

            #endregion
        }
    }
}
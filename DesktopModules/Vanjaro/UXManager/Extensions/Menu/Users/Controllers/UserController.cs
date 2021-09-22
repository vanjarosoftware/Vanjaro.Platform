using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Permissions;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using Vanjaro.UXManager.Library.Common;
using static DotNetNuke.Common.Lists.CachedCountryList;
using static DotNetNuke.Web.InternalServices.CountryRegionController;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Extensions.Menu.Users.Managers;
using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class UserController : UIEngineController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersController));
#pragma warning disable IDE0060 // Remove unused parameter
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo, UserInfo userInfo, Dictionary<string, string> parameters)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            int uid = 0;
            if (parameters.Count > 0)
            {
                uid = int.Parse(parameters["uid"]);
            }

            PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            GetUsersContract getUsersContract = new GetUsersContract()
            {
                PortalId = ps.PortalId,
                PageSize = 15,
                Filter = UserFilters.All
            };
            switch (Identifier)
            {
                case "setting_adduser":
                    {
                        List<KeyValuePair<string, int>> userFilters = new List<KeyValuePair<string, int>>();
                        userFilters = UsersController.Instance.GetUserFilters(userInfo.IsSuperUser).ToList();
                        userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.Deleted)));
                        KeyValuePair<string, int> item = userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.All));
                        userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.All)));
                        userFilters.Insert(0, item);
                        Settings.Add("UseEmailAsUsername", new UIData { Name = "UseEmailAsUsername", Options = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalSettings.Current.PortalId, false) });
                        Settings.Add("UserTemplate", new UIData { Name = "UserTemplate", Options = new CreateUserContract() { Authorize = true, Notify = false, RandomPassword = true } });
                        //Settings.Add("RedirectUrl", new UIData { Name = "RedirectUrl", Value = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + ExtensionInfo.GUID.ToLower() });
                        return Settings.Values.ToList();
                    }
                case "setting_users":
                    {
                        //we don't need show the Deleted user filter
                        List<KeyValuePair<string, int>> userFilters = new List<KeyValuePair<string, int>>();
                        userFilters = UsersController.Instance.GetUserFilters(userInfo.IsSuperUser).ToList();
                        userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.Deleted)));
                        KeyValuePair<string, int> item = userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.All));
                        userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.All)));
                        userFilters.Insert(0, item);
                        //move IsuperUser into bottom or last
                        KeyValuePair<string, int> itemSuperUsers = userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.SuperUsers));
                        userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.SuperUsers)));
                        userFilters.Insert(userFilters.Count, itemSuperUsers);

                        RecyclebinController.Instance.GetDeletedUsers(out int TotalRecords);
                        //IEnumerable<UserItem> deletedusers = from t in users select UserManager.ConvertToUserItem(t);
                        string MemberProfileUrl = string.Empty;
                        if (Library.Managers.MenuManager.GetURL().ToLower().Contains("guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3"))
                            MemberProfileUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL().ToLower().Replace("guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3", "guid=aadb4856-9f2d-44be-ac42-15bcca62df0b").TrimEnd('&');
                        else
                            MemberProfileUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL() + "mid=0&icp=true&guid=aadb4856-9f2d-44be-ac42-15bcca62df0b";

                        Settings.Add("MemberProfileUrl", new UIData { Name = "MemberProfileUrl", Value = MemberProfileUrl });
                        Settings.Add("AllUsers", new UIData { Name = "AllUsers", Options = null, Value = "" });
                        Settings.Add("UserFilters", new UIData { Name = "UserFilters", Options = userFilters, OptionsText = "Key", OptionsValue = "Value", Value = ((int)UserFilters.All).ToString() });
                        Settings.Add("Select_UserFilters", new UIData { Name = "Select_UserFilters", Options = UsersController.Instance.GetUserFilters(userInfo.IsSuperUser).Where(x => x.Value == (int)UserFilters.All).FirstOrDefault() });
                        Settings.Add("CurrentUserId", new UIData { Name = "CurrentUserId", Value = ps.UserId.ToString() });
                        Settings.Add("DeletedUsers", new UIData { Name = "DeletedUsers", Options = null, Value = TotalRecords.ToString() });
                        return Settings.Values.ToList();
                    }
                case "setting_setting":
                    {

                        if (uid > 0)
                        {
                            int UserID = userInfo.IsInRole("Administrators") || userInfo.IsSuperUser ? uid : userInfo.UserID;
                            string keyword = string.Empty;

                            UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(ps.PortalId, UserID);
                            UserBasicDto userDetails = UsersController.Instance.GetUserDetail(ps.PortalId, UserID);
                            ProfileController.GetUserProfile(ref user);
                            Settings.Add("BrowseUrl", new UIData { Name = "BrowseUrl", Value = Common.Utilities.Utils.BrowseUrl(-1, "Manage") });
                            Settings.Add("AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType });
                            Settings.Add("MaxFileSize", new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() });
                            Settings.Add("UserDetails", new UIData { Name = "UserDetails", Options = UserManager.MapUserBasicDto(user, userDetails) });
                            Settings.Add("UserRoles", new UIData { Name = "UserRoles", Options = UsersController.Instance.GetUserRoles(user, keyword, out int totalRoles).Select(r => UserRoleDto.FromRoleInfo(ps, r)) });
                            Settings.Add("IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString() });
                            Settings.Add("ProfilePropertiesByCategories", new UIData { Name = "ProfilePropertiesByCategories", Options = Managers.UserManager.GetLocalizedCategories(user.Profile.ProfileProperties, user).Select(x => new { x.Key, x.Value }) });

                            if (string.IsNullOrEmpty(user.Profile.PreferredLocale))
                            {
                                if (string.IsNullOrEmpty(DotNetNuke.Entities.Users.UserController.GetUserById(ps.PortalId, ps.AdministratorId).Profile.PreferredLocale))
                                {
                                    user.Profile.SetProfileProperty("PreferredLocale", !string.IsNullOrEmpty(ps.DefaultLanguage) ? ps.DefaultLanguage : Thread.CurrentThread.CurrentCulture.Name);
                                }
                                else
                                {
                                    user.Profile.SetProfileProperty("PreferredLocale", DotNetNuke.Entities.Users.UserController.GetUserById(ps.PortalId, ps.AdministratorId).Profile.PreferredLocale);
                                }
                            }

                            List<Entities.ProfileProperties> profileProperties = new List<Entities.ProfileProperties>();
                            ListController listController = new ListController();
                            foreach (ProfilePropertyDefinition d in user.Profile.ProfileProperties)
                            {
                                if (Managers.UserManager.CheckAccessLevel(d, user))
                                {
                                    string ControlType = UserManager.GetControlType(d.DataType);
                                    if (ControlType == "Country" || ControlType == "Region" || ControlType == "List")
                                        d.PropertyValue = string.IsNullOrEmpty(d.PropertyValue) ? "-1" : d.PropertyValue;
                                    else if (ControlType == "TimeZone")
                                        d.PropertyValue = string.IsNullOrEmpty(d.PropertyValue) ? PortalSettings.Current.TimeZone.Id : d.PropertyValue;
                                    List<ListEntryInfo> data = listController.GetListEntryInfoItems(d.PropertyName, "", PortalSettings.Current.PortalId).ToList();
                                    data.Insert(0, new ListEntryInfo { Text = Localization.GetString("NotSpecified", Components.Constants.LocalResourcesFile), Value = d.PropertyValue });
                                    profileProperties.Add(new Entities.ProfileProperties { ProfilePropertyDefinition = d, ListEntries = data });
                                }
                            }
                            Settings.Add("ProfileProperties", new UIData { Name = "ProfileProperties", Options = profileProperties });

                            //Profile URL
                            dynamic Profile = null;
                            if (!string.IsNullOrEmpty(user.Profile.Photo))
                            {
                                Profile = BrowseUploadFactory.GetFile(ps, Convert.ToInt32(user.Profile.Photo));
                            }

                            Settings.Add("PhotoURL", new UIData { Name = "PhotoURL", Options = Profile ?? user.Profile, Value = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, user.UserID, user.Email) });
                            var countries = GetCountryList(Thread.CurrentThread.CurrentCulture.Name).Values.OrderBy(x => x.NormalizedFullName).Select(x => new
                            {
                                Id = x.Id.ToString(),
                                x.FullName,
                                x.Name
                            }).ToList();
                            countries.Insert(0, new { Id = "-1", FullName = Localization.GetString("NotSpecified", Components.Constants.LocalResourcesFile), Name = Localization.GetString("NotSpecified", Components.Constants.LocalResourcesFile) });
                            Settings.Add("Countries", new UIData { Name = "Countries", OptionsText = "Key", OptionsValue = "Value", Options = countries });
                            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
                            var Timezones = timeZones.Cast<TimeZoneInfo>().Select(x => new
                            {
                                Id = x.Id.ToString(),
                                x.DisplayName
                            });

                            Settings.Add("Time_Zones", new UIData { Name = "Time_Zones", OptionsText = "Key", OptionsValue = "Value", Options = Timezones });
                            Dictionary<string, Locale> ActiveLocales = new LocaleController().GetLocales(ps.PortalId);
                            var activeLocales = ActiveLocales.Values.Cast<Locale>().Select(x => new
                            {
                                x.Code,
                                x.Culture,
                                x.EnglishName,
                                x.NativeName,
                                KeyID = x.KeyID.ToString(),
                                x.Text
                            }).ToList();
                            Settings.Add("Active_Locales", new UIData { Name = "Active_Locales", OptionsText = "Key", OptionsValue = "Value", Options = activeLocales });
                        }

                        Settings.Add("UserBasicTemplate", new UIData { Name = "UserBasicTemplate", Options = new UserBasicDto() });
                        Settings.Add("AllRoles", new UIData { Name = "AllRoles", Options = RoleController.Instance.GetRoles((PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId), Value = "" });
                        Settings.Add("RoleName", new UIData { Name = "RoleName", Value = "", Options = new UserRoleDto() });
                        Settings.Add("UseEmailAsUserName", new UIData { Name = "UseEmailAsUserName", Options = ps.Registration.UseEmailAsUserName });
                        return Settings.Values.ToList();
                    }
                case "setting_changepassword":
                    {
                        if (uid > 0)
                        {
                            Settings.Add("ChangePasswordTemplate", new UIData { Name = "ChangePasswordTemplate", Options = new ChangePasswordDto() });
                        }
                        return Settings.Values.ToList();
                    }
                default:
                    return null;
            }

        }

        [HttpGet]
        public List<Suggestion> GetSuggestionRoles(string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return new List<Suggestion>();
                }
                string searchkey = keyword.ToLower();
                IEnumerable<Suggestion> matchedRoles = RoleController.Instance.GetRoles((PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId)
                   .Where(r => r.RoleName.ToLower().Contains(searchkey) && r.Status == RoleStatus.Approved)
                   .Select(r => new Suggestion()
                   {
                       Value = r.RoleID,
                       Label = r.RoleName
                   });

                return matchedRoles.ToList();
            }
            catch (Exception)
            {
                return new List<Suggestion>();
            }
        }

        [HttpGet]
        public ActionResult GetUsers(string searchText, UserFilters filter, int pageIndex, int pageSize,
                    string sortColumn,
                    bool sortAscending)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                GetUsersContract getUsersContract = new GetUsersContract
                {
                    SearchText = searchText,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortColumn = sortColumn,
                    SortAscending = sortAscending,
                    PortalId = PortalController.GetEffectivePortalId(PortalSettings.PortalId),
                    Filter = filter
                };


                IEnumerable<UserBasicInfo> results = UsersController.Instance.GetUsers(getUsersContract, UserInfo.IsSuperUser, out int totalRecords).Select(d => new UserBasicInfo
                {
                    userId = d.UserId,
                    displayName = d.Displayname,
                    userName = d.Username,
                    email = d.Email,
                    avatar = DotNetNuke.Entities.Users.UserController.GetUserById((PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId, d.UserId).Profile.PhotoURL.Contains("no_avatar.gif") ? UserUtils.GetProfileImage(PortalSettings.PortalId, d.UserId, d.Email) : DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, d.UserId).Profile.PhotoURL,
                    firstName = d.Firstname,
                    lastName = d.Lastname,
                    createdOnDate = d.CreatedOnDate,
                    isDeleted = d.IsDeleted,
                    authorized = DotNetNuke.Entities.Users.UserController.GetUserById((PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId, d.UserId).Membership.Approved,
                    isSuperUser = d.IsSuperUser,
                    isAdmin = d.IsAdmin,
                    isLocked = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, d.UserId).Membership.LockedOut,
                });

                var response = new
                {
                    Results = results,
                    TotalResults = totalRecords
                };
                actionResult.Data = response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("GetUsers_Exception", ex.Message);
            }
            return actionResult;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(CreateUserContract contract)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                RegisterationDetails settings = new RegisterationDetails
                {
                    PortalSettings = PortalSettings,
                    Email = contract.Email,
                    FirstName = contract.FirstName,
                    LastName = contract.LastName,
                    UserName = contract.UserName,
                    Password = contract.Password,
                    Question = contract.Question,
                    Answer = contract.Answer,
                    Notify = contract.Notify,
                    Authorize = contract.Authorize,
                    RandomPassword = contract.RandomPassword,
                    IgnoreRegistrationMode = true
                };
                if (actionResult.IsSuccess)
                {
                    UserBasicDto userBasicDto = RegisteredManager.Register(settings);
                    actionResult.Data = UserManager.MapUserBasicDto(DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, userBasicDto.UserId), UsersController.Instance.GetUserDetail(PortalSettings.PortalId, userBasicDto.UserId));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("CreateUserError", ex.Message);
            }
            return actionResult;
        }


        [HttpGet]
        public ActionResult GetUserDetail(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserDetailDto userDetail = UsersController.Instance.GetUserDetail(PortalSettings.PortalId, userId);

                if (actionResult.IsSuccess)
                {
                    if (userDetail.IsSuperUser)
                    {
                        if (!UserInfo.IsSuperUser)
                        {
                            actionResult.AddError("InSufficientPermissions.Text", Localization.GetString("InSufficientPermissions.Text", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                        }

                        userDetail = UsersController.Instance.GetUserDetail(Null.NullInteger, userId);
                        actionResult.Data = userDetail;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("GetUserDetails_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpGet]
        public ActionResult GetUserFilters()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                actionResult.Data = UsersController.Instance.GetUserFilters(UserInfo.IsSuperUser);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                actionResult.AddError("GetUserFilters_Exception", exc.Message);
            }
            return actionResult;
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult GetUserRoles(string keyword, int userId, int pageIndex, int pageSize)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    IList<UserRoleInfo> userRoles = UsersController.Instance.GetUserRoles(user, Null.NullString, out int totalRecords);
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        userRoles = userRoles.Where(u => u.RoleName.ToLower().Contains(keyword.ToLower())).ToList();
                    }

                    int startIndex = pageIndex * pageSize;
                    IEnumerable<UserRoleDto> pagedData = userRoles.Skip(startIndex).Take(pageSize).Select(u => new UserRoleDto()
                    {
                        UserId = u.UserID,
                        RoleId = u.RoleID,
                        DisplayName = u.FullName,
                        StartTime = u.EffectiveDate,
                        ExpiresTime = u.ExpiryDate,
                        RoleName = u.RoleName,
                        AllowExpired = AllowExpired(u.UserID, u.RoleID),
                        AllowDelete = RoleController.CanRemoveUserFromRole(PortalSettings.Current, u.UserID, u.RoleID)
                    });

                    actionResult.Data = new { UserRoles = pagedData, totalRecords = userRoles.Count };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("GetUserRoles_Exception", ex.Message);
            }
            return actionResult;
        }

        private bool AllowExpired(int userId, int roleId)
        {
            return userId != PortalSettings.AdministratorId || roleId != PortalSettings.AdministratorRoleId;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveUserRole(UserRoleDto userRoleDto, bool notifyUser, bool isOwner, int UserId, string Action)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Validate(userRoleDto);
                UserInfo user = UsersController.GetUser(UserId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                int totalRoles = 0;
                if (Action == "add")
                {
                    foreach (UserRoleDto ur in UsersController.Instance.GetUserRoles(user, Null.NullString, out totalRoles).Select(r => UserRoleDto.FromRoleInfo(PortalSettings.Current, r)))
                    {
                        if (userRoleDto.RoleId == ur.RoleId)
                        {
                            actionResult.AddError("UserRoleAlreadyExist", Localization.GetString("UserRoleAlreadyExist", Components.Constants.LocalResourcesFile));
                        }
                    }
                }

                if (actionResult.IsSuccess && user != null)
                {
                    userRoleDto.UserId = UserId;
                    actionResult.Data = UsersController.Instance.SaveUserRole(PortalSettings.PortalId, UserInfo, userRoleDto, notifyUser, isOwner);
                    actionResult.Message = string.Format(Localization.GetString("UserRoleAdded", Components.Constants.LocalResourcesFile), userRoleDto.RoleName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("SaveUserRole_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "user")]
        public List<Country> Countries()
        {
            string searchString = (HttpContext.Current.Request.Params["keyword"] ?? "").NormalizeString();
            List<Country> countries = GetCountryList(Thread.CurrentThread.CurrentCulture.Name).Values.OrderBy(x => x.NormalizedFullName).ToList();
            return countries;
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "user")]
        public List<Region> Regions(int country = 0)
        {
            return Managers.UserManager.Regions(country);
        }

        [HttpGet]
        public ActionResult GetDeletedUserList(string searchText, int pageIndex = -1, int pageSize = -1)
        {
            ActionResult actionResult = new ActionResult();
            List<Entities.UserItem> userItems = new List<Entities.UserItem>();
            List<UserInfo> users = RecyclebinController.Instance.GetDeletedUsers(out int totalRecords, pageIndex, pageSize);
            if (users != null && actionResult.IsSuccess)
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    userItems = (from t in users
                                 where t.Username.Contains(searchText) || t.DisplayName.Contains(searchText) || t.Email.Contains(searchText)
                                 select UserManager.ConvertToUserItem(t)).ToList();
                    totalRecords = userItems.Count();

                }
                else
                {
                    userItems = (from t in users select UserManager.ConvertToUserItem(t)).ToList();
                }

                var response = new
                {
                    Success = true,
                    Results = userItems,
                    TotalResults = totalRecords
                };
                actionResult.Data = response;
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUserRole(UserRoleDto userRoleDto)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Validate(userRoleDto);
                UserInfo user = UsersController.GetUser(userRoleDto.UserId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    RoleController.Instance.UpdateUserRole(PortalSettings.PortalId, userRoleDto.UserId, userRoleDto.RoleId, RoleStatus.Approved, false, true);
                    actionResult.Message = string.Format(Localization.GetString("UserRoleRemoved", Components.Constants.LocalResourcesFile), userRoleDto.RoleName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("RemoveUserRole_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "user")]
        public ActionResult UpdateUserBasicInfo(dynamic userdata, int fileid)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserBasicDto userBasicDto = new UserBasicDto();
                KeyValuePair<HttpStatusCode, string> response;
                userBasicDto = JsonConvert.DeserializeObject<UserBasicDto>(userdata.UserBasicDto.ToString());
                List<Entities.ProfileProperties> profileProperties = JsonConvert.DeserializeObject<List<Entities.ProfileProperties>>(userdata.ProfilePropertyDefinitionCollection.ToString());
                Validate(userBasicDto);
                int UserID = UserInfo.IsInRole("Administrators") || UserInfo.IsSuperUser ? userBasicDto.UserId : UserInfo.UserID;
                UserInfo user = UsersController.GetUser(UserID, PortalSettings, UserInfo, out response);

                //for update Super User  profile picture need Photo Member profile property
                if (user.IsSuperUser)
                    Managers.UserManager.AddProfileProperties(ref user, UserInfo, ref profileProperties, ref response);

                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    //user.Profile.Photo = fileid.ToString();
                    //user.FirstName = userBasicDto.Firstname;
                    //user.LastName = userBasicDto.Lastname;
                    foreach (ProfileProperties prop in profileProperties)
                    {
                        if (prop.ProfilePropertyDefinition.PropertyName == "Photo")
                        {
                            user.Profile.Photo = fileid.ToString();
                            prop.ProfilePropertyDefinition.PropertyValue = fileid.ToString();
                        }

                        string LocalizePropertyNameMessage = Localization.GetString("FiledPropertyName", Components.Constants.LocalResourcesFile);
                        if (!string.IsNullOrEmpty(LocalizePropertyNameMessage))
                            LocalizePropertyNameMessage = LocalizePropertyNameMessage.Replace("[FiledPropertyName]", prop.ProfilePropertyDefinition.PropertyName);
                        else
                            LocalizePropertyNameMessage = prop.ProfilePropertyDefinition.PropertyName + " : ";
                        if (prop.ProfilePropertyDefinition.Required)
                        {
                            if (string.IsNullOrEmpty(prop.ProfilePropertyDefinition.PropertyValue))
                                actionResult.AddError("Validation_Required_" + prop.ProfilePropertyDefinition.PropertyName, LocalizePropertyNameMessage + (string.IsNullOrEmpty(prop.PropertyRequiredString) ? Localization.GetString("FieldisRequired", Components.Constants.LocalResourcesFile) : prop.PropertyRequiredString));
                            else if ((prop.ControlType == "Country" || prop.ControlType == "Region" || prop.ControlType == "List") && prop.ProfilePropertyDefinition.PropertyValue == "-1")
                                actionResult.AddError("Validation_Required_" + prop.ProfilePropertyDefinition.PropertyName, LocalizePropertyNameMessage + (string.IsNullOrEmpty(prop.PropertyRequiredString) ? Localization.GetString("FieldisRequired", Components.Constants.LocalResourcesFile) : prop.PropertyRequiredString));
                        }

                        if (!string.IsNullOrEmpty(prop.ProfilePropertyDefinition.PropertyValue) && !string.IsNullOrEmpty(prop.ProfilePropertyDefinition.ValidationExpression) && !Regex.IsMatch(prop.ProfilePropertyDefinition.PropertyValue, prop.ProfilePropertyDefinition.ValidationExpression))
                            actionResult.AddError("Validation_Expression_" + prop.ProfilePropertyDefinition.PropertyName, LocalizePropertyNameMessage + (string.IsNullOrEmpty(prop.PropertyValidationString) ? Localization.GetString("FieldisInvalid", Components.Constants.LocalResourcesFile) : prop.PropertyValidationString));

                        if (!string.IsNullOrEmpty(prop.ProfilePropertyDefinition.PropertyValue) && (prop.ControlType == "Text" || prop.ControlType == "Multi-line Text") && !string.Equals(prop.ProfilePropertyDefinition.DefaultValue, prop.ProfilePropertyDefinition.PropertyValue) && prop.ProfilePropertyDefinition.PropertyValue.Length > prop.ProfilePropertyDefinition.Length)
                            actionResult.AddError("Validation_Length_" + prop.ProfilePropertyDefinition.PropertyName, LocalizePropertyNameMessage + Localization.GetString("Validation_Length", Components.Constants.LocalResourcesFile));

                        if (prop.ProfilePropertyDefinition.PropertyValue == "-1" && (prop.ControlType == "Country" || prop.ControlType == "Region" || prop.ControlType == "List"))
                            prop.ProfilePropertyDefinition.PropertyValue = "";

                        if (actionResult.IsSuccess)
                        {
                            prop.ProfilePropertyDefinition.PropertyValue = string.IsNullOrEmpty(prop.ProfilePropertyDefinition.PropertyValue) ? "" : prop.ProfilePropertyDefinition.PropertyValue;
                            user.Profile.SetProfileProperty(prop.ProfilePropertyDefinition.PropertyName.ToString(), prop.ProfilePropertyDefinition.PropertyValue);
                        }
                    }
                    IFolderInfo folder = FolderManager.Instance.GetUserFolder(user);
                    if (!string.IsNullOrEmpty(user.Profile.Photo) && int.Parse(user.Profile.Photo) > 0)
                    {
                        UserManager.CreateThumbnails(int.Parse(user.Profile.Photo));
                    }

                    ProfileController.UpdateUserProfile(user);
                    if (UserInfo.IsInRole("Administrators") || UserInfo.UserID > 0)
                    {
                        if (string.IsNullOrEmpty(userBasicDto.Displayname))
                            userBasicDto.Displayname = user.DisplayName;
                        if (string.IsNullOrEmpty(userBasicDto.Email) || PortalSettings.Registration.UseEmailAsUserName)
                            userBasicDto.Email = user.Email;
                        if (string.IsNullOrEmpty(userBasicDto.Username))
                            userBasicDto.Username = user.Username;
                        actionResult.Data = UsersController.Instance.UpdateUserBasicInfo(userBasicDto);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                actionResult.AddError("UpdateUserBasicInfo_Exception", ex.Message);
                if (actionResult.Message.Contains("Violation of UNIQUE KEY constraint 'IX_Users'. Cannot insert duplicate key in object 'dbo.Users'"))
                {
                    actionResult.Message = Localization.GetString("Usernamemustbeunique", Components.Constants.LocalResourcesFile);
                }
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAuthorizeStatus(int userId, bool authorized)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (userId == UserInfo.UserID && actionResult.IsSuccess && user != null)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Localization.GetString("InSufficientPermissions", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                }

                if (user.Membership.Approved == authorized && actionResult.IsSuccess)//Do nothing if the new status is same as current status.
                {
                    return actionResult;
                }

                if (actionResult.IsSuccess)
                {
                    UsersController.Instance.UpdateAuthorizeStatus(user, PortalSettings.PortalId, authorized);
                    actionResult.Message = Localization.GetString("Authorized" + authorized.ToString(), Components.Constants.LocalResourcesFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("UpdateAuthorizeStatus_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public ActionResult UpdateSuperUserStatus(int userId, bool setSuperUser)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    user.IsSuperUser = setSuperUser;
                    //Update User
                    DotNetNuke.Entities.Users.UserController.UpdateUser(PortalSettings.PortalId, user);
                    DataCache.ClearCache();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("UpdateSuperUserStatus_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordDto changePasswordDto)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int userId = changePasswordDto.UserId;
                string password = changePasswordDto.Password;
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    IUsersController controller = UsersController.Instance;
                    controller.ChangePassword(PortalSettings.PortalId, userId, password);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("ChangePassword_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForceChangePassword(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    if (userId == UserInfo.UserID)
                    {
                        actionResult.AddError("InSufficientPermissions", Localization.GetString("InSufficientPermissions", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                    }

                    if (actionResult.IsSuccess)
                    {
                        if (!UsersController.Instance.ForceChangePassword(user, PortalSettings.PortalId, true))
                        {
                            actionResult.AddError("OptionUnavailable", Localization.GetString("OptionUnavailable", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("ForceChangePassword_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendPasswordResetLink(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                    {
                        actionResult.AddError("OptionUnavailable", Localization.GetString("OptionUnavailable", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                    }
                    else
                    {
                        try
                        {
                            if (actionResult.IsSuccess)
                            {
                                //create resettoken
                                DotNetNuke.Entities.Users.UserController.ResetPasswordToken(user, Host.AdminMembershipResetLinkValidity);
                                bool canSend = Mail.SendMail(user, MessageType.PasswordReminder, PortalSettings) == string.Empty;
                                if (!canSend)
                                {
                                    actionResult.AddError("OptionUnavailable", Localization.GetString("OptionUnavailable", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                                }
                            }
                        }
                        catch (ArgumentException exc)
                        {
                            Logger.Error(exc);
                            actionResult.AddError("InvalidPasswordAnswer", Localization.GetString("InvalidPasswordAnswer", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                        }
                        catch (Exception exc)
                        {
                            Logger.Error(exc);
                            actionResult.AddError("PasswordResetFailed", Localization.GetString("PasswordResetFailed", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("SendPasswordResetLink_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SoftDeleteUser(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    bool deleted = !user.IsDeleted && DotNetNuke.Entities.Users.UserController.DeleteUser(ref user, true, false);
                    if (!deleted)
                    {
                        actionResult.AddError("UserDeleteError", Localization.GetString("UserDeleteError", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("SoftDeleteUser_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HardDeleteUser(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess && user != null)
                {
                    bool deleted = user.IsDeleted && DotNetNuke.Entities.Users.UserController.RemoveUser(user);
                    if (!deleted)
                    {
                        actionResult.AddError("UserRemoveError", Localization.GetString("UserRemoveError", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("HardDeleteUser_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreDeletedUser(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserInfo user = UsersController.GetUser(userId, PortalSettings, UserInfo, out KeyValuePair<HttpStatusCode, string> response);
                if (user == null)
                {
                    actionResult.AddError(response.Key.ToString(), response.Value);
                }

                if (actionResult.IsSuccess)
                {
                    bool restored = user.IsDeleted && DotNetNuke.Entities.Users.UserController.RestoreUser(ref user);
                    if (!restored)
                    {
                        actionResult.AddError("UserRestoreError", Localization.GetString("UserRestoreError", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                actionResult.AddError("RestoreDeletedUser_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreUser(List<Entities.UserItem> users)
        {
            ActionResult actionResult = new ActionResult();
            string resultmessage = null;
            if (users != null && users.Any())
            {
                IEnumerable<UserInfo> Users = users.Select(u => DotNetNuke.Entities.Users.UserController.Instance.GetUserById(PortalSettings.PortalId, u.UserId));
                if (Users != null && actionResult.IsSuccess)
                {
                    foreach (UserInfo user in Users)
                    {
                        if (user == null)
                        {
                            actionResult.AddError(HttpStatusCode.NotFound.ToString(), Localization.GetString("UserNotFound", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                        }

                        if (actionResult.IsSuccess)
                        {
                            RecyclebinController.Instance.RestoreUser(user, out resultmessage);
                            if (resultmessage != null)
                            {
                                actionResult.AddError("Service_RestoreUserError", resultmessage);
                            }
                        }
                    }
                }
            }
            if (actionResult.IsSuccess)
            {
                actionResult.Data = new { Status = 0 };
            }
            else
            {
                actionResult.AddError("Service_RestoreTabModuleError", RecyclebinController.Instance.LocalizeString("Service_RestoreTabModuleError") + resultmessage);
                actionResult.Data = new { Status = 1 };
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnLockUser(int userId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
                if (user == null)
                    actionResult.AddError(response.Key.ToString(), response.Value);

                if (userId == this.UserInfo.UserID)
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Localization.GetString("InSufficientPermissions", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));

                if (actionResult.IsSuccess)
                {
                    var unlocked = user.Membership.LockedOut && DotNetNuke.Entities.Users.UserController.UnLockUser(user);
                    if (!unlocked)
                        actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), Localization.GetString("UserUnlockError", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
            }

            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUsers()
        {
            ActionResult actionResult = new ActionResult();
            List<UserInfo> deletedUsers = RecyclebinController.Instance.GetDeletedUsers(out _);
            RecyclebinController.Instance.DeleteUsers(deletedUsers);
            if (actionResult.IsSuccess)
            {
                actionResult.Data = new { Status = 0 };
            }
            else
            {
                actionResult.AddError("Service_RemoveUserError", Localization.GetString("Service_RemoveUserError", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                actionResult.Data = new { Status = 1 };
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUser(List<Entities.UserItem> users)
        {
            ActionResult actionResult = new ActionResult();
            RecyclebinController.Instance.DeleteUsers(UserManager.ConvertToDNNUserItem(users));
            if (actionResult.IsSuccess)
            {
                actionResult.Data = new { Status = 0 };
            }
            else
            {
                actionResult.AddError("Service_RemoveUserError", Localization.GetString("Service_RemoveUserError", Dnn.PersonaBar.Users.Components.Constants.LocalResourcesFile));
                actionResult.Data = new { Status = 1 };
            }
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
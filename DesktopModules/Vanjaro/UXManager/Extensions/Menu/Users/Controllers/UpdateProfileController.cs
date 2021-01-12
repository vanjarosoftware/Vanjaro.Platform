using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
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
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using Vanjaro.UXManager.Library.Common;
using static DotNetNuke.Common.Lists.CachedCountryList;
using static DotNetNuke.Web.InternalServices.CountryRegionController;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Extensions.Menu.Users.Managers;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
    public class UpdateProfileController : UIEngineController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersController));
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo, UserInfo userInfo, Dictionary<string, string> parameters)
        {
            int uid = 0;
            if (parameters.Count > 0)
            {
                int.TryParse(parameters["uid"], out uid);
            }

            PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            switch (Identifier)
            {
                case "setting_updateprofile":
                    {

                        if (uid > 0)
                        {
                            Settings.Add("BrowseUrl", new UIData { Name = "BrowseUrl", Value = Common.Utilities.Utils.BrowseUrl(-1, "Manage") });
                            Settings.Add("AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType });
                            Settings.Add("MaxFileSize", new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() });

                            if (!(parameters.ContainsKey("username") && parameters.ContainsKey("password")))
                                return Settings.Values.ToList();

                            int UserID = uid;
                            string keyword = string.Empty;

                            UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(ps.PortalId, UserID);
                            UserBasicDto userDetails = UsersController.Instance.GetUserDetail(ps.PortalId, UserID);
                            ProfileController.GetUserProfile(ref user);
                            Settings.Add("UserDetails", new UIData { Name = "UserDetails", Options = UserManager.MapUserBasicDto(user, userDetails) });
                            Settings.Add("UserRoles", new UIData { Name = "UserRoles", Options = UsersController.Instance.GetUserRoles(user, keyword, out int totalRoles).Select(r => UserRoleDto.FromRoleInfo(ps, r)) });
                            Settings.Add("IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString() });
                            Settings.Add("ProfilePropertiesByCategories", new UIData { Name = "ProfilePropertiesByCategories", Options = Managers.UserManager.GetLocalizedCategories(user.Profile.ProfileProperties, user, true).Select(x => new { x.Key, x.Value }) });

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
                                string ControlType = UserManager.GetControlType(d.DataType);
                                if (ControlType == "Country" || ControlType == "Region" || ControlType == "List")
                                    d.PropertyValue = string.IsNullOrEmpty(d.PropertyValue) ? "-1" : d.PropertyValue;
                                List<ListEntryInfo> data = listController.GetListEntryInfoItems(d.PropertyName, "", PortalSettings.Current.PortalId).ToList();
                                data.Insert(0, new ListEntryInfo { Text = Localization.GetString("NotSpecified", Components.Constants.LocalResourcesFile), Value = d.PropertyValue });
                                profileProperties.Add(new Entities.ProfileProperties { ProfilePropertyDefinition = d, ListEntries = data });
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
                        return Settings.Values.ToList();
                    }
                default:
                    return null;
            }

        }

        [HttpPost]
        public ActionResult GetSettings(dynamic formData)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int UserID = 0;
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                if (formData["userId"] != null && !string.IsNullOrEmpty(formData["userId"].Value))
                {
                    int.TryParse(formData["userId"].Value, out UserID);
                    parameters.Add("uid", formData["userId"].Value);
                }
                dynamic userLogin = new ExpandoObject();
                userLogin.Username = string.Empty;
                userLogin.Password = string.Empty;
                if (formData["username"] != null && !string.IsNullOrEmpty(formData["username"].Value))
                {
                    userLogin.Username = formData["username"].Value;
                    parameters.Add("username", formData["username"].Value);
                }
                if (formData["password"] != null && !string.IsNullOrEmpty(formData["password"].Value))
                {
                    userLogin.Password = formData["password"].Value;
                    parameters.Add("password", formData["password"].Value);
                }
                userLogin.Remember = false;
                dynamic eventArgs = Core.Managers.LoginManager.UserLogin(userLogin);
                if (eventArgs != null && eventArgs.User != null && UserID > 0 && eventArgs.User.UserID == UserID)
                {
                    actionResult.Data = DotNetNuke.Common.Utilities.Json.Serialize<dynamic>(GetData(Factories.AppFactory.Identifier.setting_updateprofile.ToString(), new Dictionary<string, string>(), this.UserInfo, parameters));
                    actionResult.Message = Managers.LoginManager.UserAuthenticated(eventArgs).Message;
                }
                else
                    actionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3#/unauthorize");
            }
            catch (Exception ex)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }
            return actionResult;
        }




        [HttpGet]
        public List<Country> Countries()
        {
            string searchString = (HttpContext.Current.Request.Params["keyword"] ?? "").NormalizeString();
            List<Country> countries = GetCountryList(Thread.CurrentThread.CurrentCulture.Name).Values.OrderBy(x => x.NormalizedFullName).ToList();
            return countries;
        }

        [HttpGet]
        public List<Region> Regions(int country = 0)
        {
            return Managers.UserManager.Regions(country);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
        public ActionResult UpdateUserBasicInfo(dynamic userdata, int fileid)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UserBasicDto userBasicDto = new UserBasicDto();
                dynamic userLogin = new ExpandoObject();
                userBasicDto = JsonConvert.DeserializeObject<UserBasicDto>(userdata.UserBasicDto.ToString());
                userLogin.Username = userdata["UserCredential"] != null && userdata["UserCredential"]["Username"] != null ? userdata["UserCredential"]["Username"].Value : string.Empty;
                userLogin.Password = userdata["UserCredential"] != null && userdata["UserCredential"]["Password"] != null ? userdata["UserCredential"]["Password"].Value : string.Empty;
                userLogin.Remember = false;
                dynamic eventArgs = Core.Managers.LoginManager.UserLogin(userLogin);
                if (eventArgs != null && eventArgs.User != null && eventArgs.User.UserID != userBasicDto.UserId)
                    actionResult.AddError("InvalidUser", Localization.GetString("InvalidUser", Components.Constants.LocalResourcesFile));
                if (actionResult.IsSuccess)
                {
                    List<Entities.ProfileProperties> profileProperties = JsonConvert.DeserializeObject<List<Entities.ProfileProperties>>(userdata.ProfilePropertyDefinitionCollection.ToString());
                    Validate(userBasicDto);

                    int UserID = userBasicDto.UserId;
                    UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, UserID);

                    if (actionResult.IsSuccess && user != null)
                    {
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

                            if (!string.IsNullOrEmpty(prop.ProfilePropertyDefinition.PropertyValue) && (prop.ControlType == "Text" || prop.ControlType == "Multi-line Text") && prop.ProfilePropertyDefinition.PropertyValue.Length > prop.ProfilePropertyDefinition.Length)
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
                        if (actionResult.IsSuccess)
                        {
                            dynamic eventArgs_Updated = Core.Managers.LoginManager.UserLogin(userLogin);
                            actionResult = Managers.LoginManager.UserAuthenticated(eventArgs_Updated);
                        }
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

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
using Dnn.PersonaBar.SiteSettings.Components;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Personalization;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Vanjaro.Core.Data.Entities;
using Vanjaro.UXManager.Library.Common;
using DNNLocalization = DotNetNuke.Services.Localization;
using PersonaBar = Dnn.PersonaBar.SiteSettings.Components.Constants;

namespace Vanjaro.UXManager.Extensions.Menu.MemberProfile
{
    public static partial class Managers
    {
        public class MemberProfileManager
        {
            public static string[] ExcludeControls = { "AutoComplete", "Checkbox ", "Page", "Image", "DateTime", "TimeZoneInfo", "Unknown" };
            public static string[] CannotDeleteProperty = { "Photo" };
            public static ActionResult GetProfileSettings(int? portalId)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = portalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    FriendlyUrlSettings urlSettings = new FriendlyUrlSettings(pid);
                    System.Collections.Hashtable userSettings = UserController.GetUserSettings(pid);

                    actionResult.Data = new
                    {
                        Settings = new
                        {
                            PortalId = pid,
                            RedirectOldProfileUrl = Config.GetFriendlyUrlProvider() == "advanced" && urlSettings.RedirectOldProfileUrl,
                            urlSettings.VanityUrlPrefix,
                            ProfileDefaultVisibility = userSettings["Profile_DefaultVisibility"] == null ? (int)UserVisibilityMode.AdminOnly : Convert.ToInt32(userSettings["Profile_DefaultVisibility"]),
                            ProfileDisplayVisibility = PortalController.GetPortalSettingAsBoolean("Profile_DisplayVisibility", pid, true)
                        },
                        UserVisibilityOptions = GetVisibilityOptions()
                    };
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }

            public static dynamic GetVisibilityOptions()
            {
                var UserVisibilityOptions = Enum.GetValues(typeof(UserVisibilityMode)).Cast<UserVisibilityMode>().Select(
                              v => new
                              {
                                  label = DNNLocalization.Localization.GetString(v.ToString(), Components.Constants.DNNLocalResourcesFile).ToString(),
                                  value = (int)v
                              }).ToList().Where(x => x.value != (int)UserVisibilityMode.FriendsAndGroups);

                return UserVisibilityOptions;
            }

            public static ActionResult GetProfileProperties(int? portalId)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = portalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        var profileProperties = ProfileController.GetPropertyDefinitionsByPortal(pid, false, false).Cast<ProfilePropertyDefinition>().Where(x => !CannotDeleteProperty.Contains(x.PropertyName)).Select(v => new
                        {
                            v.PropertyDefinitionId,
                            v.PropertyName,
                            DataType = DisplayDataType(v.DataType),
                            DefaultVisibility = v.DefaultVisibility.ToString(),
                            v.Required,
                            v.Visible,
                            v.ViewOrder,
                            CanDelete = CanDeleteProperty(v)
                        }).OrderBy(v => v.ViewOrder);

                        actionResult.Data = new
                        {
                            PortalId = pid,
                            Properties = profileProperties,
                            TotalResult = profileProperties.Count()
                        };
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static ActionResult GetProfileProperty(int? propertyId, int? portalId)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = portalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        ProfilePropertyDefinition profileProperty = ProfileController.GetPropertyDefinition(propertyId ?? -1, pid);
                        ListController listController = new ListController();
                        System.Collections.Generic.IEnumerable<System.Web.UI.WebControls.ListItem> cultureList = DNNLocalization.Localization.LoadCultureInListItems(GetCultureDropDownType(pid), Thread.CurrentThread.CurrentUICulture.Name, "", false);

                        var response = new
                        {
                            Success = true,
                            ProfileProperty = MapProfileProperty(profileProperty),
                            UserVisibilityOptions = GetVisibilityOptions(),
                            DataTypeOptions = listController.GetListEntryInfoItems("DataType").Where(x => !(ExcludeControls.Contains(x.Value) && x.Value != "TimeZone")).Select(t => new
                            {
                                t.EntryID,
                                Value = DNNLocalization.Localization.GetString(Components.Constants.Prefix + Regex.Replace(t.Value.ToString(), "[ ().-]+", ""), Components.Constants.LocalResourcesFile),

                            }).OrderBy(t => t.Value).ToList(),

                            LanguageOptions = cultureList.Select(c => new
                            {
                                c.Text,
                                c.Value
                            })
                        };
                        actionResult.Data = response;
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static ActionResult GetListInfo(string listName, int? portalId)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = portalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        ListController listController = new ListController();
                        System.Collections.Generic.IEnumerable<ListEntryInfo> entries = listController.GetListEntryInfoItems(listName, "", pid);
                        var response = new
                        {
                            Success = true,
                            listController.GetListInfo(listName, "", pid)?.EnableSortOrder,
                            Entries = entries.Select(t => new
                            {
                                t.EntryID,
                                t.Text,
                                t.Value,
                                t.SortOrder
                            }).OrderBy(x => x.SortOrder)
                        };
                        actionResult.Data = response;
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static ActionResult AddProfileProperty(UpdateProfilePropertyRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = request.PortalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        ProfilePropertyDefinition property = new ProfilePropertyDefinition(pid)
                        {
                            DataType = request.DataType,
                            DefaultValue = request.DefaultValue,
                            PropertyCategory = request.PropertyCategory,
                            PropertyName = request.PropertyName,
                            ReadOnly = request.ReadOnly,
                            Required = !Globals.IsHostTab(PortalSettings.Current.ActiveTab.TabID) && request.Required,
                            ValidationExpression = request.ValidationExpression,
                            ViewOrder = request.ViewOrder,
                            Visible = request.Visible,
                            Length = request.Length,
                            DefaultVisibility = (UserVisibilityMode)request.DefaultVisibility
                        };

                        actionResult = ValidateProperty(property);

                        if (actionResult.IsSuccess)
                        {
                            int propertyId = ProfileController.AddPropertyDefinition(property);
                            if (propertyId < Null.NullInteger)
                            {
                                actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(DNNLocalization.Localization.GetString("DuplicateName", PersonaBar.Constants.LocalResourcesFile)));
                            }
                            else
                            {
                                DataCache.ClearDefinitionsCache(pid);
                                actionResult.Data = new { MemberProfile = GetProfileProperties(pid).Data.Properties, PropertyLocalization = GetProfilePropertyLocalization(PortalSettings.Current.CultureCode, property.PropertyName, property.PropertyCategory).Data };
                                actionResult.Message = DNNLocalization.Localization.GetString("MemberProfileAdded", Components.Constants.LocalResourcesFile);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }

            public static ActionResult UpdateProfileProperty(UpdateProfilePropertyRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = request.PortalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {

                        int definitionId = request.PropertyDefinitionId ?? Null.NullInteger;

                        if (definitionId != Null.NullInteger)
                        {
                            dynamic profileProperty = GetProfileProperty(request.PropertyDefinitionId, pid).Data.ProfileProperty;
                            request.PropertyName = profileProperty.PropertyName;
                            ProfilePropertyDefinition property = new ProfilePropertyDefinition(pid)
                            {
                                PropertyDefinitionId = definitionId,
                                DataType = request.DataType,
                                DefaultValue = request.DefaultValue,
                                PropertyCategory = request.PropertyCategory,
                                PropertyName = request.PropertyName,
                                ReadOnly = request.ReadOnly,
                                Required = request.Required,
                                ValidationExpression = request.ValidationExpression,
                                ViewOrder = request.ViewOrder,
                                Visible = request.Visible,
                                Length = request.Length,
                                DefaultVisibility = (UserVisibilityMode)request.DefaultVisibility
                            };

                            actionResult = ValidateProperty(property);
                            if (actionResult.IsSuccess)
                            {
                                ProfileController.UpdatePropertyDefinition(property);
                                DataCache.ClearDefinitionsCache(pid);
                                actionResult.Data = new { MemberProfile = GetProfileProperties(pid).Data.Properties, PropertyLocalization = GetProfilePropertyLocalization(PortalSettings.Current.CultureCode, property.PropertyName, property.PropertyCategory).Data, GetListInfo(property.PropertyName, PortalSettings.Current.PortalId).Data.Entries };
                                actionResult.Message = DNNLocalization.Localization.GetString("MemberProfileUpdated", Components.Constants.LocalResourcesFile);
                            }
                        }
                    }
                    else
                    {
                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), "");
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }

            public static ActionResult GetProfilePropertyLocalization(string cultureCode, string propertyName, string propertyCategory)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = PortalSettings.Current.PortalId;
                    //if (!PortalSettings.Current.UserInfo.IsSuperUser)
                    //{
                    //    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    //}

                    if (actionResult.IsSuccess)
                    {
                        cultureCode = string.IsNullOrEmpty(cultureCode) ? DNNLocalization.LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode;

                        DNNLocalization.Locale language = DNNLocalization.LocaleController.Instance.GetLocale(pid, cultureCode);
                        if (language == null)
                        {
                            actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(DNNLocalization.Localization.GetString("InvalidLocale.ErrorMessage", PersonaBar.Constants.LocalResourcesFile), cultureCode));
                        }

                        if (actionResult.IsSuccess)
                        {
                            string resourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
                            var response = new
                            {
                                PortalId = pid,
                                PropertyName = propertyName,
                                PropertyCategory = propertyCategory,
                                Language = cultureCode,
                                PropertyNameString = DNNLocalization.Localization.GetString("ProfileProperties_" + propertyName, resourceFile, cultureCode) ?? "",
                                PropertyHelpString = DNNLocalization.Localization.GetString("ProfileProperties_" + propertyName + ".Help", resourceFile, cultureCode) ?? "",
                                PropertyRequiredString = DNNLocalization.Localization.GetString("ProfileProperties_" + propertyName + ".Required", resourceFile, cultureCode) ?? "",
                                PropertyValidationString = DNNLocalization.Localization.GetString("ProfileProperties_" + propertyName + ".Validation", resourceFile, cultureCode) ?? "",
                                CategoryNameString = DNNLocalization.Localization.GetString("ProfileProperties_" + propertyCategory + ".Header", resourceFile, cultureCode) ?? ""
                            };
                            actionResult.Data = response;
                        }
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static ActionResult UpdateProfilePropertyLocalization(UpdateProfilePropertyLocalizationRequest request)
            {
                ActionResult actionResult = new ActionResult();
                SiteSettingsController _controller = new SiteSettingsController();
                try
                {
                    int pid = request.PortalId ?? PortalSettings.Current.PortalId;
                    if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        DNNLocalization.Locale language = DNNLocalization.LocaleController.Instance.GetLocale(pid, request.Language);
                        if (language == null)
                        {
                            actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(DNNLocalization.Localization.GetString("InvalidLocale.ErrorMessage", PersonaBar.Constants.LocalResourcesFile), request.Language));
                        }

                        if (actionResult.IsSuccess)
                        {
                            _controller.SaveLocalizedKeys(pid, request.PropertyName, request.PropertyCategory, request.Language, request.PropertyNameString,
                                request.PropertyHelpString, request.PropertyRequiredString, request.PropertyValidationString, request.CategoryNameString);
                            DataCache.ClearCache();
                        }
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static void UpdateSortOrder(ListEntryInfo listEntryInfo)
            {
                using (VanjaroRepo db = new VanjaroRepo())
                {
                    db.Execute("UPDATE " + Core.Data.Scripts.CommonScript.TablePrefix + "LISTS SET SORTORDER=@0 WHERE ENTRYID=@1 AND LISTNAME=@2", listEntryInfo.SortOrder, listEntryInfo.EntryID, listEntryInfo.ListName);
                }
            }
            public static string GetControlType(int DataTypeID)
            {
                string _ControlType = string.Empty;
                ListController listController = new ListController();
                var ListDataTypeInfo = listController.GetListEntryInfoItems("DataType", "", PortalSettings.Current.PortalId).ToList();
                foreach (var dInfo in ListDataTypeInfo)
                    if (dInfo.EntryID == DataTypeID)
                        _ControlType = dInfo.Value;
                return _ControlType;
            }

            #region Private Methods          
            private static UpdateProfilePropertyRequest MapProfileProperty(dynamic profileProperty)
            {
                UpdateProfilePropertyRequest updateProfilePropertyRequest = new UpdateProfilePropertyRequest();
                if (profileProperty != null)
                {
                    updateProfilePropertyRequest.PortalId = profileProperty.PortalId;
                    updateProfilePropertyRequest.PropertyDefinitionId = profileProperty.PropertyDefinitionId;
                    updateProfilePropertyRequest.PropertyName = profileProperty.PropertyName;
                    updateProfilePropertyRequest.DataType = IsExistsDataType(profileProperty.DataType) ? profileProperty.DataType : 0;
                    updateProfilePropertyRequest.PropertyCategory = profileProperty.PropertyCategory;
                    updateProfilePropertyRequest.Length = profileProperty.Length;
                    updateProfilePropertyRequest.DefaultValue = profileProperty.DefaultValue;
                    updateProfilePropertyRequest.ValidationExpression = profileProperty.ValidationExpression;
                    updateProfilePropertyRequest.Required = profileProperty.Required;
                    updateProfilePropertyRequest.ReadOnly = profileProperty.ReadOnly;
                    updateProfilePropertyRequest.Visible = profileProperty.Visible;
                    updateProfilePropertyRequest.ViewOrder = profileProperty.ViewOrder;
                    updateProfilePropertyRequest.DefaultVisibility = (int)profileProperty.DefaultVisibility;
                }
                else
                {
                    updateProfilePropertyRequest.DefaultVisibility = (int)UserVisibilityMode.AdminOnly;
                    updateProfilePropertyRequest.Visible = true;
                }
                return updateProfilePropertyRequest;
            }
            private static ActionResult ValidateProperty(ProfilePropertyDefinition definition)
            {
                bool isValid = true;
                ActionResult httpPropertyValidationError = new ActionResult();
                ListController objListController = new ListController();
                string strDataType = objListController.GetListEntryInfo("DataType", definition.DataType).Value;
                Regex propertyNameRegex = new Regex("^[a-zA-Z0-9]+$");
                if (!propertyNameRegex.Match(definition.PropertyName).Success)
                {
                    isValid = false;
                    httpPropertyValidationError.AddError("NoSpecialCharacterName", string.Format(DNNLocalization.Localization.GetString("NoSpecialCharacterName.Text", PersonaBar.Constants.LocalResourcesFile)));
                }

                switch (strDataType)
                {
                    case "Text":
                        if (definition.Required && definition.Length == 0)
                        {
                            isValid = Null.NullBoolean;
                            httpPropertyValidationError.AddError("RequiredTextBox", string.Format(DNNLocalization.Localization.GetString("RequiredTextBox", PersonaBar.Constants.LocalResourcesFile)));
                        }
                        break;
                }

                //if (isValid == false)
                //{
                //    httpPropertyValidationError.AddError("RequiredTextBox", string.Format(Localization.GetString("RequiredTextBox", PersonaBar.Constants.LocalResourcesFile)));
                //}
                return httpPropertyValidationError;
            }
            private static string DisplayDataType(int dataType)
            {
                string retValue = Null.NullString;
                ListController listController = new ListController();
                ListEntryInfo definitionEntry = listController.GetListEntryInfo("DataType", dataType);
                if (definitionEntry != null)
                {
                    retValue = definitionEntry.Value;
                }
                return retValue;
            }
            private static bool IsExistsDataType(int dataType)
            {
                bool IsExistsDataType = false;
                ListController listController = new ListController();
                foreach (var d in listController.GetListEntryInfoItems("DataType").Where(x => !(ExcludeControls.Contains(x.Value) && x.Value != "TimeZone")).ToList())
                {
                    if (d.EntryID == dataType)
                    {
                        IsExistsDataType = true;
                        break;
                    }
                }
                return IsExistsDataType;
            }
            private static bool CanDeleteProperty(ProfilePropertyDefinition definition)
            {
                switch (definition.PropertyName.ToLowerInvariant())
                {
                    case "lastname":
                    case "firstname":
                    case "preferredtimezone":
                    case "preferredlocale":
                        return false;
                    default:
                        return true;
                }
            }
            private static DNNLocalization.CultureDropDownTypes GetCultureDropDownType(int portalId)
            {
                DNNLocalization.CultureDropDownTypes displayType;
                string viewType = GetLanguageDisplayMode(portalId);
                switch (viewType)
                {
                    case "NATIVE":
                        displayType = DNNLocalization.CultureDropDownTypes.NativeName;
                        break;
                    case "ENGLISH":
                        displayType = DNNLocalization.CultureDropDownTypes.EnglishName;
                        break;
                    default:
                        displayType = DNNLocalization.CultureDropDownTypes.DisplayName;
                        break;
                }
                return displayType;
            }
            private static string GetLanguageDisplayMode(int portalId)
            {
                string viewTypePersonalizationKey = "LanguageDisplayMode:ViewType" + portalId;
                PersonalizationController personalizationController = new PersonalizationController();
                PersonalizationInfo personalization = personalizationController.LoadProfile(PortalSettings.Current.UserInfo.UserID, portalId);

                string viewType = Convert.ToString(personalization.Profile[viewTypePersonalizationKey]);
                return string.IsNullOrEmpty(viewType) ? "NATIVE" : viewType;
            }
            #endregion
        }
    }
}
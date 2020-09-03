using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.MemberProfile.Factories;
using Vanjaro.UXManager.Library.Common;
using DataCache = DotNetNuke.Common.Utilities.DataCache;

namespace Vanjaro.UXManager.Extensions.Menu.MemberProfile.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class MemberProfileController : UIEngineController
    {
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> parameters, PortalSettings portalSettings, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            switch (Identifier.ToLower())
            {
                case "memberprofile_settings":
                    {
                        dynamic profileSettings = Managers.MemberProfileManager.GetProfileSettings(portalSettings.PortalId).Data;
                        Settings.Add("ProfileSettings", new UIData { Name = "ProfileSettings", Options = profileSettings.Settings });
                        Settings.Add("ProfileDefaultVisibility", new UIData { Name = "ProfileDefaultVisibility", Options = profileSettings.UserVisibilityOptions, OptionsText = "label", OptionsValue = "value", Value = Convert.ToString(profileSettings.Settings.ProfileDefaultVisibility) });
                        return Settings.Values.ToList();
                    }
                case "memberprofile_memberprofile":
                    {
                        dynamic profileProperties = Managers.MemberProfileManager.GetProfileProperties(portalSettings.PortalId).Data;
                        string UsersUrl = string.Empty;
                        if (Library.Managers.MenuManager.GetURL().ToLower().Contains("guid=aadb4856-9f2d-44be-ac42-15bcca62df0b"))
                            UsersUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL().ToLower().Replace("guid=aadb4856-9f2d-44be-ac42-15bcca62df0b", "guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3").TrimEnd('&');
                        else
                            UsersUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL() + "mid=0&icp=true&guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3";
                        string MemberProfileUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL() + "mid=0&icp=true&guid=aadb4856-9f2d-44be-ac42-15bcca62df0b";
                        Settings.Add("UsersUrl", new UIData { Name = "UsersUrl", Value = UsersUrl });
                        Settings.Add("MemberProfileUrl", new UIData { Name = "MemberProfileUrl", Value = MemberProfileUrl });
                        Settings.Add("MemberProfile", new UIData { Name = "MemberProfile", Options = profileProperties.Properties, Value = Convert.ToString(profileProperties.TotalResult) });
                        Settings.Add("TemplateProfilePropertyOrders", new UIData { Name = "TemplateProfilePropertyOrders", Options = new UpdateProfilePropertyOrdersRequest() { PortalId = portalSettings.PortalId } });
                        return Settings.Values.ToList();
                    }
                case "memberprofile_memberprofilesettings":
                    {
                        int? mpid = null;
                        if (parameters.Count > 0)
                        {
                            mpid = int.Parse(parameters["mpid"]);
                        }

                        dynamic profileProperty = Managers.MemberProfileManager.GetProfileProperty(mpid, portalSettings.PortalId).Data;
                        dynamic ListInfo = mpid.HasValue && mpid > 0 ? Managers.MemberProfileManager.GetListInfo(profileProperty.ProfileProperty.PropertyName, portalSettings.PortalId).Data.Entries : null;
                        Settings.Add("ProfileProperty", new UIData { Name = "ProfileProperty", Options = profileProperty });
                        Settings.Add("DefaultVisibility", new UIData { Name = "DefaultVisibility", Options = profileProperty.UserVisibilityOptions, OptionsText = "label", OptionsValue = "value", Value = Convert.ToString(profileProperty.ProfileProperty.DefaultVisibility) });
                        Settings.Add("Languages", new UIData { Name = "Languages", Options = profileProperty.LanguageOptions, OptionsText = "Text", OptionsValue = "Value", Value = PortalSettings.Current.CultureCode });
                        Settings.Add("ProfilePropertyLocalization", new UIData { Name = "ProfilePropertyLocalization", Options = new UpdateProfilePropertyLocalizationRequest() { Language = PortalSettings.Current.CultureCode } });
                        Settings.Add("Entries", new UIData { Name = "Entries", Options = ListInfo });
                        Settings.Add("ListEntryRequest", new UIData { Name = "ListEntryRequest", Options = new UpdateListEntryRequest() { PortalId = PortalSettings.Current.PortalId } });
                        Settings.Add("TemplateListEntryRequest", new UIData { Name = "TemplateListEntryRequest", Options = new UpdateListEntryRequest() { PortalId = PortalSettings.Current.PortalId, EnableSortOrder = true } });
                        Settings.Add("TemplateListEntryOrdersRequest", new UIData { Name = "TemplateListEntryOrdersRequest", Options = new UpdateListEntryOrdersRequest() { PortalId = portalSettings.PortalId } });
                        return Settings.Values.ToList();
                    }
                default:
                    return null;
            }
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult AddUpdateMemberProfile(UpdateProfilePropertyRequest request)
        {
            ActionResult actionResult = new ActionResult();
            if (request != null)
            {
                if (request.PropertyDefinitionId > 0)
                {
                    actionResult = Managers.MemberProfileManager.UpdateProfileProperty(request);
                }
                else
                {
                    actionResult = Managers.MemberProfileManager.AddProfileProperty(request);
                }
            }
            return actionResult;
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult DeleteProfileProperty(int propertyId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                if (actionResult.IsSuccess)
                {
                    ProfilePropertyDefinition propertyDefinition = ProfileController.GetPropertyDefinition(propertyId, pid);

                    if (!CanDeleteProperty(propertyDefinition))
                    {
                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), "ForbiddenDelete");
                    }
                    if (actionResult.IsSuccess)
                    {
                        ProfileController.DeletePropertyDefinition(propertyDefinition);
                        DataCache.ClearDefinitionsCache(pid);
                        actionResult.Data = new { MemberProfile = Managers.MemberProfileManager.GetProfileProperties(pid).Data.Properties };
                    }
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        [HttpGet]
        public ActionResult GetProfilePropertyLocalization(string cultureCode, string propertyName, string propertyCategory)
        {
            return Managers.MemberProfileManager.GetProfilePropertyLocalization(cultureCode, propertyName, propertyCategory);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateProfilePropertyLocalization(UpdateProfilePropertyLocalizationRequest request)
        {
            return Managers.MemberProfileManager.UpdateProfilePropertyLocalization(request);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateProfileSettings(UpdateProfileSettingsRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = request.PortalId ?? PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                if (actionResult.IsSuccess)
                {
                    if (Config.GetFriendlyUrlProvider() == "advanced")
                    {
                        PortalController.UpdatePortalSetting(pid, FriendlyUrlSettings.RedirectOldProfileUrlSetting, request.RedirectOldProfileUrl ? "Y" : "N", true);
                    }
                    PortalController.UpdatePortalSetting(pid, FriendlyUrlSettings.VanityUrlPrefixSetting, request.VanityUrlPrefix, false);
                    PortalController.UpdatePortalSetting(pid, "Profile_DefaultVisibility", request.ProfileDefaultVisibility.ToString(), false);
                    PortalController.UpdatePortalSetting(pid, "Profile_DisplayVisibility", request.ProfileDisplayVisibility.ToString(), false);

                    DataCache.ClearPortalCache(pid, false);
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateProfilePropertyOrders(UpdateProfilePropertyOrdersRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                if (request != null)
                {
                    int pid = request.PortalId ?? PortalSettings.PortalId;
                    if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        for (int i = 0; i <= request.Properties.Length - 1; i++)
                        {
                            var profileProperty = ProfileController.GetPropertyDefinition(request.Properties[i].PropertyDefinitionId.Value, pid);
                            string ControlType = Managers.MemberProfileManager.GetControlType(profileProperty.DataType);
                            if (profileProperty.PropertyValue == "-1" && (ControlType == "Country" || ControlType == "Region" || ControlType == "List"))
                                profileProperty.PropertyValue = "";
                            if (profileProperty.ViewOrder != request.Properties[i].ViewOrder)
                            {
                                profileProperty.ViewOrder = request.Properties[i].ViewOrder;
                                ProfileController.UpdatePropertyDefinition(profileProperty);
                            }
                        }
                        DataCache.ClearDefinitionsCache(pid);
                    }
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateListEntry(UpdateListEntryRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = request.PortalId ?? PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalSettings.PortalId)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                if (actionResult.IsSuccess)
                {
                    ListController listController = new ListController();
                    ListEntryInfo entry = new ListEntryInfo
                    {
                        DefinitionID = Null.NullInteger,
                        PortalID = pid,
                        ListName = request.ListName,
                        Value = request.Value,
                        Text = request.Text,
                        SortOrder = request.EnableSortOrder ? 1 : 0
                    };

                    if (request.EntryId.HasValue)
                    {
                        entry.EntryID = request.EntryId.Value;
                        listController.UpdateListEntry(entry);
                    }
                    else
                    {
                        listController.AddListEntry(entry);
                    }
                    actionResult.Data = new { Managers.MemberProfileManager.GetListInfo(request.ListName, PortalSettings.PortalId).Data.Entries };
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult DeleteListEntry(int entryId, string propertyName)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalSettings.PortalId)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                if (actionResult.IsSuccess)
                {
                    ListController listController = new ListController();
                    listController.DeleteListEntryByID(entryId, true);
                    actionResult.Data = new { Managers.MemberProfileManager.GetListInfo(propertyName, PortalSettings.PortalId).Data.Entries };
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateListEntryOrders(UpdateListEntryOrdersRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = request.PortalId ?? PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                if (actionResult.IsSuccess)
                {
                    ListController listController = new ListController();
                    for (int i = 0; i <= request.Entries.Length - 1; i++)
                    {
                        if (request.Entries[i].EntryId.HasValue)
                        {
                            ListEntryInfo entry = listController.GetListEntryInfo(request.Entries[i].EntryId.Value);
                            if (entry.SortOrder != request.Entries[i].SortOrder)
                            {
                                entry.SortOrder = request.Entries[i].SortOrder.Value;
                                Managers.MemberProfileManager.UpdateSortOrder(entry);
                                string cacheKey = string.Format(DataCache.ListEntriesCacheKey, pid, entry.ListName);
                                DataCache.RemoveCache(cacheKey);
                            }
                        }
                    }
                    DataCache.ClearListsCache(pid);
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }
        private bool CanDeleteProperty(ProfilePropertyDefinition definition)
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
        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
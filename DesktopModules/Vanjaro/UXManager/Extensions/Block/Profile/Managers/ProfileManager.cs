using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Block.Profile
{
    public static partial class Managers
    {
        public class ProfileManager
        {
            public static string[] ExcludeControls = { "AutoComplete", "Checkbox ", "Page", "Image", "DateTime", "TimeZoneInfo", "Unknown" };
            public static string[] CannotDeleteProperty = { "Photo" };
            public static bool CheckAccessLevel(ProfilePropertyDefinition property, UserInfo targetUser)
            {
                var isAdminUser = IsAdminUser(PortalSettings.Current, PortalSettings.Current.UserInfo, targetUser);

                //Use properties visible property but admins and hosts can always see the property
                var isVisible = property.Visible || isAdminUser;

                //if (isVisible && !isAdminUser)
                if (isVisible)
                {
                    switch (property.DefaultVisibility)
                    {
                        case UserVisibilityMode.AllUsers:
                            // property is visible to everyone so do nothing
                            break;
                        case UserVisibilityMode.MembersOnly:
                            // property visible if accessing user is a member
                            isVisible = IsMember(PortalSettings.Current.UserInfo);
                            break;
                        case UserVisibilityMode.AdminOnly:
                            //accessing user not admin user so property is hidden (unless it is the user him/herself)
                            isVisible = IsUser(PortalSettings.Current.UserInfo, targetUser);
                            break;
                    }
                }

                return isVisible;
            }

            public static List<Entities.ProfileProperties> GetProfileFields(UserInfo userInfo)
            {
                List<Entities.ProfileProperties> profileProperties = new List<Entities.ProfileProperties>();
                foreach (ProfilePropertyDefinition d in userInfo.Profile.ProfileProperties)
                {
                    if (Managers.ProfileManager.CheckAccessLevel(d, userInfo))
                    {
                        profileProperties.Add(new Entities.ProfileProperties { ProfilePropertyDefinition = d });
                    }
                }
                return profileProperties.Where(x => !CannotDeleteProperty.Contains(x.ProfilePropertyDefinition.PropertyName)).ToList();
            }

            public static Dictionary<string, string> GetLocalizedCategories(ProfilePropertyDefinitionCollection ProfileProperties, UserInfo userInfo)
            {
                Dictionary<string, string> CategoriesKeyValue = new Dictionary<string, string>();
                List<string> PropertiesByCategories = ProfileProperties.Cast<ProfilePropertyDefinition>().Where(x => CheckAccessLevel(x, userInfo)).Select(x => x.PropertyCategory).Distinct().ToList();
                foreach (string Category in PropertiesByCategories)
                {
                    var localizedCategoryName = Localization.GetString("ProfileProperties_" + Category + ".Header", Components.Constants.DnnUserProfileResourcesFile, PortalSettings.Current.CultureCode);
                    if (string.IsNullOrEmpty(localizedCategoryName))
                        localizedCategoryName = Category;
                    CategoriesKeyValue.Add(Category, localizedCategoryName);
                }

                return CategoriesKeyValue;
            }

            public static string GetControlType(int DataTypeID)
            {
                string _ControlType = string.Empty;
                ListController listController = new ListController();
                List<ListEntryInfo> ListDataTypeInfo = listController.GetListEntryInfoItems("DataType", "", PortalSettings.Current.PortalId).ToList();
                foreach (ListEntryInfo dInfo in ListDataTypeInfo)
                {
                    if (dInfo.EntryID == DataTypeID)
                    {
                        _ControlType = dInfo.Value;
                    }
                }

                return _ControlType;
            }

            #region Private Members
            internal static bool IsExistsDataType(int dataType)
            {
                bool IsExistsDataType = false;
                ListController listController = new ListController();
                foreach (var d in listController.GetListEntryInfoItems("DataType").Where(x => !ExcludeControls.Contains(x.Value)).ToList())
                {
                    if (d.EntryID == dataType)
                    {
                        IsExistsDataType = true;
                        break;
                    }
                }
                return IsExistsDataType;
            }
            private static bool IsAdminUser(PortalSettings portalSettings, UserInfo accessingUser, UserInfo targetUser)
            {
                bool isAdmin = false;

                if (accessingUser != null)
                {
                    //Is Super User?
                    isAdmin = accessingUser.IsSuperUser;

                    if (!isAdmin && targetUser.PortalID != -1)
                    {
                        //Is Administrator
                        var administratorRoleName = portalSettings != null
                            ? portalSettings.AdministratorRoleName
                            : PortalController.Instance.GetPortal(targetUser.PortalID).AdministratorRoleName;

                        isAdmin = accessingUser.IsInRole(administratorRoleName);
                    }
                }

                return isAdmin;
            }

            private static bool IsMember(UserInfo accessingUser)
            {
                return (accessingUser != null && accessingUser.UserID != -1);
            }

            private static bool IsUser(UserInfo accessingUser, UserInfo targetUser)
            {
                return (accessingUser != null && (targetUser != null && accessingUser.IsInRole(PortalSettings.Current.AdministratorRoleName) && targetUser.IsInRole(PortalSettings.Current.AdministratorRoleName)) || accessingUser.IsInRole(PortalSettings.Current.AdministratorRoleName));
            }
            #endregion
        }
    }
}
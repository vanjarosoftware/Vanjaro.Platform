using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using static DotNetNuke.Web.InternalServices.CountryRegionController;

namespace Vanjaro.UXManager.Extensions.Menu.Users
{
    public static partial class Managers
    {
        public class UserManager
        {
            public static string[] CannotDeleteProperty = { "Photo" };
            public static void CreateThumbnails(int fileId)
            {
                CreateThumbnail(fileId, "l", 64, 64);
                CreateThumbnail(fileId, "s", 50, 50);
                CreateThumbnail(fileId, "xs", 32, 32);
            }

            public static void CreateThumbnail(int fileId, string type, int width, int height)
            {
                IFileInfo file = FileManager.Instance.GetFile(fileId);
                if (file != null)
                {
                    IFolderInfo folder = FolderManager.Instance.GetFolder(file.FolderId);
                    string extension = "." + file.Extension;
                    string sizedPhoto = file.FileName.Replace(extension, "_" + type + extension);
                    if (!FileManager.Instance.FileExists(folder, sizedPhoto))
                    {
                        using (System.IO.Stream content = FileManager.Instance.GetFileContent(file))
                        {
                            System.IO.Stream sizedContent = ImageUtils.CreateImage(content, height, width, extension);

                            FileManager.Instance.AddFile(folder, sizedPhoto, sizedContent);
                        }
                    }
                }
            }

            public static UserBasicInfo MapUserBasicDto(UserInfo uInfo, UserBasicDto d)
            {
                IFolderInfo userFolder = FolderManager.Instance.GetUserFolder(uInfo);
                UserBasicInfo info = new UserBasicInfo()
                {
                    userId = d.UserId,
                    displayName = d.Displayname,
                    userName = d.Username,
                    email = d.Email,
                    avatar = uInfo.Profile.PhotoURL.Contains("no_avatar.gif") ? Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, d.UserId, d.Email) : d.AvatarUrl,
                    firstName = d.Firstname,
                    lastName = d.Lastname,
                    createdOnDate = d.CreatedOnDate,
                    isDeleted = d.IsDeleted,
                    authorized = uInfo.Membership.Approved,
                    isSuperUser = d.IsSuperUser,
                    isAdmin = d.IsAdmin,
                    lastLogin = uInfo.Membership.LastLoginDate,
                    lastActivity = uInfo.Membership.LastActivityDate,
                    lastPasswordChange = uInfo.Membership.LastPasswordChangeDate,
                    lastLockout = uInfo.Membership.LastLockoutDate,
                    isLocked = uInfo.Membership.LockedOut,
                    needUpdatePassword = uInfo.Membership.UpdatePassword,
                    portalId = uInfo.PortalID,
                    userFolder = FolderManager.Instance.GetUserFolder(uInfo).FolderPath.Substring(6),
                    userFolderId = userFolder != null ? userFolder.FolderID : -1,
                    hasUserFiles = FolderManager.Instance.GetFiles(userFolder, true).Any()
                };
                return info;
            }

            public static List<Region> Regions(int country)
            {
                List<Region> res = new List<Region>();
                if (country > 0)
                {
                    foreach (ListEntryInfo r in (new ListController()).GetListEntryInfoItems("Region").Where(l => l.ParentID == country))
                    {
                        res.Add(new Region
                        {
                            Text = r.Text,
                            Value = r.EntryID.ToString()
                        });
                    }
                }
                List<Region> regions = res.OrderBy(r => r.Text).ToList();
                regions.Insert(0, new Region { Text = Localization.GetString("NotSpecified", Components.Constants.LocalResourcesFile), Value = "-1" });
                return regions;
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
            public static bool CheckAccessLevel(ProfilePropertyDefinition property, UserInfo targetUser)
            {
                var isAdminUser = IsAdminUser(PortalSettings.Current, PortalSettings.Current.UserInfo, targetUser);

                //Use properties visible property but admins and hosts can always see the property
                var isVisible = property.Visible || isAdminUser;

                //if (isVisible && !isAdminUser)
                //if (isVisible)
                //{
                switch (property.DefaultVisibility)
                {
                    case UserVisibilityMode.AllUsers:
                        isVisible = true;
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
                //}

                return isVisible;
            }
            public static Dictionary<string, string> GetLocalizedCategories(ProfilePropertyDefinitionCollection ProfileProperties, UserInfo userInfo, bool isUpdateProfile = false)
            {
                Dictionary<string, string> CategoriesKeyValue = new Dictionary<string, string>();
                List<string> PropertiesByCategories = new List<string>();
                if (isUpdateProfile)
                    PropertiesByCategories = ProfileProperties.Cast<ProfilePropertyDefinition>().Where(x => !CannotDeleteProperty.Contains(x.PropertyName)).Select(x => x.PropertyCategory).Distinct().ToList();
                else
                    PropertiesByCategories = ProfileProperties.Cast<ProfilePropertyDefinition>().Where(x => CheckAccessLevel(x, userInfo) && !CannotDeleteProperty.Contains(x.PropertyName)).Select(x => x.PropertyCategory).Distinct().ToList();
                foreach (string Category in PropertiesByCategories)
                {
                    var localizedCategoryName = Localization.GetString("ProfileProperties_" + Category + ".Header", Components.Constants.DnnUserProfileResourcesFile, PortalSettings.Current.CultureCode);
                    if (string.IsNullOrEmpty(localizedCategoryName))
                        localizedCategoryName = Category;
                    CategoriesKeyValue.Add(Category, localizedCategoryName);
                }

                return CategoriesKeyValue;
            }

            #region Recycle Users && Deleted Users.
            public static Entities.UserItem ConvertToUserItem(UserInfo user)
            {
                return new Entities.UserItem
                {
                    UserId = user.UserID,
                    Username = user.Username,
                    PortalId = user.PortalID,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Avatar = user.Profile.PhotoURL.Contains("no_avatar.gif") ? Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, user.UserID, user.Email) : Utilities.GetProfileAvatar(user.UserID),
                    LastModifiedOnDate = user.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt"),
                    FriendlyLastModifiedOnDate = user.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt")
                };
            }

            public static List<Dnn.PersonaBar.Recyclebin.Components.Dto.UserItem> ConvertToDNNUserItem(List<Entities.UserItem> users)
            {
                List<Dnn.PersonaBar.Recyclebin.Components.Dto.UserItem> data = new List<Dnn.PersonaBar.Recyclebin.Components.Dto.UserItem>();
                foreach (Entities.UserItem u in users)
                {
                    data.Add(ConvertToDNNUserItem(u));
                }
                return data;
            }

            public static Dnn.PersonaBar.Recyclebin.Components.Dto.UserItem ConvertToDNNUserItem(Entities.UserItem user)
            {
                return new Dnn.PersonaBar.Recyclebin.Components.Dto.UserItem
                {
                    Id = user.UserId,
                    Username = user.Username,
                    PortalId = user.PortalId,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    LastModifiedOnDate = user.LastModifiedOnDate,
                    FriendlyLastModifiedOnDate = user.FriendlyLastModifiedOnDate
                };
            }
            #endregion
            public static void AddProfileProperties(ref UserInfo user, UserInfo UserInfo, ref List<Entities.ProfileProperties> profileProperties, ref KeyValuePair<HttpStatusCode, string> response)
            {
                if (profileProperties != null && profileProperties.Where(x => x.ProfilePropertyDefinition.PropertyName == "Photo").FirstOrDefault() == null)
                {
                    using (Core.Data.Entities.VanjaroRepo vrepo = new Core.Data.Entities.VanjaroRepo())
                    {
                        vrepo.Execute("INSERT [dbo].[ProfilePropertyDefinition] ([PortalID], [ModuleDefID], [Deleted], [DataType], [DefaultValue], [PropertyCategory], [PropertyName], [Length], [Required], [ValidationExpression], [ViewOrder], [Visible], [CreatedByUserID], [CreatedOnDate], [LastModifiedByUserID], [LastModifiedOnDate], [DefaultVisibility], [ReadOnly]) VALUES (NULL, -1, 0, 361, N'', N'Preferences', N'Photo', 0, 0, NULL, 42, 1, -1, CAST(N'2000-01-01T00:00:00.000' AS DateTime), NULL, NULL, 0, 0)");
                    }
                    if (user.UserID == PortalSettings.Current.AdministratorId)
                    {
                        //Clear the Portal Cache
                        DataCache.ClearPortalCache(PortalSettings.Current.PortalId, true);
                    }
                    DataCache.ClearHostCache(true);

                    user = UsersController.GetUser(user.UserID, PortalSettings.Current, UserInfo, out response);
                    ProfilePropertyDefinition profilePropertyDefinition = user.Profile.ProfileProperties.GetByName("Photo");
                    if (profilePropertyDefinition != null)
                        profileProperties.Add(new Entities.ProfileProperties { ProfilePropertyDefinition = profilePropertyDefinition, ListEntries = new List<ListEntryInfo>() });
                }
            }

            #region Private Members
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
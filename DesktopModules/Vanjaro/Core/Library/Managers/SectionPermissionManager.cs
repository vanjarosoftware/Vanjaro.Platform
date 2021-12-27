using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Vanjaro.Common.Permissions;
using Vanjaro.Core.Data.Entities;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class SectionPermissionManager
        {
            public static Dictionary<string, dynamic> GetPermissions(int EntityID)
            {
                Dictionary<string, dynamic> permData = new Dictionary<string, dynamic>();
                Permissions Permissions = new Permissions();
                int InheritPermissionID = -1;
                List<Permission> PermissionDefinitions = new List<Permission>();
                foreach (DNNModulePermissionInfo p in Common.Manager.PermissionManager.GetPermissionInfo("SYSTEM_MODULE_DEFINITION").Where(p => p.PermissionKey == "VIEW"))
                {
                    PermissionDefinitions.Add(AddPermissionDefinitions(p.PermissionKey, p.PermissionID));
                    if (p.PermissionKey.ToLower() == "view")
                    {
                        InheritPermissionID = p.PermissionID;
                    }
                }

                bool Inherit = true;
                BlockSection blockSection = GetBlockSection(EntityID);
                if (blockSection != null && blockSection.Inherit.HasValue)
                    Inherit = blockSection.Inherit.Value;

                Permissions _permission = GetSectionPermission(EntityID);
                _permission.PermissionDefinitions = PermissionDefinitions;
                Permissions = _permission;
                Permissions.Inherit = Inherit;
                Permissions.ShowInheritCheckBox = true;
                Permissions.InheritPermissionID = InheritPermissionID;
                permData.Add("Permissions", Permissions);
                return permData;
            }

            public static BlockSection GetBlockSection(int EntityID)
            {
                return SectionPermissionFactory.GetBlockSection(EntityID);
            }

            public static void Delete(int EntityID)
            {
                SectionPermissionFactory.Delete(EntityID);
            }

            public static int AddBlockSection(int TabID, bool? Inherit)
            {
                return SectionPermissionFactory.AddBlockSection(TabID, Inherit);
            }

            public static void UpdateInherit(int EntityID, bool? Inherit)
            {
                SectionPermissionFactory.UpdateInherit(EntityID, Inherit);
            }

            public static void Update(int EntityID, dynamic Data)
            {
                List<GenericPermissionInfo> Permissions = new List<GenericPermissionInfo>();
                foreach (dynamic item in Data.PermissionsRoles)
                {
                    string PermissionID = "";
                    foreach (dynamic p in item.Permissions)
                    {
                        if (bool.Parse(p.AllowAccess.ToString()))
                        {
                            PermissionID = p.PermissionId.ToString();
                            if (!string.IsNullOrEmpty(PermissionID))
                            {
                                Permissions.Add(new GenericPermissionInfo { AllowAccess = true, RoleID = int.Parse(item.RoleId.ToString()), PermissionID = int.Parse(PermissionID) });
                            }
                        }
                    }
                }
                foreach (dynamic item in Data.PermissionsUsers)
                {
                    string PermissionID = "";
                    foreach (dynamic p in item.Permissions)
                    {
                        if (bool.Parse(p.AllowAccess.ToString()))
                        {
                            PermissionID = p.PermissionId.ToString();
                            if (!string.IsNullOrEmpty(PermissionID))
                            {
                                Permissions.Add(new GenericPermissionInfo { AllowAccess = true, UserID = int.Parse(item.UserId.ToString()), PermissionID = int.Parse(PermissionID) });
                            }
                        }
                    }

                }
                SectionPermissionFactory.ClearAllPermissions(EntityID);
                SectionPermissionFactory.UpdatePermissions(GetSectionPermissions(EntityID, Permissions));
            }
            public static List<BlockSectionPermission> GetSectionPermissions(int EntityID, List<GenericPermissionInfo> GenericPermissions)
            {
                List<BlockSectionPermission> Permissions = new List<BlockSectionPermission>();
                foreach (GenericPermissionInfo gp in GenericPermissions)
                {
                    if (gp.RoleID != -4 || gp.UserID != -4)
                    {
                        BlockSectionPermission ws = new BlockSectionPermission
                        {
                            AllowAccess = gp.AllowAccess,
                            EntityID = EntityID,
                            CreatedOn = gp.CreatedOnDate,
                            RoleID = gp.RoleID,
                            UpdatedBy = gp.LastModifiedByUserID,
                            PermissionID = gp.PermissionID
                        };

                        if (gp.CreatedOnDate == DateTime.MinValue)
                        {
                            ws.CreatedOn = DateTime.Now;
                        }
                        else
                        {
                            ws.CreatedBy = gp.CreatedByUserID;
                        }

                        if (gp.LastModifiedOnDate == DateTime.MinValue)
                        {
                            ws.UpdatedOn = null;
                        }
                        else
                        {
                            ws.UpdatedOn = gp.LastModifiedOnDate;
                        }

                        if (gp.UserID < 0)
                        {
                            ws.UserID = null;
                        }
                        else
                        {
                            ws.RoleID = null;
                            ws.UserID = gp.UserID;
                        }
                        Permissions.Add(ws);
                    }
                }
                return Permissions;
            }
            private static Permission AddPermissionDefinitions(string PermissionKey, int PermissionID)
            {
                Permission Permission = new Permission
                {
                    AllowAccess = PermissionKey.ToLower() == "view" ? true : false,
                    PermissionName = PermissionKey,
                    PermissionId = PermissionID
                };
                return Permission;
            }
            public static Permissions GetSectionPermission(int EntityID)
            {
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();
                Permissions Permissions = new Permissions(true, false);
                DNNModulePermissionInfo DnnPermissionInfo = Common.Manager.PermissionManager.GetPermissionInfo("SYSTEM_MODULE_DEFINITION").Where(p => p.PermissionKey == "VIEW").FirstOrDefault();
                if (DnnPermissionInfo != null)
                {
                    foreach (GenericPermissionInfo perm in GetGenericPermissions(GetPermissionsByEntityID(EntityID)))
                    {
                        foreach (SectionPermissionInfo p in GetPermissionByCode("SYSTEM_MODULE_DEFINITION"))
                        {
                            if (p.PermissionID == perm.PermissionID)
                            {
                                perm.PermissionName = p.PermissionKey;
                            }
                        }
                        if (perm.UserID == -1 && perm.RoleID != -4)
                        {
                            if (perm.RoleID == -3)
                            {
                                perm.RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName;
                            }
                            else if (perm.RoleID != -1)
                            {
                                perm.RoleName = RoleController.Instance.GetRoleById(UserInfo.PortalID, perm.RoleID).RoleName;
                            }
                            Common.Manager.PermissionManager.AddRolePermission(Permissions, perm);
                        }
                        else if (perm.UserID != -1 && perm.RoleID == -4)
                        {
                            Common.Manager.PermissionManager.AddUserPermission(Permissions, perm);
                        }
                    }
                    GenericPermissionInfo Permission = new GenericPermissionInfo();
                    RoleInfo AdministratorRole = RoleController.Instance.GetRoleById(UserInfo.PortalID, PortalSettings.Current.AdministratorRoleId);
                    Permission.RoleID = AdministratorRole.RoleID;
                    Permission.RoleName = AdministratorRole.RoleName;
                    Permission.PermissionName = DnnPermissionInfo.PermissionKey;
                    Permission.PermissionID = DnnPermissionInfo.PermissionID;
                    Permission.AllowAccess = true;
                    Common.Manager.PermissionManager.AddRolePermission(Permissions, Permission);
                }
                return Permissions;
            }
            public static List<BlockSectionPermission> GetPermissionsByEntityID(int EntityID)
            {
                return SectionPermissionFactory.GetPermissionsByEntityID(EntityID);
            }
            public static List<SectionPermissionInfo> GetPermissionByCode(string Code)
            {
                return SectionPermissionFactory.GetPermissionByCode(Code);
            }
            public static List<GenericPermissionInfo> GetGenericPermissions(List<BlockSectionPermission> Permissions)
            {
                List<GenericPermissionInfo> GenericPermissions = new List<GenericPermissionInfo>();
                foreach (BlockSectionPermission wp in Permissions)
                {
                    GenericPermissionInfo gp = new GenericPermissionInfo
                    {
                        AllowAccess = wp.AllowAccess,
                        RoleID = wp.RoleID.HasValue ? wp.RoleID.Value : -4,
                        UserID = wp.UserID.HasValue ? wp.UserID.Value : -1,
                        PermissionID = wp.PermissionID
                    };
                    if (gp.UserID > -1)
                    {
                        UserInfo UserInfo = UserController.GetUserById(PortalSettings.Current.PortalId, gp.UserID);
                        if (UserInfo != null)
                        {
                            gp.DisplayName = UserInfo.DisplayName;
                        }
                    }
                    GenericPermissions.Add(gp);
                }
                return GenericPermissions;
            }
            public static bool HasViewPermission(int EntityID)
            {
                return (CheckPermissionsWithPermissionKey(EntityID, "VIEW", "SYSTEM_MODULE_DEFINITION"));
            }
            private static bool CheckPermissionsWithPermissionKey(int EntityID, string PermissionKey, string PermissionCode)
            {
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.SectionPermission + "CheckPermissionsWithPermissionKey", EntityID, PermissionKey, PermissionCode, userInfo.UserID);
                string result = CacheFactory.Get(CacheKey);
                if (string.IsNullOrEmpty(result))
                {
                    PermissionController permissionController = new PermissionController();
                    ArrayList arrPermissions = permissionController.GetPermissionByCodeAndKey(PermissionCode, PermissionKey);
                    if (arrPermissions.Count > 0)
                    {
                        PermissionInfo pInfo = arrPermissions[0] as PermissionInfo;
                        if (pInfo != null)
                            result = CheckPermissions(EntityID, userInfo).ToString();
                        else
                            result = "false";
                    }
                    else
                        result = "false";
                    CacheFactory.Set(CacheKey, result);
                }
                return bool.Parse(result);
            }
            private static bool CheckPermissions(int EntityID, UserInfo userInfo)
            {
                StringBuilder sb = new StringBuilder();
                List<BlockSectionPermission> Permissions = GetPermissionsByEntityID(EntityID);
                foreach (BlockSectionPermission vPerm in Permissions)
                {
                    string temp = null;
                    string temp2 = null;

                    if (!vPerm.AllowAccess)
                    {
                        temp = "!";
                    }
                    else
                    {
                        temp = "";
                    }

                    if (vPerm.UserID.HasValue && vPerm.UserID.Value > -1)
                    {
                        temp2 = string.Concat(new object[] { temp, "[", vPerm.UserID, "];" });
                    }
                    else if (vPerm.RoleID.HasValue && vPerm.RoleID.Value > -1)
                    {
                        if (PortalSettings.Current != null)
                        {

                            RoleInfo rInfo = new RoleController().GetRoleById(PortalSettings.Current.PortalId, vPerm.RoleID.Value);
                            if (rInfo != null)
                            {
                                temp2 = temp + rInfo.RoleName + ";";
                            }
                        }
                    }
                    else
                    {
                        temp2 = string.Concat(new object[] { temp, "[", vPerm.RoleID, "];" });
                    }

                    if (temp == "!")
                    {
                        sb.Insert(0, temp2);
                    }
                    else
                    {
                        sb.Append(temp2);
                    }
                }

                string temp3 = sb.ToString();
                if (!temp3.StartsWith(";"))
                {
                    temp3.Insert(0, ";");
                }

                if (userInfo == null)
                {
                    if (PortalSecurity.IsInRoles("Administrators"))
                    {
                        return true;
                    }
                    else
                    {
                        return PortalSecurity.IsInRoles(temp3);
                    }
                }
                else
                {
                    return IsInRoles(temp3, userInfo);
                }
            }
            private static bool IsInRoles(string roles, UserInfo currentUserInfo)
            {
                bool isSuperUser = currentUserInfo.IsSuperUser || currentUserInfo.IsInRole("Administrators");
                if (!isSuperUser && (roles != null))
                {
                    HttpContext current = HttpContext.Current;
                    string[] strArray = roles.Split(new char[] { ';' });
                    for (int i = 0; i < strArray.Length; i = i + 1)
                    {
                        string role = strArray[i];
                        if (!string.IsNullOrEmpty(role))
                        {
                            if (role.StartsWith("!"))
                            {
                                PortalSettings currentPortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                                if ((currentPortalSettings.PortalId != currentUserInfo.PortalID) || (currentPortalSettings.AdministratorId != currentUserInfo.UserID))
                                {
                                    string str2 = role.Replace("!", "");
                                    if ((!current.Request.IsAuthenticated && (str2 == "[-3]")) || ((str2 == "[-1]") || currentUserInfo.IsInRole(str2)))
                                    {
                                        return false;
                                    }
                                }
                            }
                            else if ((!current.Request.IsAuthenticated && (role == "[-3]")) || ((role == "[-1]") || currentUserInfo.IsInRole(role)))
                            {
                                return true;
                            }
                        }
                    }
                }
                return isSuperUser;
            }
        }
    }
}
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Permissions;

namespace Vanjaro.Common.Manager
{
    public static class PermissionManager
    {
        public static void EnsureDefaultRoles(this Permissions.Permissions Permissions, bool Locked)
        {
            Permissions.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorRoleId), Locked == true ? true : true, true);
            Permissions.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.RegisteredRoleId), Locked == true ? true : false, true);
            Permissions.EnsureRole(new RoleInfo { RoleID = int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers), RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName }, Locked == true ? true : false, true);
        }

        public static void EnsureRole(this Permissions.Permissions Permissions, RoleInfo role, bool locked, bool isDefault)
        {
            if (Permissions.RolePermissions.All(r => r.RoleId != role.RoleID))
            {
                Permissions.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.RoleID,
                    RoleName = role.RoleName,
                    Locked = locked,
                    IsDefault = isDefault
                });
            }
        }

        public static void AddRolePermission(this Permissions.Permissions Permissions, GenericPermissionInfo permissionInfo)
        {
            RolePermission rolePermission = Permissions.RolePermissions.FirstOrDefault(p => p.RoleId == permissionInfo.RoleID);
            if (rolePermission == null)
            {
                rolePermission = new RolePermission
                {
                    RoleId = permissionInfo.RoleID,
                    RoleName = permissionInfo.RoleName
                };
                Permissions.RolePermissions.Add(rolePermission);
            }

            if (rolePermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionID))
            {
                rolePermission.Permissions.Add(new Permission
                {
                    PermissionId = permissionInfo.PermissionID,
                    PermissionName = permissionInfo.PermissionName,
                    AllowAccess = permissionInfo.AllowAccess
                });
            }
        }

        public static void AddUserPermission(this Permissions.Permissions Permissions, PermissionInfoBase permissionInfo)
        {
            UserPermission userPermission = Permissions.UserPermissions.FirstOrDefault(p => p.UserId == permissionInfo.UserID);
            if (userPermission == null)
            {
                UserInfo uinfo = UserController.GetUserById(PortalSettings.Current.PortalId, permissionInfo.UserID);
                string Email = string.Empty;
                string UserName = string.Empty;
                string AvatarUrl = string.Empty;
                if (uinfo != null)
                {
                    Email = uinfo.Email;
                    UserName = uinfo.Username;
                    AvatarUrl = Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, permissionInfo.UserID, Email);
                }
                userPermission = new UserPermission
                {
                    UserId = permissionInfo.UserID,
                    DisplayName = permissionInfo.DisplayName,
                    Email = Email,
                    UserName = UserName,
                    AvatarUrl = AvatarUrl
                };
                Permissions.UserPermissions.Add(userPermission);
            }

            if (userPermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionID))
            {
                userPermission.Permissions.Add(new Permission
                {
                    PermissionId = permissionInfo.PermissionID,
                    PermissionName = permissionInfo.PermissionName,
                    AllowAccess = permissionInfo.AllowAccess
                });
            }
        }

        public static List<DNNModulePermissionInfo> GetPermissionInfo(string Code)
        {
            List<DNNModulePermissionInfo> DnnModulePermission = Data.Entities.CommonLibraryRepo.GetInstance().Query<DNNModulePermissionInfo>("SELECT p.* FROM " + Config.GetDataBaseOwner() + Config.GetObjectQualifer() + "Permission AS p WHERE p.PermissionCode = @0", Code).ToList();
            return DnnModulePermission;
        }

        public static List<DNNModulePermissionInfo> GetPermissionInfo(int ModuleDefID)
        {
            List<DNNModulePermissionInfo> DnnModulePermission = Data.Entities.CommonLibraryRepo.GetInstance().Query<DNNModulePermissionInfo>("SELECT p.* FROM " + Config.GetDataBaseOwner() + Config.GetObjectQualifer() + "Permission AS p WHERE p.ModuleDefID = @0", ModuleDefID).ToList();
            return DnnModulePermission;
        }
    }
}
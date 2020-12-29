using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Permissions;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Assets.Components;
using Vanjaro.UXManager.Library;

namespace Vanjaro.UXManager.Extensions.Menu.Assets.Managers
{
    public class PermissionManager
    {
        internal static Dictionary<string, dynamic> GetPermission(int PortalID, int FolderID)
        {
            Dictionary<string, dynamic> setting = new Dictionary<string, dynamic>();

            List<Permission> PermissionDefinitions = new List<Permission>();
            foreach (DNNModulePermissionInfo p in Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo(Components.Permissions.ModuleSecurity.SYSTEM_PERMISSION_CODE))
            {
                PermissionDefinitions.Add(AddPermissionDefinitions(Localization.Get(p.PermissionName, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix), p.PermissionID));
            }

            PermissionDefinitions = PermissionDefinitions.OrderBy(o => o.PermissionName).ToList();

            IFolderInfo parentFolder = FolderManager.Instance.GetFolder(FolderID);
            Permissions Permissions = new Permissions();
            if (parentFolder != null)
            {
                Permissions = GetAllPermission(false, PortalID, GetGenericPermissions(parentFolder.FolderPermissions), PermissionDefinitions);
                Permissions.Inherit = false;
                Permissions.ShowInheritCheckBox = false;
                Permissions.InheritPermissionID = -1;
            }
            setting.Add("Permissions", Permissions);
            return setting;
        }

        private static List<GenericPermissionInfo> GetGenericPermissions(FolderPermissionCollection FpCollection)
        {
            List<GenericPermissionInfo> GenericPermissions = new List<GenericPermissionInfo>();

            foreach (PermissionInfoBase pinfo in FpCollection.ToList())
            {
                GenericPermissionInfo gp = new GenericPermissionInfo
                {
                    AllowAccess = pinfo.AllowAccess,
                    PermissionID = pinfo.PermissionID,
                    RoleID = pinfo.RoleID,
                    UserID = pinfo.UserID,
                    DisplayName = pinfo.DisplayName
                };
                GenericPermissions.Add(gp);
            }
            return GenericPermissions;
        }

        private static Permissions GetAllPermission(bool Locked, int PortalID, List<GenericPermissionInfo> GenericPermissionInfo, List<Permission> PermissionDefinitions)
        {
            Permissions Permissions = new Permissions(true, Locked);
            Permissions.RolePermissions =
                        Permissions.RolePermissions.OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();

            foreach (GenericPermissionInfo perm in GenericPermissionInfo)
            {
                foreach (Permission p in PermissionDefinitions)
                {
                    if (p.PermissionId == perm.PermissionID)
                    {
                        perm.PermissionName = p.PermissionName;
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
                        perm.RoleName = RoleController.Instance.GetRoleById(PortalID, perm.RoleID).RoleName;
                    }

                    Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, perm);
                }
                else if (perm.UserID != -1 && perm.RoleID == -4)
                {
                    Vanjaro.Common.Manager.PermissionManager.AddUserPermission(Permissions, perm);
                }
            }
            int RoleID = RoleController.Instance.GetRoleById(PortalID, PortalSettings.Current.AdministratorRoleId).RoleID;
            string RoleName = RoleController.Instance.GetRoleById(PortalID, PortalSettings.Current.AdministratorRoleId).RoleName;
            foreach (Permission p in PermissionDefinitions)
            {
                GenericPermissionInfo Permission = new GenericPermissionInfo
                {
                    PermissionName = p.PermissionName,
                    PermissionID = p.PermissionId,
                    AllowAccess = true,
                    RoleID = RoleID,
                    RoleName = RoleName
                };
                Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, Permission);
            }
            Permissions.PermissionDefinitions = PermissionDefinitions;
            return Permissions;
        }

        private static Permission AddPermissionDefinitions(string PermissionKey, int PermissionID)
        {
            Permission Permission = new Permission
            {
                AllowAccess = true,
                PermissionName = PermissionKey,
                PermissionId = PermissionID
            };
            return Permission;
        }
    }
}
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using System.Collections.Generic;
using Vanjaro.Common.Permissions;

namespace Vanjaro.UXManager.Extensions.Apps.ModuleSettings.Managers
{
    public class SettingManager
    {
        public static Dictionary<string, dynamic> GetPermission(int ModuleID, int PortalID)
        {
            Dictionary<string, dynamic> permData = new Dictionary<string, dynamic>();
            ModuleInfo moduleinfo = ModuleController.Instance.GetModule(ModuleID, Null.NullInteger, false);
            if (moduleinfo != null)
            {
                int InheritPermissionID = -1;
                List<Permission> PermissionDefinitions = new List<Permission>();
                foreach (DNNModulePermissionInfo p in Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo(Components.Permissions.ModuleSecurity.SYSTEM_PERMISSION_CODE))
                {
                    PermissionDefinitions.Add(AddPermissionDefinitions(p.PermissionKey, p.PermissionID));
                    if (p.PermissionKey.ToLower() == "view")
                    {
                        InheritPermissionID = p.PermissionID;
                    }
                }
                foreach (DNNModulePermissionInfo p in Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo(moduleinfo.ModuleDefID))
                {
                    PermissionDefinitions.Add(AddPermissionDefinitions(p.PermissionKey, p.PermissionID));
                }

                Permissions Permissions = new Permissions();
                Permissions = GetAllPermission(false, ModuleID, PortalID, GetGenericPermissions(moduleinfo.ModulePermissions), PermissionDefinitions);
                Permissions.Inherit = moduleinfo.InheritViewPermissions;
                Permissions.ShowInheritCheckBox = true;
                Permissions.InheritPermissionID = InheritPermissionID;
                permData.Add("Permissions", Permissions);
            }
            return permData;
        }
        private static List<GenericPermissionInfo> GetGenericPermissions(ModulePermissionCollection MpCollection)
        {
            List<GenericPermissionInfo> GenericPermissions = new List<GenericPermissionInfo>();

            foreach (PermissionInfoBase pinfo in MpCollection.ToList())
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

        private static Permissions GetAllPermission(bool Locked, int ModuleID, int PortalID, List<GenericPermissionInfo> GenericPermissionInfo, List<Permission> PermissionDefinitions)
        {
            Permissions Permissions = new Permissions(true, Locked);

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

        public static void UpdateModule(ModuleInfo moduleinfo, dynamic Data)
        {
            moduleinfo.InheritViewPermissions = bool.Parse(Data.PermissionsInherit.ToString());
            List<ModulePermissionInfo> ModulePerInfo = new List<ModulePermissionInfo>();
            moduleinfo.ModulePermissions.Clear();
            foreach (dynamic item in Data.PermissionsRoles)
            {
                foreach (dynamic p in item.Permissions)
                {
                    bool AllowAcess = bool.Parse(p.AllowAccess.ToString());
                    string PermissionID = p.PermissionId.ToString();
                    if (AllowAcess)
                    {
                        moduleinfo.ModulePermissions.Add(new ModulePermissionInfo { AllowAccess = AllowAcess, RoleID = int.Parse(item.RoleId.ToString()), PermissionID = int.Parse(PermissionID) });
                    }
                }

            }

            foreach (dynamic item in Data.PermissionsUsers)
            {
                foreach (dynamic p in item.Permissions)
                {
                    bool AllowAcess = bool.Parse(p.AllowAccess.ToString());
                    string PermissionID = p.PermissionId.ToString();
                    if (AllowAcess)
                    {
                        moduleinfo.ModulePermissions.Add(new ModulePermissionInfo { AllowAccess = AllowAcess, UserID = int.Parse(item.UserId.ToString()), PermissionID = int.Parse(PermissionID) });
                    }
                }
            }
            bool allTabsChanged = false;
            moduleinfo.StartDate = Data.StartDate == "" ? Null.NullDate : Data.StartDate;
            moduleinfo.EndDate = Data.EndDate == "" ? Null.NullDate : Data.EndDate;
            if (moduleinfo.AllTabs != Data.chkAllTabs.Value)
            {
                allTabsChanged = true;
            }
            moduleinfo.AllTabs = Data.chkAllTabs.Value;
            ModuleController.Instance.UpdateModule(moduleinfo);
            if (allTabsChanged)
            {
                List<TabInfo> listTabs = TabController.GetPortalTabs(moduleinfo.PortalID, Null.NullInteger, false, true);
                if (Data.chkAllTabs.Value)
                {
                    if (!Data.chkNewTabs.Value)
                    {
                        foreach (TabInfo destinationTab in listTabs)
                        {
                            ModuleInfo module = ModuleController.Instance.GetModule(moduleinfo.ModuleID, destinationTab.TabID, false);
                            if (module != null)
                            {
                                if (module.IsDeleted)
                                {
                                    ModuleController.Instance.RestoreModule(module);
                                }
                            }
                            else
                            {
                                if (!PortalSettings.Current.ContentLocalizationEnabled || (moduleinfo.CultureCode == destinationTab.CultureCode))
                                {
                                    ModuleController.Instance.CopyModule(moduleinfo, destinationTab, moduleinfo.PaneName, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ModuleController.Instance.DeleteAllModules(moduleinfo.ModuleID, moduleinfo.TabID, listTabs, true, false, false);
                }
            }
        }
    }
}
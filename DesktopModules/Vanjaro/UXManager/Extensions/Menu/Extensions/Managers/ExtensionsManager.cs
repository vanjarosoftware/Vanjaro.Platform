using Dnn.PersonaBar.Extensions.Components;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Vanjaro.Common.Permissions;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Managers
{
    public class ExtensionsManager
    {


        #region Extensions Lists

        public static Dictionary<string, dynamic> GetPermission(dynamic ModulePermission)
        {
            Dictionary<string, dynamic> permData = new Dictionary<string, dynamic>();
            Permissions Permissions = new Permissions
            {
                PermissionDefinitions = new List<Permission>()
            };
            foreach (dynamic p in ModulePermission.PermissionDefinitions)
            {
                Permission permission = new Permission
                {
                    PermissionName = p.PermissionName,
                    PermissionId = p.PermissionId,
                    AllowAccess = true
                };
                Permissions.PermissionDefinitions.Add(permission);
            }
            foreach (dynamic RolePerm in ModulePermission.RolePermissions)
            {
                RolePermission rolepermission = new RolePermission
                {
                    RoleId = RolePerm.RoleId,
                    RoleName = RolePerm.RoleName,
                    Locked = RolePerm.Locked,
                    IsDefault = RolePerm.IsDefault
                };
                foreach (dynamic item in RolePerm.Permissions)
                {
                    Permission permission = new Permission
                    {
                        PermissionName = item.PermissionName,
                        PermissionId = item.PermissionId,
                        View = item.View,
                        AllowAccess = item.AllowAccess
                    };

                    rolepermission.Permissions.Add(permission);
                }
                Permissions.RolePermissions.Add(rolepermission);
            }
            foreach (dynamic UserPerm in ModulePermission.UserPermissions)
            {
                UserPermission userpermission = new UserPermission
                {
                    UserId = UserPerm.UserId,
                    DisplayName = UserPerm.DisplayName
                };
                UserInfo uinfo = UserController.GetUserById(PortalSettings.Current.PortalId, UserPerm.UserId);
                if (uinfo != null)
                {
                    userpermission.Email = uinfo.Email;
                    userpermission.UserName = uinfo.Username;
                    userpermission.AvatarUrl = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, UserPerm.UserId, uinfo.Email);
                }
                foreach (dynamic item in UserPerm.Permissions)
                {
                    Permission permission = new Permission
                    {
                        PermissionName = item.PermissionName,
                        PermissionId = item.PermissionId,
                        View = item.View,
                        AllowAccess = item.AllowAccess
                    };
                    userpermission.Permissions.Add(permission);
                }
                Permissions.UserPermissions.Add(userpermission);
            }

            Permissions.Inherit = false;
            Permissions.ShowInheritCheckBox = false;
            Permissions.InheritPermissionID = -1;



            Permissions.Inherit = false;
            Permissions.ShowInheritCheckBox = false;
            Permissions.InheritPermissionID = -1;
            permData.Add("Permissions", Permissions);
            return permData;
        }


        internal static List<PackageExtensionInfo> GetAllExtensions(UserInfo UserInfo, int PortalId, bool IsInstall)
        {
            List<PackageExtensionInfo> Extensions = new List<PackageExtensionInfo>();
            PackageExtensionInfo Pinfo = new PackageExtensionInfo();
            ExtensionsController ExtensionsController = new ExtensionsController();
            foreach (PackageType packageType in PackageController.Instance.GetExtensionPackageTypes())
            {

                if (IsInstall)
                {
                    foreach (PackageInfoSlimDto PackageInfo in ExtensionsController.GetInstalledPackages(UserInfo.IsSuperUser ? -1 : PortalId, packageType.PackageType).ToList())
                    {


                        Pinfo = new PackageExtensionInfo
                        {
                            PackageId = PackageInfo.PackageId,
                            Type = packageType.PackageType,
                            FriendlyName = PackageInfo.FriendlyName,
                            Name = PackageInfo.Name,
                            FileName = PackageInfo.FileName,
                            Description = PackageInfo.Description,
                            Version = PackageInfo.Version,
                            IsInUse = PackageInfo.IsInUse,
                            PackageIcon = VirtualPathUtility.ToAbsolute(PackageInfo.PackageIcon),
                            UpgradeUrl = PackageInfo.UpgradeUrl,
                            UpgradeIndicator = PackageInfo.UpgradeIndicator,
                            CanDelete = PackageInfo.CanDelete,
                            ReadOnly = PackageInfo.ReadOnly
                        };
                        Extensions.Add(Pinfo);

                    }
                }
                else
                {
                    if (ExtensionsController.HasAvailablePackage(packageType.PackageType, out string rootPath))
                    {
                        foreach (AvailablePackagesDto Packages in ExtensionsController.GetAvailablePackages(packageType.PackageType).ToList())
                        {
                            foreach (PackageInfoSlimDto PackageInfo in Packages.ValidPackages)
                            {
                                Pinfo = new PackageExtensionInfo
                                {
                                    PackageId = PackageInfo.PackageId,
                                    Type = packageType.PackageType,
                                    FriendlyName = PackageInfo.FriendlyName,
                                    Name = PackageInfo.Name,
                                    FileName = PackageInfo.FileName,
                                    Description = PackageInfo.Description,
                                    Version = PackageInfo.Version,
                                    IsInUse = PackageInfo.IsInUse,
                                    PackageIcon = VirtualPathUtility.ToAbsolute(PackageInfo.PackageIcon),
                                    UpgradeUrl = PackageInfo.UpgradeUrl,
                                    UpgradeIndicator = PackageInfo.UpgradeIndicator,
                                    CanDelete = PackageInfo.CanDelete,
                                    ReadOnly = PackageInfo.ReadOnly
                                };
                                Extensions.Add(Pinfo);
                            }
                        }
                    }
                }

            }
            return Extensions.OrderBy(o => o.FriendlyName).ToList();
        }



        internal static List<TempExt> GetModuleCategories()
        {
            List<TempExt> ModuleCategories = new List<TempExt>
            {
                new TempExt { Key = "", Value = "Please Select" }
            };
            DotNetNuke.Entities.Content.Taxonomy.ITermController termController = DotNetNuke.Entities.Content.Common.Util.GetTermController();
            foreach (string cat in termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Select(t => t.Name).ToList())
            {
                if (cat != "&lt; None &gt;")
                {
                    ModuleCategories.Add(new TempExt { Key = cat, Value = cat });
                }
            }
            return ModuleCategories;
        }
        internal static List<TempExt> GetModuleSharing()
        {
            List<TempExt> ModuleSharing = new List<TempExt>
            {
                new TempExt { Key = 0, Value = "Unknown" },
                new TempExt { Key = 1, Value = "Unsupported" },
                new TempExt { Key = 2, Value = "Supported" }
            };
            return ModuleSharing;
        }
        internal static List<TempExt> GetControlTypes()
        {
            List<TempExt> Types = new List<TempExt>
            {
                new TempExt { Key = -2, Value = "Theme Object" },
                new TempExt { Key = -1, Value = "Anonymous" },
                new TempExt { Key = 0, Value = "View" },
                new TempExt { Key = 1, Value = "Edit" },
                new TempExt { Key = 2, Value = "Admin" },
                new TempExt { Key = 3, Value = "Host" }
            };
            return Types;
        }
        internal static ActionResult GetSourceFolders()
        {
            ActionResult actionResult = new ActionResult();
            string path = Path.Combine(Globals.ApplicationMapPath, "DesktopModules");
            List<string> controlfolders = (
                from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                select subdirectory).ToList();

            List<TempExt> response = new List<TempExt>();
            int appPathLen = Globals.ApplicationMapPath.Length + 1;
            foreach (string folder in controlfolders)
            {
                int moduleControls = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Count(s => s.EndsWith(".ascx") || s.EndsWith(".cshtml") ||
                                s.EndsWith(".vbhtml") || s.EndsWith(".html") || s.EndsWith(".htm"));
                if (moduleControls > 0)
                {
                    string shortFolder = folder.Substring(appPathLen).Replace('\\', '/');
                    TempExt item = new TempExt { Key = shortFolder.ToLower(), Value = shortFolder };
                    response.Add(item);
                }
            }
            actionResult.Data = response;
            actionResult.IsSuccess = true;
            return actionResult;
        }
        internal static string GetPackageInstallFolder(string packageType)
        {
            switch ((packageType ?? "").ToLowerInvariant())
            {
                case "authsystem":
                case "auth_system":
                    return "AuthSystem";
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return "Language";
                case "javascriptlibrary":
                case "javascript_library":
                    return "JavaScriptLibrary";
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    return packageType;
                default:
                    return string.Empty;
            }
        }
        internal static ActionResult GetSourceFiles(string root)
        {
            ActionResult actionResult = new ActionResult();
            List<TempExt> response = new List<TempExt>
            {
                new TempExt{Key="", Value="Please Select" }
            };

            if (!string.IsNullOrEmpty(root))
            {
                string path = Path.Combine(Globals.ApplicationMapPath, root.Replace('/', '\\'));
                if (Directory.Exists(path))
                {
                    AddFiles(response, path, root, "*.ascx");
                    AddFiles(response, path, root, "*.cshtml");
                    AddFiles(response, path, root, "*.vbhtml");
                    AddFiles(response, path, root, "*.html");
                    AddFiles(response, path, root, "*.htm");
                }
            }

            actionResult.Data = response;
            actionResult.IsSuccess = true;
            return actionResult;
        }
        internal static ActionResult GetIcons(string controlPath)
        {
            ActionResult actionResult = new ActionResult();

            List<TempExt> response = new List<TempExt>
            {
                new TempExt{Key="", Value="Please Select" }
            };

            if (!string.IsNullOrEmpty(controlPath))
            {
                int idx = controlPath.LastIndexOf("/", StringComparison.Ordinal);
                string root = controlPath.Substring(0, Math.Max(0, idx));
                string path = Path.Combine(Globals.ApplicationMapPath, root.Replace('/', '\\'));
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path);
                    if (files.Length > 0)
                    {
                        string[] extensions = Globals.glbImageFileTypes.ToLowerInvariant().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string file in files)
                        {
                            string ext = Path.GetExtension(file) ?? "";
                            string extension = ext.Length <= 1 ? "" : ext.Substring(1).ToLowerInvariant();
                            if (extensions.Contains(extension))
                            {
                                path = Path.GetFileName(file);
                                if (path != null)
                                {
                                    TempExt item = new TempExt { Key = path.ToLower(), Value = Path.GetFileName(file) };
                                    response.Add(item);
                                }
                            }
                        }

                    }
                }
            }
            actionResult.Data = response;
            actionResult.IsSuccess = true;
            return actionResult;
        }

        internal static List<string> GetPages(int PortalID, int PackageID, UserInfo UserInfo)
        {
            IDictionary<int, TabInfo> tabsWithModule = TabController.Instance.GetTabsByPackageID(PortalID, PackageID, false);
            TabCollection allPortalTabs = TabController.Instance.GetTabsByPortal(PortalID);
            IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();

            foreach (TabInfo tab in allPortalTabs.Values)
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }
            List<string> PackageUsage = new List<string>();
            foreach (KeyValuePair<int, TabInfo> item in tabsInOrder)
            {
                PackageUsage.Add(GetFormattedLink(item.Value));
            }
            return PackageUsage;
        }
        internal static List<TempExt> GetPortals(int PortalId, UserInfo UserInfo)
        {
            List<TempExt> TotalPortals = new List<TempExt>();
            try
            {
                IEnumerable<PortalInfo> portals = UserInfo.IsSuperUser ? PortalController.Instance.GetPortals().OfType<PortalInfo>() : PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => p.PortalID == PortalId);

                foreach (PortalInfo p in portals)
                {
                    TotalPortals.Add(new TempExt { Key = p.PortalID, Value = p.PortalName });
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return TotalPortals;
        }

        internal static ParseResultDto ParsePackageFile(PortalSettings PortalSettings, UserInfo UserInfo, string Type, string FileName)
        {
            string installFolder = GetPackageInstallFolder(Type);
            if (!string.IsNullOrEmpty(installFolder) && !string.IsNullOrEmpty(FileName))
            {
                string packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, FileName);
                if (File.Exists(packagePath))
                {
                    using (FileStream stream = new FileStream(packagePath, FileMode.Open))
                    {
                        try
                        {
                            return InstallController.Instance.ParsePackage(PortalSettings, UserInfo, packagePath, stream);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }
                }

            }
            ParseResultDto result = new ParseResultDto
            {
                NoManifest = true
            };
            return result;
        }
        #endregion

        #region Update delete Extension

        public static ActionResult DeletePackage(DeletePackageDto deletePackage)
        {
            ActionResult actionResult = new ActionResult();

            try
            {
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == deletePackage.Id);
                if (package == null)
                {
                    actionResult.AddError("NotFound", "NotFound");
                    return actionResult;
                }

                Installer installer = new Installer(package, Globals.ApplicationMapPath);
                installer.UnInstall(deletePackage.DeleteFiles);

                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.HasErrors = true;
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }
        #endregion


        #region Private Methods
        private static void AddFiles(ICollection<TempExt> collection, string path, string root, string filter)
        {
            string[] files = Directory.GetFiles(path, filter);
            foreach (string strFile in files)
            {
                string file = root.Replace('\\', '/') + "/" + Path.GetFileName(strFile);
                TempExt item = new TempExt { Key = file.ToLower(), Value = file };
                collection.Add(item);
            }
        }
        private static void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
            if (!tabsWithModule.ContainsKey(currentTab.TabID) || tabsInOrder.ContainsKey(currentTab.TabID))
            {
                return;
            }

            tabsInOrder.Add(currentTab.TabID, currentTab);
            foreach (TabInfo tab in allPortalTabs.WithParentId(currentTab.TabID))
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }
        }
        private static string GetFormattedLink(object dataItem)
        {
            PortalAliasInfo ps = new PortalAliasInfo();

            StringBuilder returnValue = new StringBuilder();
            if ((dataItem is TabInfo))
            {
                TabInfo tab = (TabInfo)dataItem;
                {
                    int index = 0;
                    TabController.Instance.PopulateBreadCrumbs(ref tab);
                    foreach (TabInfo t in tab.BreadCrumbs)
                    {
                        if (index > 0)
                        {
                            returnValue.Append(" > ");
                        }
                        if ((tab.BreadCrumbs.Count - 1 == index))
                        {
                            string url = Globals.AddHTTP(t.PortalID == Null.NullInteger ? ps.HTTPAlias : PortalAliasController.Instance.GetPortalAliasesByPortalId(t.PortalID).ToList().OrderByDescending(a => a.IsPrimary).FirstOrDefault().HTTPAlias) + "/Default.aspx?tabId=" + t.TabID;
                            returnValue.AppendFormat("<a target=\"_blank\" href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                        }
                        else
                        {
                            returnValue.AppendFormat("{0}", t.LocalizedTabName);
                        }
                        index = index + 1;
                    }
                }
            }
            return returnValue.ToString();
        }

        #endregion

    }

    public class PackageExtensionInfo
    {
        public int PackageId { get; set; }
        public string Type { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string IsInUse { get; set; }
        public string UpgradeUrl { get; set; }
        public string UpgradeIndicator { get; set; }
        public string PackageIcon { get; set; }
        public bool CanDelete { get; set; }
        public bool ReadOnly { get; set; }
    }

    public class TempExt
    {
        public dynamic Key { get; set; }
        public string Value { get; set; }
    }

}
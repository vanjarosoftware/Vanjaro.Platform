using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Assets.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class PermissionController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, UserInfo UserInfo, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            if (parameters != null && parameters.Keys.Count > 0 && parameters.ContainsKey("pid"))
            {
                Settings.Add("Permissions", new UIData { Name = "Permissions", Options = Managers.PermissionManager.GetPermission(PortalID, Convert.ToInt32(parameters["pid"])) });
                Settings.Add("FolderID", new UIData { Name = "FolderID", Value = parameters["pid"].ToString() });
            }
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Save(int folderid, bool Copyfolder, dynamic Data)
        {
            ActionResult actionResult = new ActionResult();
            IFolderInfo parentFolder = FolderManager.Instance.GetFolder(folderid);
            if (parentFolder != null)
            {
                parentFolder.FolderPermissions.Clear();

                DotNetNuke.Security.Permissions.PermissionController permController = new DotNetNuke.Security.Permissions.PermissionController();
                ArrayList permArray = permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "READ");

                List<PermissionInfo> SYS_FOLDER_PERM = new List<PermissionInfo>();
                if (permArray.Count == 1)
                {
                    SYS_FOLDER_PERM.Add(permArray[0] as PermissionInfo);
                }

                permArray = permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "BROWSE");
                if (permArray.Count == 1)
                {
                    SYS_FOLDER_PERM.Add(permArray[0] as PermissionInfo);
                }

                permArray = permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "WRITE");
                if (permArray.Count == 1)
                {
                    SYS_FOLDER_PERM.Add(permArray[0] as PermissionInfo);
                }

                foreach (dynamic item in Data.PermissionsRoles)
                {
                    foreach (dynamic p in item.Permissions)
                    {
                        bool AllowAcess = bool.Parse(p.AllowAccess.ToString());
                        string PermissionID = p.PermissionId.ToString();
                        if (AllowAcess)
                        {
                            FolderPermissionInfo folderpermissioninfo = new FolderPermissionInfo
                            {
                                AllowAccess = AllowAcess,
                                PermissionID = Convert.ToInt32(PermissionID)
                            };
                            PermissionInfo SYS_PERM = SYS_FOLDER_PERM.Where(a => a.PermissionID == folderpermissioninfo.PermissionID).FirstOrDefault();
                            if (SYS_PERM != null)
                            {
                                folderpermissioninfo.PermissionKey = SYS_PERM.PermissionKey;
                            }

                            folderpermissioninfo.RoleID = int.Parse(item.RoleId.ToString());
                            parentFolder.FolderPermissions.Add(folderpermissioninfo);
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
                            FolderPermissionInfo folderpermissioninfo = new FolderPermissionInfo
                            {
                                AllowAccess = AllowAcess,
                                PermissionID = Convert.ToInt32(PermissionID)
                            };
                            PermissionInfo SYS_PERM = SYS_FOLDER_PERM.Where(a => a.PermissionID == folderpermissioninfo.PermissionID).FirstOrDefault();
                            if (SYS_PERM != null)
                            {
                                folderpermissioninfo.PermissionKey = SYS_PERM.PermissionKey;
                            }

                            folderpermissioninfo.UserID = int.Parse(item.UserId.ToString());
                            parentFolder.FolderPermissions.Add(folderpermissioninfo);
                        }
                    }
                }
                FolderManager.Instance.UpdateFolder(parentFolder);
                if (Copyfolder)
                {
                    FolderPermissionController.CopyPermissionsToSubfolders(parentFolder, parentFolder.FolderPermissions);
                }
            }
            Dictionary<int, bool> result = new Dictionary<int, bool>();
            if (Data != null && Data.FolderIds != null && Data.FolderIds.Count > 0)
            {
                foreach (dynamic id in Data.FolderIds)
                {
                    result.Add(int.Parse(id.Value), BrowseUploadFactory.HasBrowseOpenPerm(new Vanjaro.Common.Components.TreeView() { Value = int.Parse(id.Value) }));
                }
            }
            actionResult.Data = result;
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]
    public class UploadController : UIEngineController
    {
        [ValidateAntiForgeryToken]
        [HttpGet]
        public List<TreeView> GetSubFolders(int folderid)
        {
            return BrowseUploadFactory.GetFoldersChildrensTree(PortalSettings.PortalId, folderid, UserInfo, "image");
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetFiles(int folderid, string uid, int skip, int pagesize, string keyword)
        {
            if (!string.IsNullOrEmpty(uid) && uid == "FavIcon")
            {
                return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, uid, "ico", skip, pagesize, keyword);
            }
            else
            {
                return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, uid, Entities.FileSetting.FileType, skip, pagesize, keyword);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Files(string Identifier)
        {
            return Files(Identifier, null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Files(string Identifier, string uid)
        {
            string result = string.Empty;
            string ControlName = string.Empty;

            if (!string.IsNullOrEmpty(Identifier) && PortalSettings != null && UserInfo != null)
            {
                List<IUIData> settings = new List<IUIData>
                {
                    new UIData { Name = "AllowedAttachmentFileExtensions", Value = Entities.FileSetting.FileType },
                    new UIData { Name = "MaxFileSize", Value = Entities.FileSetting.FileSize.ToString() }
                };

                if (HttpContext.Current.Request.Form.AllKeys.Contains("logotype"))
                {
                    ControlName = HttpContext.Current.Request.Form.Get("logotype");
                }

                IFolderInfo fi = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, "Images/");
                
                int FolderID = fi.FolderID;
                if (ControlName == "FavIcon")
                {
                    result = BrowseUploadFactory.UploadFile(Identifier, HttpContext.Current, PortalSettings, new ModuleInfo(), UserInfo, isUploadAllowed(ActiveModule, UserInfo), "ico", 100, AppFactory.GetAppInformation().Name, uid, FolderID);
                }
                else
                {
                    result = BrowseUploadFactory.UploadFile(Identifier, HttpContext.Current, PortalSettings, new ModuleInfo(), UserInfo, isUploadAllowed(ActiveModule, UserInfo), getFileTypes(settings), getMaxSize(Identifier, settings), AppFactory.GetAppInformation().Name, uid, FolderID);
                }
            }
            return result;
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetFile(int fileid)
        {
            return BrowseUploadFactory.GetFile(PortalSettings, fileid);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetMultipleFileDetails(string fileids)
        {
            List<dynamic> fileDetails = new List<dynamic>();
            foreach (string fileid in fileids.Split(','))
            {
                dynamic result = new ExpandoObject();
                IFileInfo file = FileManager.Instance.GetFile(int.Parse(fileid));
                if (file != null)
                {
                    result.Name = file.FileName;
                    result.FileId = file.FileId;
                    //result.FolderId = file.FolderId;
                    string MapPath = string.Empty;
                    MapPath = PortalSettings.HomeDirectory;
                    if (!MapPath.EndsWith("/"))
                    {
                        MapPath = MapPath + "/";
                    }

                    MapPath = MapPath + file.RelativePath;

                    result.FileUrl = MapPath;
                    result.Size = file.Size;
                    fileDetails.Add(result);
                }
            }
            return fileDetails;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public string GetLink(string fileurl, int urltype)
        {
            return BrowseUploadFactory.GetLink(PortalSettings, ActiveModule, fileurl, urltype);
        }

        private string getFileTypes(List<IUIData> settings)
        {
            return settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault() != null ? settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault().Value : "";
        }

        private int getMaxSize(string Identifier, List<IUIData> settings)
        {
            return int.Parse(settings.Where(s => s.Name == "MaxFileSize").FirstOrDefault().Value);
        }

        private bool isUploadAllowed(ModuleInfo minfo, UserInfo UserInfo)
        {
            if (UserInfo.UserID > -1 && (UserInfo.IsSuperUser || UserInfo.IsInRole("Administrators") || ModulePermissionController.CanEditModuleContent(minfo)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
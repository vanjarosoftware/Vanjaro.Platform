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
using Vanjaro.UXManager.Extensions.Menu.Pages.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Pages.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "user")]
    [ValidateAntiForgeryToken]
    public class UploadController : UIEngineController
    {
        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetFiles(int folderid, string uid, int skip, int pagesize, string keyword)
        {
            if (!string.IsNullOrEmpty(uid) && uid == "null")
            {
                uid = null;
            }

            return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, uid, null, skip, pagesize, keyword);
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
                if (HttpContext.Current.Request.Form.AllKeys.Contains("ControlName"))
                {
                    ControlName = HttpContext.Current.Request.Form.Get("ControlName");
                }

                int FolderID = BrowseUploadFactory.GetRootFolder(PortalSettings.Current.PortalId).FolderID;
                if (string.IsNullOrEmpty(uid))
                {
                    result = BrowseUploadFactory.UploadFile(Identifier, HttpContext.Current, PortalSettings, new ModuleInfo(), UserInfo, isUploadAllowed(ActiveModule, UserInfo, ControlName), "jpg,jpeg,png,gif,pdf,zip,rar,xls,xlsx,css,xml,doc,docx,webp", 100, AppFactory.GetAppInformation().Name, uid, FolderID);
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

        private bool isUploadAllowed(ModuleInfo minfo, UserInfo UserInfo, string ControlName)
        {
            if (UserInfo.UserID > -1 && (ControlName == "UserImage" || UserInfo.IsSuperUser || UserInfo.IsInRole("Administrators") || ModulePermissionController.CanEditModuleContent(minfo)))
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
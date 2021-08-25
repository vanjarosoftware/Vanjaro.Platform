using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Apps.Image.Entities;
using Vanjaro.UXManager.Extensions.Apps.Image.Factories;

namespace Vanjaro.UXManager.Extensions.Apps.Image.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class UploadController : UIEngineController
    {
        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetFiles(int folderid, string uid, int skip, int pagesize, string keyword)
        {
            return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, null, FileSetting.FileType, skip, pagesize, keyword);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public List<TreeView> GetSubFolders(int folderid)
        {
            return BrowseUploadFactory.GetFoldersChildrensTree(PortalSettings.PortalId, folderid, UserInfo, "image");
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public string GetLink(string fileurl, int urltype)
        {
            return BrowseUploadFactory.GetLink(PortalSettings, ActiveModule, fileurl, urltype);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic GetUrl(int fileid)
        {
            return BrowseUploadFactory.GetUrl(fileid);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic GetUrl(int fileid, string absolutelink)
        {
            dynamic Result = new ExpandoObject();

            if (bool.Parse(absolutelink))
            {
                IFileInfo file = FileManager.Instance.GetFile(fileid);
                if (file != null)
                    Result.Url = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileManager.Instance.GetUrl(file).Replace(file.FileName, GetEscapedFileName(file.FileName)));
                else
                    Result.Url = "";
                Result.Status = "Success";
                Result.Urls = new List<ImageUrl>();
            }
            else
                Result = BrowseUploadFactory.GetUrl(fileid);

            return Result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Files(string Identifier, string uid)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(Identifier) && PortalSettings != null && UserInfo != null)
            {
                List<IUIData> settings = new List<IUIData>
                {
                    new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType },
                    new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() }
                };
                if (settings != null && settings.Count > 0)
                {
                    result = BrowseUploadFactory.UploadFile(Identifier, HttpContext.Current, PortalSettings, new ModuleInfo(), UserInfo, isUploadAllowed(UserInfo), getFileTypes(settings), getMaxSize(Identifier, settings), AppFactory.GetAppInformation().Name, null);
                }
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Files(string Identifier)
        {
            return Files(Identifier, null);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetFile(int fileid)
        {
            return BrowseUploadFactory.GetFile(PortalSettings, fileid);
        }

        private string getFileTypes(List<IUIData> settings)
        {
            return settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault() != null ? settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault().Value : "";
        }

        private int getMaxSize(string Identifier, List<IUIData> settings)
        {
            return int.Parse(settings.Where(s => s.Name == "MaxFileSize").FirstOrDefault().Value);
        }

        private bool isUploadAllowed(UserInfo UserInfo)
        {
            if (AppFactory.GetAccessRoles(UserInfo).Contains("editpage"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetEscapedFileName(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Replace(" ", "%20");
            }

            return fileName;
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
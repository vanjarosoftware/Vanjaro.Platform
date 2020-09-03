using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using Vanjaro.UXManager.Extensions.Menu.Users.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
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
            int? UserID = null;

            if (!string.IsNullOrEmpty(Identifier) && PortalSettings != null && UserInfo != null)
            {
                List<IUIData> settings = new List<IUIData>
                {
                    new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType },
                    new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() }
                };

                if (HttpContext.Current.Request.Form.AllKeys.Contains("ControlName"))
                {
                    ControlName = HttpContext.Current.Request.Form.Get("ControlName");
                }

                if (HttpContext.Current.Request.Form.AllKeys.Contains("UserID"))
                {
                    UserID = Convert.ToInt32(HttpContext.Current.Request.Form.Get("UserID"));
                    if (UserID.HasValue)
                    {
                        UserInfo User = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalSettings.Current.PortalId, UserID.Value);
                        int folderid = FolderManager.Instance.GetUserFolder(User).FolderID;
                        if (string.IsNullOrEmpty(uid))
                        {
                            result = BrowseUploadFactory.UploadFile(Identifier, HttpContext.Current, PortalSettings, new ModuleInfo(), UserInfo, isUploadAllowed(ActiveModule, UserInfo, ControlName), getFileTypes(settings), getMaxSize(Identifier, settings), AppFactory.GetAppInformation().Name, uid, folderid);
                        }
                    }
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
                dynamic file = BrowseUploadFactory.GetFile(PortalSettings, int.Parse(fileid));
                if (file != null)
                {
                    fileDetails.Add(file);
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

        private string getFileTypes(List<IUIData> settings)
        {
            return settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault() != null ? settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault().Value : "";
        }

        private int getMaxSize(string Identifier, List<IUIData> settings)
        {
            return int.Parse(settings.Where(s => s.Name == "MaxFileSize").FirstOrDefault().Value);
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }



}
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Assets.Components;
using Vanjaro.UXManager.Extensions.Menu.Assets.Factories;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Assets.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class UploadController : UIEngineController
    {
        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetFiles(int folderid, string uid, int skip, int pagesize, string keyword)
        {
            return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, null, Host.AllowedExtensionWhitelist.ToStorageString(), skip, pagesize, keyword);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public List<TreeView> GetSubFolders(int folderid)
        {
            IFolderInfo folder = FolderManager.Instance.GetFolder(folderid);
            return BrowseUploadFactory.GetFoldersChildrensTree(folder.PortalID, folderid, UserInfo);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public dynamic SynchronizeFolder(int folderid, bool recursive)
        {
            return BrowseUploadFactory.SyncFolderContent(folderid, recursive);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public string DeleteItems(int folderID)
        {
            try
            {
                int count = BrowseUploadFactory.DeleteFolders(folderID);
                if (count == 0)
                {
                    return "Success";
                }
                else
                {
                    return Localization.Get("DeleteFolderFailed", "Text", Constants.LocalResourcesFile, Library.Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic RenameFolder(int folderID, string newFolderName)
        {
            try
            {
                return BrowseUploadFactory.RenameFolder(folderID, newFolderName);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public string MoveFolder(int folderId, int destinationFolderId)
        {
            try
            {
                return BrowseUploadFactory.MoveFolder(folderId, destinationFolderId);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string CreateNewFolder(int folderparentID, string folderName, int FolderType)
        {
            try
            {
                IFolderInfo folderinfo = null;
                return BrowseUploadFactory.CreateNewFolder(folderparentID, folderName, FolderType, ref folderinfo);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public string RenameFile(int fileID, string newFileName)
        {
            try
            {
                return BrowseUploadFactory.RenameFile(fileID, newFileName);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public string CopyFile(int fileID, int destinationFolderID, bool overWrite)
        {
            try
            {
                return BrowseUploadFactory.CopyFile(fileID, destinationFolderID, overWrite);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public dynamic CopyFiles(string fileids, int destinationFolderID, bool overWrite)
        {
            try
            {
                List<int> ExistingIds = new List<int>();
                foreach (int fileID in fileids.Split(',').Select(s => int.Parse(s)).Distinct().ToList())
                {
                    string result = BrowseUploadFactory.CopyFile(fileID, destinationFolderID, overWrite);
                    if (!string.IsNullOrEmpty(result) && result.ToLower() == "exist")
                    {
                        ExistingIds.Add(fileID);
                    }
                }
                if (ExistingIds.Count > 0)
                {
                    dynamic result = new ExpandoObject();
                    result.Error = "Exist";
                    result.ExistingFiles = new List<object>();
                    foreach (int id in ExistingIds)
                    {
                        result.ExistingFiles.Add(BrowseUploadFactory.GetFile(PortalSettings, id));
                    }
                    return result;
                }
                else
                {
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public string MoveFile(int fileID, int destinationFolderID, bool overWrite)
        {
            try
            {
                return BrowseUploadFactory.MoveFile(fileID, destinationFolderID, overWrite);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public dynamic MoveFiles(string fileids, int destinationFolderID, bool overWrite)
        {
            try
            {
                List<int> ExistingIds = new List<int>();
                foreach (int fileID in fileids.Split(',').Select(s => int.Parse(s)).Distinct().ToList())
                {
                    string result = BrowseUploadFactory.MoveFile(fileID, destinationFolderID, overWrite);
                    if (!string.IsNullOrEmpty(result) && result.ToLower() == "exist")
                    {
                        ExistingIds.Add(fileID);
                    }
                }
                if (ExistingIds.Count > 0)
                {
                    dynamic result = new ExpandoObject();
                    result.Error = "Exist";
                    result.ExistingFiles = new List<object>();
                    foreach (int id in ExistingIds)
                    {
                        result.ExistingFiles.Add(BrowseUploadFactory.GetFile(PortalSettings, id));
                    }
                    return result;
                }
                else
                {
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public dynamic DeleteFile(int fileID)
        {
            try
            {
                return BrowseUploadFactory.DeleteFile(fileID);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public dynamic DeleteFiles(string fileIDs)
        {
            try
            {
                foreach (int fileID in fileIDs.Split(',').Select(s => int.Parse(s)).Distinct().ToList())
                {
                    BrowseUploadFactory.DeleteFile(fileID);
                }

                return "Success";
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Download(int fileID, bool forceDownload)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            System.IO.Stream streamContent = BrowseUploadFactory.GetFileContent(fileID, out string fileName, out string contentType);
            result.Content = new StreamContent(streamContent);
            result.Content.Headers.Add("x-filename", fileName);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(forceDownload ? "attachment" : "inline")
            {
                FileName = fileName
            };
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ExtractFiles(string fileids)
        {
            if (!string.IsNullOrEmpty(fileids))
            {
                foreach (string fileid in fileids.Split(','))
                {
                    BrowseUploadFactory.ExtractFiles(int.Parse(fileid));
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic GetUrl(int fileid)
        {
            return BrowseUploadFactory.GetUrl(fileid);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Files(string Identifier)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(Identifier) && PortalSettings != null && UserInfo != null)
            {
                List<IUIData> settings = new List<IUIData>
                {
                    new UIData { Name = "AllowedAttachmentFileExtensions", Value = Host.AllowedExtensionWhitelist.ToStorageString() },
                    new UIData { Name = "MaxFileSize", Value = Config.GetMaxUploadSize().ToString() }
                };
                if (settings != null && settings.Count > 0)
                {
                    result = BrowseUploadFactory.UploadFile(Identifier, HttpContext.Current, PortalSettings, new ModuleInfo(), UserInfo, isUploadAllowed(UserInfo), getFileTypes(settings), getMaxSize(Identifier, settings), AppFactory.GetAppInformation().Name, null);
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

        private string getFileTypes(List<IUIData> settings)
        {
            return settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault() != null ? settings.Where(s => s.Name == "AllowedAttachmentFileExtensions").FirstOrDefault().Value : "";
        }

        private int getMaxSize(string Identifier, List<IUIData> settings)
        {
            return int.Parse(settings.Where(s => s.Name == "MaxFileSize").FirstOrDefault().Value) / 1000000;
        }

        private bool isUploadAllowed(UserInfo UserInfo)
        {
            if (AppFactory.GetAccessRoles(UserInfo).Contains("admin"))
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
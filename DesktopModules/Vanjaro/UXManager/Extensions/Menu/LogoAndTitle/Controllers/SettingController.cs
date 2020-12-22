using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Internals;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.Core.Entities;
using Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.Entities;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            SettingController sc = new SettingController();
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            List<Common.Components.TreeView> folders = BrowseUploadFactory.GetFoldersTree(PortalSettings.Current.PortalId, "image");
            Settings.Add("AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType });
            Settings.Add("MaxFileSize", new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() });
            Settings.Add("Folders", new UIData { Name = "Folders", Options = folders, Value = folders.Count > 0 ? folders.FirstOrDefault().Value.ToString() : "0" });
            Settings.Add("Files", new UIData { Name = "Files", Options = null });
            string timezoneid = TimeZoneInfo.Local.DisplayName;
            //Settings.Add("TimeZone", new UIData { Name = "TimeZone", Options = TimeZoneInfo.GetSystemTimeZones().Select(a => new { a.Id, a.DisplayName }), OptionsText = "DisplayName", OptionsValue = "Id", Value = timezoneid });
            Settings.Add("PortalSetting", new UIData { Name = "PortalSetting", Value = "", Options = new UpdateSiteSettingsRequest() { LogoFile = new FileDto(), FavIcon = new FileDto() } });

            string cultureCode = PortalSettings.Current.CultureCode;
            dynamic siteSetting = sc.GetPortalSettings(PortalSettings.Current.PortalId, cultureCode).Data;

            Settings.Add("SiteSettings", new UIData { Name = "SiteSettings", Value = "", Options = siteSetting });
            Settings.Add("SocialSharing", new UIData { Name = "SocialSharing", Value = "", Options = new FileDto() });

            string data = PortalController.GetPortalSetting("SocialSharingLogo", PortalSettings.Current.PortalId, null, cultureCode);

            string LogoURL = string.Empty, FavIconurl = string.Empty, SocialSharingLogourl = string.Empty, HomeScreenIcon = string.Empty;
            if (siteSetting.LogoFile.fileId > 0)
            {
                LogoURL = FileManager.Instance.GetUrl(FileManager.Instance.GetFile(siteSetting.LogoFile.fileId));
            }

            if (siteSetting.FavIcon.fileId > 0)
            {
                FavIconurl = FileManager.Instance.GetUrl(FileManager.Instance.GetFile(siteSetting.FavIcon.fileId));
            }

            string PortalRoot = Core.Managers.PageManager.GetPortalRoot(PortalSettings.Current.PortalId);
            PortalRoot = PortalRoot + "/";

            SocialSharingLogourl = PortalRoot + PortalController.GetPortalSetting("SocialSharingLogo", PortalSettings.Current.PortalId, "", cultureCode);
            HomeScreenIcon = PortalRoot + PortalController.GetPortalSetting("HomeScreenIcon", PortalSettings.Current.PortalId, "", cultureCode);
            Settings.Add("LogoFile", new UIData { Name = "LogoFile", Value = LogoURL, Options = LogoURL });
            Settings.Add("FavIcon", new UIData { Name = "FavIcon", Value = FavIconurl, Options = FavIconurl });
            Settings.Add("SocialSharingLogo", new UIData { Name = "SocialSharingLogo", Value = SocialSharingLogourl, Options = new FileDto() { fileId = -1, folderId = 0 } });
            Settings.Add("HomeScreenIcon", new UIData { Name = "HomeScreenIcon", Value = HomeScreenIcon, Options = new FileDto() { fileId = -1, folderId = 0 } });
            Settings.Add("Uid", new UIData() { Name = "Uid", Value = "" });
            Settings.Add("DefaultFavIcon", new UIData() { Name = "DefaultFavIcon", Value = Globals.ApplicationPath + "/favicon.ico" });
            string[] IconFolders = GetIconFolders();
            Settings.Add("IconFolders", new UIData { Name = "IconFolders", Options = IconFolders.Select(a => new { Value = a }), Value = "0", OptionsText = "Value", OptionsValue = "Value" });

            return Settings.Values.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePortalSettings(string FileId, string HomeIcon, UpdateSiteSettingsRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = request.PortalId ?? (PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId;
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
                if (!userInfo.IsSuperUser && pid != (PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId)
                {
                    actionResult.AddError("InvalidUser.Error", DotNetNuke.Services.Localization.Localization.GetString("InvalidUser.Error", Components.Constants.LocalResourcesFile));
                }

                string cultureCode = string.IsNullOrEmpty(request.CultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : request.CultureCode;

                Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
                {
                    actionResult.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.LocalResourcesFile));
                }

                PortalInfo portalInfo = PortalController.Instance.GetPortal(pid, cultureCode);
                //portalInfo.PortalName = request.PortalName;

                if (request.LogoFile != null && request.LogoFile.fileId > 0)
                    portalInfo.LogoFile = FileManager.Instance.GetFile(request.LogoFile.fileId).RelativePath;

                //portalInfo.FooterText = request.FooterText;

                if (portalInfo.LogoFile.Length > 50)
                    actionResult.AddError("InvalidLogoURL.Error" + HttpStatusCode.BadRequest, Localization.GetString("InvalidLogoURL_Error", Components.Constants.LocalResourcesFile));
                else
                    PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(pid, "TimeZone", request.TimeZone, false);

                if (request.FavIcon != null && request.FavIcon.fileId > 0)
                {
                    new FavIcon(pid).Update(request.FavIcon.fileId);
                }

                PortalController.UpdatePortalSetting(pid, "DefaultIconLocation", "icons/" + request.IconSet, false, cultureCode);

                //update social sharing logo
                if (!string.IsNullOrEmpty(FileId) && Convert.ToInt32(FileId) > 0)
                {
                    PortalController.UpdatePortalSetting(pid, "SocialSharingLogo", "FileID=" + FileId, true, cultureCode);
                }

                if (!string.IsNullOrEmpty(HomeIcon) && Convert.ToInt32(HomeIcon) > 0)
                {
                    PortalController.UpdatePortalSetting(pid, "HomeScreenIcon", "Fileid=" + HomeIcon, true, cultureCode);
                }

                actionResult.Data = BrowseUploadFactory.GetFile(PortalSettings, request.LogoFile.fileId);
            }
            catch (Exception ex)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError" + HttpStatusCode.InternalServerError, ex.Message);
            }
            return actionResult;
        }

        [HttpGet]
        public ActionResult GetPortalSettings(int? portalId, string cultureCode)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = portalId ?? PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalSettings.PortalId)
                {
                    actionResult.AddError("", "");
                }

                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : cultureCode;

                Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
                {
                    actionResult.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.LocalResourcesFile));
                }

                PortalInfo portal = PortalController.Instance.GetPortal(pid, cultureCode);
                PortalSettings portalSettings = new PortalSettings(portal);
                IFileInfo logoFile = string.IsNullOrEmpty(portal.LogoFile) ? null : FileManager.Instance.GetFile(pid, portal.LogoFile);
                IFileInfo favIcon = string.IsNullOrEmpty(new FavIcon(portal.PortalID).GetSettingPath()) ? null : FileManager.Instance.GetFile(pid, new FavIcon(portal.PortalID).GetSettingPath());

                var settings = new
                {
                    PortalId = portal.PortalID,
                    portal.CultureCode,
                    portal.PortalName,
                    portal.Description,
                    portal.KeyWords,
                    GUID = portal.GUID.ToString().ToUpper(),
                    portal.FooterText,
                    TimeZone = portalSettings.TimeZone.Id,
                    portal.HomeDirectory,
                    LogoFile = logoFile != null ? new FileDto()
                    {
                        fileName = logoFile.FileName,
                        folderPath = logoFile.Folder,
                        fileId = logoFile.FileId,
                        folderId = logoFile.FolderId
                    } : new FileDto(),
                    FavIcon = favIcon != null ? new FileDto()
                    {
                        fileName = favIcon.FileName,
                        folderPath = favIcon.Folder,
                        fileId = favIcon.FileId,
                        folderId = favIcon.FolderId
                    } : new FileDto(),
                    IconSet = PortalController.GetPortalSetting("DefaultIconLocation", pid, "Sigma", cultureCode).Replace("icons/", "")
                };
                actionResult.Data = settings;
            }
            catch (Exception ex)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError" + HttpStatusCode.InternalServerError, ex.Message);
            }
            return actionResult;
        }

        public static string[] GetIconFolders()
        {
            string str = Path.Combine(Globals.ApplicationMapPath, "icons");
            DirectoryInfo directoryInfo = new DirectoryInfo(str);
            string str1 = "";
            foreach (DirectoryInfo directoryInfo1 in directoryInfo.EnumerateDirectories())
            {
                str1 = string.Concat(str1, directoryInfo1.Name, ",");
            }
            return str1.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static ExportTemplate GetExportSettings(int portalId)
        {
            dynamic siteSetting = new SettingController().GetPortalSettings(portalId, null).Data;
            if (siteSetting != null)
            {
                ExportTemplate result = new ExportTemplate();
                if (siteSetting.LogoFile.fileId > 0)
                    result.LogoFile = FileManager.Instance.GetUrl(FileManager.Instance.GetFile(siteSetting.LogoFile.fileId));
                if (siteSetting.FavIcon.fileId > 0)
                    result.FavIcon = FileManager.Instance.GetUrl(FileManager.Instance.GetFile(siteSetting.FavIcon.fileId));
                string PortalRoot = Core.Managers.PageManager.GetPortalRoot(portalId);
                PortalRoot = PortalRoot + "/";
                result.SocialSharingLogo = PortalRoot + PortalController.GetPortalSetting("SocialSharingLogo", portalId, "", null);
                result.HomeScreenIcon = PortalRoot + PortalController.GetPortalSetting("HomeScreenIcon", portalId, "", null);
                return result;
            }
            return null;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
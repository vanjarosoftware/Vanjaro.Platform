using Dnn.PersonaBar.Sites.Components;
using Dnn.PersonaBar.Sites.Services.Dto;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Vanjaro.Core.Entities;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Factories;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Sites.Managers
{
    public class SitesManager
    {

        public static ActionResult GetAllPortals(int PortalID, string filter, int pageIndex, int pageSize)
        {
            ActionResult actionResult = new ActionResult();
            SitesController sitecontroller = new SitesController();
            dynamic Result = new ExpandoObject();
            List<dynamic> PortalsInfo = new List<dynamic>();
            try
            {
                int totalRecords = 0;
                IEnumerable<PortalInfo> portals = PortalController.GetPortalsByName($"%{filter}%", pageIndex, pageSize,
                                 ref totalRecords).Cast<PortalInfo>();

                foreach (PortalInfo pinfo in portals.ToList())
                {
                    var portalInfo = new
                    {
                        pinfo.PortalID,
                        pinfo.PortalName,
                        PortalAliases = sitecontroller.FormatPortalAliases(pinfo.PortalID),
                        allowDelete = (pinfo.PortalID != PortalID && !PortalController.IsMemberOfPortalGroup(pinfo.PortalID))
                    };
                    PortalsInfo.Add(portalInfo);
                }

                Result.Items = PortalsInfo;
                double NumberOfPages = (double)totalRecords / pageSize;
                if ((int)NumberOfPages > 0)
                {
                    NumberOfPages = Math.Ceiling(NumberOfPages);
                }

                Result.NumberOfPages = NumberOfPages;
                actionResult.IsSuccess = true;
                actionResult.Data = Result;
            }
            catch (Exception ex)
            {
                actionResult.AddError("exceptions", ex.Message);
            }
            return actionResult;
        }
        internal static HttpResponseMessage Export(int PortalID, string Name)
        {
            HttpResponseMessage Response = new HttpResponseMessage();
            string Theme = ThemeManager.CurrentTheme.Name;
            Dictionary<string, string> Assets = new Dictionary<string, string>();

            string LogoURL = string.Empty, FavIconurl = string.Empty, SocialSharingLogourl = string.Empty, HomeScreenIcon = string.Empty;
            var siteSetting = LogoAndTitle.Controllers.SettingController.GetExportSettings(PortalID);
            if (siteSetting != null)
            {
                if (!string.IsNullOrEmpty(siteSetting.LogoFile))
                    LogoURL = siteSetting.LogoFile.Split('?')[0];
                if (!string.IsNullOrEmpty(siteSetting.FavIcon))
                    FavIconurl = siteSetting.FavIcon.Split('?')[0];
                if (!string.IsNullOrEmpty(siteSetting.SocialSharingLogo))
                    SocialSharingLogourl = siteSetting.SocialSharingLogo.Split('?')[0];
                if (!string.IsNullOrEmpty(siteSetting.HomeScreenIcon))
                    HomeScreenIcon = siteSetting.HomeScreenIcon.Split('?')[0];
            }

            ExportTemplate exportTemplate = new ExportTemplate
            {
                Name = Name,
                Type = TemplateType.Site.ToString(),
                UpdatedOn = DateTime.UtcNow,
                Templates = new List<Layout>(),
                ThemeName = Theme,
                ThemeGuid = ThemeManager.CurrentTheme.GUID,
                LogoFile = Path.GetFileName(LogoURL),
                FavIcon = Path.GetFileName(FavIconurl),
                SocialSharingLogo = Path.GetFileName(SocialSharingLogourl),
                HomeScreenIcon = Path.GetFileName(HomeScreenIcon)
            };

            if (!string.IsNullOrEmpty(exportTemplate.LogoFile) && !Assets.ContainsKey(exportTemplate.LogoFile))
                Assets.Add(exportTemplate.LogoFile, LogoURL);
            if (!string.IsNullOrEmpty(exportTemplate.FavIcon) && !Assets.ContainsKey(exportTemplate.FavIcon))
                Assets.Add(exportTemplate.FavIcon, FavIconurl);
            if (!string.IsNullOrEmpty(exportTemplate.SocialSharingLogo) && !Assets.ContainsKey(exportTemplate.SocialSharingLogo))
                Assets.Add(exportTemplate.SocialSharingLogo, SocialSharingLogourl);
            if (!string.IsNullOrEmpty(exportTemplate.HomeScreenIcon) && !Assets.ContainsKey(exportTemplate.HomeScreenIcon))
                Assets.Add(exportTemplate.HomeScreenIcon, HomeScreenIcon);

            foreach (Core.Data.Entities.Pages page in Core.Managers.PageManager.GetAllPublishedPages(PortalID, null))
            {
                Dnn.PersonaBar.Pages.Services.Dto.PageSettings pageSettings = Dnn.PersonaBar.Pages.Components.PagesController.Instance.GetPageSettings(page.TabID);
                Layout layout = new Layout
                {
                    Content = Core.Managers.PageManager.TokenizeTemplateLinks(page.Content, false, Assets)
                };

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(layout.Content);
                IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                foreach (HtmlNode item in query.ToList())
                {
                    if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value) && item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                    {
                        string guid = item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value.ToLower();
                        Core.Data.Entities.CustomBlock Block = Core.Managers.BlockManager.GetByLocale(PortalID, guid, null);
                        if (Block != null)
                        {
                            if (layout.Blocks == null)
                            {
                                layout.Blocks = new List<Core.Data.Entities.CustomBlock>();
                            }
                            layout.Blocks.Add(Block);
                        }
                    }
                }
                if (layout.Blocks != null)
                {
                    foreach (Core.Data.Entities.CustomBlock block in layout.Blocks)
                    {
                        if (!string.IsNullOrEmpty(block.Html))
                            block.Html = Core.Managers.PageManager.TokenizeTemplateLinks(Core.Managers.PageManager.DeTokenizeLinks(block.Html, PortalID), false, Assets);
                        if (!string.IsNullOrEmpty(block.Css))
                            block.Css = Core.Managers.PageManager.DeTokenizeLinks(block.Css, PortalID);
                    }
                    CacheFactory.Clear(CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", PortalID));
                }
                layout.Name = pageSettings.Name;
                layout.Content = html.DocumentNode.OuterHtml;
                layout.SVG = "";
                layout.ContentJSON = Core.Managers.PageManager.TokenizeTemplateLinks(page.ContentJSON, true, Assets);
                layout.Style = page.Style;
                layout.StyleJSON = page.StyleJSON;
                layout.Type = pageSettings.PageType = pageSettings.PageType.ToLower() == "url" ? "URL" : (pageSettings.DisableLink && pageSettings.IncludeInMenu) ? "Folder" : "Standard";
                exportTemplate.Templates.Add(layout);
            }
            string serializedExportTemplate = JsonConvert.SerializeObject(exportTemplate);
            if (!string.IsNullOrEmpty(serializedExportTemplate))
            {
                byte[] fileBytes = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        AddZipItem("Template.json", Encoding.ASCII.GetBytes(serializedExportTemplate), zip);

                        //AddZipItem(ScreenShotFileInfo.FileName, ToByteArray(FileManager.Instance.GetFileContent(ScreenShotFileInfo)), zip);

                        if (Assets != null && Assets.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> asset in Assets)
                            {
                                string FileName = asset.Key.Replace(Vanjaro.Core.Managers.PageManager.ExportTemplateRootToken, "");
                                string FileUrl = asset.Value;
                                if (FileUrl.StartsWith("/"))
                                {
                                    FileUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileUrl);
                                }
                                AddZipItem("Assets/" + FileName, new WebClient().DownloadData(FileUrl), zip);
                            }
                        }
                        if (!string.IsNullOrEmpty(Theme))
                        {
                            string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/" + PortalID + "/vThemes/" + Theme);
                            if (Directory.Exists(FolderPath))
                            {
                                foreach (string file in Directory.EnumerateFiles(FolderPath, "*", SearchOption.AllDirectories))
                                {
                                    if (!file.ToLower().Contains("theme.backup.css"))
                                        AddZipItem("Theme" + file.Replace(FolderPath, "").Replace("\\", "/"), File.ReadAllBytes(file), zip);
                                }
                            }
                        }
                    }
                    fileBytes = memoryStream.ToArray();
                }
                string fileName = Name + "_Site.zip";
                Response.Content = new ByteArrayContent(fileBytes.ToArray());
                Response.Content.Headers.Add("x-filename", fileName);
                Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                Response.StatusCode = HttpStatusCode.OK;
            }
            return Response;
        }

        private static void AddZipItem(string zipItemName, byte[] zipData, ZipArchive zip)
        {
            ZipArchiveEntry zipItem = zip.CreateEntry(zipItemName);
            using (MemoryStream originalFileMemoryStream = new MemoryStream(zipData))
            {
                using (Stream entryStream = zipItem.Open())
                {
                    originalFileMemoryStream.CopyTo(entryStream);
                }
            }
        }

        private byte[] ToByteArray(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
            {
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            }
            return buffer;
        }

        public static ActionResult CreatePortal(CreatePortalRequest request)
        {
            SitesController sitecontroller = new SitesController();
            request.SiteTemplate = sitecontroller.GetDefaultTemplate();

            ActionResult actionResult = new ActionResult();
            try
            {
                List<string> errors = new List<string>();
                int portalId = sitecontroller.CreatePortal(errors, GetDomainName(), GetAbsoluteServerPath(),
                    request.SiteTemplate, request.SiteName,
                    request.SiteAlias, request.SiteDescription, request.SiteKeywords,
                    request.IsChildSite, request.HomeDirectory, request.SiteGroupId, request.UseCurrentUserAsAdmin,
                    request.Firstname, request.Lastname, request.Username, request.Email, request.Password,
                    request.PasswordConfirm, request.Question, request.Answer);

                if (portalId < 0)
                {
                    actionResult.AddError("", errors[portalId]);
                    return actionResult;
                }

                if (portalId > 0)
                {
                    Core.Managers.SettingManager.ApplyingSettings(true, portalId);
                }

                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                actionResult.AddError("exceptions", ex.Message);
                return actionResult;
            }
        }
        public static ActionResult DeletePortal(int portalId, PortalSettings portalSettings)
        {
            ActionResult actionResult = new ActionResult();

            string LocalResourceFile = Components.Constants.LocalResourcesFile;

            try
            {
                PortalInfo portal = PortalController.Instance.GetPortal(portalId);
                if (portal != null)
                {
                    if (portal.PortalID != portalSettings.PortalId && !PortalController.IsMemberOfPortalGroup(portal.PortalID))
                    {
                        string strMessage = PortalController.DeletePortal(portal, portalSettings.HomeDirectoryMapPath);// need to check this line
                        if (string.IsNullOrEmpty(strMessage))
                        {
                            DeleteVanjaroPortal(portalId);
                            EventLogController.Instance.AddLog("PortalName", portal.PortalName, portalSettings as IPortalSettings, portalSettings.UserInfo.UserID, EventLogController.EventLogType.PORTAL_DELETED);
                            actionResult.IsSuccess = true;
                            return actionResult;
                        }
                        actionResult.AddError("", strMessage);
                        return actionResult;
                    }
                    actionResult.AddError("PortalDeletionDenied", Localization.GetString("PortalDeletionDenied", LocalResourceFile));
                    return actionResult;


                }
                actionResult.AddError("PortalNotFound", Localization.GetString("PortalNotFound", Components.Constants.LocalResourcesFile));
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }
        public static string GetDomainName()
        {
            return HttpContext.Current != null ? Globals.GetDomainName(HttpContext.Current.Request, true) : string.Empty;
        }
        private static void UpdateDefaultSignUpTab(int PortalID)
        {
            TabInfo SignUpTab = TabController.Instance.GetTabByName("Signup", PortalID);
            PortalInfo portalInfo = PortalController.Instance.GetPortal(PortalID);
            portalInfo.RegisterTabId = SignUpTab != null && !SignUpTab.IsDeleted ? SignUpTab.TabID : Null.NullInteger;
            PortalController.Instance.UpdatePortalInfo(portalInfo);
        }
        public static string GetAbsoluteServerPath()
        {
            string strServerPath = string.Empty;
            if (HttpContext.Current != null)
            {
                strServerPath = HttpContext.Current.Request.MapPath(HttpContext.Current.Request.ApplicationPath);
            }

            if (!strServerPath.EndsWith("\\"))
            {
                strServerPath += "\\";
            }
            return strServerPath;
        }
        private static void DeleteVanjaroPortal(int PortalID)
        {
            Core.Factories.PortalFactory.DeletePages(PortalID);
            Core.Factories.PortalFactory.DeleteCustomBlocks(PortalID);
            Core.Factories.PortalFactory.DeleteSetting(PortalID);
            Core.Factories.PortalFactory.DeleteWorkflows(PortalID);

        }
    }
}
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Sites.Components;
using Dnn.PersonaBar.Sites.Services.Dto;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
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
using Vanjaro.UXManager.Extensions.Menu.Pages.Entities;
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
                        allowDelete = (pinfo.PortalID != PortalID && !PortalController.IsMemberOfPortalGroup(pinfo.PortalID)),
                        IsVjToursGuided = PortalController.Instance.GetPortalSettings(pinfo.PortalID).ContainsKey("VanjaroToursGuided")
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
            Dictionary<int, string> ExportedModulesContent = new Dictionary<int, string>();
            PortalSettings ps = new PortalSettings(PortalID);
            List<Pages.Entities.PagesTreeView> PagesTreeView = Pages.Managers.PagesManager.GetPagesTreeView(new PortalSettings(PortalID), null);
            exportTemplate.Templates.AddRange(ConvertToLayouts(PortalID, Assets, ExportedModulesContent, ps, PagesTreeView));
            string serializedExportTemplate = JsonConvert.SerializeObject(exportTemplate);
            if (!string.IsNullOrEmpty(serializedExportTemplate))
            {
                byte[] fileBytes = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        AddZipItem("Template.json", Encoding.Unicode.GetBytes(serializedExportTemplate), zip);
                        foreach (var exportedModuleContent in ExportedModulesContent)
                            AddZipItem("PortableModules/" + exportedModuleContent.Key + ".json", Encoding.Unicode.GetBytes(exportedModuleContent.Value), zip);

                        string PortalPageTemplatePath = HttpContext.Current.Server.MapPath("~/Portals/" + PortalID + "/vThemes/" + Theme + "/templates/pages/");
                        if (Directory.Exists(PortalPageTemplatePath))
                        {
                            foreach (string layout in Directory.GetFiles(PortalPageTemplatePath, "*.json"))
                            {
                                AddZipItem("Templates/Pages/" + layout.Replace(PortalPageTemplatePath, string.Empty), File.ReadAllBytes(layout), zip);
                            }
                        }

                        //AddZipItem(ScreenShotFileInfo.FileName, ToByteArray(FileManager.Instance.GetFileContent(ScreenShotFileInfo)), zip);

                        if (Assets != null && Assets.Count > 0)
                        {
                            List<string> FileNames = new List<string>();
                            foreach (KeyValuePair<string, string> asset in Assets)
                            {
                                string FileName = asset.Key.Replace(Vanjaro.Core.Managers.PageManager.ExportTemplateRootToken, "");
                                string FileUrl = asset.Value;
                                if (FileUrl.StartsWith("/"))
                                {
                                    FileUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileUrl);
                                }
                                if (!FileNames.Contains(FileName))
                                {
                                    try
                                    {
                                        AddZipItem("Assets/" + FileName, new WebClient().DownloadData(FileUrl), zip);
                                    }
                                    catch (Exception ex) { ExceptionManager.LogException(ex); }
                                }
                                FileNames.Add(FileName);
                            }
                        }
                        if (!string.IsNullOrEmpty(Theme))
                        {
                            string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/" + PortalID + "/vThemes/" + Theme);
                            if (Directory.Exists(FolderPath))
                            {
                                foreach (string file in Directory.EnumerateFiles(FolderPath, "*", SearchOption.AllDirectories))
                                {
                                    if (!file.ToLower().Contains("theme.css") && !file.ToLower().Contains("theme.backup.css") && !file.ToLower().Contains("theme.editor.js"))
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

        private static List<Layout> ConvertToLayouts(int PortalID, Dictionary<string, string> Assets, Dictionary<int, string> ExportedModulesContent, PortalSettings ps, List<PagesTreeView> PagesTreeView)
        {
            List<Layout> result = new List<Layout>();
            int ctr = 1;
            foreach (Pages.Entities.PagesTreeView page in PagesTreeView)
            {
                Dnn.PersonaBar.Pages.Services.Dto.PageSettings pageSettings = Dnn.PersonaBar.Pages.Components.PagesController.Instance.GetPageSettings(page.Value);
                TabInfo tab = TabController.Instance.GetTab(page.Value, PortalID);
                if (!CanExport(pageSettings, tab))
                    continue;
                var version = PageManager.GetLatestVersion(page.Value, PageManager.GetCultureCode(ps));
                Layout layout = new Layout
                {
                    Content = PageManager.TokenizeTemplateLinks(version.Content, false, Assets)
                };

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(layout.Content);
                IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                foreach (HtmlNode item in query.ToList())
                {
                    if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value) && item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                    {
                        string guid = item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value.ToLower();
                        Core.Data.Entities.GlobalBlock Block = Core.Managers.BlockManager.GetGlobalByLocale(PortalID, guid, null);
                        if (Block != null)
                        {
                            if (layout.Blocks == null)
                            {
                                layout.Blocks = new List<Core.Data.Entities.GlobalBlock>();
                            }
                            layout.Blocks.Add(Block);
                        }
                    }
                }
                if (layout.Blocks != null)
                {
                    foreach (Core.Data.Entities.GlobalBlock block in layout.Blocks)
                    {
                        if (!string.IsNullOrEmpty(block.Html))
                            block.Html = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.Html, PortalID), false, Assets);
                        if (!string.IsNullOrEmpty(block.Css))
                            block.Css = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.Css, PortalID), false, Assets);
                        if (!string.IsNullOrEmpty(block.ContentJSON))
                            block.ContentJSON = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.ContentJSON, PortalID), true, Assets);
                        if (!string.IsNullOrEmpty(block.StyleJSON))
                            block.StyleJSON = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.StyleJSON, PortalID), true, Assets);
                    }
                    CacheFactory.Clear(CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", PortalID));
                }
                layout.Name = pageSettings.Name;
                layout.Content = html.DocumentNode.OuterHtml;
                layout.SVG = "";
                layout.ContentJSON = PageManager.TokenizeTemplateLinks(version.ContentJSON, true, Assets);
                layout.Style = PageManager.TokenizeTemplateLinks(version.Style, false, Assets);
                layout.StyleJSON = PageManager.TokenizeTemplateLinks(version.StyleJSON, true, Assets);
                layout.Type = pageSettings.PageType = pageSettings.PageType.ToLower() == "url" ? "URL" : (pageSettings.DisableLink && pageSettings.IncludeInMenu) ? "Folder" : "Standard";
                layout.Children = ConvertToLayouts(PortalID, Assets, ExportedModulesContent, ps, page.children);
                layout.SortOrder = ctr;
                ProcessPortableModules(PortalID, tab, ExportedModulesContent);
                result.Add(layout);
                ctr++;
            }
            return result;
        }

        private static void ProcessPortableModules(int PortalID, TabInfo tab, Dictionary<int, string> ExportedModulesContent)
        {
            foreach (var tabmodule in ModuleController.Instance.GetTabModules(tab.TabID).Values)
            {
                var moduleDef = ModuleDefinitionController.GetModuleDefinitionByID(tabmodule.ModuleDefID);
                var desktopModuleInfo = DesktopModuleController.GetDesktopModule(moduleDef.DesktopModuleID, PortalID);
                if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                {
                    var module = ModuleController.Instance.GetModule(tabmodule.ModuleID, tab.TabID, true);
                    if (!module.IsDeleted && !string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
                    {
                        var businessController = Reflection.CreateObject(
                            module.DesktopModule.BusinessControllerClass,
                            module.DesktopModule.BusinessControllerClass);
                        var controller = businessController as IPortable;
                        var content = controller?.ExportModule(module.ModuleID);
                        if (!string.IsNullOrEmpty(content))
                            ExportedModulesContent.Add(tabmodule.ModuleID, content);
                    }
                }
            }
        }

        private static bool CanExport(Dnn.PersonaBar.Pages.Services.Dto.PageSettings pageSettings, TabInfo tab)
        {
            if (tab.IsDeleted)
                return false;
            string Name = pageSettings.Name.ToLower().Replace(" ", "");
            if (Name == "home" || Name == "signup" || Name == "notfoundpage" || Name == "profile" || Name == "searchresults" || Name == "404errorpage" || Name == "signin")
                return true;
            if (tab.IsVisible && tab.TabPermissions.Where(t => t != null && t.RoleName == "All Users").FirstOrDefault() != null && tab.TabPermissions.Where(t => t != null && t.RoleName == "All Users").FirstOrDefault().AllowAccess)
                return true;
            else
                return false;
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
                ExceptionManager.LogException(ex);
                actionResult.AddError("exceptions", ex.Message);
                return actionResult;
            }
        }
        public static ActionResult DeletePortal(int portalIdToDelete, PortalSettings currentPortalSettings)
        {
            ActionResult actionResult = new ActionResult();
            string LocalResourceFile = Components.Constants.LocalResourcesFile;
            try
            {
                PortalInfo portal = PortalController.Instance.GetPortal(portalIdToDelete);
                if (portal != null)
                {
                    if (portal.PortalId != currentPortalSettings.PortalId && !PortalController.IsMemberOfPortalGroup(portal.PortalId))
                    {
                        PortalSettings ps = new PortalSettings(portal.PortalId);
                        List<UserInfo> portalUsers = SoftDeleteUsers(portal.PortalId);
                        DataCache.ClearHostCache(true);
                        RecyclebinController.Instance.DeleteUsers(portalUsers);
                        string strMessage = PortalController.DeletePortal(portal, GetAbsoluteServerPath());
                        if (string.IsNullOrEmpty(strMessage))
                        {
                            DeleteVanjaroPortal(portal);
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
        private static string GetAbsoluteServerPath()
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
        private static List<UserInfo> SoftDeleteUsers(int portalID)
        {
            List<UserInfo> result = new List<UserInfo>();
            using (Core.Data.Entities.VanjaroRepo rp = new Core.Data.Entities.VanjaroRepo())
            {
                List<int> userids = rp.Query<int>("select u.UserId from " + Core.Data.Scripts.CommonScript.DnnTablePrefix + "UserPortals u join " + Core.Data.Scripts.CommonScript.DnnTablePrefix + "UserPortals u1 on u1.UserId=u.UserId where u.PortalId=@0 group by u.UserId having count(u1.UserId)=1", portalID).ToList();
                if (userids != null && userids.Count > 0)
                {
                    rp.Execute("update " + Core.Data.Scripts.CommonScript.DnnTablePrefix + "UserPortals set IsDeleted=1 where portalid=@0 and userid in(@1)", portalID, userids);
                    foreach (int userid in userids)
                    {
                        result.Add(new UserInfo() { UserID = userid, PortalID = portalID, IsDeleted = true });
                    }
                }
            }
            return result;
        }
        private static void DeleteVanjaroPortal(PortalInfo portal)
        {
            using (Core.Data.Entities.VanjaroRepo rp = new Core.Data.Entities.VanjaroRepo())
            {
                rp.Execute("delete from " + Core.Data.Scripts.CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile where PortalID=@0", portal.PortalId);
            }
            PortalFactory.DeleteWorkflows(portal.PortalId);
        }
    }
}
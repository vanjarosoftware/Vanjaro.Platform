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
using DotNetNuke.UI.Internals;
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
using System.Threading.Tasks;
using System.Web;
using Vanjaro.Core.Data.Entities;
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
            PortalSettings ps = new PortalSettings(PortalID);

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
                HomeScreenIcon = Path.GetFileName(HomeScreenIcon),
                CustomBlocks = GetAllCustomBlocksForExport(ps, Assets),
                Settings = GetExportTemplateSettings(ps.PortalId)
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

        private static Dictionary<string, dynamic> GetExportTemplateSettings(int PortalId)
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            Dictionary<string, string> portalSettings = PortalController.Instance.GetPortalSettings(PortalId);
            if (portalSettings != null)
            {
                foreach (var ps in portalSettings)
                {
                    if (ps.Key.StartsWith("Theme_"))
                        result.Add(ps.Key, ps.Value);
                }
            }
            return result;
        }

        private static List<Core.Data.Entities.CustomBlock> GetAllCustomBlocksForExport(PortalSettings portalSettings, Dictionary<string, string> Assets)
        {
            List<Core.Data.Entities.CustomBlock> Blocks = BlockManager.GetAllCustomBlocks(portalSettings);
            if (Blocks != null)
            {
                foreach (Core.Data.Entities.CustomBlock block in Blocks)
                {
                    if (!string.IsNullOrEmpty(block.ContentJSON))
                    {
                        block.ContentJSON = PageManager.TokenizeTemplateLinks(portalSettings.PortalId, PageManager.DeTokenizeLinks(block.ContentJSON, portalSettings.PortalId), true, Assets);
                        block.ContentJSON = BlockManager.RemovePermissions(null, block.ContentJSON);
                    }
                    if (!string.IsNullOrEmpty(block.StyleJSON))
                        block.StyleJSON = PageManager.TokenizeTemplateLinks(portalSettings.PortalId, PageManager.DeTokenizeLinks(block.StyleJSON, portalSettings.PortalId), true, Assets);
                }
                CacheFactory.Clear(CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", portalSettings.PortalId));
            }
            return Blocks;
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
                if (version == null)
                    version = new Core.Data.Entities.Pages();
                Layout layout = new Layout
                {
                    Content = PageManager.TokenizeTemplateLinks(PortalID, version.Content != null ? version.Content : string.Empty, false, Assets)
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
                            block.Html = PageManager.TokenizeTemplateLinks(PortalID, PageManager.DeTokenizeLinks(block.Html, PortalID), false, Assets);
                        if (!string.IsNullOrEmpty(block.Css))
                            block.Css = PageManager.TokenizeTemplateLinks(PortalID, PageManager.DeTokenizeLinks(block.Css, PortalID), false, Assets);
                        if (!string.IsNullOrEmpty(block.ContentJSON))
                            block.ContentJSON = PageManager.TokenizeTemplateLinks(PortalID, PageManager.DeTokenizeLinks(block.ContentJSON, PortalID), true, Assets);
                        if (!string.IsNullOrEmpty(block.StyleJSON))
                            block.StyleJSON = PageManager.TokenizeTemplateLinks(PortalID, PageManager.DeTokenizeLinks(block.StyleJSON, PortalID), true, Assets);

                        if (!string.IsNullOrEmpty(block.Html))
                        {
                            HtmlDocument bhtml = new HtmlDocument();
                            bhtml.LoadHtml(block.Html);
                            block.ContentJSON = BlockManager.RemovePermissions(bhtml, block.ContentJSON);
                            block.Html = bhtml.DocumentNode.OuterHtml;
                        }
                    }
                    CacheFactory.Clear(CacheFactory.GetCacheKey(CacheFactory.Keys.GlobalBlock + "ALL", PortalID));
                }
                layout.Settings = PageManager.GetLayoutSettings(tab);
                layout.Name = pageSettings.Name;
                layout.SVG = "";
                layout.ContentJSON = PageManager.TokenizeTemplateLinks(PortalID, version.ContentJSON != null ? version.ContentJSON : string.Empty, true, Assets);
                layout.ContentJSON = BlockManager.RemovePermissions(html, layout.ContentJSON);
                layout.Content = html.DocumentNode.OuterHtml;
                layout.Style = PageManager.TokenizeTemplateLinks(PortalID, version.Style != null ? version.Style : string.Empty, false, Assets);
                layout.StyleJSON = PageManager.TokenizeTemplateLinks(PortalID, version.StyleJSON != null ? version.StyleJSON : string.Empty, true, Assets);
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
            else
                return true;
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

        public static ActionResult CreatePortal(CreatePortalRequest request, dynamic template)
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
                    if (template == null)
                        Core.Managers.SettingManager.ApplyingSettings(true, portalId);
                    else
                    {
                        PortalInfo pinfo = PortalController.Instance.GetPortal(portalId);
                        Core.Managers.SettingManager.UpdateValue(portalId, -1, "setting_theme", "Theme", ThemeManager.GetThemes().Where(t => t.GUID.ToString().ToLower() == template.Theme.Value.ToString().ToLower()).FirstOrDefault().Name);
                        ImportTemplate(pinfo, template.TemplatePath.Value, template.TemplateHash.Value);
                    }
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

        public static void ImportTemplate(PortalInfo pinfo, string SiteTemplatePath, string SiteTemplateHash)
        {
            try
            {
                UserInfo portalAdmin = UserController.Instance.GetUser(pinfo.PortalID, pinfo.AdministratorId);
                Core.Managers.SettingManager.ApplyingSettings(false, pinfo.PortalID);
                Core.Managers.SettingManager.AddThemeFont(pinfo.PortalID);
                IFolderInfo fi = FolderManager.Instance.GetFolder(pinfo.PortalID, "Images/");
                IFolderInfo foldersizeinfo = FolderManager.Instance.GetFolder(pinfo.PortalID, fi.FolderPath + ".versions");
                if (foldersizeinfo == null)
                    foldersizeinfo = FolderManager.Instance.AddFolder(pinfo.PortalID, fi.FolderPath + ".versions");
                string path = HttpContext.Current.Server.MapPath("~/Portals/_default/ThemeLibrary/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = path + Path.GetFileName(SiteTemplatePath).Replace(".zip", "");
                if (!Directory.Exists(path))
                {
                    DownloadTemplate(SiteTemplateHash, SiteTemplatePath, path);
                }
                if (Directory.Exists(path))
                {
                    if (SiteTemplateHash != Path.GetFileNameWithoutExtension(Directory.GetFiles(path, "Hash*").FirstOrDefault()).Replace("Hash", ""))
                    {
                        Directory.Delete(path, true);
                        DownloadTemplate(SiteTemplateHash, SiteTemplatePath, path);
                    }
                    if (File.Exists(path + "/Template.json"))
                    {
                        ExportTemplate exportTemplate = JsonConvert.DeserializeObject<ExportTemplate>(File.ReadAllText(path + "/Template.json", Encoding.Unicode));
                        if (exportTemplate != null && exportTemplate.ThemeGuid.ToLower() == ThemeManager.GetCurrent(pinfo.PortalID).GUID.ToLower())
                        {
                            fi = ProcessAssets(pinfo, fi, foldersizeinfo, path);
                            ProcessTemplatePages(pinfo, path);

                            Dictionary<int, int> tabKeyValuePairs = new Dictionary<int, int>();
                            ProcessTemplates(pinfo, portalAdmin, path, exportTemplate, tabKeyValuePairs);

                            ProcessTheme(pinfo, path);

                            ProcessPortalSettings(pinfo, fi, exportTemplate);
                            ProcessCustomBlocks(pinfo, exportTemplate);
                            ProcessTemplateSettings(pinfo, exportTemplate, tabKeyValuePairs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
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
                    if (portal.PortalID != currentPortalSettings.PortalId && !PortalController.IsMemberOfPortalGroup(portal.PortalID))
                    {
                        PortalSettings ps = new PortalSettings(portal.PortalID);
                        List<UserInfo> portalUsers = SoftDeleteUsers(portal.PortalID);
                        DataCache.ClearHostCache(true);
                        RecyclebinController.Instance.DeleteUsers(portalUsers);
                        foreach (var au in portalUsers.Where(p => p.IsAdmin))
                            UserController.RemoveUser(UserController.Instance.GetUser(portal.PortalID, au.UserID));
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
                    actionResult.AddError("PortalDeletionDenied", DotNetNuke.Services.Localization.Localization.GetString("PortalDeletionDenied", LocalResourceFile));
                    return actionResult;
                }
                actionResult.AddError("PortalNotFound", DotNetNuke.Services.Localization.Localization.GetString("PortalNotFound", Components.Constants.LocalResourcesFile));
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
                rp.Execute("delete from " + Core.Data.Scripts.CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile where PortalID=@0", portal.PortalID);
            }
            PortalFactory.DeleteWorkflows(portal.PortalID);
            string portalSystemPath = GetAbsoluteServerPath() + "Portals\\" + portal.PortalID + "-System";
            if (Directory.Exists(portalSystemPath))
                Directory.Delete(portalSystemPath, true);
        }
        private static void DownloadTemplate(string SiteTemplateHash, string SiteTemplatePath, string path)
        {
            new WebClient().DownloadFile(SiteTemplatePath, path + ".zip");
            ZipFile.ExtractToDirectory(path + ".zip", path);
            File.Create(path + "/Hash" + SiteTemplateHash + ".txt").Dispose();
            File.Delete(path + ".zip");
        }
        private static IFolderInfo ProcessAssets(PortalInfo pinfo, IFolderInfo fi, IFolderInfo foldersizeinfo, string path)
        {
            List<string> assets = Directory.GetFiles(path + "/Assets", "*", SearchOption.AllDirectories).ToList();
            if (assets != null && assets.Count > 0)
            {
                Parallel.ForEach(assets,
                new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.50) * 1.0)) },
                asset =>
                {
                    if (fi == null)
                        fi = FolderManager.Instance.AddFolder(pinfo.PortalID, "Images");
                    if (fi != null)
                    {
                        string fileName = Path.GetFileName(asset);
                        if (fileName.ToLower().EndsWith("w.webp") || fileName.ToLower().EndsWith("w.png") || fileName.ToLower().EndsWith("w.jpeg"))
                        {
                            using (FileStream fs = File.OpenRead(asset))
                            {
                                try
                                {
                                    FileManager.Instance.AddFile(foldersizeinfo, fileName, fs);
                                }
                                catch (Exception ex) { ExceptionManager.LogException(ex); }
                            }
                        }
                        else
                        {
                            using (FileStream fs = File.OpenRead(asset))
                            {
                                try
                                {
                                    FileManager.Instance.AddFile(fi, fileName, fs);
                                }
                                catch (Exception ex) { ExceptionManager.LogException(ex); }
                            }
                        }
                    }
                });
            }

            return fi;
        }
        private static void ProcessTemplatePages(PortalInfo pinfo, string path)
        {
            if (Directory.Exists(path + "/Templates/Pages"))
            {
                List<string> TemplatePages = Directory.GetFiles(path + "/Templates/Pages").ToList();
                if (TemplatePages != null && TemplatePages.Count > 0)
                {
                    string PortalPageTemplatePath = HttpContext.Current.Server.MapPath("~/Portals/" + pinfo.PortalID + "/vThemes/" + ThemeManager.GetCurrent(pinfo.PortalID).Name + "/templates/pages/");
                    if (!Directory.Exists(PortalPageTemplatePath))
                        Directory.CreateDirectory(PortalPageTemplatePath);
                    foreach (string tp in TemplatePages)
                    {
                        File.Copy(tp, PortalPageTemplatePath + Path.GetFileName(tp));
                    }
                }
            }
        }
        private static void ProcessTemplates(PortalInfo pinfo, UserInfo portalAdmin, string path, ExportTemplate exportTemplate, Dictionary<int, int> tabKeyValuePairs)
        {
            List<Layout> pageLayouts = exportTemplate.Templates;
            if (pageLayouts != null)
            {
                Layout sitelayout = pageLayouts.Where(p => p.Name.ToLower() == "site").FirstOrDefault();
                if (sitelayout != null && sitelayout.Children != null && sitelayout.Children.Count > 0)
                    pageLayouts.AddRange(sitelayout.Children);
                Layout userlayout = pageLayouts.Where(p => p.Name.ToLower() == "user").FirstOrDefault();
                if (userlayout != null && userlayout.Children != null && userlayout.Children.Count > 0)
                    pageLayouts.AddRange(userlayout.Children);
            }
            Core.Managers.SettingManager.ApplyDefaultLayouts(pinfo, portalAdmin, pageLayouts, path + "/PortableModules");
            Core.Managers.SettingManager.UpdateSignInTab(pinfo, portalAdmin, pageLayouts, true, path + "/PortableModules");
            TabInfo SigninTab = TabController.Instance.GetTabByName("Signin", pinfo.PortalID);
            if (SigninTab != null)
                pinfo.LoginTabId = SigninTab != null && !SigninTab.IsDeleted ? SigninTab.TabID : Null.NullInteger;
            mapTabKeyValuePairs(pageLayouts, tabKeyValuePairs);
            ImportChildren(pinfo, portalAdmin, path + "/PortableModules", tabKeyValuePairs, pageLayouts.Where(p => p.Name.ToLower().Replace(" ", "") == "home" || p.Name.ToLower().Replace(" ", "") == "signup" || p.Name.ToLower().Replace(" ", "") == "notfoundpage" || p.Name.ToLower().Replace(" ", "") == "profile" || p.Name.ToLower().Replace(" ", "") == "searchresults" || p.Name.ToLower().Replace(" ", "") == "404errorpage" || p.Name.ToLower().Replace(" ", "") == "signin" || p.Name.ToLower().Replace(" ", "") == "site" || p.Name.ToLower().Replace(" ", "") == "terms" || p.Name.ToLower().Replace(" ", "") == "user" || p.Name.ToLower().Replace(" ", "") == "privacy").ToList());
            pageLayouts = pageLayouts.Where(p => p.Name.ToLower().Replace(" ", "") != "home" && p.Name.ToLower().Replace(" ", "") != "signup" && p.Name.ToLower().Replace(" ", "") != "notfoundpage" && p.Name.ToLower().Replace(" ", "") != "profile" && p.Name.ToLower().Replace(" ", "") != "searchresults" && p.Name.ToLower().Replace(" ", "") != "404errorpage" && p.Name.ToLower().Replace(" ", "") != "signin" && p.Name.ToLower().Replace(" ", "") != "site" && p.Name.ToLower().Replace(" ", "") != "terms" && p.Name.ToLower().Replace(" ", "") != "user" && p.Name.ToLower().Replace(" ", "") != "privacy").ToList();
            UXManager.Extensions.Menu.Pages.Managers.PagesManager.ImportTemplates(pinfo, portalAdmin, pageLayouts, path + "/PortableModules");
            mapTabKeyValuePairs(pageLayouts, tabKeyValuePairs);
        }
        private static void mapTabKeyValuePairs(List<Layout> pageLayouts, Dictionary<int, int> tabKeyValuePairs)
        {
            if (pageLayouts != null && pageLayouts.Count > 0)
            {
                if (tabKeyValuePairs == null)
                    tabKeyValuePairs = new Dictionary<int, int>();
                foreach (Layout layout in pageLayouts)
                {
                    if (layout.Settings != null && layout.Settings.Count > 0 && layout.Settings.ContainsKey("TabID") && layout.Settings.ContainsKey("NewTabID") && !tabKeyValuePairs.ContainsKey(int.Parse(layout.Settings["TabID"].ToString())))
                        tabKeyValuePairs.Add(int.Parse(layout.Settings["TabID"].ToString()), int.Parse(layout.Settings["NewTabID"].ToString()));
                }
            }
        }
        private static void ImportChildren(PortalInfo pinfo, UserInfo uInfo, string portableModulesPath, Dictionary<int, int> tabKeyValuePairs, List<Layout> layouts)
        {
            int defaultWorkflow = WorkflowManager.GetDefaultWorkflow(0);
            int maxRevisions = WorkflowManager.GetMaxRevisions(0);
            PortalSettings portalSettings = new PortalSettings(pinfo);
            if (portalSettings.PortalAlias == null)
            {
                foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(pinfo.PortalID).Where(alias => alias.IsPrimary))
                    portalSettings.PortalAlias = alias;
            }

            foreach (Layout layout in layouts)
            {
                TabInfo tab = TabController.Instance.GetTabByName(layout.Name.ToLower(), pinfo.PortalID);
                if (tab != null)
                {
                    foreach (Layout childLayout in layout.Children)
                    {
                        TabInfo childtab = TabController.Instance.GetTabByName(childLayout.Name, pinfo.PortalID);
                        if (childtab == null)
                            UXManager.Extensions.Menu.Pages.Managers.PagesManager.ImportTemplate(pinfo, uInfo, portableModulesPath, defaultWorkflow, maxRevisions, portalSettings, childLayout, tab.TabID);
                    }
                }
            }
            mapTabKeyValuePairs(layouts, tabKeyValuePairs);
        }
        private static void ProcessTheme(PortalInfo pinfo, string path)
        {
            if (Directory.Exists(path + "/Theme"))
            {
                SettingManager.Copy(path + "/Theme", HttpContext.Current.Server.MapPath("~/Portals/" + pinfo.PortalID + "/vThemes/" + ThemeManager.GetCurrent(pinfo.PortalID).Name + ""));
                try
                {
                    ThemeManager.ProcessScss(pinfo.PortalID, false);
                }
                catch (Exception ex) { ExceptionManager.LogException(ex); }
            }
        }
        private static void ProcessPortalSettings(PortalInfo pinfo, IFolderInfo fi, ExportTemplate exportTemplate)
        {
            if (!string.IsNullOrEmpty(exportTemplate.LogoFile))
            {
                pinfo.LogoFile = fi.FolderPath + exportTemplate.LogoFile;
                PortalController.Instance.UpdatePortalInfo(pinfo);
            }

            if (!string.IsNullOrEmpty(exportTemplate.FavIcon))
            {
                var favfile = FileManager.Instance.GetFile(fi, exportTemplate.FavIcon);
                if (favfile != null)
                    new FavIcon(pinfo.PortalID).Update(favfile.FileId);
            }

            if (!string.IsNullOrEmpty(exportTemplate.SocialSharingLogo))
            {
                var socialfile = FileManager.Instance.GetFile(fi, exportTemplate.SocialSharingLogo);
                if (socialfile != null)
                    PortalController.UpdatePortalSetting(pinfo.PortalID, "SocialSharingLogo", "FileID=" + socialfile.FileId, true);
            }

            if (!string.IsNullOrEmpty(exportTemplate.HomeScreenIcon))
            {
                var homefile = FileManager.Instance.GetFile(fi, exportTemplate.HomeScreenIcon);
                if (homefile != null)
                    PortalController.UpdatePortalSetting(pinfo.PortalID, "HomeScreenIcon", "FileID=" + homefile.FileId, true);
            }
        }
        private static void ProcessCustomBlocks(PortalInfo pinfo, ExportTemplate exportTemplate)
        {
            if (exportTemplate.CustomBlocks != null && exportTemplate.CustomBlocks.Count > 0)
            {
                PortalSettings portalSettings = new PortalSettings(pinfo.PortalID);
                foreach (CustomBlock _CustomBlocks in exportTemplate.CustomBlocks)
                {
                    GlobalBlock cb = new GlobalBlock();
                    cb.ID = 0;
                    cb.Name = _CustomBlocks.Name;
                    cb.Category = _CustomBlocks.Category;
                    cb.ContentJSON = _CustomBlocks.ContentJSON;
                    cb.StyleJSON = _CustomBlocks.StyleJSON;
                    BlockManager.Add(portalSettings, cb);
                }
            }
        }
        private static void ProcessTemplateSettings(PortalInfo pinfo, ExportTemplate exportTemplate, Dictionary<int, int> tabKeyValuePairs)
        {
            if (exportTemplate.Settings != null && exportTemplate.Settings.Count > 0)
            {
                foreach (var sett in exportTemplate.Settings)
                {
                    PortalController.UpdatePortalSetting(pinfo.PortalID, sett.Key, sett.Value);
                }
            }
        }
    }
}
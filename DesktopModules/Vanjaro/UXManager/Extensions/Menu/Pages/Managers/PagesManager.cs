using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Components.Security;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.Permissions;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities;
using Vanjaro.UXManager.Extensions.Menu.Pages.Entities;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Extensions.Menu.Pages.Factories.AppFactory;
using static Vanjaro.UXManager.Library.Factories;
using Recyclebin = Dnn.PersonaBar.Recyclebin.Components.Dto;

namespace Vanjaro.UXManager.Extensions.Menu.Pages
{
    public static partial class Managers
    {
        public class PagesManager
        {
            #region Public method
            public static ActionResult GetPageDetails(int pageId)
            {
                ActionResult actionResult = new ActionResult();

                if (!SecurityService.Instance.CanManagePage(pageId))
                {
                    //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                }

                try
                {
                    if (actionResult.IsSuccess)
                    {
                        actionResult.Data = PagesController.Instance.GetPageSettings(pageId);
                    }
                }
                catch (PageNotFoundException)
                {
                    //PageNotFound  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.NotFound", DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Components.Constants.LocalResourcesFile));
                }
                return actionResult;
            }
            public static PageSettings GetDefaultSettings(int pageId = 0)
            {
                return PagesController.Instance.GetDefaultSettings(pageId);
            }
            public static ActionResult SavePageDetails(PageSettingLayout PageSettingLayout)
            {
                ActionResult actionResult = new ActionResult();
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                try
                {
                    if (!SecurityService.Instance.CanSavePageDetails(PageSettingLayout.PageSettings))
                    {
                        //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                        actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                    }
                    if (actionResult.IsSuccess)
                    {
                        PageSettingLayout.PageSettings.Clean();
                        TabInfo tab = PagesController.Instance.SavePageDetails(portalSettings, PageSettingLayout.PageSettings);
                        List<TabInfo> tabs = TabController.GetPortalTabs(portalSettings.PortalId, Null.NullInteger, false, true, false, true);

                        actionResult.Data = GetPagesTreeView();
                    }
                }
                catch (PageNotFoundException)
                {
                    //PageNotFound  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.NotFound", DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Components.Constants.LocalResourcesFile));
                }
                catch (PageValidationException ex)
                {
                    actionResult.AddError("HttpStatusCode.OK", ex.Message);
                }
                return actionResult;
            }

            public static ActionResult SavePageDetails(int DefaultWorkflow, int MaxRevisions, PageSettingLayout PageSettingLayout)
            {
                PageSettings pageSettings = PageSettingLayout.PageSettings;
                if (pageSettings.PageType.ToLower() == "url")
                {
                    pageSettings.PageType = pageSettings.PageType.ToLower();
                }
                else if (pageSettings.PageType.ToLower() == "folder")
                {
                    //Page Type Folder
                    pageSettings.DisableLink = true;
                    //pageSettings.IncludeInMenu = true;
                    pageSettings.PageType = "normal";
                }
                else
                {
                    pageSettings.PageType = "normal";
                }

                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;

                dynamic Data = new ExpandoObject();
                Data.NewTabId = 0;
                ActionResult actionResult = new ActionResult();
                try
                {
                    if (!SecurityService.Instance.CanSavePageDetails(pageSettings))
                    {
                        //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                        actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                    }
                    if (actionResult.IsSuccess)
                    {
                        pageSettings.Clean();

                        TabInfo tab = PagesController.Instance.SavePageDetails(portalSettings, pageSettings);

                        //New Page in Vanjaro
                        tab.SkinSrc = "[g]skins/vanjaro/base.ascx";
                        tab.ContainerSrc = "[g]containers/vanjaro/base.ascx";

                        TabController.Instance.UpdateTab(tab);


                        if (tab != null)
                        {
                            if (tab.TabUrls.Count > 0)
                            {
                                TabUrlInfo tabUrl = tab.TabUrls.SingleOrDefault(t => t.IsSystem && t.HttpStatus == "200" && t.SeqNum == 0);
                                if (tabUrl != null)
                                {
                                    Managers.UrlManager.DeleteCustomUrl(new Dnn.PersonaBar.Pages.Components.Dto.UrlIdDto() { Id = tabUrl.SeqNum, TabId = tabUrl.TabId });
                                }
                            }
                            Data.NewTabId = tab.TabID;
                            Core.Managers.PageManager.ChangePageWorkflow(portalSettings, tab.TabID, DefaultWorkflow);
                            if (PageSettingLayout.EntityID == 0)
                            {
                                Managers.PagesManager.UpdatePageWorkflow(DefaultWorkflow, tab.TabID);
                            }
                            else
                            {
                                Core.Managers.SettingManager.UpdateValue(portalSettings.PortalId, tab.TabID, "setting_workflow", "MaxRevisions", MaxRevisions.ToString());
                            }

                            Core.Managers.SettingManager.UpdateValue(portalSettings.PortalId, tab.TabID, Identifier.setting_detail.ToString(), "ReplaceTokens", PageSettingLayout.ReplaceTokens.ToString());

                            if (LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId).Code != PortalSettings.Current.CultureCode)
                            {
                                LocalizationManager.AddProperty(PageSettingLayout.LocaleProperties);
                            }
                        }
                        List<TabInfo> tabs = TabController.GetPortalTabs(portalSettings.PortalId, Null.NullInteger, false, true, false, true);
                        string url = tab.FullUrl;
                        Data.url = tab.FullUrl;
                        actionResult.Data = GetPagesTreeView();
                    }
                }
                catch (PageNotFoundException)
                {
                    //PageNotFound  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.NotFound", DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Components.Constants.LocalResourcesFile));
                }
                catch (PageValidationException ex)
                {
                    actionResult.AddError("HttpStatusCode.OK", ex.Message);
                }
                if (actionResult.IsSuccess)
                {
                    Data.PageSettings = actionResult.Data;
                }

                ReviewSettings ReviewSetting = Core.Managers.PageManager.GetPageReviewSettings(portalSettings);
                Data.IsContentApproval = ReviewSetting.IsContentApproval;
                Data.NextStateName = ReviewSetting.NextStateName;
                Data.IsPageDraft = ReviewSetting.IsPageDraft;
                Data.IsLocked = ReviewSetting.IsLocked;
                Data.IsModeratorEditPermission = ReviewSetting.IsModeratorEditPermission;
                actionResult.Data = Data;
                PageSettingLayout.PageSettings.TabId = Data.NewTabId;
                return actionResult;
            }
            internal static List<Layout> GetLayouts()
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.PageLayout, PortalSettings.Current.PortalId);
                List<Layout> layouts = CacheFactory.Get(CacheKey);
                if (layouts == null)
                {
                    layouts = new List<Layout>();
                    List<string> FolderPaths = new List<string>
                    {
                        HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.GetCurrentThemeName() + "/templates/pages/"),
                        HttpContext.Current.Server.MapPath("~/Portals/" + PortalSettings.Current.PortalId + "/vThemes/" + ThemeManager.GetCurrentThemeName() + "/templates/pages/")
                    };
                    foreach (string FolderPath in FolderPaths)
                    {
                        if (Directory.Exists(FolderPath))
                        {
                            foreach (string layout in Directory.GetFiles(FolderPath, "*.json"))
                            {
                                string stringJson = File.ReadAllText(layout);
                                if (!string.IsNullOrEmpty(stringJson))
                                {
                                    Layout lay = JsonConvert.DeserializeObject<Layout>(stringJson);
                                    if (lay != null)
                                    {
                                        lay.Name = Path.GetFileNameWithoutExtension(layout);
                                        if (layout.ToLower().Contains("_default"))
                                            lay.IsSystem = true;
                                        layouts.Add(lay);
                                    }
                                }
                            }
                        }
                    }
                    CacheFactory.Set(CacheKey, layouts);
                }
                return layouts;
            }
            public static void DeleteLayout(string name)
            {
                string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/" + PortalSettings.Current.PortalId + "/vThemes/" + ThemeManager.GetCurrentThemeName() + "/templates/pages/");
                if (Directory.Exists(FolderPath))
                {
                    File.Delete(FolderPath + name + ".json");
                    CacheFactory.Clear(CacheFactory.Keys.PageLayout);
                }
            }

            public static ActionResult SaveLayoutAs(int pid, string name, dynamic Data, PortalSettings PortalSettings)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    Layout layout = new Layout();
                    Core.Data.Entities.Pages data = Core.Managers.PageManager.GetLatestVersion(pid, true, Core.Managers.PageManager.GetCultureCode(PortalSettings), true);
                    if (data != null)
                    {
                        int PortalId = PortalSettings.PortalId;
                        dynamic PageSettings = GetPageDetails(pid).Data;
                        layout.Content = Core.Managers.PageManager.TokenizeLinks(data.Content, PortalId);

                        HtmlDocument html = new HtmlDocument();
                        html.LoadHtml(layout.Content);
                        IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                        foreach (HtmlNode item in query.ToList())
                        {
                            if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value) && item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                            {
                                string guid = item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value.ToLower();
                                CustomBlock Block = Core.Managers.BlockManager.GetByLocale(PortalId, guid, null);
                                if (Block != null)
                                {
                                    if (layout.Blocks == null)
                                    {
                                        layout.Blocks = new List<CustomBlock>();
                                    }

                                    layout.Blocks.Add(Block);
                                }
                            }
                        }
                        layout.Content = html.DocumentNode.OuterHtml;
                        layout.SVG = Core.Managers.PageManager.TokenizeLinks(Data.Icon.Value, PortalId);
                        layout.ContentJSON = Core.Managers.PageManager.TokenizeLinks(data.ContentJSON, PortalId);
                        layout.Style = Core.Managers.PageManager.TokenizeLinks(data.Style, PortalId);
                        layout.StyleJSON = Core.Managers.PageManager.TokenizeLinks(data.StyleJSON, PortalId);
                        layout.Type = PageSettings.PageType = PageSettings.PageType.ToLower() == "url" ? "URL" : (PageSettings.DisableLink && PageSettings.IncludeInMenu) ? "Folder" : "Standard";
                        string SerializedLayoutData = JsonConvert.SerializeObject(layout);
                        string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/" + PortalId + "/vThemes/" + ThemeManager.GetCurrentThemeName() + "/templates/pages/");
                        if (!Directory.Exists(FolderPath))
                            Directory.CreateDirectory(FolderPath);
                        if (Directory.Exists(FolderPath))
                        {
                            if (!File.Exists(FolderPath + name + ".json"))
                            {
                                File.Create(FolderPath + name + ".json").Dispose();
                            }

                            if (File.Exists(FolderPath + name + ".json"))
                            {
                                File.WriteAllText(FolderPath + name + ".json", SerializedLayoutData);
                                CacheFactory.Clear(CacheFactory.Keys.PageLayout);
                            }
                        }
                    }
                    else
                    {
                        actionResult.AddError("", "");
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    actionResult.IsSuccess = false;
                }
                return actionResult;
            }

            public static ActionResult DeletePage(PageItem page, [FromUri] bool hardDelete = false)
            {
                ActionResult actionResult = new ActionResult();
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                try
                {
                    if (!SecurityService.Instance.CanDeletePage(page.Id))
                    {
                        //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                        actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                    }
                    if (actionResult.IsSuccess)
                    {
                        PagesController.Instance.DeletePage(page, hardDelete, portalSettings);
                        List<PagesTreeView> PagesTree = GetPagesTreeView();
                        if (portalSettings.ActiveTab != null && portalSettings.ActiveTab.TabID == page.Id)
                        {
                            if (portalSettings.HomeTabId != Null.NullInteger)
                            {
                                actionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL(portalSettings.HomeTabId);
                            }
                            else
                            {
                                int firstTabId = PagesTree.Select(a => a.Value).FirstOrDefault();
                                actionResult.RedirectURL = ServiceProvider.NavigationManager.NavigateURL(firstTabId);
                            }
                        }
                        actionResult.Data = new { PagesTree, DeletedPages = GetDeletedPageList().Data };
                    }
                }
                catch (PageNotFoundException)
                {
                    //PageNotFound  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.NotFound", DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Components.Constants.LocalResourcesFile));
                }
                catch (Exception ex)
                {
                    actionResult.AddError("PageDeleteException", ex.Message);
                }

                return actionResult;
            }

            internal static dynamic UpdatePageWorkflow(int workflowID, int TabID)
            {

                dynamic data = new ExpandoObject();
                Workflow workflow = new Workflow();
                if (workflowID > 0)
                {
                    workflow = WorkflowManager.GetWorkflow(workflowID);
                    data.Revisions = workflow.Revisions;
                }
                if (workflowID > 0 && TabID > 0)
                {
                    PageSettings PageSettings = PagesController.Instance.GetPageSettings(TabID);

                    Permissions Permissions = WorkflowManager.GetWorkflowPermission(workflowID);
                    PageSettings.Permissions.RolePermissions.Clear();
                    foreach (RolePermission RolePerm in Permissions.RolePermissions)
                    {
                        Dnn.PersonaBar.Library.DTO.RolePermission rolepermission = new Dnn.PersonaBar.Library.DTO.RolePermission
                        {
                            RoleId = RolePerm.RoleId,
                            RoleName = RolePerm.RoleName,
                            Locked = RolePerm.Locked,
                            IsDefault = RolePerm.IsDefault
                        };

                        rolepermission.Permissions.Clear();
                        foreach (Permission item in RolePerm.Permissions)
                        {
                            Dnn.PersonaBar.Library.DTO.Permission permission = new Dnn.PersonaBar.Library.DTO.Permission
                            {
                                PermissionName = item.PermissionName,
                                PermissionId = item.PermissionId,
                                View = item.View,
                                AllowAccess = item.AllowAccess
                            };

                            rolepermission.Permissions.Add(permission);
                        }

                        PageSettings.Permissions.RolePermissions.Add(rolepermission);
                    }

                    PageSettings.Permissions.UserPermissions.Clear();
                    foreach (UserPermission UserPerm in Permissions.UserPermissions)
                    {
                        Dnn.PersonaBar.Library.DTO.UserPermission userpermission = new Dnn.PersonaBar.Library.DTO.UserPermission
                        {
                            UserId = UserPerm.UserId,
                            DisplayName = UserPerm.DisplayName
                        };

                        userpermission.Permissions.Clear();
                        foreach (Permission item in UserPerm.Permissions)
                        {
                            Dnn.PersonaBar.Library.DTO.Permission permission = new Dnn.PersonaBar.Library.DTO.Permission
                            {
                                PermissionName = item.PermissionName,
                                PermissionId = item.PermissionId,
                                View = item.View,
                                AllowAccess = item.AllowAccess
                            };
                            userpermission.Permissions.Add(permission);
                        }
                        PageSettings.Permissions.UserPermissions.Add(userpermission);
                    }

                    PortalSettings PortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    PagesController.Instance.SavePageDetails(PortalSettings, PageSettings);

                    PageManager.ChangePageWorkflow(PortalSettings, TabID, workflowID);
                    SettingManager.UpdateValue(PortalSettings.PortalId, TabID, "setting_workflow", "MaxRevisions", workflow.Revisions.ToString());
                    data.Permissions = Permissions;
                }

                return data;
            }

            internal static void ApplyLayout(int PortalId, Layout layout, ActionResult ActionResult)
            {
                ApplyLayout(PortalId, layout, ActionResult, true);
            }

            internal static void ApplyLayout(int PortalId, Layout layout, ActionResult ActionResult, bool IsPublished, bool SaveContentJson = true, bool m2v = false)
            {
                if (layout != null)
                {
                    ProcessBlocks(PortalId, layout.Blocks);
                    Dictionary<string, object> LayoutData = new Dictionary<string, object>
                    {
                        ["IsPublished"] = false,
                        ["Comment"] = string.Empty,
                        ["gjs-assets"] = string.Empty,
                        ["gjs-css"] = Core.Managers.PageManager.DeTokenizeLinks(layout.Style.ToString(), PortalId),
                        ["gjs-html"] = Core.Managers.PageManager.DeTokenizeLinks(layout.Content.ToString(), PortalId)
                    };
                    if (SaveContentJson)
                    {
                        LayoutData["gjs-components"] = Core.Managers.PageManager.DeTokenizeLinks(layout.ContentJSON.ToString(), PortalId);
                    }
                    else
                    {
                        LayoutData["gjs-components"] = string.Empty;
                    }

                    LayoutData["gjs-styles"] = Core.Managers.PageManager.DeTokenizeLinks(layout.StyleJSON.ToString(), PortalId);
                    PortalSettings.Current.ActiveTab.TabID = ActionResult.Data.NewTabId;
                    Core.Managers.PageManager.Update(PortalSettings.Current, LayoutData);

                    if (PortalSettings.Current.DefaultLanguage != PortalSettings.Current.CultureCode)
                    {
                        string CultureCode = PortalSettings.Current.CultureCode;
                        PortalSettings.Current.CultureCode = PortalSettings.Current.DefaultLanguage;
                        Core.Managers.PageManager.Update(PortalSettings.Current, LayoutData);
                        PortalSettings.Current.CultureCode = CultureCode;
                    }

                    Core.Data.Entities.Pages DefaultLanguage = PageManager.GetLatestVersion(PortalSettings.Current.ActiveTab.TabID, null, false);
                    if (DefaultLanguage != null && PortalSettings.Current.DefaultLanguage != PortalSettings.Current.CultureCode)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(DefaultLanguage.StateID.Value);
                        DefaultLanguage.Version = 1;
                        DefaultLanguage.Locale = null;
                        DefaultLanguage.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                        DefaultLanguage.IsPublished = true;
                        DefaultLanguage.PublishedBy = PortalSettings.Current.UserId;
                        DefaultLanguage.PublishedOn = DateTime.UtcNow;
                        PageManager.UpdatePage(DefaultLanguage, PortalSettings.Current.UserId);
                    }

                    Core.Data.Entities.Pages Page = PageManager.GetLatestVersion(PortalSettings.Current.ActiveTab.TabID, PageManager.GetCultureCode(PortalSettings.Current), false);
                    if (Page != null)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                        Page.Version = 1;
                        Page.StateID = !m2v ? (state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2) : 1;
                        Page.IsPublished = IsPublished;
                        if (IsPublished)
                        {
                            Page.PublishedBy = PortalSettings.Current.UserId;
                            Page.PublishedOn = DateTime.UtcNow;
                        }
                        else
                        {
                            Page.PublishedBy = null;
                            Page.PublishedOn = null;
                        }
                        Page.Locale = PageManager.GetCultureCode(PortalSettings.Current);
                        PageManager.UpdatePage(Page, PortalSettings.Current.UserId);
                    }

                }
            }
            internal static HttpResponseMessage ExportLayout(int portalID, string name, bool isSystem)
            {
                HttpResponseMessage Response = new HttpResponseMessage();
                Layout baseLayout = GetLayouts().Where(l => l.IsSystem == isSystem && l.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (baseLayout != null)
                {
                    string Theme = Core.Managers.ThemeManager.GetCurrentThemeName();
                    ExportTemplate exportTemplate = new ExportTemplate
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Type = TemplateType.PageTemplate.ToString(),
                        UpdatedOn = DateTime.UtcNow,
                        Templates = new List<Layout>(),
                        ThemeName = Theme,
                        ThemeGuid = "49A70BA1-206B-471F-800A-679799FF09DF"
                    };
                    Dictionary<string, string> Assets = new Dictionary<string, string>();
                    Layout layout = new Layout
                    {
                        Content = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(baseLayout.Content, portalID), false, Assets)
                    };
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(layout.Content);
                    IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                    foreach (HtmlNode item in query.ToList())
                    {
                        if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value) && item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                        {
                            string guid = item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value.ToLower();
                            CustomBlock Block = BlockManager.GetByLocale(portalID, guid, null);
                            if (Block != null)
                            {
                                if (layout.Blocks == null)
                                {
                                    layout.Blocks = new List<CustomBlock>();
                                }
                                layout.Blocks.Add(Block);
                            }
                        }
                    }
                    layout.Name = name;
                    layout.Content = html.DocumentNode.OuterHtml;
                    layout.SVG = "";
                    layout.ContentJSON = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(baseLayout.ContentJSON, portalID), true, Assets);
                    layout.Style = PageManager.DeTokenizeLinks(baseLayout.Style.ToString(), portalID);
                    layout.StyleJSON = PageManager.DeTokenizeLinks(baseLayout.StyleJSON.ToString(), portalID);
                    layout.Type = baseLayout.Type;
                    exportTemplate.Templates.Add(layout);
                    string serializedExportTemplate = JsonConvert.SerializeObject(exportTemplate);
                    if (!string.IsNullOrEmpty(serializedExportTemplate))
                    {
                        byte[] fileBytes = null;
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                            {
                                AddZipItem("ExportTemplate.json", Encoding.ASCII.GetBytes(serializedExportTemplate), zip);

                                if (Assets != null && Assets.Count > 0)
                                {
                                    foreach (KeyValuePair<string, string> asset in Assets)
                                    {
                                        string FileName = asset.Key.Replace(PageManager.ExportTemplateRootToken, "");
                                        string FileUrl = asset.Value;
                                        if (FileUrl.StartsWith("/"))
                                        {
                                            FileUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileUrl);
                                        }
                                        AddZipItem("Assets/" + FileName, new WebClient().DownloadData(FileUrl), zip);
                                    }
                                }
                            }
                            fileBytes = memoryStream.ToArray();
                        }
                        string fileName = name + "_PageTemplate.zip";
                        Response.Content = new ByteArrayContent(fileBytes.ToArray());
                        Response.Content.Headers.Add("x-filename", fileName);
                        Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileName
                        };
                        Response.StatusCode = HttpStatusCode.OK;
                    }
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
            private static void ProcessBlocks(int PortalId, List<CustomBlock> Blocks)
            {
                if (Blocks != null)
                {
                    foreach (CustomBlock item in Blocks)
                    {
                        if (Core.Managers.BlockManager.GetByLocale(PortalId, item.Guid, null) == null)
                        {
                            item.ID = 0;
                            Core.Managers.BlockManager.Add(PortalController.Instance.GetCurrentSettings() as PortalSettings, item, 1);
                        }
                    }
                }
            }

            public static ActionResult CopyPage(int DefaultWorkflow, int MaxRevisions, PageSettingLayout PageSettingLayout)
            {
                ActionResult actionResult = new ActionResult();
                int oldtabid = PageSettingLayout.PageSettings.TabId;
                PageSettingLayout.PageSettings.TabId = 0;
                PageSettingLayout.PageSettings.Url = string.Empty;
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                ActionResult Response = PagesManager.SavePageDetails(DefaultWorkflow, MaxRevisions, PageSettingLayout);
                if (Response.IsSuccess)
                {
                    foreach (string Locale in Core.Managers.PageManager.GetCultureListItems())
                    {
                        string _Locale = Locale == portalSettings.DefaultLanguage ? null : Locale;
                        Core.Data.Entities.Pages latestVersion = Vanjaro.Core.Managers.PageManager.GetLatestVersion(oldtabid, _Locale);
                        if (latestVersion != null)
                        {
                            Dictionary<string, object> Data = new Dictionary<string, object>
                            {
                                ["IsPublished"] = false,
                                ["Comment"] = string.Empty,
                                ["gjs-css"] = latestVersion.Style.ToString(),
                                ["gjs-html"] = latestVersion.Content.ToString(),
                                ["gjs-components"] = latestVersion.ContentJSON.ToString(),
                                ["gjs-styles"] = latestVersion.StyleJSON.ToString()
                            };
                            portalSettings.ActiveTab.TabID = Response.Data.NewTabId;
                            HtmlDocument html = new HtmlDocument();
                            html.LoadHtml(Data["gjs-html"].ToString());
                            IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                            foreach (HtmlNode div in query.ToList())
                            {
                                if (div.Attributes.Where(a => a.Name == "dmid").FirstOrDefault() != null && div.Attributes.Where(a => a.Name == "mid").FirstOrDefault() != null)
                                {
                                    div.Remove();
                                }
                            }
                            Data["gjs-html"] = html.DocumentNode.OuterHtml;

                            Vanjaro.Core.Managers.PageManager.Update(portalSettings, Data);
                            Core.Data.Entities.Pages Page = Core.Managers.PageManager.GetLatestVersion(PortalSettings.Current.ActiveTab.TabID, _Locale);
                            if (Page != null)
                            {
                                WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                                Page.Locale = _Locale;
                                Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                                Page.IsPublished = true;
                                Page.PublishedBy = PortalSettings.Current.UserId;
                                Page.PublishedOn = DateTime.UtcNow;
                                Core.Managers.PageManager.UpdatePage(Page, PortalSettings.Current.UserId);
                            }

                            actionResult.Data = Response.Data;
                            actionResult.Message = Response.Message;

                        }

                    }
                }
                else
                {
                    actionResult.Errors = Response.Errors;
                    actionResult.Message = Response.Message;
                }


                return actionResult;
            }

            public static ActionResult UpdateHireracy(dynamic dynamicdata)
            {
                ActionResult actionResult = new ActionResult();
                PageMoveRequest request;
                if (dynamicdata != null)
                {
                    request = JsonConvert.DeserializeObject<PageMoveRequest>(dynamicdata.Pagemove.ToString());

                    if (!SecurityService.Instance.CanManagePage(request.PageId)
                        || !SecurityService.Instance.CanManagePage(request.ParentId)
                        || !SecurityService.Instance.CanManagePage(request.RelatedPageId)
                        || !SecurityService.Instance.CanManagePage(TabController.Instance.GetTab(request.RelatedPageId, PortalSettings.Current.PortalId)?.ParentId ?? -1))
                    {
                        //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                        actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                    }

                    try
                    {
                        if (actionResult.IsSuccess)
                        {
                            TabInfo tab = PagesController.Instance.MovePage(request);
                            List<TabInfo> tabs = TabController.GetPortalTabs(PortalSettings.Current.PortalId, Null.NullInteger, false, true, false, true);
                            PageItem pageItem = Converters.ConvertToPageItem<PageItem>(tab, tabs);

                            actionResult.Data = pageItem;
                        }
                    }
                    catch (PageNotFoundException)
                    {
                        //PageNotFound  message is hardcoded in DotNetnuke so we localized our side.
                        actionResult.AddError("HttpStatusCode.NotFound", DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Components.Constants.LocalResourcesFile));
                    }
                    catch (PageException ex)
                    {
                        actionResult.AddError("PageMoveRequestException", ex.Message);
                    }
                }

                return actionResult;
            }

            public static ActionResult RestorePage(List<PageItem> pages)
            {
                ActionResult actionResult = new ActionResult();

                StringBuilder errors = new StringBuilder();
                if (pages != null && pages.Any())
                {
                    foreach (
                        TabInfo tab in pages.Select(page => TabController.Instance.GetTab(page.Id, PortalSettings.Current.PortalId)))
                    {
                        if (tab == null)
                        {
                            //PageNotFound  message is hardcoded in DotNetnuke so we localized our side.
                            actionResult.AddError("HttpStatusCode.NotFound", DotNetNuke.Services.Localization.Localization.GetString("PageNotFound", Components.Constants.LocalResourcesFile));
                        }
                        if (actionResult.IsSuccess)
                        {
                            RecyclebinController.Instance.RestoreTab(tab, out string resultmessage);
                            errors.Append(resultmessage);
                        }
                    }
                }
                if (errors != null && errors.Length > 0)
                {
                    actionResult.AddError("Service_RestoreTabModuleError", errors.ToString());
                }
                else
                {
                    actionResult.Data = new { PagesTree = GetPagesTreeView(), DeletedPages = GetDeletedPageList().Data };
                }

                return actionResult;
            }
            public static ActionResult RemovePage(List<Recyclebin.PageItem> pages)
            {
                ActionResult actionResult = new ActionResult();
                StringBuilder errors = new StringBuilder();

                RecyclebinController.Instance.DeleteTabs(pages, errors);
                PortalController.Instance.GetPortalSettings(PortalSettings.Current.PortalId).TryGetValue("Redirect_AfterLogin", out string Redirect_AfterLogin);
                foreach (Recyclebin.PageItem p in pages)
                {
                    Core.Managers.PageManager.Delete(p.Id);
                    Core.Managers.LocalizationManager.RemoveProperty("Page", p.Id);
                    //Update Redirect_AfterLogin TabID home page change.   
                    if (Redirect_AfterLogin == p.Id.ToString())
                    {
                        PortalController.UpdatePortalSetting(PortalSettings.Current.PortalId, "Redirect_AfterLogin", ValidateTabId(p.Id, PortalSettings.Current.PortalId).ToString(), false, PortalSettings.Current.CultureCode, false);
                    }
                }

                if (errors != null && errors.Length > 0)
                {
                    actionResult.AddError("Service_RemoveTabError", string.Format(RecyclebinController.Instance.LocalizeString("Service_RemoveTabError"), errors));
                }

                if (actionResult.IsSuccess)
                {
                    actionResult.Data = new { DeletedPages = GetDeletedPageList().Data };
                }

                return actionResult;
            }

            public static List<PagesTreeView> GetPagesTreeView()
            {
                return GetPagesTreeView(null);
            }

            public static List<PagesTreeView> GetPagesTreeView(IEnumerable<TabInfo> TabInfo)
            {
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                List<PagesTreeView> result = new List<PagesTreeView>();
                IEnumerable<TabInfo> AvailablePages = TabInfo ?? Core.Managers.PageManager.GetPageList(PortalSettings.Current).ToList();
                if (AvailablePages != null)
                {
                    foreach (TabInfo c in AvailablePages)
                    {
                        bool IsRedirectPage = c.TabType.ToString().ToLower() == "url" ? true : false;
                        bool LinkNewWindow = c.TabSettings.ContainsKey("LinkNewWindow") ? Convert.ToBoolean(c.TabSettings["LinkNewWindow"]) : false;

                        //RoleID=-1(All Users)
                        bool HasBeenPublished = (c.TabPermissions.Where(t => t != null && t.RoleID == -1 && t.AllowAccess == true).FirstOrDefault() != null) ? true : false;

                        //Display in Menu Yes and DisbaledLink Yes then page is folder page
                        bool IsFolder = c.DisableLink;
                        bool HasEditPermission = TabPermissionController.HasTabPermission(c.TabPermissions, "EDIT");

                        if (c.TabID > Null.NullInteger)
                        {
                            result.Add(new PagesTreeView { HasContent = Core.Managers.PageManager.GetAllTabIdByPortalID(PortalSettings.Current.PortalId).Where(x => x == c.TabID).FirstOrDefault() > 0 ? true : false, label = c.TabName.TrimStart('.'), Value = c.TabID, selected = false, children = TabInfo == null ? GetPageTreeChildrens(c.TabID, portalSettings) : new List<PagesTreeView>(), usedbyCount = 0, PageUrl = c.FullUrl, FolderPage = IsFolder, LinkNewWindow = LinkNewWindow, HasBeenPublished = HasBeenPublished, IsVisible = c.IsVisible, IsRedirectPage = IsRedirectPage, HasEditPermission = HasEditPermission });
                        }
                    }
                }
                return result;
            }

            //public static List<TabInfo> GetParentPages(PortalSettings portalSettings)
            //{
            //    List<TabInfo> RootCategory = new List<TabInfo>();
            //    List<TabInfo> AvailablePages = PagesController.Instance.GetPageList(PortalSettings.Current).ToList();
            //    foreach (TabInfo pages in AvailablePages)
            //    {
            //        RootCategory.Add(pages);
            //        RootCategory.AddRange(GetChildPages(pages.TabID, ".", portalSettings));
            //    }
            //    RootCategory.Insert(0, new TabInfo { TabID = -1, TabName = "< None Specified > " });
            //    return RootCategory;
            //}

            public static Dictionary<string, dynamic> GetPagePermission(dynamic data, int PortalID)
            {
                Dictionary<string, dynamic> permData = new Dictionary<string, dynamic>();
                Permissions Permissions = new Permissions
                {
                    PermissionDefinitions = new List<Permission>()
                };
                int InheritPermissionID = -1;
                foreach (dynamic p in data.Permissions.PermissionDefinitions)
                {
                    Permission permission = new Permission
                    {
                        PermissionName = p.PermissionName,
                        PermissionId = p.PermissionId,
                        AllowAccess = true
                    };
                    if (permission.PermissionName.ToLower().Replace(" ", "") == "viewtab")
                    {
                        InheritPermissionID = permission.PermissionId;
                    }

                    Permissions.PermissionDefinitions.Add(permission);
                }
                foreach (dynamic RolePerm in data.Permissions.RolePermissions)
                {
                    RolePermission rolepermission = new RolePermission
                    {
                        RoleId = RolePerm.RoleId,
                        RoleName = RolePerm.RoleName,
                        Locked = RolePerm.Locked,
                        IsDefault = RolePerm.IsDefault
                    };
                    foreach (dynamic item in RolePerm.Permissions)
                    {
                        Permission permission = new Permission
                        {
                            PermissionName = item.PermissionName,
                            PermissionId = item.PermissionId,
                            View = item.View,
                            AllowAccess = item.AllowAccess
                        };

                        rolepermission.Permissions.Add(permission);
                    }
                    //Adding default page permission for administrator
                    if (RolePerm.Permissions.Count == 0 && RolePerm.RoleName == "Administrators")
                    {
                        foreach (dynamic p in data.Permissions.PermissionDefinitions)
                        {
                            Permission permission = new Permission
                            {
                                PermissionName = p.PermissionName,
                                PermissionId = p.PermissionId,
                                AllowAccess = true
                            };
                            rolepermission.Permissions.Add(permission);
                        }
                    }
                    Permissions.RolePermissions.Add(rolepermission);
                }
                foreach (dynamic UserPerm in data.Permissions.UserPermissions)
                {
                    UserPermission userpermission = new UserPermission
                    {
                        UserId = UserPerm.UserId
                    };
                    userpermission.AvatarUrl = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalID, userpermission.UserId);
                    UserInfo userInfo = UserController.GetUserById(PortalID, userpermission.UserId);
                    userpermission.Email = userInfo.Email;
                    userpermission.UserName = userInfo.Username;
                    userpermission.DisplayName = UserPerm.DisplayName;

                    foreach (dynamic item in UserPerm.Permissions)
                    {
                        Permission permission = new Permission
                        {
                            PermissionName = item.PermissionName,
                            PermissionId = item.PermissionId,
                            View = item.View,
                            AllowAccess = item.AllowAccess
                        };
                        userpermission.Permissions.Add(permission);
                    }
                    Permissions.UserPermissions.Add(userpermission);
                }

                Permissions.Inherit = false;
                Permissions.ShowInheritCheckBox = false;
                Permissions.InheritPermissionID = InheritPermissionID;
                permData.Add("Permissions", Permissions);

                return permData;
            }
            public static dynamic GetDeletedPageList(int pageIndex = -1, int pageSize = -1)
            {
                List<TabInfo> tabs = GetDeletedTabs(out int totalRecords, pageIndex, pageSize);
                IEnumerable<Recyclebin.PageItem> deletedtabs = from t in tabs select ConvertToPageItem(t, tabs);

                return new { Data = deletedtabs, totalRecords };
            }
            public static List<TabInfo> GetDeletedTabs(out int totalRecords, int pageIndex = -1, int pageSize = -1)
            {
                int adminTabId = PortalSettings.Current.AdminTabId;
                List<TabInfo> tabs = TabController.GetPortalTabs(PortalSettings.Current.PortalId, adminTabId, true, true, true, true);
                IEnumerable<TabInfo> deletedtabs =
                    tabs.Where(t => /*t.ParentId != adminTabId && */t.IsDeleted && TabPermissionController.CanDeletePage(t));
                totalRecords = deletedtabs.Count();
                return pageIndex == -1 || pageSize == -1 ? deletedtabs.ToList() : deletedtabs.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            }
            private static Recyclebin.PageItem ConvertToPageItem(TabInfo tab, IEnumerable<TabInfo> portalTabs)
            {
                return new Recyclebin.PageItem
                {
                    Id = tab.TabID,
                    Name = tab.LocalizedTabName,
                    Url = tab.FullUrl,
                    ChildrenCount = portalTabs != null ? portalTabs.Count(ct => ct.ParentId == tab.TabID) : 0,
                    PublishDate = tab.CreatedOnDate.ToString("MM/dd/yyyy"),
                    Status = RecyclebinController.Instance.GetTabStatus(tab),
                    ParentId = tab.ParentId,
                    Level = tab.Level,
                    IsSpecial = TabController.IsSpecialTab(tab.TabID, PortalSettings.Current),
                    TabPath = tab.TabPath.Replace("//", "/"),
                    LastModifiedOnDate =
                        tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                            CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                    FriendlyLastModifiedOnDate =
                        tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                            CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                    UseDefaultSkin = UseDefaultSkin(tab)
                };
            }
            private static bool UseDefaultSkin(TabInfo tab)
            {
                return !string.IsNullOrEmpty(tab.SkinSrc) && tab.SkinSrc.Equals(PortalSettings.Current.DefaultPortalSkin, StringComparison.OrdinalIgnoreCase);
            }
            #endregion

            #region Private method
            private static List<PagesTreeView> GetPageTreeChildrens(int pageId, PortalSettings portalSettings)
            {
                List<PagesTreeView> result = new List<PagesTreeView>();
                List<TabInfo> PageTreeChildrens = TabController.GetTabsByParent(pageId, portalSettings.PortalId).Where(a => a.IsDeleted == false).ToList();
                if (PageTreeChildrens == null)
                {
                    return result;
                }

                foreach (TabInfo c in PageTreeChildrens)
                {
                    bool IsRedirectPage = c.TabType.ToString().ToLower() == "url" ? true : false;
                    bool LinkNewWindow = c.TabSettings.ContainsKey("LinkNewWindow") ? Convert.ToBoolean(c.TabSettings["LinkNewWindow"]) : false;
                    //RoleID=-1(All Users)
                    bool HasBeenPublished = (c.TabPermissions.Where(t => t != null && t.RoleID == -1 && t.AllowAccess == true).FirstOrDefault() != null) ? true : false;
                    bool HasEditPermission = TabPermissionController.HasTabPermission(c.TabPermissions, "EDIT");

                    if (PageTreeChildrens != null && PageTreeChildrens.Count > 0)
                    {
                        result.Add(new PagesTreeView { HasContent = Core.Managers.PageManager.GetAllTabIdByPortalID(PortalSettings.Current.PortalId).Where(x => x == c.TabID).FirstOrDefault() > 0 ? true : false, label = c.TabName.TrimStart('.'), Value = c.TabID, selected = false, children = GetPageTreeChildrens(c.TabID, portalSettings), usedbyCount = 0, PageUrl = c.FullUrl, FolderPage = c.DisableLink, LinkNewWindow = LinkNewWindow, HasBeenPublished = HasBeenPublished, IsVisible = c.IsVisible, IsRedirectPage = IsRedirectPage, HasEditPermission = HasEditPermission });
                    }
                }
                return result;
            }
            //private static List<TabInfo> GetChildPages(int TabId, string NamePrefix, PortalSettings portalSettings)
            //{
            //    List<TabInfo> ChildPages = new List<TabInfo>();
            //    List<TabInfo> PageChildrens = TabController.GetTabsByParent(TabId, portalSettings.PortalId).Where(a => a.IsDeleted == false).ToList();
            //    foreach (var item in PageChildrens)
            //    {
            //        TabInfo currentCat = item as TabInfo; 
            //        currentCat.TabName = NamePrefix + " " + currentCat.TabName.TrimStart('.');
            //        ChildPages.Add(item as TabInfo);
            //        ChildPages.AddRange(GetChildPages(item.TabID, NamePrefix + ".", portalSettings));
            //    }
            //    return ChildPages;
            //}
            private static TabInfo TabSanitizer(int tabId, int portalId)
            {
                TabInfo tab = TabController.Instance.GetTab(tabId, portalId);
                if (tab != null && !tab.IsDeleted)
                {
                    return tab;
                }
                else
                {
                    return null;
                }
            }

            private static int ValidateTabId(int tabId, int portalId)
            {
                TabInfo tab = TabController.Instance.GetTab(tabId, portalId);
                return tab != null && !tab.IsDeleted ? tab.TabID : Null.NullInteger;
            }
            #endregion

            #region Default Page Setting
            public static ActionResult GetDefaultPagesSettings(int? portalId, string cultureCode)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalSettings portalS = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
                    int pid = portalId ?? portalS.PortalId;
                    if (!userInfo.IsSuperUser && portalS.PortalId != pid)
                    {
                        actionResult.AddError("HttpStatusCode.Unauthorized", Components.Constants.AuthFailureMessage);
                    }

                    cultureCode = string.IsNullOrEmpty(cultureCode)
                        ? LocaleController.Instance.GetCurrentLocale(pid).Code
                        : cultureCode;

                    Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);
                    if (language == null)
                    {
                        actionResult.AddError("HttpStatusCode.BadRequest", string.Format(DotNetNuke.Services.Localization.Localization.GetString("InvalidLocale.ErrorMessage", Dnn.PersonaBar.SiteSettings.Components.Constants.Constants.LocalResourcesFile), cultureCode));
                    }

                    if (actionResult.IsSuccess)
                    {
                        PortalInfo portal = PortalController.Instance.GetPortal(pid, cultureCode);
                        PortalSettings portalSettings = new PortalSettings(portal);

                        actionResult.Data = new
                        {
                            PortalId = portal.PortalID,
                            portal.CultureCode,
                            SplashTabId = TabSanitizer(portal.SplashTabId, pid)?.TabID,
                            SplashTabName = TabSanitizer(portal.SplashTabId, pid)?.TabName,
                            HomeTabId = TabSanitizer(portal.HomeTabId, pid)?.TabID,
                            HomeTabName = TabSanitizer(portal.HomeTabId, pid)?.TabName,
                            LoginTabId = TabSanitizer(portal.LoginTabId, pid)?.TabID,
                            LoginTabName = TabSanitizer(portal.LoginTabId, pid)?.TabName,
                            RegisterTabId = TabSanitizer(portal.RegisterTabId, pid)?.TabID,
                            RegisterTabName = TabSanitizer(portal.RegisterTabId, pid)?.TabName,
                            UserTabId = TabSanitizer(portal.UserTabId, pid)?.TabID,
                            UserTabName = TabSanitizer(portal.UserTabId, pid)?.TabName,
                            SearchTabId = TabSanitizer(portal.SearchTabId, pid)?.TabID,
                            SearchTabName = TabSanitizer(portal.SearchTabId, pid)?.TabName,
                            Custom404TabId = TabSanitizer(portal.Custom404TabId, pid)?.TabID,
                            Custom404TabName = TabSanitizer(portal.Custom404TabId, pid)?.TabName,
                            Custom500TabId = TabSanitizer(portal.Custom500TabId, pid)?.TabID,
                            Custom500TabName = TabSanitizer(portal.Custom500TabId, pid)?.TabName,
                            TermsTabId = TabSanitizer(portal.TermsTabId, pid)?.TabID,
                            TermsTabName = TabSanitizer(portal.TermsTabId, pid)?.TabName,
                            PrivacyTabId = TabSanitizer(portal.PrivacyTabId, pid)?.TabID,
                            PrivacyTabName = TabSanitizer(portal.PrivacyTabId, pid)?.TabName,
                            portalSettings.PageHeadText
                        };
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", null, exc);
                }
                return actionResult;
            }

            public static ActionResult UpdateDefaultPagesSettings(string key, UpdateDefaultPagesSettingsRequest request)
            {
                ActionResult actionResult = new ActionResult();
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
                try
                {
                    int pid = request.PortalId ?? portalSettings.PortalId;
                    if (!userInfo.IsSuperUser && portalSettings.PortalId != pid)
                    {
                        actionResult.AddError("HttpStatusCode.Unauthorized", Components.Constants.AuthFailureMessage);
                    }

                    string cultureCode = string.IsNullOrEmpty(request.CultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : request.CultureCode;
                    Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);

                    if (language == null)
                    {
                        actionResult.AddError("HttpStatusCode.BadRequest", string.Format(DotNetNuke.Services.Localization.Localization.GetString("InvalidLocale.ErrorMessage", Dnn.PersonaBar.SiteSettings.Components.Constants.Constants.LocalResourcesFile), cultureCode));
                    }

                    if (actionResult.IsSuccess)
                    {
                        PortalInfo portalInfo = PortalController.Instance.GetPortal(pid, cultureCode);
                        bool MoveLoginModule = portalInfo.LoginTabId != request.LoginTabId;
                        portalInfo.SplashTabId = ValidateTabId(request.SplashTabId, pid);
                        portalInfo.HomeTabId = ValidateTabId(request.HomeTabId, pid);
                        portalInfo.LoginTabId = ValidateTabId(request.LoginTabId, pid);
                        portalInfo.RegisterTabId = ValidateTabId(request.RegisterTabId, pid);
                        portalInfo.UserTabId = ValidateTabId(request.UserTabId, pid);
                        portalInfo.SearchTabId = ValidateTabId(request.SearchTabId, pid);
                        portalInfo.Custom404TabId = ValidateTabId(request.Custom404TabId, pid);
                        portalInfo.Custom500TabId = ValidateTabId(request.Custom500TabId, pid);
                        portalInfo.TermsTabId = ValidateTabId(request.TermsTabId, pid);
                        portalInfo.PrivacyTabId = ValidateTabId(request.PrivacyTabId, pid);

                        PortalController.Instance.UpdatePortalInfo(portalInfo);
                        PortalController.UpdatePortalSetting(pid, "PageHeadText", string.IsNullOrEmpty(request.PageHeadText) ? "false" : request.PageHeadText);
                        if (actionResult.IsSuccess)
                        {
                            if (MoveLoginModule)
                            {
                                Core.Managers.LoginManager.AddUpdateLoginModule(request.LoginTabId, portalSettings.PortalId);
                            }

                            actionResult.Data = GetDefaultPagesSettings(portalSettings.PortalId, cultureCode).Data;
                            actionResult.Message = DotNetNuke.Services.Localization.Localization.GetString(key, Components.Constants.LocalResourcesFile);
                        }
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", null, exc);
                }
                return actionResult;
            }
            #endregion

            #region Deleted Module (Bind/Restore/Delete)

            public static List<AppItem> GetAllDeletedModules(int PageId, string CultureCode)
            {
                Core.Data.Entities.Pages page = PageManager.GetLatestVersion(PageId, CultureCode);
                List<AppItem> AppItems = new List<AppItem>();
                if (page != null)
                {
                    foreach (ModuleInfo ModuleInfo in ModuleController.Instance.GetTabModules(PageId).Values)
                    {
                        if (!page.Content.Contains("<app id=\"" + ModuleInfo.ModuleID + "\"></app>"))
                        {
                            AppItem AppItem = new AppItem
                            {
                                TabModuleId = ModuleInfo.ModuleID,
                                Title = ModuleInfo.ModuleTitle
                            };
                            AppItems.Add(AppItem);
                        }
                    }
                }
                return AppItems;
            }

            private static AppItem ConvertToModuleItem(ModuleInfo mod)
            {
                TabInfo tab = TabController.Instance.GetTab(mod.TabID, PortalSettings.Current.PortalId);
                return new AppItem
                {
                    Id = mod.ModuleID,
                    Title = mod.ModuleTitle,
                    Name = mod.DesktopModule.FriendlyName,
                    TabModuleId = mod.TabModuleID,
                    PortalId = mod.PortalID,
                    TabName = tab.TabName,
                    TabID = tab.TabID,
                    TabDeleted = tab.IsDeleted,
                    LastModifiedOnDate = mod.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CreateSpecificCulture(mod.CultureCode ?? "en-US")),
                    FriendlyLastModifiedOnDate = mod.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CreateSpecificCulture(mod.CultureCode ?? "en-US"))
                };
            }

            public static void RestoreModule(int PageId, int ModuleId)
            {
                Core.Data.Entities.Pages page = PageManager.GetLatestVersion(PageId, PortalSettings.Current.CultureCode);
                if (page != null)
                {
                    ModuleInfo minfo = ModuleController.Instance.GetModule(ModuleId, PageId, true);
                    if (minfo != null && !page.Content.Contains("<app id=\"" + ModuleId + "\"></app>"))
                    {

                        string Content = page.Content + "<div dmid=\"" + minfo.DesktopModuleID + "\" mid=\"" + ModuleId + "\" uid=\"0\"><div vjmod=\"true\"><app id=\"" + ModuleId + "\"></app></div></div>";
                        Dictionary<string, object> LayoutData = new Dictionary<string, object>
                        {
                            ["IsPublished"] = false,
                            ["Comment"] = string.Empty,
                            ["gjs-assets"] = string.Empty,
                            ["gjs-css"] = page.Style,
                            ["gjs-html"] = Content,
                            ["gjs-components"] = page.ContentJSON,
                            ["gjs-styles"] = page.StyleJSON
                        };
                        PortalSettings.Current.ActiveTab.TabID = page.TabID;
                        Core.Managers.PageManager.Update(PortalSettings.Current, LayoutData);
                    }

                }
            }

            #endregion

            public static PageSettingLayout AddDefaultLocalization(int TabId, PageSettings pageSettings)
            {
                PageSettingLayout pageSettingLayout = new PageSettingLayout(TabId);
                Dictionary<string, Locale> ActiveLocales = new LocaleController().GetLocales(PortalSettings.Current.PortalId);
                if (ActiveLocales.Count > 1)
                {
                    foreach (KeyValuePair<string, Locale> locale in ActiveLocales.Where(a => a.Key.ToLower() == PortalSettings.Current.CultureCode.ToLower()))
                    {
                        if (!pageSettingLayout.GetLocaleProperty(TabId, locale.Key, "Name"))
                        {
                            pageSettingLayout.AddLocaleProperty(TabId, locale.Key, "Name", pageSettings.Name);
                        }

                        if (!pageSettingLayout.GetLocaleProperty(TabId, locale.Key, "Title"))
                        {
                            pageSettingLayout.AddLocaleProperty(TabId, locale.Key, "Title", pageSettings.Title);
                        }

                        if (!pageSettingLayout.GetLocaleProperty(TabId, locale.Key, "Description"))
                        {
                            pageSettingLayout.AddLocaleProperty(TabId, locale.Key, "Description", pageSettings.Description);
                        }
                    }
                }
                return pageSettingLayout;
            }
        }
    }
}
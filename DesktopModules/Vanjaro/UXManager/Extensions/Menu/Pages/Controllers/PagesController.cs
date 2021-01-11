using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Components.Security;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities;
using Vanjaro.UXManager.Extensions.Menu.Pages.Entities;
using Vanjaro.UXManager.Extensions.Menu.Pages.Factories;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using Localization = Dnn.PersonaBar.Pages.Components.Localization;
using Recyclebin = Dnn.PersonaBar.Recyclebin.Components.Dto;
using Url = Dnn.PersonaBar.Pages.Services.Dto.Url;

namespace Vanjaro.UXManager.Extensions.Menu.Pages.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin,edit")]
    public class PagesController : UIEngineController
    {
        [AuthorizeAccessRoles(AccessRoles = "edit")]
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> parameters, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "AppPath", new UIData { Name = "AppPath", Value = HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/DesktopModules/Vanjaro/UXManager/Extensions/Menu/Pages" } }
            };

            PagesController pc = new PagesController();
            switch (Identifier.ToLower())
            {
                case "setting_pages":
                    {
                        dynamic DeletedPages = Managers.PagesManager.GetDeletedPageList();
                        Settings.Add("PagesTree", new UIData { Name = "PagesTree", Options = Managers.PagesManager.GetPagesTreeView() });
                        Settings.Add("DeletedPages", new UIData { Name = "DeletedPages", Options = DeletedPages.Data });
                        Settings.Add("DeletedPagesCount", new UIData { Name = "DeletedPagesCount", Options = DeletedPages.totalRecords });
                        Settings.Add("PageItem", new UIData { Name = "PageItem", Options = new PageItem() });
                        Settings.Add("List_PageItem", new UIData { Name = "List_PageItem", Options = new List<PageItem>() });
                        Settings.Add("PageSetting", new UIData { Name = "PageSetting", Options = pc.GetDefaultPagesSettings(PortalSettings.Current.PortalId, PortalSettings.Current.CultureCode).Data });
                        Settings.Add("DefaultPagesSettingsRequest", new UIData { Name = "DefaultPagesSettingsRequest", Options = new UpdateDefaultPagesSettingsRequest() });
                        Settings.Add("PageMoveRequest", new UIData { Name = "PageMoveRequest", Options = new PageMoveRequest() });

                        Settings.Add("HasTabPermission", new UIData { Name = "HasTabPermission", Value = Factories.AppFactory.GetAccessRoles(userInfo).Contains("admin").ToString().ToLower() });
                        return Settings.Values.ToList();
                    }
                case "setting_detail":
                    {
                        int pid = 0;
                        bool copy = false;
                        string ParentPageValue = "-1", SitemapPriorityValue = "-1";
                        if (parameters.Count > 0)
                        {
                            pid = int.Parse(parameters["pid"]);
                            if (parameters.ContainsKey("copy"))
                            {
                                copy = bool.Parse(parameters["copy"]);
                            }
                        }
                        bool HasTabPermission = pid > 0 ? TabPermissionController.HasTabPermission(TabController.Instance.GetTab(pid, PortalSettings.Current.PortalId).TabPermissions, "EDIT") : true;
                        Settings.Add("HasTabPermission", new UIData { Name = "HasTabPermission", Value = HasTabPermission.ToString() });
                        if (copy)
                        {
                            Settings.Add("IsCopy", new UIData { Name = "IsCopy", Value = copy.ToString() });
                        }

                        if (HasTabPermission)
                        {
                            PageSettings pageSettings = new PageSettings();
                            pageSettings = pid > 0 ? Managers.PagesManager.GetPageDetails(pid).Data : Managers.PagesManager.GetDefaultSettings();
                            if (copy)
                            {
                                pageSettings.Name += "_Copy";
                                pageSettings.AbsoluteUrl = null;
                            }
                            ParentPageValue = pid > 0 && pageSettings.ParentId.HasValue ? pageSettings.ParentId.ToString() : ParentPageValue;
                            SitemapPriorityValue = pid > 0 && pageSettings.SiteMapPriority != Null.NullInteger ? pageSettings.SiteMapPriority.ToString() : SitemapPriorityValue;
                            if (pageSettings.TabId > 0)
                            {
                                Settings.Add("PageUrls", new UIData { Name = "PageUrls", Options = GetCustomUrls(pageSettings.TabId) });
                            }
                            else
                            {
                                Settings.Add("PageUrls", new UIData { Name = "PageUrls", Options = "" });
                            }

                            if (pageSettings.SiteAliases != null)
                            {
                                Settings.Add("SiteAlias", new UIData { Name = "SiteAlias", Options = pageSettings.SiteAliases.Where(x => x.Key == PortalSettings.Current.PortalAlias.PortalAliasID), OptionsText = "Value", OptionsValue = "Key", Value = PortalSettings.Current.PortalAlias.PortalAliasID.ToString() });
                            }

                            Settings.Add("DeletedModules", new UIData { Name = "DeletedModules", Options = Managers.PagesManager.GetAllDeletedModules(pid, PortalSettings.Current.CultureCode) });

                            //disableLink Yes and Display In Menu Yes mean Folder Page
                            if (string.IsNullOrEmpty(pageSettings.PageType))
                            {
                                pageSettings.PageType = "Standard";
                            }

                            pageSettings.PageType = pageSettings.PageType.ToLower() == "url" ? "URL" : (pageSettings.DisableLink && pageSettings.IncludeInMenu) ? "Folder" : "Standard";
                            pageSettings.AllowIndex = pid > 0 ? pageSettings.AllowIndex : true;

                            string SiteUrl = PortalSettings.Current.PortalAlias != null ? PortalSettings.Current.PortalAlias.HTTPAlias : ServiceProvider.NavigationManager.NavigateURL();
                            List<Layout> pageLayouts = Managers.PagesManager.GetLayouts();
                            Settings.Add("PagesTemplate", new UIData { Name = "PagesTemplate", Options = pageSettings });
                            Settings.Add("PageLayouts", new UIData { Name = "PageLayouts", Options = pageLayouts });
                            Settings.Add("Permissions", new UIData { Name = "Permissions", Options = Managers.PagesManager.GetPagePermission(pageSettings, PortalSettings.Current.PortalId) });
                            Settings.Add("ParentPage", new UIData { Name = "ParentPage", Options = Library.Managers.PageManager.GetParentPages(PortalSettings.Current).Select(a => new { a.TabID, a.TabName }), OptionsText = "TabName", OptionsValue = "TabID", Value = ParentPageValue });
                            Settings.Add("SitemapPriority", new UIData { Name = "SitemapPriority", Options = "", OptionsText = "label", OptionsValue = "value", Value = SitemapPriorityValue });
                            Settings.Add("URLType", new UIData { Name = "URLType", Options = Managers.UrlManager.StatusCodes, OptionsText = "Value", OptionsValue = "Key", Value = "200" });
                            Settings.Add("WorkingPageUrls", new UIData { Name = "WorkingPageUrls", Options = new SeoUrl() { SaveUrl = new DotNetNuke.Entities.Urls.SaveUrlDto() } });

                            Settings.Add("ModuleItem", new UIData { Name = "ModuleItem", Options = new Recyclebin.ModuleItem() });
                            Settings.Add("List_ModuleItem", new UIData { Name = "List_ModuleItem", Options = new List<Recyclebin.ModuleItem>() });
                            Settings.Add("DefaultPageTitle", new UIData { Name = "DefaultPageTitle", Value = GetPageTitle(pid, PortalSettings.Current.PortalId) });
                            Settings.Add("SiteUrl", new UIData { Name = "SiteUrl", Value = SiteUrl.EndsWith("/") ? SiteUrl : SiteUrl + "/" });

                            int defaultWorkflow = Core.Managers.WorkflowManager.GetDefaultWorkflow(pageSettings.TabId);
                            Settings.Add("ddlWorkFlows", new UIData { Name = "ddlWorkFlows", Options = Core.Managers.WorkflowManager.GetDDLWorkflow(PortalSettings.Current.PortalId, false), OptionsText = "Text", OptionsValue = "Value", Value = defaultWorkflow.ToString() });
                            Settings.Add("MaxRevisions", new UIData { Name = "MaxRevisions", Value = Core.Managers.WorkflowManager.GetMaxRevisions(pageSettings.TabId).ToString() });
                            Settings.Add("ReplaceTokens", new UIData { Name = "ReplaceTokens", Value = Core.Managers.SettingManager.GetValue(PortalSettings.Current.PortalId, pageSettings.TabId, Identifier, "ReplaceTokens", AppFactory.GetViews()) ?? bool.FalseString });
                            Settings.Add("WorkflowStateInfo", new UIData { Name = "WorkflowStateInfo", Value = Core.Managers.WorkflowManager.GetWorkflowStatesInfo(defaultWorkflow) });

                            DotNetNuke.Services.Localization.Locale DefaultLocale = DotNetNuke.Services.Localization.LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId);
                            Settings.Add("IsDefaultLocale", new UIData { Name = "IsDefaultLocale", Options = pid > 0 ? DefaultLocale.Code == PortalSettings.Current.CultureCode : true, Value = DefaultLocale.Code });
                            Settings.Add("LocalizedPage", new UIData { Name = "LocalizedPage", Options = Managers.PagesManager.AddDefaultLocalization(pid, pageSettings) });
                            Settings.Add("Languages", new UIData { Name = "Languages", Value = PortalSettings.Current.CultureCode, Options = LocalizationManager.GetActiveLocale(PortalSettings.Current.PortalId).Where(a => a.Value.ToLower() == PortalSettings.Current.CultureCode.ToLower()), OptionsText = "Text", OptionsValue = "Value" });
                        }

                        return Settings.Values.ToList();
                    }
                case "setting_savetemplateas":
                    {
                        int pid = 0;
                        if (parameters.Count > 0)
                        {
                            pid = int.Parse(parameters["pid"]);
                        }

                        PageSettings pageSettings = new PageSettings();
                        pageSettings = pid > 0 ? Managers.PagesManager.GetPageDetails(pid).Data : Managers.PagesManager.GetDefaultSettings();
                        Settings.Add("Name", new UIData { Name = "Name", Value = pageSettings.Name });
                        Settings.Add("PID", new UIData { Name = "PID", Value = pid.ToString() });
                        Settings.Add("Icon", new UIData { Name = "Icon", Value = "" });
                        return Settings.Values.ToList();
                    }
                case "setting_recyclebin":
                    {
                        Settings.Add("DeletedPages", new UIData { Name = "DeletedPages", Options = Managers.PagesManager.GetDeletedPageList().Data });
                        return Settings.Values.ToList();
                    }
                case "setting_choosetemplate":
                    {
                        int pid = PortalSettings.Current.ActiveTab.TabID;
                        bool HasTabPermission = pid > 0 ? TabPermissionController.HasTabPermission(TabController.Instance.GetTab(pid, PortalSettings.Current.PortalId).TabPermissions, "EDIT") : true;
                        PageSettings pageSettings = new PageSettings();
                        pageSettings = pid > 0 ? Managers.PagesManager.GetPageDetails(pid).Data : Managers.PagesManager.GetDefaultSettings();
                        List<Layout> pageLayouts = Managers.PagesManager.GetLayouts();
                        Settings.Add("HasTabPermission", new UIData { Name = "HasTabPermission", Value = HasTabPermission.ToString() });
                        Settings.Add("PagesTemplate", new UIData { Name = "PagesTemplate", Options = pageSettings });
                        Settings.Add("PageLayouts", new UIData { Name = "PageLayouts", Options = pageLayouts });
                        Settings.Add("DisplayChooseTemplate", new UIData { Name = "DisplayChooseTemplate", Options = (PageManager.GetPages(PortalSettings.Current.ActiveTab.TabID).Count == 0) });
                        return Settings.Values.ToList();
                    }

                default:
                    return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePageDetails(int DefaultWorkflow, int MaxRevisions, bool Copy, PageSettingLayout PageSettingLayout)
        {
            ActionResult ActionResult;
            if (Copy)
            {
                ActionResult = Managers.PagesManager.CopyPage(DefaultWorkflow, MaxRevisions, PageSettingLayout);
            }
            else
            {
                ActionResult = Managers.PagesManager.SavePageDetails(DefaultWorkflow, MaxRevisions, PageSettingLayout);
                if (ActionResult.IsSuccess && PageSettingLayout.PageLayout != null)
                {
                    try
                    {
                        Managers.PagesManager.ApplyLayout(PortalSettings.PortalId, PageSettingLayout.PageLayout, ActionResult);
                    }
                    catch (Exception ex)
                    {
                        Core.Managers.ExceptionManage.LogException(ex);
                        ActionResult.AddError("", ex.Message);
                    }
                }
                else
                {
                    int TabID = PageSettingLayout.PageSettings.TabId;
                    if (TabID > 0 && PageManager.GetPages(TabID).Count == 0)
                    {
                        try
                        {
                            Layout layout = new Layout
                            {
                                Style = "",
                                Content = "",
                                ContentJSON = "",
                                StyleJSON = ""
                            };
                            layout.Style = "";
                            Managers.PagesManager.ApplyLayout(PortalSettings.PortalId, layout, ActionResult);
                        }
                        catch (Exception ex)
                        {
                            Core.Managers.ExceptionManage.LogException(ex);
                            ActionResult.AddError("", ex.Message);
                        }
                    }
                }


            }
            return ActionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateWorkflow(int WorkflowID, int PageID)
        {
            ActionResult actionresult = new ActionResult
            {
                Data = Managers.PagesManager.UpdatePageWorkflow(WorkflowID, PageID)
            };
            return actionresult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult DeleteLayout(string Name)
        {
            ActionResult ActionResult = new ActionResult();
            Managers.PagesManager.DeleteLayout(Name);
            return ActionResult;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public HttpResponseMessage ExportLayout(string Name, bool IsSystem)
        {
            return Managers.PagesManager.ExportLayout(PortalSettings.Current.PortalId, Name, IsSystem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePage(PageItem page, [FromUri] bool hardDelete = false)
        {
            return Managers.PagesManager.DeletePage(page, hardDelete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchPages(string searchKey = "", string pageType = "", string tags = "", string publishStatus = "All",
           string publishDateStart = "", string publishDateEnd = "", int workflowId = -1, int pageIndex = 0, int pageSize = 10)
        {
            ActionResult actionResult = new ActionResult();
            List<PagesTreeView> PagesTreeList;
            if (!string.IsNullOrEmpty(searchKey))
            {
                IEnumerable<TabInfo> pages = Dnn.PersonaBar.Pages.Components.PagesController.Instance.SearchPages(out _, searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize);
                PagesTreeList = Managers.PagesManager.GetPagesTreeView(PortalSettings.Current, pages);
            }
            else
            {
                PagesTreeList = Managers.PagesManager.GetPagesTreeView();
            }

            actionResult.Data = new { PagesTree = PagesTreeList };
            return actionResult;
        }

        [HttpPost]
        public ActionResult UpdateHireracy(dynamic dynamicdata)
        {
            ActionResult actionResult;
            actionResult = Managers.PagesManager.UpdateHireracy(dynamicdata);

            if (actionResult.HasErrors)
            {
                actionResult.Data = new { PagesTree = Managers.PagesManager.GetPagesTreeView() };
            }

            return actionResult;
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult GetDefaultPagesSettings(int? portalId, string cultureCode)
        {
            return Managers.PagesManager.GetDefaultPagesSettings(portalId, cultureCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateDefaultPagesSettings(string key, UpdateDefaultPagesSettingsRequest request)
        {
            return Managers.PagesManager.UpdateDefaultPagesSettings(key, request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult SaveLayoutAs(int pid, string name, dynamic Data)
        {
            return Managers.PagesManager.SaveLayoutAs(pid, name, Data, PortalSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin,edit")]
        public ActionResult UpdateSettings(string key, dynamic request)
        {
            ActionResult actionResult = new ActionResult();
            if (request.TabId <= 0 && string.IsNullOrEmpty(request.Key) && string.IsNullOrEmpty(request.Value))
            {
                return actionResult;
            }

            TabController tab = new TabController();
            TabInfo tabinfo = tab.GetTab(Convert.ToInt32(request.TabId.Value), PortalSettings.PortalId);
            if (request.Key.Value == "IsVisible")
            {
                tabinfo.IsVisible = Convert.ToBoolean(request.Value.Value);
            }

            if (request.Key.Value == "HasBeenPublished")
            {
                if (tabinfo.TabPermissions.Where(t => t != null && t.RoleName == "All Users").FirstOrDefault() != null)
                {
                    foreach (TabPermissionInfo p in tabinfo.TabPermissions.Where(t => t != null && t.RoleName == "All Users"))
                    {
                        if (request.Value.Value && p.PermissionKey.ToLower() == "view")
                        {
                            p.AllowAccess = true;
                        }
                        else
                        {
                            p.AllowAccess = false;
                        }
                    }
                }
                else
                {
                    tabinfo.TabPermissions.Add(new TabPermissionInfo
                    {
                        PermissionID = 3,
                        TabID = tabinfo.TabID,
                        AllowAccess = true,
                        RoleID = -1,
                        RoleName = "All Users",
                    });
                }
                TabPermissionController.SaveTabPermissions(tabinfo);
            }
            tab.UpdateTab(tabinfo);
            if (actionResult.IsSuccess)
            {
                actionResult.Data = new { PagesTree = Managers.PagesManager.GetPagesTreeView() };
                actionResult.Message = DotNetNuke.Services.Localization.Localization.GetString(key + request.Value.Value, Components.Constants.LocalResourcesFile);
            }
            return actionResult;
        }

        #region Recycle Bin

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestorePage(List<PageItem> pages)
        {
            return Managers.PagesManager.RestorePage(pages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePage(List<Recyclebin.PageItem> pages)
        {
            return Managers.PagesManager.RemovePage(pages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveAllPages()
        {
            List<Recyclebin.PageItem> pages = new List<Recyclebin.PageItem>();
            foreach (dynamic p in Managers.PagesManager.GetDeletedPageList().Data)
            {
                pages.Add(p);
            }

            return Managers.PagesManager.RemovePage(pages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UrlAddUpdate(SeoUrl dto)
        {
            ActionResult actionResult = new ActionResult();
            PageUrlsServices pageUrlsController = new PageUrlsServices();
            PageUrlResult pageUrlResult;
            try
            {
                if (dto.SaveUrl.Id > 0)
                {
                    pageUrlResult = pageUrlsController.UpdateCustomUrl(dto);
                    if (!pageUrlResult.Success)
                    {
                        actionResult.AddError("UpdateCustomUrl.Error", pageUrlResult.ErrorMessage);
                        actionResult.Data = new { pageUrlResult.SuggestedUrlPath };
                    }
                }
                else
                {
                    pageUrlResult = pageUrlsController.CreateCustomUrl(dto);
                    if (!pageUrlResult.Success)
                    {
                        actionResult.AddError("CreateCustomUrl.Error", pageUrlResult.ErrorMessage);
                        actionResult.Data = new { pageUrlResult.SuggestedUrlPath };
                    }
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            if (actionResult.IsSuccess)
            {
                actionResult.Data = GetCustomUrls(dto.TabId);
            }

            return actionResult;
        }

        public static IEnumerable<Url> GetCustomUrls(int pageId)
        {
            ActionResult actionResult = new ActionResult();
            if (!SecurityService.Instance.CanManagePage(pageId))
            {
                actionResult.AddError("CustomUrlPortalAlias.Error", Localization.GetString("CustomUrlPortalAlias.Error"));
            }

            IEnumerable<Url> data = Managers.UrlManager.GetCustomUrls(pageId);
            return data;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCustomUrl(UrlIdDto dto)
        {
            ActionResult actionResult = new ActionResult();
            PageUrlsServices pageUrlsController = new PageUrlsServices();
            PageUrlResult pageUrlResult;
            try
            {
                pageUrlResult = pageUrlsController.DeleteCustomUrl(dto);
                if (!pageUrlResult.Success)
                {
                    actionResult.AddError("DeleteCustomUrl.Error", pageUrlResult.ErrorMessage);
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            if (actionResult.IsSuccess)
            {
                actionResult.Data = GetCustomUrls(dto.TabId);
            }

            return actionResult;
        }

        #endregion

        #region When Migrate Choose Template
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin,edit")]
        public ActionResult ChooseTemplate(PageSettingLayout PageSettingLayout)
        {
            ActionResult ActionResult = new ActionResult
            {
                Data = new ExpandoObject()
            };
            ActionResult.Data.NewTabId = PageSettingLayout.PageSettings.TabId;
            if (PageSettingLayout.PageLayout != null && PageManager.GetPages(PortalSettings.Current.ActiveTab.TabID).Count == 0)
            {
                try
                {
                    Managers.PagesManager.ApplyLayout(PortalSettings.PortalId, PageSettingLayout.PageLayout, ActionResult, false, false, true);
                    ActionResult.RedirectURL = PortalSettings.Current.ActiveTab.FullUrl;
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    ActionResult.AddError("", ex.Message);
                }
            }
            return ActionResult;
        }
        #endregion

        [HttpGet]
        [ValidateAntiForgeryToken]
        public void RestoreModule(int PageId, int ModuleId)
        {
            Managers.PagesManager.RestoreModule(PageId, ModuleId);
        }

        private static string GetPageTitle(int PageId, int PortalId)
        {
            string Title;
            StringBuilder strTitle = new StringBuilder(PortalSettings.Current.PortalName);
            if (PageId > 0)
            {
                TabInfo tabInfo = TabController.Instance.GetTab(PageId, PortalId);
                //Elected for SB over true concatenation here due to potential for long nesting depth
                int i = 1;
                foreach (TabInfo tab in tabInfo.BreadCrumbs)
                {
                    if (i == tabInfo.BreadCrumbs.Count)
                    {
                        strTitle.Append(string.Concat(" > ", string.Empty));
                    }
                    else
                    {
                        strTitle.Append(string.Concat(" > ", tab.TabName));
                    }

                    i++;
                }
                Title = strTitle.ToString();
            }
            else
            {
                Title = strTitle.Append(string.Concat(" > ", string.Empty)).ToString();
            }

            return Title;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
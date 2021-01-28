using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Data.PetaPoco;
using Vanjaro.Core.Data.Scripts;
using static Vanjaro.Core.Components.Enum;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class PageFactory
        {
            public static ReviewContentInfo GetReviewContentInfo(int Version, string Entity, int EntityID, UserInfo UserInfo)
            {
                ReviewContentInfo rinfo = new ReviewContentInfo();
                if (Entity == WorkflowType.Page.ToString())
                {
                    Pages Pages = Version > 0 ? GetByVersion(EntityID, Version, null) : PageManager.GetLatestVersion(EntityID, UserInfo);
                    if (Pages != null)
                    {
                        rinfo.Entity = WorkflowType.Page.ToString();
                        rinfo.EntityID = Pages.TabID;
                        rinfo.IsPublished = Pages.IsPublished;
                        rinfo.StateID = Pages.StateID.Value;
                        rinfo.Version = Pages.Version;
                        return rinfo;
                    }
                }
                return null;

            }
            public static void Update(Pages page, int UserID)
            {
                if (page != null && page.StateID.HasValue)
                {
                    if (page.ID == 0)
                    {
                        page.CreatedBy = UserID;
                        page.UpdatedBy = UserID;
                        page.CreatedOn = DateTime.UtcNow;
                        page.UpdatedOn = DateTime.UtcNow;
                        page.Insert();
                        CacheFactory.Clear(CacheFactory.Keys.Page);
                        RemoveHistory(page.TabID, WorkflowManager.GetMaxRevisions(page.TabID));
                    }
                    else
                    {
                        page.UpdatedBy = UserID;
                        page.UpdatedOn = DateTime.UtcNow;
                        page.Update();
                        CacheFactory.Clear(CacheFactory.Keys.Page);
                    }

                    //For TabIndexer
                    if (page.IsPublished)
                    {
                        TabInfo tabinfo = TabController.Instance.GetTab(page.TabID, page.PortalID);
                        if (tabinfo != null)
                        {
                            TabController.Instance.UpdateTab(tabinfo);
                        }
                    }
                }
            }

            internal static Pages GetByVersion(int TabID, int Version, string Locale)
            {
                return GetAllByTabID(TabID).Where(a => a.Version == Version && a.Locale == Locale).FirstOrDefault();
            }

            internal static Pages Get(int TabID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page + "ALL", TabID);
                Pages Page = CacheFactory.Get(CacheKey);
                if (Page == null)
                {
                    Page = Pages.Query("Where TabID=@0", TabID).FirstOrDefault();
                    CacheFactory.Set(CacheKey, Page);
                }
                return Page;
            }

            internal static List<Pages> GetAllByTabID(int TabID, bool HasTabEditPermission = true)
            {

                string Locale = string.Empty;
                if (!HasTabEditPermission)
                    Locale = PageManager.GetCultureCode(PortalController.Instance.GetCurrentSettings() as PortalSettings);

                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page + "GetAllByTabID", TabID, HasTabEditPermission, Locale);
                List<Pages> _Pages = CacheFactory.Get(CacheKey) as List<Pages>;
                if (_Pages == null)
                {
                    if (HasTabEditPermission)
                        _Pages = Pages.Query("Where TabID=@0", TabID).ToList();
                    else
                    {
                        using (VanjaroRepo db = new VanjaroRepo())
                        {
                            _Pages = db.Fetch<Pages>(PageScript.GetPublishPage(Locale), TabID, Locale, true).ToList();
                            if (_Pages.Count == 0)
                                _Pages = db.Fetch<Pages>(PageScript.GetPublishPage(Null.NullString), TabID, Locale, true).ToList();
                        }
                    }
                    CacheFactory.Set(CacheKey, _Pages);
                }
                return _Pages;
            }

            internal static void Delete(int TabID, int Version)
            {
                Pages.Delete("Where TabID=@0 and Version=@1", TabID, Version);
                CacheFactory.Clear();
            }

            internal static void RemoveHistory(int TabID, int MaxVersion)
            {
                List<int> pages = GetAllByTabID(TabID).OrderByDescending(a => a.Version).Select(a => a.Version).Distinct().Take(MaxVersion).ToList();
                if (pages.Count > 0)
                {
                    RemoveSectionPermissions(TabID, pages);
                    Pages.Delete("Where TabID=@0 and Version not in (" + string.Join(",", pages) + ")", TabID);
                    CacheFactory.Clear(CacheFactory.Keys.Page);

                    List<Pages> Core_pages = GetAllByTabID(TabID).OrderByDescending(a => a.Version).ToList();
                    foreach (KeyValuePair<int, ModuleInfo> item in ModuleController.Instance.GetTabModules(TabID))
                    {
                        bool IsDelete = true;
                        foreach (Pages page in Core_pages)
                        {
                            if (page.Content.Contains("mid=\"" + item.Key + "\"") && page.Content.Contains("<app id=\"" + item.Key + "\">"))
                            {
                                IsDelete = false;
                            }
                        }
                        if (IsDelete && item.Value.ModuleTitle != Components.Constants.AccountLogin)
                        {
                            if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["m2v"]) && !HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys.Contains("skinsrc") && !HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys.Contains("m2v") && !HttpContext.Current.Request.UrlReferrer.AbsoluteUri.Contains("m2v/"))
                            {
                                DeleteModule(TabID, item.Key);
                            }
                        }
                    }
                }
            }

            private static void RemoveSectionPermissions(int TabID, List<int> pages)
            {
                List<int> EntityIDs = new List<int>();
                List<Pages> PagesToDelete = Pages.Query("Where TabID=@0 and Version not in (" + string.Join(",", pages) + ")", TabID).ToList();
                if (PagesToDelete != null)
                {
                    foreach (Pages _page in PagesToDelete)
                    {
                        HtmlDocument html = new HtmlDocument();
                        html.LoadHtml(_page.Content);
                        IEnumerable<HtmlNode> query = html.DocumentNode.SelectNodes("//*[@perm]");
                        if (query != null)
                        {
                            foreach (HtmlNode item in query.ToList())
                            {
                                if (!string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "perm").FirstOrDefault().Value))
                                    EntityIDs.Add(int.Parse(item.Attributes.Where(a => a.Name == "perm").FirstOrDefault().Value));
                            }
                        }
                    }
                }
                if (EntityIDs.Count > 0)
                {
                    SectionPermissionFactory.DeletePermissions(EntityIDs);
                }
            }

            public static void DeleteModule(int TabID, int ModuleID)
            {
                try
                {
                    ModuleController.Instance.DeleteTabModule(TabID, ModuleID, false);
                    DataCache.ClearCache();
                }
                catch (Exception ex)
                {
                    ExceptionManager.LogException(ex);
                }
            }

            internal static List<Pages> GetAllByState(int State)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page + "GetAllByState", State);
                List<Pages> _Pages = CacheFactory.Get(CacheKey) as List<Pages>;
                if (_Pages == null)
                {
                    _Pages = Pages.Query("Where StateID=@0", State).ToList();
                    CacheFactory.Set(CacheKey, _Pages);
                }
                return _Pages;
            }

            internal static Pages GetById(int PageID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page + "GetById", PageID);
                Pages Page = CacheFactory.Get(CacheKey);
                if (Page == null)
                {
                    Page = Pages.Query("Where ID=@0", PageID).FirstOrDefault();
                    CacheFactory.Set(CacheKey, Page);
                }
                return Page;
            }

            internal static void Delete(int TabID)
            {
                Pages.Delete("Where TabID=@0", TabID);
                CacheFactory.Clear();
            }

            internal static List<int> GetAllTabIdByPortalID(int PortalID, bool OnlyPublished)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page + PortalID + "DistinctIds", OnlyPublished);
                List<int> DistinctIDs = CacheFactory.Get(CacheKey);
                if (DistinctIDs == null)
                {
                    if (OnlyPublished)
                    {
                        DistinctIDs = Pages.Query("Where PortalID=@0 and IsPublished=@1", PortalID, OnlyPublished).Select(e => e.TabID).Distinct().ToList();
                    }
                    else
                    {
                        DistinctIDs = Pages.Query("Where PortalID=@0", PortalID).Select(e => e.TabID).Distinct().ToList();
                    }

                    CacheFactory.Set(CacheKey, DistinctIDs);
                }
                return DistinctIDs;
            }
            internal static List<Pages> GetAllPublishedPages(int portalID, string Locale)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page, portalID, "AllPublishedPages", Locale);
                List<Pages> pages = CacheFactory.Get(CacheKey);
                if (pages == null || pages.Count == 0)
                {
                    Sql Query = PageScript.GetAllPublishedPages(portalID, Locale);
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        pages = db.Fetch<Pages>(Query).ToList();
                    }
                    CacheFactory.Set(CacheKey, pages);
                }
                return pages;
            }

            internal static void MigrateToVanjaro(PortalSettings PortalSettings)
            {
                TabInfo Tab = TabController.Instance.GetTab(PortalSettings.ActiveTab.TabID, PortalSettings.Current.PortalId);
                if (Tab != null && Tab.TabID > 0)
                {
                    try
                    {
                        Tab.SkinSrc = "[g]skins/vanjaro/base.ascx";
                        Tab.ContainerSrc = "[g]containers/vanjaro/base.ascx";

                        using (VanjaroRepo db = new VanjaroRepo())
                        {
                            db.Execute(PortalScript.UpdateTabContainerSrc(PortalSettings.Current.PortalId, Tab.TabID));
                        }

                        //Clear Cache for all TabModules
                        foreach (ModuleInfo tabModule in ModuleController.Instance.GetTabModules(Tab.TabID).Values)
                        {
                            DataCache.RemoveCache(string.Format(DataCache.SingleTabModuleCacheKey, tabModule.TabModuleID));
                        }
                        DataCache.ClearModuleCache(Tab.TabID);

                        TabController.Instance.UpdateTab(Tab);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
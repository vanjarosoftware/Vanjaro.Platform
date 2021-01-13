using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Web.InternalServices.Views.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using DotNetNukeSearch = DotNetNuke.Services.Search.Entities;

namespace Vanjaro.UXManager.Extensions.Block.SearchResult
{
    public static partial class Managers
    {
        public class SearchResultManager
        {
            public static ActionResult Search(string search, Dictionary<string, string> Attributes)
            {
                ActionResult actionResult = new ActionResult();
                int pageIndex = Attributes.ContainsKey("data-block-pageindex") && int.TryParse(Attributes["data-block-pageindex"], out int outPageIndex) ? outPageIndex : 1;
                int pageSize = 10; int sortOption = 0;
                string culture = PortalSettings.Current.CultureCode;
                search = (search ?? string.Empty).Trim();
                IList<string> tags = SearchQueryStringParser.Instance.GetTags(search, out string cleanedKeywords);
                DateTime beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
                IList<string> searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(cleanedKeywords, out cleanedKeywords);

                IList<SearchContentSource> contentSources = GetSearchContentSources(searchTypes);
                Hashtable settings = GetSearchModuleSettings(PortalSettings.Current, Attributes);
                List<int> searchTypeIds = GetSearchTypeIds(settings, contentSources);
                IEnumerable<int> moduleDefids = GetSearchModuleDefIds(settings, contentSources);
                List<int> portalIds = GetSearchPortalIds(settings, -1);
                int userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;

                bool more = false;
                int totalHits = 0;
                List<GroupedDetailView> results = new List<GroupedDetailView>();
                if (portalIds.Any() && searchTypeIds.Any() &&
                    (!string.IsNullOrEmpty(cleanedKeywords) || tags.Any()))
                {
                    SearchQuery query = new SearchQuery
                    {
                        KeyWords = cleanedKeywords,
                        Tags = tags,
                        PortalIds = portalIds,
                        SearchTypeIds = searchTypeIds,
                        ModuleDefIds = moduleDefids,
                        BeginModifiedTimeUtc = beginModifiedTimeUtc,
                        EndModifiedTimeUtc = beginModifiedTimeUtc > DateTime.MinValue ? DateTime.MaxValue : DateTime.MinValue,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortField = (SortFields)sortOption,
                        TitleSnippetLength = 120,
                        BodySnippetLength = 300,
                        CultureCode = culture,
                        WildCardSearch = IsWildCardEnabledForModule(Attributes)
                    };

                    try
                    {
                        results = GetGroupedDetailViews(query, userSearchTypeId, out totalHits, out more).ToList();
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.LogException(ex);
                    }
                }
                actionResult.Data = new { results, totalHits, more, results.Count };

                return actionResult;
            }

            private static IList<SearchContentSource> GetSearchContentSources(IList<string> typesList)
            {
                List<SearchContentSource> sources = new List<SearchContentSource>();
                IEnumerable<SearchContentSource> list = InternalSearchController.Instance.GetSearchContentSourceList(PortalSettings.Current.PortalId);

                if (typesList.Any())
                {
                    foreach (IEnumerable<SearchContentSource> contentSources in typesList.Select(t1 => list.Where(src => string.Equals(src.LocalizedName, t1, StringComparison.OrdinalIgnoreCase))))
                    {
                        sources.AddRange(contentSources);
                    }
                }
                else
                {
                    // no types fitler specified, add all available content sources
                    sources.AddRange(list);
                }

                return sources;
            }
            private static Hashtable GetSearchModuleSettings(PortalSettings portalSettings, Dictionary<string, string> Attributes)
            {
                string LinkTarget = (Attributes.ContainsKey("data-block-linktarget") && Attributes["data-block-linktarget"].ToLower() == "false") ? "1" : "0";
                Hashtable searchResultSettings = new Hashtable
                {
                    { "LinkTarget", LinkTarget },
                    { "EnableWildSearch", !(HostController.Instance.GetString("Search_AllowLeadingWildcard", "N") == "Y") ? "False" : "True" }
                };
                return searchResultSettings;
            }
            private static List<int> GetSearchTypeIds(IDictionary settings, IEnumerable<SearchContentSource> searchContentSources)
            {
                List<int> list = new List<int>();
                List<string> configuredList = new List<string>();
                if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForFilters"])))
                {
                    configuredList = Convert.ToString(settings["ScopeForFilters"]).Split('|').ToList();
                }

                // check content source in configured list or not
                foreach (SearchContentSource contentSource in searchContentSources)
                {
                    if (contentSource.IsPrivate)
                    {
                        continue;
                    }

                    if (configuredList.Count > 0)
                    {
                        if (configuredList.Any(l => l.Contains(contentSource.LocalizedName))) // in configured list
                        {
                            list.Add(contentSource.SearchTypeId);
                        }
                    }
                    else
                    {
                        list.Add(contentSource.SearchTypeId);
                    }
                }

                return list.Distinct().ToList();
            }
            private static ModuleInfo GetSearchModule()
            {
                ArrayList arrModules = GetModulesByDefinition(PortalSettings.Current.PortalId, "Search Results");
                ModuleInfo findModule = null;
                if (arrModules.Count > 1)
                {
                    findModule = arrModules.Cast<ModuleInfo>().FirstOrDefault(searchModule => searchModule.CultureCode == PortalSettings.Current.CultureCode);
                }

                return findModule ?? (arrModules.Count > 0 ? (ModuleInfo)arrModules[0] : null);
            }
            private const string ModuleInfosCacheKey = "ModuleInfos{0}";
            private const CacheItemPriority ModuleInfosCachePriority = CacheItemPriority.AboveNormal;
            private const int ModuleInfosCacheTimeOut = 20;
            private static ArrayList GetModulesByDefinition(int portalID, string friendlyName)
            {
                string cacheKey = string.Format(ModuleInfosCacheKey, portalID);
                return CBO.GetCachedObject<ArrayList>(
                    new CacheItemArgs(cacheKey, ModuleInfosCacheTimeOut, ModuleInfosCachePriority),
                    args => CBO.FillCollection(DotNetNuke.Data.DataProvider.Instance().GetModuleByDefinition(portalID, friendlyName), typeof(ModuleInfo)));
            }
            private static IEnumerable<int> GetSearchModuleDefIds(IDictionary settings, IEnumerable<SearchContentSource> searchContentSources)
            {
                List<int> list = new List<int>();
                List<string> configuredList = new List<string>();
                if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForFilters"])))
                {
                    configuredList = Convert.ToString(settings["ScopeForFilters"]).Split('|').ToList();
                }

                // check content source in configured list or not
                foreach (SearchContentSource contentSource in searchContentSources)
                {
                    if (contentSource.IsPrivate)
                    {
                        continue;
                    }

                    if (configuredList.Count > 0)
                    {
                        if (configuredList.Any(l => l.Contains(contentSource.LocalizedName)) && contentSource.ModuleDefinitionId > 0) // in configured list
                        {
                            list.Add(contentSource.ModuleDefinitionId);
                        }
                    }
                    else
                    {
                        if (contentSource.ModuleDefinitionId > 0)
                        {
                            list.Add(contentSource.ModuleDefinitionId);
                        }
                    }
                }

                return list;
            }
            private static List<int> GetSearchPortalIds(IDictionary settings, int portalId)
            {
                UserInfo currentuser = UserController.Instance.GetCurrentUserInfo();
                List<int> list = new List<int>();
                if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForPortals"])))
                {
                    list = Convert.ToString(settings["ScopeForPortals"]).Split('|').Select(s => Convert.ToInt32(s)).ToList();
                }

                if (portalId == -1)
                {
                    portalId = PortalSettings.Current.ActiveTab.PortalID;
                }

                if (portalId > -1 && !list.Contains(portalId))
                {
                    list.Add(portalId);
                }

                //Add Host 
                UserInfo userInfo = currentuser;
                if (userInfo.IsSuperUser)
                {
                    list.Add(-1);
                }

                return list;
            }
            private static bool IsWildCardEnabledForModule(Dictionary<string, string> Attributes)
            {
                Hashtable searchModuleSettings = GetSearchModuleSettings(PortalSettings.Current, Attributes);
                bool enableWildSearch = true;
                if (!string.IsNullOrEmpty(Convert.ToString(searchModuleSettings["EnableWildSearch"])))
                {
                    enableWildSearch = Convert.ToBoolean(searchModuleSettings["EnableWildSearch"]);
                }

                return enableWildSearch;
            }
            private static readonly int HtmlModuleDefitionId = -1;
            internal static IEnumerable<GroupedDetailView> GetGroupedDetailViews(SearchQuery searchQuery, int userSearchTypeId, out int totalHits, out bool more)
            {
                SearchResults searchResults = SearchController.Instance.SiteSearch(searchQuery);
                totalHits = searchResults.TotalHits;
                more = totalHits > searchQuery.PageSize * searchQuery.PageIndex;

                List<GroupedDetailView> groups = new List<GroupedDetailView>();
                Dictionary<string, IList<DotNetNukeSearch.SearchResult>> tabGroups = new Dictionary<string, IList<DotNetNukeSearch.SearchResult>>();

                foreach (DotNetNukeSearch.SearchResult result in searchResults.Results)
                {
                    //var key = result.TabId + result.Url;
                    string key = result.Url;
                    if (!tabGroups.ContainsKey(key))
                    {
                        tabGroups.Add(key, new List<DotNetNukeSearch.SearchResult> { result });
                    }
                    else
                    {
                        //when the result is a user search type, we should only show one result
                        // and if duplicate, we should also reduce the totalHit number.
                        if (result.SearchTypeId != userSearchTypeId ||
                            tabGroups[key].All(r => r.Url != result.Url))
                        {
                            tabGroups[key].Add(result);
                        }
                        else
                        {
                            totalHits--;
                        }
                    }
                }

                bool showFriendlyTitle = false;
                //ActiveModule == null
                //                        || !ActiveModule.ModuleSettings.ContainsKey("ShowFriendlyTitle")
                //                        || Convert.ToBoolean(ActiveModule.ModuleSettings["ShowFriendlyTitle"]);
                foreach (IList<DotNetNukeSearch.SearchResult> results in tabGroups.Values)
                {
                    GroupedDetailView group = new GroupedDetailView();

                    //first entry
                    DotNetNukeSearch.SearchResult first = results[0];
                    group.Title = showFriendlyTitle ? GetFriendlyTitle(first) : first.Title;
                    group.DocumentUrl = first.Url;

                    //Find a different title for multiple entries with same url
                    if (results.Count > 1)
                    {
                        if (first.TabId > 0)
                        {
                            TabInfo tab = TabController.Instance.GetTab(first.TabId, first.PortalId, false);
                            if (tab != null)
                            {
                                group.Title = showFriendlyTitle && !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.TabName;
                            }
                        }
                        else if (first.ModuleId > 0)
                        {
                            string tabTitle = GetTabTitleFromModuleId(first.ModuleId);
                            if (!string.IsNullOrEmpty(tabTitle))
                            {
                                group.Title = tabTitle;
                            }
                        }
                    }
                    else if (first.ModuleDefId > 0 && first.ModuleDefId == HtmlModuleDefitionId) //special handling for Html module
                    {
                        string tabTitle = GetTabTitleFromModuleId(first.ModuleId);
                        if (!string.IsNullOrEmpty(tabTitle))
                        {
                            group.Title = tabTitle;
                            if (first.Title != "Enter Title" && first.Title != "Text/HTML")
                            {
                                group.Title += " > " + first.Title;
                            }

                            first.Title = group.Title;
                        }
                    }

                    foreach (DotNetNukeSearch.SearchResult result in results)
                    {
                        string title = showFriendlyTitle ? GetFriendlyTitle(result) : result.Title;
                        DetailedView detail = new DetailedView
                        {
                            Title = title != null && title.Contains("<") ? HttpUtility.HtmlEncode(title) : title,
                            DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                            DocumentUrl = result.Url,
                            Snippet = result.Snippet,
                            Description = result.Description,
                            DisplayModifiedTime = result.DisplayModifiedTime,
                            Tags = result.Tags.ToList(),
                            AuthorProfileUrl = result.AuthorUserId > 0 ? Globals.UserProfileURL(result.AuthorUserId) : string.Empty,
                            AuthorName = result.AuthorName
                        };
                        group.Results.Add(detail);
                    }

                    groups.Add(group);
                }

                return groups;
            }
            private static string GetFriendlyTitle(DotNetNukeSearch.SearchResult result)
            {
                if (result.Keywords.ContainsKey("title") && !string.IsNullOrEmpty(result.Keywords["title"]))
                {
                    return result.Keywords["title"];
                }

                return result.Title;
            }
            private const string ModuleTitleCacheKey = "SearchModuleTabTitle_{0}";
            private const CacheItemPriority ModuleTitleCachePriority = CacheItemPriority.Normal;
            private const int ModuleTitleCacheTimeOut = 20;
            private static string GetTabTitleFromModuleId(int moduleId)
            {
                // no manual clearing of the cache exists; let is just expire
                string cacheKey = string.Format(ModuleTitleCacheKey, moduleId);

                return CBO.GetCachedObject<string>(new CacheItemArgs(cacheKey, ModuleTitleCacheTimeOut, ModuleTitleCachePriority, moduleId), GetTabTitleCallBack);
            }
            private static object GetTabTitleCallBack(CacheItemArgs cacheItemArgs)
            {
                int moduleId = (int)cacheItemArgs.ParamList[0];
                ModuleInfo moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
                if (moduleInfo != null)
                {
                    TabInfo tab = moduleInfo.ParentTab;

                    return !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.TabName;
                }

                return string.Empty;
            }
        }
    }
}
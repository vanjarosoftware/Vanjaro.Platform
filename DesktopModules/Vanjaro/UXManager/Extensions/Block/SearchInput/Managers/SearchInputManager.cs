using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Web.InternalServices.Views.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
namespace Vanjaro.UXManager.Extensions.Block.SearchInput
{
    public static partial class Managers
    {
        public class SearchInputManager
        {
            #region Private Const Variables
            private const string ModuleTitleCacheKey = "SearchModuleTabTitle_{0}";
            private const string ModuleInfosCacheKey = "ModuleInfos{0}";
            private const int ModuleInfosCacheTimeOut = 20;
            private static readonly int HtmlModuleDefitionId = 0;
            private const CacheItemPriority ModuleInfosCachePriority = CacheItemPriority.AboveNormal;
            private const int ModuleTitleCacheTimeOut = 20;
            private const CacheItemPriority ModuleTitleCachePriority = CacheItemPriority.Normal;
            private static readonly Regex GroupedBasicViewRegex = new Regex("userid(/|\\|=)(\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            #endregion

            internal static List<GroupedBasicView> GetGroupedBasicViews(SearchQuery query, SearchContentSource userSearchSource, int portalId)
            {
                List<GroupedBasicView> results = new List<GroupedBasicView>();
                IEnumerable<BasicView> previews = GetBasicViews(query, out int totalHists);

                foreach (BasicView preview in previews)
                {
                    //if the document type is user, then try to add user pic into preview's custom attributes.
                    if (userSearchSource != null && preview.DocumentTypeName == userSearchSource.LocalizedName)
                    {
                        Match match = GroupedBasicViewRegex.Match(preview.DocumentUrl);
                        if (match.Success)
                        {
                            int userid = Convert.ToInt32(match.Groups[2].Value);
                            UserInfo user = UserController.Instance.GetUserById(portalId, userid);
                            if (user != null)
                            {
                                preview.Attributes.Add("Avatar", user.Profile.PhotoURL);
                            }
                        }
                    }

                    GroupedBasicView groupedResult = results.SingleOrDefault(g => g.DocumentTypeName == preview.DocumentTypeName);
                    if (groupedResult != null)
                    {
                        if (!groupedResult.Results.Any(r => string.Equals(r.DocumentUrl, preview.DocumentUrl)))
                        {
                            groupedResult.Results.Add(new BasicView
                            {
                                Title = preview.Title.Contains("<") ? HttpUtility.HtmlEncode(preview.Title) : preview.Title,
                                Snippet = preview.Snippet,
                                Description = preview.Description,
                                DocumentUrl = preview.DocumentUrl,
                                Attributes = preview.Attributes
                            });
                        }
                    }
                    else
                    {
                        results.Add(new GroupedBasicView(preview));
                    }
                }

                return results;
            }
            internal static IEnumerable<BasicView> GetBasicViews(SearchQuery searchQuery, out int totalHits)
            {
                SearchResults sResult = DotNetNuke.Services.Search.Controllers.SearchController.Instance.SiteSearch(searchQuery);
                totalHits = sResult.TotalHits;
                bool showFriendlyTitle = GetBooleanSetting("ShowFriendlyTitle", true);
                bool showDescription = GetBooleanSetting("ShowDescription", true);
                bool showSnippet = GetBooleanSetting("ShowSnippet", true);
                int maxDescriptionLength = GetIntegerSetting("MaxDescriptionLength", 100);

                return sResult.Results.Select(result =>
                {
                    string description = result.Description;
                    if (!string.IsNullOrEmpty(description) && description.Length > maxDescriptionLength)
                    {
                        description = description.Substring(0, maxDescriptionLength) + "...";
                    }

                    return new BasicView
                    {
                        Title = GetTitle(result, showFriendlyTitle),
                        DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                        DocumentUrl = result.Url,
                        Snippet = showSnippet ? result.Snippet : string.Empty,
                        Description = showDescription ? description : string.Empty
                    };
                });
            }
            private static string GetTitle(SearchResult result, bool showFriendlyTitle = false)
            {
                if (result.ModuleDefId > 0 && result.ModuleDefId == HtmlModuleDefitionId) //special handling for Html module
                {
                    string tabTitle = GetTabTitleFromModuleId(result.ModuleId);
                    if (!string.IsNullOrEmpty(tabTitle))
                    {
                        if (result.Title != "Enter Title" && result.Title != "Text/HTML")
                        {
                            return tabTitle + " > " + result.Title;
                        }

                        return tabTitle;
                    }
                }

                return showFriendlyTitle ? GetFriendlyTitle(result) : result.Title;
            }
            private static bool GetBooleanSetting(string settingName, bool defaultValue)
            {
                if (PortalSettings.Current == null)
                {
                    return defaultValue;
                }

                Hashtable settings = GetSearchModuleSettings();
                if (settings == null || !settings.ContainsKey(settingName))
                {
                    return defaultValue;
                }

                return Convert.ToBoolean(settings[settingName]);
            }
            private static int GetIntegerSetting(string settingName, int defaultValue)
            {
                if (PortalSettings.Current == null)
                {
                    return defaultValue;
                }

                Hashtable settings = GetSearchModuleSettings();
                if (settings == null || !settings.ContainsKey(settingName))
                {
                    return defaultValue;
                }

                string settingValue = Convert.ToString(settings[settingName]);
                if (!string.IsNullOrEmpty(settingValue) && Regex.IsMatch(settingValue, "^\\d+$"))
                {
                    return Convert.ToInt32(settingValue);
                }

                return defaultValue;
            }
            internal static IList<SearchContentSource> GetSearchContentSources(IList<string> typesList)
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
            internal static List<int> GetSearchPortalIds(IDictionary settings, int portalId)
            {
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
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
                if (userInfo.IsSuperUser)
                {
                    list.Add(-1);
                }

                return list;
            }
            internal static IEnumerable<int> GetSearchModuleDefIds(IDictionary settings, IEnumerable<SearchContentSource> searchContentSources)
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
            internal static List<int> GetSearchTypeIds(IDictionary settings, IEnumerable<SearchContentSource> searchContentSources)
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

            internal static IEnumerable<GroupedDetailView> GetGroupedDetailViews(SearchQuery searchQuery, int userSearchTypeId, out int totalHits, out bool more)
            {
                SearchResults searchResults = DotNetNuke.Services.Search.Controllers.SearchController.Instance.SiteSearch(searchQuery);
                //var searchResults = searchQuery.KeyWords;
                totalHits = searchResults.TotalHits;
                more = totalHits > searchQuery.PageSize * searchQuery.PageIndex;

                List<GroupedDetailView> groups = new List<GroupedDetailView>();
                Dictionary<string, IList<SearchResult>> tabGroups = new Dictionary<string, IList<SearchResult>>();

                foreach (SearchResult result in searchResults.Results)
                {
                    //var key = result.TabId + result.Url;
                    string key = result.Url;
                    if (!tabGroups.ContainsKey(key))
                    {
                        tabGroups.Add(key, new List<SearchResult> { result });
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

                bool showFriendlyTitle = false;// ActiveModule == null || !ActiveModule.ModuleSettings.ContainsKey("ShowFriendlyTitle") || Convert.ToBoolean(ActiveModule.ModuleSettings["ShowFriendlyTitle"]);

                foreach (IList<SearchResult> results in tabGroups.Values)
                {
                    GroupedDetailView group = new GroupedDetailView();

                    //first entry
                    SearchResult first = results[0];
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

                    foreach (SearchResult result in results)
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
            private static string GetFriendlyTitle(SearchResult result)
            {
                if (result.Keywords.ContainsKey("title") && !string.IsNullOrEmpty(result.Keywords["title"]))
                {
                    return result.Keywords["title"];
                }

                return result.Title;
            }
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
            internal static Hashtable GetSearchModuleSettings()
            {
                //if (ActiveModule != null && ActiveModule.ModuleDefinition.FriendlyName == "Search Results")
                //{
                //    return ActiveModule.ModuleSettings;
                //}

                ModuleInfo searchModule = GetSearchModule();
                return searchModule?.ModuleSettings;
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
            private static ArrayList GetModulesByDefinition(int portalID, string friendlyName)
            {
                string cacheKey = string.Format(ModuleInfosCacheKey, portalID);
                return CBO.GetCachedObject<ArrayList>(
                    new CacheItemArgs(cacheKey, ModuleInfosCacheTimeOut, ModuleInfosCachePriority),
                    args => CBO.FillCollection(DataProvider.Instance().GetModuleByDefinition(portalID, friendlyName), typeof(ModuleInfo)));
            }
        }
    }
}
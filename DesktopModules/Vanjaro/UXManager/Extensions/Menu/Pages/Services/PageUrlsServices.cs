using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DNNLocalization = DotNetNuke.Services.Localization;

namespace Vanjaro.UXManager.Extensions.Menu.Pages
{

    //PageUrlsServices's DNN PageUrlsController
    internal class PageUrlsServices : ServiceLocator<IPageUrlsController, PageUrlsController>, IPageUrlsController
    {
        private enum SortingFields { None = 0, Url, Locale, Status };

        public PageUrlResult CreateCustomUrl(SeoUrl dto)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            return CreateCustomUrl(dto.SaveUrl, TabController.Instance.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        public PageUrlResult UpdateCustomUrl(SeoUrl dto)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            return UpdateCustomUrl(dto.SaveUrl, TabController.Instance.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        public PageUrlResult DeleteCustomUrl(UrlIdDto dto)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            return DeleteCustomUrl(dto.Id, TabController.Instance.GetTab(dto.TabId, portalSettings.PortalId, false));
        }
        public IEnumerable<Url> GetPageUrls(TabInfo tab, int portalId)
        {

            Lazy<Dictionary<string, DNNLocalization.Locale>> locales = new Lazy<Dictionary<string, DNNLocalization.Locale>>(() => DNNLocalization.LocaleController.Instance.GetLocales(portalId));
            IEnumerable<Url> customUrls = GetSortedUrls(tab, portalId, locales, 1, true, false);
            List<Url> automaticUrls = GetSortedUrls(tab, portalId, locales, 1, true, true).ToList();

            automaticUrls.AddRange(customUrls);
            return automaticUrls.OrderBy(url => url.StatusCode, new KeyValuePairComparer()).ThenBy(url => url.Path);
        }

        public PageUrlResult CreateCustomUrl(SaveUrlDto dto, TabInfo tab)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            PortalInfo aliasPortal = new PortalAliasController().GetPortalByPortalAliasID(dto.SiteAliasKey);

            if (aliasPortal != null && portalSettings.PortalId != aliasPortal.PortalID)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPortalAlias.Error"),
                    SuggestedUrlPath = string.Empty
                };
            }

            string urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            //Clean Url
            FriendlyUrlOptions options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));

            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out bool modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, portalSettings, out modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            if (tab.TabUrls.Any(u => u.Url.ToLowerInvariant() == dto.Path.ValueOrEmpty().ToLowerInvariant()
                                     && (u.PortalAliasId == dto.SiteAliasKey || u.PortalAliasId == -1)))
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("DuplicateUrl.Error")
                };
            }

            int seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;
            Dictionary<string, DNNLocalization.Locale> portalLocales = DNNLocalization.LocaleController.Instance.GetLocales(portalSettings.PortalId);
            string cultureCode = portalLocales.Where(l => l.Value.KeyID == dto.LocaleKey)
                                .Select(l => l.Value.Code)
                                .SingleOrDefault() ?? portalSettings.CultureCode;

            PortalAliasUsageType portalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
            if (portalAliasUsage == PortalAliasUsageType.Default)
            {
                PortalAliasInfo alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalSettings.PortalId)
                                                        .SingleOrDefault(a => a.PortalAliasID == dto.SiteAliasKey);

                if (string.IsNullOrEmpty(cultureCode) || alias == null)
                {
                    return new PageUrlResult
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error")
                    };
                }
            }
            else
            {
                PortalAliasInfo cultureAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalSettings.PortalId)
                                                            .FirstOrDefault(a => a.CultureCode == cultureCode);

                if (portalLocales.Count > 1 && !portalSettings.ContentLocalizationEnabled && (string.IsNullOrEmpty(cultureCode) || cultureAlias == null))
                {
                    return new PageUrlResult
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error")
                    };
                }
            }

            TabUrlInfo tabUrl = new TabUrlInfo
            {
                TabId = tab.TabID,
                SeqNum = seqNum,
                PortalAliasId = dto.SiteAliasKey,
                PortalAliasUsage = portalAliasUsage,
                QueryString = dto.QueryString.ValueOrEmpty(),
                Url = dto.Path.ValueOrEmpty(),
                CultureCode = cultureCode,
                HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                IsSystem = false
            };

            //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
            SaveTabUrl(tabUrl, portalSettings.PortalId, true);
            return new PageUrlResult
            {
                Success = true,
                Id = seqNum // returns Id of the created Url
            };
        }

        public PageUrlResult UpdateCustomUrl(SaveUrlDto dto, TabInfo tab)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            string urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            //Clean Url
            FriendlyUrlOptions options =
                UrlRewriterUtils.ExtendOptionsForCustomURLs(
                    UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));

            //now clean the path
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out bool modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, tab.TabID, portalSettings, out modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            string cultureCode = DNNLocalization.LocaleController.Instance.GetLocales(portalSettings.PortalId)
                .Where(l => l.Value.KeyID == dto.LocaleKey)
                .Select(l => l.Value.Code)
                .SingleOrDefault() ?? portalSettings.DefaultLanguage;

            string statusCodeKey = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture);
            TabUrlInfo tabUrl = tab.TabUrls.SingleOrDefault(t => t.SeqNum == dto.Id && t.HttpStatus == statusCodeKey);

            if (statusCodeKey == "200")
            {
                //We need to check if we are updating a current url or creating a new 200                
                if (tabUrl == null)
                {
                    //Just create Url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = "200",
                        IsSystem = dto.IsSystem // false
                    };
                    //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    if (!tabUrl.Url.Equals("/" + urlPath, StringComparison.OrdinalIgnoreCase))
                    {
                        //Change the original 200 url to a redirect
                        tabUrl.HttpStatus = "301";
                        tabUrl.SeqNum = dto.Id;
                        //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                        SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                        //Add new custom url
                        tabUrl.Url = dto.Path.ValueOrEmpty();
                        tabUrl.HttpStatus = "200";
                        tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                        tabUrl.CultureCode = cultureCode;
                        tabUrl.PortalAliasId = dto.SiteAliasKey;
                        tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                        tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                        //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                        SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    }
                    else
                    {
                        //Update the original 200 url
                        tabUrl.CultureCode = cultureCode;
                        tabUrl.PortalAliasId = dto.SiteAliasKey;
                        tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                        tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                        //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                        SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    }
                }
            }
            else
            {
                //Update the original non 200 url
                if (tabUrl == null)
                {
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = statusCodeKey,
                        IsSystem = dto.IsSystem // false
                    };
                    //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    tabUrl.CultureCode = cultureCode;
                    tabUrl.PortalAliasId = dto.SiteAliasKey;
                    tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                    tabUrl.Url = dto.Path.ValueOrEmpty();
                    tabUrl.HttpStatus = statusCodeKey;
                    tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                    //TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
            }


            return new PageUrlResult
            {
                Success = true
            };
        }

        public PageUrlResult DeleteCustomUrl(int id, TabInfo tab)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            TabUrlInfo tabUrl = tab.TabUrls.SingleOrDefault(u => u.SeqNum == id);

            TabController.Instance.DeleteTabUrl(tabUrl, portalSettings.PortalId, true);
            DataCache.RemoveCache(string.Format(DataCache.TabUrlCacheKey, portalSettings.PortalId));
            DataCache.ClearCache("url_CustomAliasList");
            TabController.Instance.ClearCache(portalSettings.PortalId);
            return new PageUrlResult
            {
                Success = true
            };
        }

        private IEnumerable<Url> GetSortedUrls(TabInfo tab, int portalId, Lazy<Dictionary<string, DNNLocalization.Locale>> locales, int sortColumn, bool sortOrder, bool isSystem)
        {
            FriendlyUrlSettings friendlyUrlSettings = new FriendlyUrlSettings(tab.PortalID);
            List<Url> tabs = new List<Url>();

            if (isSystem)
            {
                //Add generated urls
                foreach (PortalAliasInfo alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId))
                {
                    DNNLocalization.Locale urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == alias.CultureCode);

                    /*var isRedirected = tab.TabUrls.Any(u => u.HttpStatus == "200"
                                                            && u.CultureCode == ((urlLocale != null) ? urlLocale.Code : String.Empty))
                                            || alias.PortalAliasID != PrimaryAliasId;*/

                    bool isRedirected = false;
                    bool isCustom200Urls = tab.TabUrls.Any(u => u.HttpStatus == "200");//are there any custom Urls for this tab?
                    string baseUrl = Globals.AddHTTP(alias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                    if (urlLocale != null)
                    {
                        baseUrl += "&language=" + urlLocale.Code;
                    }

                    string customPath = null;
                    if (isCustom200Urls)
                    {
                        //get the friendlyUrl, including custom Urls
                        customPath = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                                                                                baseUrl,
                                                                                Globals.glbDefaultPage,
                                                                                alias.HTTPAlias,
                                                                                false,
                                                                                friendlyUrlSettings,
                                                                                Guid.Empty);

                        customPath = customPath.Replace(Globals.AddHTTP(alias.HTTPAlias), "");
                    }
                    //get the friendlyUrl and ignore and custom Urls
                    string path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                                                                                baseUrl,
                                                                                Globals.glbDefaultPage,
                                                                                alias.HTTPAlias,
                                                                                true,
                                                                                friendlyUrlSettings,
                                                                                Guid.Empty);

                    path = path.Replace(Globals.AddHTTP(alias.HTTPAlias), "");
                    int status = 200;
                    if (customPath != null && (string.Compare(customPath, path, StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        //difference in custom/standard URL, so standard is 301
                        status = 301;
                        isRedirected = true;
                    }
                    //AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200);
                    //27139 : only show primary aliases in the tab grid (gets too confusing otherwise)
                    if (alias.IsPrimary) //alias was provided to FriendlyUrlCall, so will always get the correct canonical Url back
                    {
                        AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, string.Empty, status, isSystem, friendlyUrlSettings, null);
                    }

                    //Add url with diacritics
                    isRedirected = friendlyUrlSettings.RedirectUnfriendly;
                    string asciiTabPath = TabPathHelper.ReplaceDiacritics(tab.TabPath, out bool replacedDiacritic).Replace("//", "/");
                    if (replacedDiacritic)
                    {
                        if (friendlyUrlSettings.AutoAsciiConvert)
                        {
                            if (friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                            {
                                path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, string.Empty);
                            }
                            path = path.Replace(asciiTabPath, tab.TabPath.Replace("//", "/"));
                            AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, string.Empty, (isRedirected) ? 301 : 200, isSystem, friendlyUrlSettings, null);
                        }
                    }
                    else
                    {
                        //Add url with space
                        if (tab.TabName.Contains(" ") && friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                        {
                            path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, string.Empty);
                            if (customPath != null && string.Compare(customPath, path, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, string.Empty, (isRedirected) ? 301 : 200, isSystem, friendlyUrlSettings, null);
                            }
                        }

                    }
                }
            }

            foreach (TabUrlInfo url in tab.TabUrls.Where(u => u.IsSystem == isSystem).OrderBy(u => u.SeqNum))
            {
                int.TryParse(url.HttpStatus, out int statusCode);

                //27133 : Only show a custom URL 
                if (url.PortalAliasUsage == PortalAliasUsageType.Default)
                {
                    IEnumerable<PortalAliasInfo> aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
                    PortalAliasInfo alias = aliases.FirstOrDefault(primary => primary.IsPrimary == true);
                    if (alias == null)
                    {
                        //if no primary alias just get first in list, need to use something
                        alias = aliases.FirstOrDefault(a => a.PortalID == portalId);
                    }
                    if (alias != null)
                    {
                        DNNLocalization.Locale urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == alias.CultureCode);
                        AddUrlToList(tabs, portalId, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings, url.LastModifiedByUserId);
                    }
                }
                else
                {
                    DNNLocalization.Locale urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == url.CultureCode);
                    PortalAliasInfo alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId)
                        .SingleOrDefault(p => p.PortalAliasID == url.PortalAliasId);
                    if (alias != null)
                    {
                        AddUrlToList(tabs, portalId, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings, url.LastModifiedByUserId);
                    }
                }
            }

            KeyValuePairComparer pairComparer = new KeyValuePairComparer();
            switch ((SortingFields)sortColumn)
            {
                case SortingFields.Url:
                case SortingFields.None:
                    return sortOrder ?
                        tabs.OrderBy(url => url.SiteAlias, pairComparer).ThenBy(url => url.Path) :
                        tabs.OrderByDescending(url => url.SiteAlias, pairComparer).ThenByDescending(url => url.Path);
                case SortingFields.Locale:
                    return sortOrder ?
                        tabs.OrderBy(url => url.Locale, pairComparer) :
                        tabs.OrderByDescending(url => url.Locale, pairComparer);
                case SortingFields.Status:
                    return sortOrder ?
                        tabs.OrderBy(url => url.StatusCode, pairComparer) :
                        tabs.OrderByDescending(url => url.StatusCode, pairComparer);
                default:
                    return sortOrder ?
                        tabs.OrderBy(url => url.SiteAlias, pairComparer).ThenBy(url => url.Path) :
                        tabs.OrderByDescending(url => url.SiteAlias, pairComparer).ThenByDescending(url => url.Path);
            }
        }

        private void AddUrlToList(List<Url> tabs, int portalId, int id, PortalAliasInfo alias, DNNLocalization.Locale urlLocale, string path, string queryString, int statusCode, bool isSystem, FriendlyUrlSettings friendlyUrlSettings, int? lastModifiedByUserId)
        {
            string userName = "";
            if (lastModifiedByUserId.HasValue)
            {
                userName = UserController.Instance.GetUser(portalId, lastModifiedByUserId.Value)?.DisplayName;
            }

            tabs.Add(new Url
            {
                Id = id,
                SiteAlias = new KeyValuePair<int, string>(alias.KeyID, alias.HTTPAlias),
                Path = path,
                PathWithNoExtension = GetCleanPath(path, friendlyUrlSettings),
                QueryString = queryString,
                Locale = (urlLocale != null) ? new KeyValuePair<int, string>(urlLocale.KeyID, urlLocale.EnglishName)
                                             : new KeyValuePair<int, string>(-1, ""),
                StatusCode = StatusCodes.SingleOrDefault(kv => kv.Key == statusCode),
                SiteAliasUsage = (int)PortalAliasUsageType.ChildPagesInherit,
                IsSystem = isSystem,
                UserName = userName
            });
        }

        protected IEnumerable<KeyValuePair<int, string>> StatusCodes => new[]
                {
                    new KeyValuePair<int, string>(200, "Active (200)"),
                    new KeyValuePair<int, string>(301, "Redirect (301)")
                };
        private string GetCleanPath(string path, FriendlyUrlSettings friendlyUrlSettings)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string urlPath = path.TrimStart('/');
            urlPath = UrlRewriterUtils.CleanExtension(urlPath, friendlyUrlSettings, string.Empty);

            return string.Format("/{0}", urlPath);
        }
        public class KeyValuePairComparer : IComparer<KeyValuePair<int, string>>
        {
            public int Compare(KeyValuePair<int, string> pair1, KeyValuePair<int, string> pair2)
            {
                return string.Compare(pair1.Value, pair2.Value, StringComparison.OrdinalIgnoreCase);
            }
        }

        public void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            //var portalAliasId = (tabUrl.PortalAliasUsage == PortalAliasUsageType.Default) ? Null.NullInteger : tabUrl.PortalAliasId;            
            int portalAliasId = tabUrl.PortalAliasId;
            //if (portalAliasId > 0)
            tabUrl.CultureCode = PortalSettings.Current.CultureCode; //PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId).CultureCode;
            EventLogController.EventLogType saveLog = EventLogController.EventLogType.TABURL_CREATED;

            if (tabUrl.HttpStatus == "200")
            {
                saveLog = EventLogController.EventLogType.TABURL_CREATED;

            }
            else
            {
                //need to see if sequence number exists to decide if insert or update
                List<TabUrlInfo> t = TabController.Instance.GetTabUrls(portalId, tabUrl.TabId);
                TabUrlInfo existingSeq = t.FirstOrDefault(r => r.SeqNum == tabUrl.SeqNum);
                if (existingSeq == null)
                {
                    saveLog = EventLogController.EventLogType.TABURL_CREATED;
                }
            }

            DataProvider.Instance().SaveTabUrl(tabUrl.TabId, tabUrl.SeqNum, portalAliasId, (int)tabUrl.PortalAliasUsage, tabUrl.Url, tabUrl.QueryString, tabUrl.CultureCode, tabUrl.HttpStatus, tabUrl.IsSystem, UserController.Instance.GetCurrentUserInfo().UserID);

            EventLogController.Instance.AddLog("tabUrl",
                               tabUrl.ToString(),
                               PortalController.Instance.GetCurrentSettings() as IPortalSettings,
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               saveLog);

            if (clearCache)
            {
                DataCache.RemoveCache(string.Format(DataCache.TabUrlCacheKey, portalId));
                TabController.Instance.ClearCache(portalId);
            }
        }

        protected override Func<IPageUrlsController> GetFactory()
        {
            return () => new PageUrlsController();
        }
    }
}
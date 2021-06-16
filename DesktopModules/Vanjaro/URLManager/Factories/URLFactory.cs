using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Url.FriendlyUrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Specialized;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common.Utilities;
using Vanjaro.URL.Data.Entities;
using Vanjaro.URL.PetaPoco;
using Vanjaro.URL.Managers;
using Vanjaro.URL.Common;
using Vanjaro.URL.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using Vanjaro.URL.Data.Scripts;
using DotNetNuke.Abstractions.Portals;

namespace Vanjaro.URL.Factories
{
    internal static class URLFactory
    {
        public static void Add(URLEntity URL)
        {
            URL.CreatedOn = DateTime.Now;
            URL.UpdatedOn = DateTime.Now;
            URL.Insert();
            Cache.Clear();
            //DataCache.RemoveCache(Cache.Keys.URLEntity + URL.ModuleID.ToString() + "-" + URL.EntityID.ToString() + "-" + URL.Entity.ToString());
        }
        public static URLEntity GetURL(int ModuleID, string Slug)
        {
            if (Slug == null)
                throw new Exception("Slug cannot be empty or null");

            //Handle multiple slugs or query parameters with null key
            string[] Slugs = Slug.Split(',');

            foreach (string s in Slugs)
            {
                URLEntity URL = DataCache.GetCache<URLEntity>(Cache.Keys.Slug + ModuleID.ToString() + "-" + s);

                if (URL == null)
                {
                    if (ModuleID > 0)
                        URL = URLEntity.Fetch("WHERE ModuleID=@0 AND Slug=@1", ModuleID, s).SingleOrDefault();
                    else
                        URL = URLEntity.Fetch("WHERE Slug=@0", s).SingleOrDefault();

                    DataCache.SetCache(Cache.Keys.Slug + ModuleID.ToString() + "-" + s, URL, Cache.Keys.Cache_Time_Heavy);
                }

                if (URL != null)
                    return URL;
            }

            return null;
        }
        /// <summary>
        /// Returns the first matched slug in the Slugs Array
        /// </summary>
        /// <param name="Slugs"></param>
        /// <returns></returns>
        public static URLEntity GetURL(int ModuleID, string[] Slugs)
        {
            foreach (string s in Slugs)
            {
                URLEntity urlEntity = GetURL(ModuleID, s);

                if (urlEntity != null)
                    return urlEntity;
            }

            return null;
        }

        internal static int UpdatePageName(int moduleID, bool value)
        {
            int result = 0;
            if (value)
                result = URLLibraryRepo.GetInstance().Execute("update " + CommonScript.TablePrefix + "VJ_URL_URLEntity set PageName=1 where ModuleID=" + moduleID);
            else
                result = URLLibraryRepo.GetInstance().Execute("update " + CommonScript.TablePrefix + "VJ_URL_URLEntity set PageName=0 where ModuleID=" + moduleID);
            Cache.Clear();
            return result;
        }

        public static List<URLEntity> GetUrlHistory(int ModuleID, int EntityID, string Entity)
        {
            return URLEntity.Fetch("WHERE ModuleID=@0 AND EntityID=@1 AND Entity=@2", ModuleID, EntityID, Entity).ToList();
        }
        public static void DeleteUrls(int ModuleID, int EntityID, string Entity)
        {
            List<URLEntity> URls = URLEntity.Fetch("WHERE ModuleID=@0 AND EntityID=@1 AND Entity=@2", ModuleID, EntityID, Entity);
            foreach (URLEntity url in URls)
            {
                url.Delete();
            }
            if (URls.Count > 0)
                Cache.Clear();
        }
        public static List<URLEntity> GetURLs(int ModuleID, int EntityID, string Entity)
        {
            List<URLEntity> URls = DataCache.GetCache<List<URLEntity>>(Cache.Keys.URLEntity + ModuleID.ToString() + "-" + EntityID.ToString() + "-" + Entity.ToString());
            if (URls == null)
            {
                URls = URLEntity.Fetch("WHERE ModuleID=@0 AND EntityID=@1 AND Entity=@2", ModuleID, EntityID, Entity);
                DataCache.SetCache(Cache.Keys.URLEntity + ModuleID.ToString() + "-" + EntityID.ToString() + "-" + Entity.ToString(), URls, Cache.Keys.Cache_Time_Heavy);
            }
            return URls;
        }
        public static URLEntity GetDefaultURL(int ModuleID, int EntityID, string Entity, string Language)
        {
            URLEntity DefaultURL = null;
            List<URLEntity> URLs = GetURLs(ModuleID, EntityID, Entity);

            if (!string.IsNullOrEmpty(Language)) //Looking for language specific url
                DefaultURL = URLs.Where(u => u.IsDefault && u.Language == Language).OrderByDescending(u => u.Updatedby).FirstOrDefault();

            if (DefaultURL == null) //Language Specific URL Not Found
                DefaultURL = URLs.Where(u => u.IsDefault && u.Language == null).OrderByDescending(u => u.Updatedby).FirstOrDefault();

            //if (DefaultURL == null) //Language Invariant URL Not Found
            //    throw new Exception(LocalizationMananger.GetString(ResourceKeys.URL_Default_NotFound,LocalizationMananger.SharedResources,string.Format(ResourceKeys.URL_Default_NotFound_Message,Entity,EntityID)));

            return DefaultURL;
        }
        public static void InitDefaultURLs<T>(int ModuleID, List<T> Entities)
        {
            InitDefaultURLs(ModuleID, Entities, URLFactory.GetCurrentLanguage());
        }
        public static void InitDefaultURLs<T>(int ModuleID, List<T> Entities, string Language)
        {
            if (Entities.Count == 0)
                return;

            PortalSettings pS = PortalSettings.Current;

            if (pS == null)
                throw new Exception("PortalSettings.Current must be Non-Nullable to generate an entity url");

            Sql sql = Sql.Builder.Append("WHERE ModuleID = @0 AND IsDefault=@1 AND (", ModuleID, true);

            foreach (IURLService e in Entities)
                sql.Append("(EntityID=@0 AND Entity=@1) OR ", e.EntityID, e.Entity);

            string s = sql.SQL.TrimEnd('O', 'R', ' ') + ")";

            List<URLEntity> URLs = URLEntity.Fetch(s, sql.Arguments);
            List<URLEntity> DefaultURLs = new List<URLEntity>();

            foreach (IURLService url in Entities)
            {
                URLEntity DefaultURL = null;

                if (!string.IsNullOrEmpty(Language)) //Looking for language specific url
                    DefaultURL = URLs.Where(u => u.EntityID == url.EntityID && u.Entity == url.Entity && u.Language == Language).OrderByDescending(u => u.Updatedby).FirstOrDefault();

                if (DefaultURL == null) //Language Specific URL Not Found
                    DefaultURL = URLs.Where(u => u.EntityID == url.EntityID && u.Entity == url.Entity && u.Language == null).OrderByDescending(u => u.Updatedby).FirstOrDefault();

                //if (DefaultURL == null) //Language Invariant URL Not Found
                //    throw new Exception(LocalizationMananger.GetString(ResourceKeys.URL_Default_NotFound, LocalizationMananger.SharedResources, string.Format(ResourceKeys.URL_Default_NotFound_Message, url.Entity, url.EntityID)));

                if (DefaultURL != null)
                    url.PermLink = GetFriendlyURL(ModuleID, pS, DefaultURL.Slug, null, Language, DefaultURL.PageName);

            }
        }
        public static bool IsUnique(string Slug)
        {
            return URLEntity.Query("where Slug=@0", Slug).Count() == 0;
        }
        public static bool IsUnique(string Slug, int ModuleID)
        {
            ModuleController mc = new ModuleController();
            ModuleInfo ActiveModule = mc.GetModule(ModuleID);
            foreach (URLEntity s in URLEntity.Query("where Slug=@0", Slug))
            {
                ModuleInfo minfo = mc.GetModule(s.ModuleID);
                if (minfo.PortalID == ActiveModule.PortalID && s.Slug == Slug)
                    return false;
            }
            return true;
        }
        public static string GetUnique(int ModuleID, string Slug)
        {
            int i = 1;
            string sSlug = Sanitize(Slug);
            string NewSlug = sSlug;

            while (!IsUnique(NewSlug, ModuleID))
            {
                NewSlug = sSlug + i.ToString();
                i++;
            }

            return NewSlug;
        }
        public static string Sanitize(string Slug)
        {
            string str = RemoveAccent(Slug).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            //str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            str = Regex.Replace(str, @"\-+", "-");
            return str;
        }
        private static string RemoveAccent(string Slug)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(Slug);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }


        public static string GetFriendlyURL(int? ModuleID, PortalSettings pS, string Slug, string QueryParameters, string Language, bool IncludePageName)
        {
            URLEntity u = new URLEntity()
            {
                Slug = Slug,
                Language = Language,
                PageName = IncludePageName
            };

            return GetFriendlyURL(ModuleID, pS, QueryParameters, u);
        }
        public static string GetFriendlyURL(int? ModuleID, PortalSettings pS, string QueryParameters, URLEntity u)
        {
            string strLanguage = string.IsNullOrEmpty(u.Language) ? URLManager.GetCurrentLanguage() : u.Language;

            string path = "~/Default.aspx";

            if (!string.IsNullOrEmpty(strLanguage))
                pS.CultureCode = strLanguage;
            else
                pS.CultureCode = pS.DefaultLanguage;

            TabInfo tab = null;

            if (u.PageName)
            {
                if (ModuleID.HasValue)
                {
                    int? ModuleTabID = null;

                    var Index = GetModuleTabIndex();

                    if (Index.ContainsKey(ModuleID.Value))
                        ModuleTabID = Index[ModuleID.Value];
                    else
                    {
                        ModuleInfo minfo = new ModuleController().GetModule(ModuleID.Value, Null.NullInteger, false);
                        ModuleTabID = minfo.TabID;
                    }
                    if (ModuleTabID.HasValue)
                        tab = new TabController().GetTab(ModuleTabID.Value, pS.PortalId);

                }
                else if (pS.HomeTabId != pS.ActiveTab.TabID)
                    tab = pS.ActiveTab;

            }


            if (tab == null)
                tab = new TabController().GetTab(pS.HomeTabId, pS.PortalId);


            path = AppendQueryParameters(pS, tab.TabID, path, QueryParameters, strLanguage, u);
            if (pS != null && pS.PortalAlias != null)
                return FriendlyUrlProvider.Instance().FriendlyUrl(tab, path, u.Slug, pS as IPortalSettings);
            else
                return path.TrimStart('~', '/') + "&" + u.Slug;
        }
        //public static string GetFriendlyURL(int PortalID, int HomeTabID, int TabID, string portalAlias, string Slug, string QueryParameters, string Language, bool IncludePageName)
        //{
        //    string strLanguage = string.IsNullOrEmpty(Language) ? pS.DefaultLanguage : Language;

        //    string URL = "~/Default.aspx";


        //    TabInfo tab;

        //    if (IncludePageName)
        //        tab = new TabController().GetTab(TabID, PortalID);
        //    else
        //        tab = new TabController().GetTab(HomeTabID, PortalID);


        //    URL += "&TabId=" + TabID;

        //    if (!string.IsNullOrEmpty(Language))
        //        URL += "&language=" + Language;

        //    URL += QueryParameters;
        //    URL = URL.ReplaceFirst("&", "?");

        //    return FriendlyUrlProvider.Instance().FriendlyUrl(tab, URL, Slug, portalAlias);
        //}
        private static string AppendQueryParameters(PortalSettings pS, int? TabID, string URL, string QueryParameters, string Language, URLEntity u)
        {
            if (QueryParameters == null)
                QueryParameters = string.Empty;


            if (TabID != pS.HomeTabId)
            {
                URL += "&TabId=" + TabID.Value;

                if (LocaleController.Instance.GetLocales(pS.PortalId).Count() > 1)
                    URL += "&language=" + Language;

            }
            //Apps that support same slug on multiple tabs
            //Handle homepage by including page name
            else if (TabID == pS.HomeTabId && u.IgnorePageName)
            {
                string PageName = string.Empty;
                GetPageName(ref PageName, -1, pS);
                if (!string.IsNullOrEmpty(PageName))
                    URL += "&" + PageName;
            }

            URL += QueryParameters;
            URL = URL.ReplaceFirst("&", "?");
            return URL;
        }

        internal static void GetPageName(ref string PageName, int TabID, PortalSettings pS)
        {
            //Find pagename
            TabInfo tab;
            string PageNameURL = string.Empty;
            if (TabID > -1)
            {
                tab = new TabController().GetTab(TabID, pS.PortalId);
                PageNameURL = FriendlyUrlProvider.Instance().FriendlyUrl(tab, "~/Default.aspx?TabId=" + tab.TabID, " ", pS.PortalAlias.HTTPAlias).ToLower();
                int Index = PageNameURL.IndexOf(pS.PortalAlias.HTTPAlias) + pS.PortalAlias.HTTPAlias.Length;

                if (Index < PageNameURL.Length)
                    PageName = PageNameURL.Substring(Index).Trim().TrimStart('/').TrimEnd('/');
            }
            else
            {
                tab = new TabController().GetTab(pS.HomeTabId, pS.PortalId);
                PageNameURL = FriendlyUrlProvider.Instance().FriendlyUrl(tab, "~/Default.aspx", tab.TabName, pS.PortalAlias.HTTPAlias).ToLower();
                PageName = PageNameURL.TrimStart('~', '/').Trim();
            }
        }

        public static Dictionary<string, string> GetFriendlyURLs(PortalSettings pS, int ModuleID, string Language, Dictionary<string, int> Entities)
        {
            Dictionary<string, string> results = DataCache.GetCache<Dictionary<string, string>>(Cache.Keys.FriendlyUrls + "-" + pS.PortalId + "-" + ModuleID + "-" + Language + "-" + string.Join("-", Entities.Keys));
            if (results == null)
            {
                results = new Dictionary<string, string>();

                Sql sql = Sql.Builder.Append("WHERE ModuleID = @0 AND (", ModuleID);
                foreach (var e in Entities)
                {
                    if (!string.IsNullOrEmpty(e.Key))
                        sql.Append("(EntityID=@0 AND Entity=@1) OR ", e.Value, e.Key);
                }
                string s = sql.SQL.TrimEnd('O', 'R', ' ') + ")";
                List<URLEntity> URLs = URLEntity.Fetch(s, sql.Arguments);

                foreach (var e in Entities)
                {
                    if (!string.IsNullOrEmpty(e.Key))
                    {
                        URLEntity DefaultURL = null;
                        if (!string.IsNullOrEmpty(Language)) //Looking for language specific url
                            DefaultURL = URLs.Where(u => u.IsDefault && u.Language == Language && u.Entity == e.Key && u.EntityID == e.Value).OrderByDescending(u => u.Updatedby).FirstOrDefault();

                        if (DefaultURL == null) //Language Specific URL Not Found
                            DefaultURL = URLs.Where(u => u.IsDefault && u.Language == null && u.Entity == e.Key && u.EntityID == e.Value).OrderByDescending(u => u.Updatedby).FirstOrDefault();

                        if (DefaultURL != null)
                            results.Add(e.Key, GetFriendlyURL(ModuleID, pS, DefaultURL.Slug, null, Language, true));
                    }
                    else
                        results.Add(e.Key, GetFriendlyURL(ModuleID, pS, string.Empty, null, null, true));
                }
                DataCache.SetCache(Cache.Keys.FriendlyUrls + "-" + pS.PortalId + "-" + ModuleID + "-" + Language + "-" + string.Join("-", Entities.Keys), results);
            }
            return results;
        }

        public static string GetEntityURL(int ModuleID, int EntityID, string Entity, int? ActiveModuleID = null)
        {
            PortalSettings pS = PortalSettings.Current;

            if (pS == null && ModuleID > 0)
            {
                ModuleInfo minfo = ModuleController.Instance.GetModule(ModuleID, Null.NullInteger, false);
                if (minfo != null)
                    pS = new PortalSettings(minfo.PortalID);
            }

            if (pS != null)
            {
                URLEntity u = GetDefaultURL(ModuleID, EntityID, Entity, GetCurrentLanguage(pS));

                if (u != null)
                    return GetFriendlyURL(ActiveModuleID == null ? ModuleID : ActiveModuleID.Value, pS, null, u);
                else
                    return string.Empty;

            }
            else
                throw new Exception("PortalSettings.Current must be Non-Nullable to generate an entity url");
        }
        public static string GetCurrentLanguage()
        {
            return GetCurrentLanguage(PortalSettings.Current);
        }
        public static string GetCurrentLanguage(PortalSettings pS)
        {
            string Language = Thread.CurrentThread.CurrentCulture.ToString();

            if (pS != null && Thread.CurrentThread.CurrentCulture.ToString() == pS.DefaultLanguage)
                Language = null;

            return Language;
        }
        private static Dictionary<int, int> GetModuleTabIndex()
        {
            Dictionary<int, int> ModuleTabIndex = DataCache.GetCache<Dictionary<int, int>>(Cache.Keys.ModuleTabIndex);

            if (ModuleTabIndex == null)
            {
                ModuleTabIndex = URLLibraryRepo.GetInstance().Fetch<int>("SELECT DISTINCT ModuleID FROM " + CommonScript.TablePrefix + "VJ_URL_URLEntity").ToDictionary(u => u, u => -1);

                ModuleController mc = new ModuleController();

                foreach (int i in ModuleTabIndex.Keys.ToArray())
                {
                    ModuleInfo m = mc.GetModule(i, Null.NullInteger, false);

                    if (m != null)
                        ModuleTabIndex[i] = m.TabID;
                    else
                        ModuleTabIndex[i] = -1;
                }

                DataCache.SetCache(Cache.Keys.ModuleTabIndex, ModuleTabIndex);
            }

            return ModuleTabIndex;
        }
        private static Dictionary<string, int> GetSlugIndex(int PortalID)
        {
            Dictionary<string, int> Index = DataCache.GetCache<Dictionary<string, int>>(Cache.Keys.SlugIndex);

            if (Index == null)
            {
                Index = new Dictionary<string, int>();
                //Index = URLEntity.Fetch("").ToList().ToDictionary(u => u.Slug, u => u.ModuleID);
                foreach (URLEntity s in URLEntity.Fetch("").ToList())
                {
                    ModuleController mController = new ModuleController();
                    ModuleInfo minfo = mController.GetModule(s.ModuleID);
                    if (minfo.PortalID == PortalID)
                    {
                        if (Index != null && !Index.ContainsKey(s.Slug))
                            Index.Add(s.Slug, s.ModuleID);
                    }
                }

                ModuleController mc = new ModuleController();

                //Generate TabID Index
                Dictionary<int, int> ModuleTabIndex = new Dictionary<int, int>();
                foreach (int i in Index.Values.Distinct())
                {
                    ModuleInfo m = mc.GetModule(i, Null.NullInteger, false);

                    if (m != null)
                        ModuleTabIndex.Add(m.ModuleID, m.TabID);
                    else
                        ModuleTabIndex.Add(i, -1);
                }

                foreach (int ModuleId in ModuleTabIndex.Keys) //Swap ModuleIDs for TabIDs
                    Index.Where(i => i.Value == ModuleId).ToList().ForEach(i => Index[i.Key] = ModuleTabIndex[ModuleId]);

                DataCache.SetCache(Cache.Keys.SlugIndex, Index);
            }

            return Index;
        }

        //Create an overload and pass -1 for backward compatibilty
        public static int GetSlugTabID(string[] urlParms, ref string Slug, int TabID, PortalInfo Pi)
        {
            if (urlParms.Length > 0)
            {
                Dictionary<string, int> Index = GetSlugIndex(Pi.PortalID);

                for (int i = 0; i < urlParms.Length; i++)
                {
                    string slugFound = urlParms[i];

                    if (slugFound != null && Index.ContainsKey(slugFound))
                    {
                        if (TabID == -1 || (TabID > -1 && TabID == Index[slugFound]))
                        {
                            Slug = slugFound;
                            return Index[slugFound];
                        }
                    }
                }
            }
            return -1;
        }
    }
}
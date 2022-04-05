using DotNetNuke.Entities.Portals;
using Vanjaro.URL.Data.Entities;
using Vanjaro.URL.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Tabs;

namespace Vanjaro.URL.Managers
{
    public class URLManager
    {
        public static void Add(URLEntity URL)
        {
            URLFactory.Add(URL);
        }
        public static List<URLEntity> AddURL(List<URLEntity> URLs, string Entity, int EntityID, string Language, string Slug, bool IsDefault)
        {
            return AddURL(URLs, Entity, EntityID, Language, Slug, IsDefault, true);
        }
        public static List<URLEntity> AddURL(List<URLEntity> URLs, string Entity, int EntityID, string Language, string Slug, bool IsDefault, bool PageName)
        {
            URLEntity url = URLs.Where(u => u.Language == Language && u.Slug == Slug && u.EntityID == EntityID && u.Entity == Entity).FirstOrDefault();

            if (IsDefault)
            {
                //There can only be one Default URL for each Language
                foreach (URLEntity u in URLs.Where(u => u.Language == Language))
                {
                    u.IsDefault = false;
                    u.HasChanged = true;
                }
            }

            if (url != null)
            {
                url.IsDefault = IsDefault;
                url.HasChanged = true;
            }
            else
            {
                url = new URLEntity() { EntityID = EntityID, Entity = Entity, IsDefault = IsDefault, Language = Language, Slug = Slug, PageName = PageName };
                URLs.Add(url);
            }

            return URLs;
        }

        public static int UpdatePageName(int ModuleID, bool value)
        {
            return URLFactory.UpdatePageName(ModuleID, value);
        }

        public static List<URLEntity> RemoveURL(List<URLEntity> URLs, string Language, string Slug)
        {
            URLEntity url = URLs.Where(u => u.Language == Language && u.Slug == Slug).FirstOrDefault();

            if (url.IsDefault)
                throw new Exception("Cannot remove default URL of this language.");

            if (url != null)
            {
                url.HasChanged = true;
                url.HasDeleted = true;
            }

            return URLs;
        }
        public static List<URLEntity> GetURLs(List<URLEntity> _URLs, int ModuleID, string Entity, int EntityID)
        {
            if (_URLs == null) //Check if we've already persisted the object once
            {
                if (ModuleID != 0) //Check if data even exists in db yet. Product.ModuleID == 0 indicates a new category
                    _URLs = URLFactory.GetURLs(ModuleID, EntityID, Entity);

                if (_URLs == null) //No URLs are available yet
                    _URLs = new List<URLEntity>();
            }

            return _URLs;
        }

        public static string GetPermLink(string _PermLink, int ModuleID, string Entity, int EntityID)
        {
            if (string.IsNullOrEmpty(_PermLink))
                _PermLink = URLFactory.GetEntityURL(ModuleID, EntityID, Entity, null);

            return _PermLink;
        }


        public static string GetPermLink(string _PermLink, int ModuleID, string Entity, int EntityID, int? ActiveModuleID = null)
        {
            if (string.IsNullOrEmpty(_PermLink))
                _PermLink = URLFactory.GetEntityURL(ModuleID, EntityID, Entity, ActiveModuleID);

            return _PermLink;
        }

        public static string GetUnique(int ModuleID, string Slug)
        {
            return URLFactory.GetUnique(ModuleID, Slug);
        }

        public static string GetCurrentLanguage()
        {
            return URLFactory.GetCurrentLanguage();
        }
        public static void InitDefaultURLs<T>(int ModuleID, List<T> Entities)
        {
            URLFactory.InitDefaultURLs(ModuleID, Entities);
        }
        public static void InitDefaultURLs<T>(int ModuleID, List<T> Entities, string Language)
        {
            URLFactory.InitDefaultURLs(ModuleID, Entities, Language);
        }
        public static URLEntity GetDefaultURL(int ModuleID, int EntityID, string Entity, string Language)
        {
            return URLFactory.GetDefaultURL(ModuleID, EntityID, Entity, Language);
        }
        public static URLEntity GetURL(string Slug, int PortalID)
        {
            return URLFactory.GetURL(0, Slug, PortalID);
        }
        public static URLEntity GetURL(int ModuleID, string Slug, int PortalID)
        {
            return URLFactory.GetURL(ModuleID, Slug, PortalID);
        }
        public static URLEntity GetURL(int ModuleID, string[] Slugs, int PortalID)
        {
            return URLFactory.GetURL(ModuleID, Slugs, PortalID);
        }
        public static int GetSlugTabID(string[] urlParms, ref string Slug, PortalInfo Pi)
        {
            return URLFactory.GetSlugTabID(urlParms, ref Slug, -1, Pi);
        }
        public static int GetSlugTabID(string[] urlParms, ref string Slug, int TabID, PortalInfo Pi)
        {
            return URLFactory.GetSlugTabID(urlParms, ref Slug, TabID, Pi);
        }

        public static string GetFriendlyURL(PortalSettings PortalSettings, TabInfo tabInfo, int ModuleID, string QueryParameter)
        {
            return URLFactory.GetFriendlyURL(PortalSettings, tabInfo, ModuleID, QueryParameter);
        }

        public static string GetFriendlyURL(PortalSettings pS, URLEntity urlEntity)
        {
            return GetFriendlyURL(urlEntity.ModuleID, pS, urlEntity.Slug, null, urlEntity.Language, urlEntity.PageName);
        }
        public static string GetFriendlyURL(int ModuleID, PortalSettings pS, URLEntity urlEntity)
        {
            return GetFriendlyURL(ModuleID, pS, urlEntity.Slug, null, urlEntity.Language, urlEntity.PageName);
        }

        public static string GetFriendlyURL(PortalSettings pS, string Slug)
        {
            return GetFriendlyURL(null, pS, Slug, null, null, true);
        }
        public static string GetFriendlyURL(PortalSettings pS, string Slug, string QueryParameters)
        {
            return GetFriendlyURL(null, pS, Slug, QueryParameters, null, true);
        }
        public static string GetFriendlyURL(PortalSettings pS, string Slug, string QueryParameters, string Language)
        {
            return GetFriendlyURL(null, pS, Slug, QueryParameters, Language, true);
        }
        public static string GetFriendlyURL(PortalSettings pS, string Slug, string QueryParameters, string Language, bool IncludePageName)
        {
            return GetFriendlyURL(null, pS, Slug, QueryParameters, Language, IncludePageName);
        }

        public static string GetFriendlyURL(int? ModuleID, PortalSettings pS, string Slug)
        {
            return GetFriendlyURL(ModuleID, pS, Slug, null, null, true);
        }
        public static string GetFriendlyURL(int? ModuleID, PortalSettings pS, string Slug, string QueryParameters)
        {
            return GetFriendlyURL(ModuleID, pS, Slug, QueryParameters, null, true);
        }
        public static string GetFriendlyURL(int? ModuleID, PortalSettings pS, string Slug, string QueryParameters, string Language)
        {
            return GetFriendlyURL(ModuleID, pS, Slug, QueryParameters, Language, true);
        }
        public static string GetFriendlyURL(int? ModuleID, PortalSettings pS, string Slug, string QueryParameters, string Language, bool IncludePageName)
        {
            return URLFactory.GetFriendlyURL(ModuleID, pS, Slug, QueryParameters, Language, IncludePageName);
        }
        public static Dictionary<string, string> GetFriendlyURLs(PortalSettings pS, int ModuleID, string Language, Dictionary<string, int> Entities)
        {
            return URLFactory.GetFriendlyURLs(pS, ModuleID, Language, Entities);
        }
        public static List<URLEntity> GetUrlHistory(int ModuleID, int EntityID, string Entity)
        {
            return URLFactory.GetUrlHistory(ModuleID, EntityID, Entity);
        }
        public static void DeleteUrls(int ModuleID, int EntityID, string Entity)
        {
            URLFactory.DeleteUrls(ModuleID, EntityID, Entity);
        }
    }
}

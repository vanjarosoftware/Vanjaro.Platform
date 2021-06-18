using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Url.FriendlyUrl;
using Vanjaro.URL.Data.Entities;
using Vanjaro.URL.Factories;
using Vanjaro.URL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanjaro.URL.Providers
{
    public class URLProvider : ExtensionUrlProvider
    {
        public override bool AlwaysCallForRewrite(int portalId)
        {
            return false;
        }
        public override bool AlwaysUsesDnnPagePath(int portalId)
        {
            return true;
        }

        public override string ChangeFriendlyUrl(DotNetNuke.Entities.Tabs.TabInfo tab, string friendlyUrlPath, FriendlyUrlOptions options, string cultureCode, ref string endingPageName, out bool useDnnPagePath, ref List<string> messages)
        {
            useDnnPagePath = true;
            return friendlyUrlPath;
        }

        public override bool CheckForRedirect(int tabId, int portalid, string httpAlias, Uri requestUri, System.Collections.Specialized.NameValueCollection queryStringCol, FriendlyUrlOptions options, out string redirectLocation, ref List<string> messages)
        {
            redirectLocation = "";
            return false;
        }

        public override Dictionary<string, string> GetProviderPortalSettings()
        {
            throw new NotImplementedException();
        }

        public override string TransformFriendlyUrlToQueryString(string[] urlParms, int tabId, int portalId, FriendlyUrlOptions options, string cultureCode, DotNetNuke.Entities.Portals.PortalAliasInfo portalAlias, ref List<string> messages, out int status, out string location)
        {
            status = 200;
            location = null;

            string result = string.Empty, Slug = string.Empty, PageName = string.Empty;
            PortalSettings pS = null;
            bool PageNameRedirect = false;

            urlParms = urlParms.Select(s => s.ToLower()).Distinct().ToArray();
            List<string> urlParmsList = new List<string>(urlParms).ConvertAll(u => u.ToLower());

            int SlugTabID = URLManager.GetSlugTabID(urlParms, ref Slug, tabId, PortalController.Instance.GetPortal(portalId));

            if (SlugTabID != -1) //Slug Found
            {
                URLEntity CurrentURL = URLManager.GetURL(Slug, portalId);

                //Redirect if it's not the default URL 
                if (CurrentURL != null && !CurrentURL.IsDefault)
                {
                    InitPortalSettings(ref pS, tabId, portalAlias);
                    return Redirect(out status, out location, portalId, pS, SlugTabID, result, CurrentURL, null);
                }

                if (pS == null)
                    pS = new PortalSettings(tabId, portalAlias);

                //DNN 8580
                //https://dnntracker.atlassian.net/browse/DNN-8580?page=com.googlecode.jira-suite-utilities%3Atransitions-summary-tabpanel
                ClearUrlParams(ref urlParmsList, ref PageName, Slug, SlugTabID, pS);

                //Tab not identified...need redirect
                if (tabId == -1)
                {
                    InitPortalSettings(ref pS, tabId, portalAlias);

                    if (CurrentURL.IgnorePageName)
                        return string.Empty;
                    else
                    {
                        //Redirect based on Include PageName if we're not on homepage
                        if (SlugTabID != pS.HomeTabId && CurrentURL.PageName && !urlParmsList.Contains(PageName) && !string.Join("/", urlParmsList).Contains(PageName))
                            PageNameRedirect = true;
                        else if (!CurrentURL.PageName && (urlParmsList.Contains(PageName) || string.Join("/", urlParmsList).Contains(PageName)))
                            PageNameRedirect = true;

                        //Remove PageName
                        foreach (var item in PageName.Split('/'))
                            urlParmsList.Remove(item);

                        //Setup Rewrite Path
                        result = "?TabId=" + SlugTabID.ToString();
                    }
                }

                string remainder = base.CreateQueryStringFromParameters(urlParmsList.ToArray(), -1);

                string Prefix = urlParmsList.Count % 2 == 0 ? string.Empty : "=";


                if (PageNameRedirect)
                {
                    InitPortalSettings(ref pS, tabId, portalAlias);
                    return Redirect(out status, out location, portalId, pS, SlugTabID, result, CurrentURL, remainder);
                }


                //Rewrite URL to appropriate page
                if (result.StartsWith("?TabId=") && !string.IsNullOrEmpty(Slug))
                    result += "&" + Slug + remainder;
                else
                    result += Slug + remainder;
            }

            return result;
        }

        private void InitPortalSettings(ref PortalSettings PS, int tabId, PortalAliasInfo portalAlias)
        {
            if (PS == null)
                PS = new PortalSettings(tabId, portalAlias);
        }

        private static string Redirect(out int status, out string location, int PortalID, PortalSettings pS, int TabID, string result, URLEntity CurrentURL, string QueryParameters)
        {
            URLEntity RedirectURL = URLManager.GetDefaultURL(CurrentURL.ModuleID, CurrentURL.EntityID, CurrentURL.Entity, CurrentURL.Language);

            location = URLFactory.GetFriendlyURL(null, pS, RedirectURL.Slug, QueryParameters, RedirectURL.Language, RedirectURL.PageName);
            status = 301;

            return result;
        }

        private void ClearUrlParams(ref List<string> urlParmsList, ref string PageName, string Slug, int TabID, PortalSettings pS)
        {
            var Locales = LocaleController.Instance.GetLocales(pS.PortalId);

            //Remove Slug
            urlParmsList.Remove(Slug);

            //Check if locale matches
            foreach (var key in Locales.Keys)
            {
                if (urlParmsList.Contains(key.ToLower()))
                {
                    urlParmsList.Remove(key.ToLower());
                    break;
                }
            }

            // if locale is changed in portal alias, for Live Article
            foreach (var key in pS.PortalAlias.HTTPAlias.ToString().Split('/'))
            {
                if (urlParmsList.Contains(key.ToLower()))
                {
                    urlParmsList.Remove(key.ToLower());
                    break;
                }
            }

            URLFactory.GetPageName(ref PageName, TabID, pS);
        }
    }
}
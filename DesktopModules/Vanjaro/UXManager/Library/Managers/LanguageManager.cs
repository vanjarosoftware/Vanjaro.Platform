using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Vanjaro.UXManager.Library
{
    public static partial class Managers
    {
        public class LanguageManager
        {
            public static List<Library.Entities.Language> GetCultureListItems(bool All)
            {

                List<Library.Entities.Language> Languages = new List<Library.Entities.Language>();
                IEnumerable<System.Web.UI.WebControls.ListItem> cultureListItems = DotNetNuke.Services.Localization.Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, CultureInfo.CurrentCulture.ToString(), "", false);
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                foreach (Locale loc in LocaleController.Instance.GetLocales(ps.PortalId).Values)
                {
                    string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), ps.PortalId, "Administrators");
                    if (!ps.ContentLocalizationEnabled || (LocaleIsAvailable(loc) && (PortalSecurity.IsInRoles(ps.AdministratorRoleName) || loc.IsPublished || PortalSecurity.IsInRoles(defaultRoles))))
                    {
                        if (cultureListItems.Count() > 1 || All)
                        {
                            foreach (System.Web.UI.WebControls.ListItem cultureItem in cultureListItems)
                            {
                                if (cultureItem.Value == loc.Code)
                                {
                                    Library.Entities.Language language = new Library.Entities.Language
                                    {
                                        EnglishName = RemoveBracket(loc.EnglishName),
                                        NativeName = RemoveBracket(loc.NativeName),
                                        URL = NewUrl(loc.Code, ps),
                                        FlagPath = Globals.ResolveUrl("~/images/Flags") + "/" + loc.Code + ".gif",
                                        Code = loc.Code
                                    };
                                    Languages.Add(language);
                                }
                            }
                        }
                    }
                }
                return Languages;
            }

            private static string RemoveBracket(string Value)
            {
                if (!string.IsNullOrEmpty(Value) && Value.Contains("("))
                {
                    string[] Values = Value.Split('(');
                    Value = Values[0].TrimEnd();
                }

                return Value;
            }

            private static bool LocaleIsAvailable(Locale Locale)
            {
                TabInfo tab = (PortalController.Instance.GetCurrentSettings() as PortalSettings).ActiveTab;
                if (tab.DefaultLanguageTab != null)
                {
                    tab = tab.DefaultLanguageTab;
                }

                TabInfo localizedTab = TabController.Instance.GetTabByCulture(tab.TabID, tab.PortalID, Locale);

                return localizedTab != null && !localizedTab.IsDeleted && TabPermissionController.CanViewPage(localizedTab);
            }

            private static string NewUrl(string NewLanguage, PortalSettings PortalSettings)
            {
                Locale newLocale = LocaleController.Instance.GetLocale(NewLanguage);

                //Ensure that the current ActiveTab is the culture of the new language
                int tabId = PortalSettings.ActiveTab.TabID;
                bool islocalized = false;

                TabInfo localizedTab = TabController.Instance.GetTabByCulture(tabId, PortalSettings.PortalId, newLocale);
                if (localizedTab != null)
                {
                    islocalized = true;
                    if (localizedTab.IsDeleted || !TabPermissionController.CanViewPage(localizedTab))
                    {
                        PortalInfo localizedPortal = PortalController.Instance.GetPortal(PortalSettings.PortalId, newLocale.Code);
                        tabId = localizedPortal.HomeTabId;
                    }
                    else
                    {
                        string fullurl = string.Empty;
                        switch (localizedTab.TabType)
                        {
                            case TabType.Normal:
                                //normal tab
                                tabId = localizedTab.TabID;
                                break;
                            case TabType.Tab:
                                //alternate tab url                                
                                fullurl = TestableGlobals.Instance.NavigateURL(Convert.ToInt32(localizedTab.Url));
                                break;
                            case TabType.File:
                                //file url
                                fullurl = TestableGlobals.Instance.LinkClick(localizedTab.Url, localizedTab.TabID, Null.NullInteger);
                                break;
                            case TabType.Url:
                                //external url
                                fullurl = localizedTab.Url;
                                break;
                        }
                        if (!string.IsNullOrEmpty(fullurl))
                        {
                            return GetCleanUrl(fullurl);
                        }
                    }
                }

                string rawQueryString = string.Empty;
                if (DotNetNuke.Entities.Host.Host.UseFriendlyUrls)
                {
                    // Remove returnurl from query parameters to prevent that the language is changed back after the user has logged in
                    // Example: Accessing protected page /de-de/Page1 redirects to /de-DE/Login?returnurl=%2f%2fde-de%2fPage1 and changing language to en-us on the login page
                    // using the language links won't change the language in the returnurl parameter and the user will be redirected to the de-de version after logging in
                    // Assumption: Loosing the returnurl information is better than confusing the user by switching the language back after the login
                    NameValueCollection queryParams = HttpUtility.ParseQueryString(new Uri(string.Concat(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), HttpContext.Current.Request.RawUrl)).Query);
                    queryParams.Remove("returnurl");
                    string queryString = queryParams.ToString();
                    if (queryString.Length > 0)
                    {
                        rawQueryString = string.Concat("?", queryString);
                    }
                }

                string controlKey = HttpContext.Current.Request.QueryString["ctl"];
                string[] queryStrings = GetQsParams(newLocale.Code, islocalized, PortalSettings);
                bool isSuperTab = PortalSettings.ActiveTab.IsSuperTab;
                string url = $"{TestableGlobals.Instance.NavigateURL(tabId, isSuperTab, PortalSettings, controlKey, NewLanguage, queryStrings)}{rawQueryString}";

                return GetCleanUrl(url);
            }

            private static string GetCleanUrl(string url)
            {
                string cleanUrl = PortalSecurity.Instance.InputFilter(url, PortalSecurity.FilterFlag.NoScripting);
                if (url != cleanUrl)
                {
                    return string.Empty;
                }
                if (!string.IsNullOrEmpty(url))
                {
                    url = url.Replace("/uxmode/true", "").Replace("uxmode/true", "").Replace("?uxmode=true", "").Replace("&uxmode=true", "");
                }

                return url;
            }

            internal static string RenderLanguages()
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                string Markup = string.Empty;

                if (GetCultureListItems(false).Count > 1)
                {
                    Markup = "<div id='LanguageManager' class='language-manager'>";
                    foreach (Library.Entities.Language language in GetCultureListItems(false))
                    {
                        string active = ps.CultureCode.ToLower() == language.Code.ToLower() ? "active" : string.Empty;
                        Markup += "<a class='language " + active + "' href='" + language.URL + "'>" + language.EnglishName + "</a>";
                    }
                    Markup += "</div>";
                }
                return Markup;
            }


            private static string[] GetQsParams(string NewLanguage, bool IsLocalized, PortalSettings PortalSettings)
            {
                string returnValue = "";
                NameValueCollection queryStringCollection = HttpContext.Current.Request.QueryString;
                NameValueCollection rawQueryStringCollection =
                    HttpUtility.ParseQueryString(new Uri(HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.RawUrl).Query);

                PortalSettings settings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                string[] arrKeys = queryStringCollection.AllKeys;

                for (int i = 0; i <= arrKeys.GetUpperBound(0); i++)
                {
                    if (arrKeys[i] != null)
                    {
                        switch (arrKeys[i].ToLowerInvariant())
                        {
                            case "tabid":
                            case "ctl":
                            case "language": //skip parameter
                                break;
                            case "mid":
                            case "moduleid": //start of patch (Manzoni Fausto) gemini 14205 
                                if (IsLocalized)
                                {
                                    string ModuleIdKey = arrKeys[i].ToLowerInvariant();

                                    int.TryParse(queryStringCollection[ModuleIdKey], out int moduleID);
                                    int.TryParse(queryStringCollection["tabid"], out int tabid);
                                    ModuleInfo localizedModule = ModuleController.Instance.GetModuleByCulture(moduleID, tabid, settings.PortalId, LocaleController.Instance.GetLocale(NewLanguage));
                                    if (localizedModule != null)
                                    {
                                        if (!string.IsNullOrEmpty(returnValue))
                                        {
                                            returnValue += "&";
                                        }
                                        returnValue += ModuleIdKey + "=" + localizedModule.ModuleID;
                                    }
                                }
                                break;
                            default:
                                if ((arrKeys[i].ToLowerInvariant() == "portalid") && PortalSettings.ActiveTab.IsSuperTab)
                                {
                                    //skip parameter
                                    //navigateURL adds portalid to querystring if tab is superTab
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(rawQueryStringCollection.Get(arrKeys[i])))
                                    {
                                        //skip parameter as it is part of a querystring param that has the following form
                                        // [friendlyURL]/?param=value
                                        // gemini 25516

                                        if (!DotNetNuke.Entities.Host.Host.UseFriendlyUrls)
                                        {
                                            if (!string.IsNullOrEmpty(returnValue))
                                            {
                                                returnValue += "&";
                                            }
                                            returnValue += arrKeys[i] + "=" + HttpUtility.UrlEncode(rawQueryStringCollection.Get(arrKeys[i]));
                                        }


                                    }
                                    // on localised pages most of the module parameters have no sense and generate duplicate urls for the same content
                                    // because we are on a other tab with other modules (example : /en-US/news/articleid/1)
                                    else //if (!isLocalized) -- this applies only when a portal "Localized Content" is enabled.
                                    {
                                        string[] arrValues = queryStringCollection.GetValues(i);
                                        if (arrValues != null)
                                        {
                                            for (int j = 0; j <= arrValues.GetUpperBound(0); j++)
                                            {
                                                if (!string.IsNullOrEmpty(returnValue))
                                                {
                                                    returnValue += "&";
                                                }
                                                string qsv = arrKeys[i];
                                                qsv = qsv.Replace("\"", "");
                                                qsv = qsv.Replace("'", "");
                                                returnValue += qsv + "=" + HttpUtility.UrlEncode(arrValues[j]);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                if (!settings.ContentLocalizationEnabled && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1 && !settings.EnableUrlLanguage)
                {
                    //because useLanguageInUrl is false, navigateUrl won't add a language param, so we need to do that ourselves
                    if (returnValue != "")
                    {
                        returnValue += "&";
                    }
                    returnValue += "language=" + NewLanguage.ToLowerInvariant();
                }

                //return the new querystring as a string array
                return returnValue.Split('&');
            }
        }
    }
}
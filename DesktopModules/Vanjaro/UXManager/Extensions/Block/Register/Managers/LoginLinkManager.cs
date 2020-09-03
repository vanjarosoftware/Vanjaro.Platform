using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using System;
using System.Web;
using Vanjaro.Common.Utilities;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.Register
{
    public static partial class Managers
    {
        public class LoginLinkManager
        {
            public static string LoginURL(string returnUrl, bool overrideSetting)
            {
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                string loginUrl;
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = string.Format("returnurl={0}", returnUrl);
                }
                string popUpParameter = "";
                if (HttpUtility.UrlDecode(returnUrl).IndexOf("popUp=true", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    popUpParameter = "popUp=true";
                }

                if (portalSettings.LoginTabId != -1 && !overrideSetting)
                {
                    //if (ValidateLoginTabID(portalSettings.LoginTabId))
                    if (portalSettings.LoginTabId != -1)
                    {
                        loginUrl = string.IsNullOrEmpty(returnUrl)
                                            ? ServiceProvider.NavigationManager.NavigateURL(portalSettings.LoginTabId, "", popUpParameter)
                                            : ServiceProvider.NavigationManager.NavigateURL(portalSettings.LoginTabId, "", returnUrl, popUpParameter);
                    }
                    else
                    {
                        string strMessage = string.Format("error={0}", Localization.GetString("NoLoginControl", Localization.GlobalResourceFile));
                        //No account module so use portal tab
                        loginUrl = string.IsNullOrEmpty(returnUrl)
                                     ? ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", strMessage, popUpParameter)
                                     : ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnUrl, strMessage, popUpParameter);
                    }
                }
                else
                {
                    //portal tab
                    loginUrl = string.IsNullOrEmpty(returnUrl)
                                    ? ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", popUpParameter)
                                    : ServiceProvider.NavigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnUrl, popUpParameter);
                }
                return loginUrl;
            }
        }

        #region Testing Code Remove after done testing

        //private static string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        //{
        //    PortalSettings _portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
        //    return NavigateURL(tabID, _portalSettings, controlKey, additionalParameters);
        //}

        //private static string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        //{
        //    bool isSuperTab = IsHostTab(tabID);

        //    return NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        //}
        //private static bool ValidateLoginTabID(int tabId)
        //{
        //    return ValidateModuleInTab(tabId, "Account Login");
        //}
        //private static bool ValidateModuleInTab(int tabId, string moduleName)
        //{
        //    bool hasModule = Null.NullBoolean;
        //    foreach (ModuleInfo objModule in ModuleController.Instance.GetTabModules(tabId).Values)
        //    {
        //        if (objModule.ModuleDefinition.FriendlyName == moduleName)
        //        {
        //            //We need to ensure that Anonymous Users or All Users have View permissions to the login page
        //            TabInfo tab = TabController.Instance.GetTab(tabId, objModule.PortalID, false);
        //            if (TabPermissionController.CanViewPage(tab))
        //            {
        //                hasModule = true;
        //                break;
        //            }
        //        }
        //    }
        //    return hasModule;
        //}

        //private static bool IsHostTab(int tabId)
        //{
        //    bool isHostTab = false;
        //    TabCollection hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);

        //    if (hostTabs != null)
        //    {
        //        isHostTab = hostTabs.Any(t => t.Value.TabID == tabId);
        //    }
        //    return isHostTab;
        //}

        //private static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        //{
        //    string cultureCode = GetCultureCode(tabID, isSuperTab, settings);
        //    return NavigateURL(tabID, isSuperTab, settings, controlKey, cultureCode, additionalParameters);
        //}

        //private static string GetCultureCode(int TabID, bool IsSuperTab, PortalSettings settings)
        //{
        //    string cultureCode = Null.NullString;
        //    if (settings != null)
        //    {
        //        TabInfo linkTab = TabController.Instance.GetTab(TabID, IsSuperTab ? Null.NullInteger : settings.PortalId, false);
        //        if (linkTab != null)
        //        {
        //            cultureCode = linkTab.CultureCode;
        //        }
        //        if (string.IsNullOrEmpty(cultureCode))
        //        {
        //            cultureCode = Thread.CurrentThread.CurrentCulture.Name;
        //        }
        //    }

        //    return cultureCode;
        //}

        //private static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        //{
        //    return NavigateURL(tabID, isSuperTab, settings, controlKey, language, glbDefaultPage, additionalParameters);
        //}

        //private static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        //{
        //    string url = tabID == Null.NullInteger ? ApplicationURL() : ApplicationURL(tabID);
        //    if (!String.IsNullOrEmpty(controlKey))
        //    {
        //        url += "&ctl=" + controlKey;
        //    }
        //    if (additionalParameters != null)
        //    {
        //        url = additionalParameters.Where(parameter => !string.IsNullOrEmpty(parameter)).Aggregate(url, (current, parameter) => current + ("&" + parameter));
        //    }
        //    if (isSuperTab)
        //    {
        //        url += "&portalid=" + settings.PortalId;
        //    }

        //    TabInfo tab = null;

        //    if (settings != null)
        //    {
        //        tab = TabController.Instance.GetTab(tabID, isSuperTab ? Null.NullInteger : settings.PortalId, false);
        //    }

        //    //only add language to url if more than one locale is enabled
        //    if (settings != null && language != null && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1)
        //    {
        //        if (settings.ContentLocalizationEnabled)
        //        {
        //            if (language == "")
        //            {
        //                if (tab != null && !string.IsNullOrEmpty(tab.CultureCode))
        //                {
        //                    url += "&language=" + tab.CultureCode;
        //                }
        //            }
        //            else
        //            {
        //                url += "&language=" + language;
        //            }
        //        }
        //        else if (settings.EnableUrlLanguage)
        //        {
        //            //legacy pre 5.5 behavior
        //            if (language == "")
        //            {
        //                url += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
        //            }
        //            else
        //            {
        //                url += "&language=" + language;
        //            }
        //        }
        //    }

        //    if (Host.UseFriendlyUrls || Config.GetFriendlyUrlProvider() == "advanced")
        //    {
        //        if (String.IsNullOrEmpty(pageName))
        //        {
        //            pageName = glbDefaultPage;
        //        }

        //        url = (settings == null) ? FriendlyUrl(tab, url, pageName) : FriendlyUrl(tab, url, pageName, settings);
        //    }
        //    else
        //    {
        //        url = ResolveUrl(url);
        //    }

        //    return url;
        //}

        //private static string ApplicationURL()
        //{
        //    PortalSettings _portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
        //    if (_portalSettings != null && _portalSettings.ActiveTab.HasAVisibleVersion)
        //    {
        //        return (ApplicationURL(_portalSettings.ActiveTab.TabID));
        //    }
        //    return (ApplicationURL(-1));
        //}
        //private static string ApplicationURL(int TabID)
        //{
        //    string strURL = "~/" + glbDefaultPage;
        //    if (TabID != -1)
        //    {
        //        strURL += "?tabid=" + TabID;
        //    }
        //    return strURL;
        //}
        //private static string FriendlyUrl(TabInfo tab, string path, string pageName)
        //{
        //    PortalSettings _portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
        //    return FriendlyUrl(tab, path, pageName, _portalSettings);
        //}
        //private static string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        //{
        //    return FriendlyUrlProvider.Instance().FriendlyUrl(tab, path, pageName, settings);
        //}

        //private static string ResolveUrl(string url)
        //{
        //    // String is Empty, just return Url
        //    if (String.IsNullOrEmpty(url))
        //    {
        //        return url;
        //    }
        //    // String does not contain a ~, so just return Url
        //    if ((url.StartsWith("~") == false))
        //    {
        //        return url;
        //    }
        //    // There is just the ~ in the Url, return the appPath
        //    if ((url.Length == 1))
        //    {
        //        return ApplicationPath;
        //    }
        //    if ((url.ToCharArray()[1] == '/' || url.ToCharArray()[1] == '\\'))
        //    {
        //        // Url looks like ~/ or ~\
        //        if (!string.IsNullOrEmpty(ApplicationPath) && ApplicationPath.Length > 1)
        //        {
        //            return ApplicationPath + "/" + url.Substring(2);
        //        }
        //        else
        //        {
        //            return "/" + url.Substring(2);
        //        }
        //    }
        //    else
        //    {
        //        // Url look like ~something
        //        if (!string.IsNullOrEmpty(ApplicationPath) && ApplicationPath.Length > 1)
        //        {
        //            return ApplicationPath + "/" + url.Substring(1);
        //        }
        //        else
        //        {
        //            return ApplicationPath + url.Substring(1);
        //        }
        //    }
        //}

        //private static string ApplicationPath
        //{
        //    get
        //    {
        //        if (_applicationPath == null && (HttpContext.Current != null))
        //        {
        //            if (HttpContext.Current.Request.ApplicationPath == "/")
        //            {
        //                _applicationPath = string.IsNullOrEmpty(Config.GetSetting("InstallationSubfolder")) ? "" : (Config.GetSetting("InstallationSubfolder") + "/").ToLowerInvariant();
        //            }
        //            else
        //            {
        //                _applicationPath = HttpContext.Current.Request.ApplicationPath.ToLowerInvariant();
        //            }
        //        }

        //        return _applicationPath;
        //    }
        //}
        #endregion
    }
}

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using System;
using System.Web;
using Vanjaro.Common.Utilities;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.Core.Providers.Authentication
{
    public static partial class Managers
    {
        private const int RedirectTimeout = 3000;
        public class ResetPasswordManager
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
            public static string RedirectAfterLogin()
            {
                string redirectURL = "";
                object setting = DotNetNuke.Entities.Modules.UserModuleBase.GetSetting(PortalSettings.Current.PortalId, "Redirect_AfterLogin");

                if (Convert.ToInt32(setting) == Null.NullInteger)
                {
                    if (HttpContext.Current.Request.QueryString["returnurl"] != null)
                    {
                        //return to the url passed to signin
                        redirectURL = HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString["returnurl"]);

                        //clean the return url to avoid possible XSS attack.
                        redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                    }

                    if (HttpContext.Current.Request.Cookies["returnurl"] != null)
                    {
                        //return to the url passed to signin
                        redirectURL = HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies["returnurl"].Value);

                        //clean the return url to avoid possible XSS attack.
                        redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                    }
                    if (string.IsNullOrEmpty(redirectURL))
                    {
                        if (PortalSettings.Current.RegisterTabId != -1 && PortalSettings.Current.HomeTabId != -1)
                        {
                            //redirect to portal home page specified
                            redirectURL = ServiceProvider.NavigationManager.NavigateURL(PortalSettings.Current.HomeTabId);
                        }
                        else
                        {
                            //redirect to current page 
                            redirectURL = ServiceProvider.NavigationManager.NavigateURL();
                        }
                    }
                }
                else //redirect to after login page
                {
                    redirectURL = ServiceProvider.NavigationManager.NavigateURL(Convert.ToInt32(setting));
                }

                return redirectURL;
            }
        }
    }
}
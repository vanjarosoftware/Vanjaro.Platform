using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using System;
using System.Web;
using Vanjaro.Common.Utilities;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.RegisterLink
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
       
    }
}

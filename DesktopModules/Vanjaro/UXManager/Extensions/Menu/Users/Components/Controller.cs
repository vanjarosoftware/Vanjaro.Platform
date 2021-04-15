using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using System;
using System.Web;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Components
{
    public class Controller
    {
        internal static class Impersonation
        {
            public static string Impersonate(HttpContext context, PortalSettings pS, int cUserId, int iUserId, bool RememberOriginalUser)
            {
                try
                {
                    PortalAliasInfo p = new PortalAliasInfo();

                    UserInfo iUserInfo = UserController.GetUserById(pS.PortalId, iUserId);

                    //if (iUserInfo.IsInRole("Administrators") && !pS.UserInfo.IsSuperUser)
                    //{
                    //    return "Cannot impersonate an Administrator!";
                    //}

                    if (iUserInfo != null)
                    {
                        if (context.Session != null)
                        {
                            if (RememberOriginalUser)
                            {
                                if (context.Session["LA-RevertId"] == null)
                                {
                                    context.Session.Add("LA-RevertId", cUserId);
                                }

                                if (context.Session["LA-RevertTabId"] == null)
                                {
                                    context.Session.Add("LA-RevertTabId", pS.ActiveTab.TabID);
                                }
                            }
                            else
                            {
                                context.Session.Remove("LA-RevertId");
                                context.Session.Remove("LA-RevertTabId");

                            }
                        }

                        DotNetNuke.Common.Utilities.DataCache.ClearUserCache(pS.PortalId, context.User.Identity.Name);
                        new PortalSecurity().SignOut();
                        UserController.UserLogin(pS.PortalId, iUserInfo, pS.PortalName, context.Request.UserHostAddress, false);

                        return "Success";
                    }

                }
                catch (Exception ex)
                {
                    ExceptionManager.LogException(ex);
                }

                return "An error occurred impersonating the user. Please refer to event viewer for details!";

            }

        }
    }
}

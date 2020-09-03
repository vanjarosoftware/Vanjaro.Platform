using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Users.Components;
using Vanjaro.UXManager.Library.Common;


namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class ImpersonationController : UIEngineController
    {

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleImpersonation(int iUserId)
        {
            ActionResult actionResult = new ActionResult();
            ClearChache();
            UpdateImpersonation();
            if (ImpersonationEnabled)
            {
                if (iUserId > -1)
                {

                    string iResult = Controller.Impersonation.Impersonate(HttpContext.Current, PortalSettings, PortalSettings.UserId, iUserId, true);
                    if (iResult == "Success")
                    {
                        PortalController.Instance.GetPortalSettings(PortalSettings.PortalId).TryGetValue("Redirect_AfterLogin", out string s1);
                        
                        string RedirectUrl;

                        if (!string.IsNullOrEmpty(s1) && int.Parse(s1) > -1)
                        {
                            RedirectUrl = ServiceProvider.NavigationManager.NavigateURL(int.Parse(s1));// HttpContext.Current.Response.Redirect(ServiceProvider.NavigationManager.NavigateURL(int.Parse(s1)));
                        }
                        else
                        {
                            RedirectUrl = ServiceProvider.NavigationManager.NavigateURL(PortalSettings.HomeTabId);// HttpContext.Current.Response.Redirect(ServiceProvider.NavigationManager.NavigateURL(PortalSettings.HomeTabId));
                        }

                        actionResult.Data = RedirectUrl;
                    }
                    else
                    {
                        actionResult.AddError("HandleImpersonation", iResult);
                        //Skin.AddModuleMessage(this, iResult, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }
            return actionResult;
        }

        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo, UserInfo userInfo, Dictionary<string, string> parameters)
        {
            return new List<IUIData>();
        }

        [HttpGet]
        public void ClearChache()
        {
            DotNetNuke.Common.Utilities.Config.Touch();
        }

        #region Private Method
        private bool HasPermission => (UserInfo.UserID > -1 && UserInfo.IsInRole("Administrators"));
        private void UpdateImpersonation()
        {
            if (HasPermission)
            {
                if (!ImpersonationEnabled)
                {
                    PortalController.UpdatePortalSetting(PortalSettings.PortalId, "Vanjaro_Impersonation", "TRUE");
                }
            }
        }
        private bool ImpersonationEnabled
        {
            get
            {
                PortalController.Instance.GetPortalSettings(PortalSettings.PortalId).TryGetValue("Vanjaro_Impersonation", out string imp);
                if (imp == "TRUE")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }


}
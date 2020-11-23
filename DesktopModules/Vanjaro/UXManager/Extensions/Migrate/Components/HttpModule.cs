using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using System;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
namespace Vanjaro.Migrate.Components
{
    public class HttpModule : IHttpModule, IRequiresSessionState
    {
        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public string ModuleName => "VJMigration";

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
        }

        private void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (app != null)
            {
                HttpContext context = app.Context;

                //Make sure we have a valid HttpContext and we're not dealing with static requests such as images etc...
                if (context != null && context.CurrentHandler is CDefault && context.Request.QueryString["popUp"] == null)
                {
                    PortalSettings p = PortalController.Instance.GetCurrentSettings() as PortalSettings;

                    if (p != null && p.UserInfo != null && TabPermissionController.HasTabPermission("EDIT"))
                    {
                        if (p.ActiveTab != null)
                        {
                            Page _Page = app.Context.CurrentHandler as Page;

                            string TabSkinSrc = p.ActiveTab.SkinSrc != null ? p.ActiveTab.SkinSrc.ToLower() : string.Empty;
                            string QuerySkinSrc = context.Request.QueryString["SkinSrc"] != null ? context.Request.QueryString["SkinSrc"].ToLower() : null;

                            if (TabSkinSrc == "[G]Skins/Vanjaro/Base.ascx".ToLower() || QuerySkinSrc == "[G]Skins/Vanjaro/Base".ToLower())
                            {
                                ToggleUserMode(p);

                                string ControlPanel = HostController.Instance.GetSettings().ContainsKey("ControlPanel") ? HostController.Instance.GetSettings()["ControlPanel"].Value.ToLower() : null;

                                HostController.Instance.GetSettings()["ControlPanel"].Value = "DesktopModules/Vanjaro/UXManager/Library/Base.ascx";
                                if (HostController.Instance.GetBoolean("DisableEditBar"))
                                    HostController.Instance.GetSettings()["DisableEditBar"].Value = "True";

                                _Page.PreRender += delegate (object ss, EventArgs ee)
                                {
                                    HostController.Instance.GetSettings()["ControlPanel"].Value = "DesktopModules/admin/Dnn.PersonaBar/UserControls/PersonaBarContainer.ascx";
                                    if (HostController.Instance.GetBoolean("DisableEditBar"))
                                        HostController.Instance.GetSettings()["DisableEditBar"].Value = "False";
                                };
                            }
                            else
                            {
                                string NavigateURL = p.ActiveTab.FullUrl;

                                string Script = "<script>var VanjaroMigrate_CurrentTabURL = '" + NavigateURL + "'; </script>";
                                _Page.ClientScript.RegisterStartupScript(_Page.GetType(), "VJ-HttpMod-Div", Script);

                            }
                        }
                    }
                }
            }
        }


        private bool IsPopUp(HttpContext context)
        {
            return context.Request.QueryString["popUp"] != null;
        }
        public static void ToggleUserMode(PortalSettings portalSettings, string mode = "VIEW")
        {
            DotNetNuke.Services.Personalization.PersonalizationController personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            DotNetNuke.Services.Personalization.PersonalizationInfo personalization = personalizationController.LoadProfile(portalSettings.UserInfo.UserID, portalSettings.PortalId);
            if (personalization.Profile["Usability:UserMode" + portalSettings.PortalId] != null && personalization.Profile["Usability:UserMode" + portalSettings.PortalId].ToString() != mode.ToUpper())
            {
                personalization.Profile["Usability:UserMode" + portalSettings.PortalId] = mode.ToUpper();
                personalization.IsModified = true;
                personalizationController.SaveProfile(personalization);
            }
        }
        #endregion
    }
}

using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Client.ClientResourceManagement;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Vanjaro.Common;
using Vanjaro.Common.ASPNET;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Manager;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.Core.Services;
using static Vanjaro.Core.Factories;
using static Vanjaro.Core.Managers;
using static Vanjaro.Skin.Managers;

namespace Vanjaro.Skin
{
    public partial class Base : DotNetNuke.UI.Skins.Skin
    {
        private readonly string[] JSExcludes = new string[] { "dnn.modalpopup.js", "dnncore.js", "dnn.dragdrop.js", "jquery.js", "jquery.min.js", "jquery-migrate.min.js", "jquery-ui.min.js", "jquery-ui.js", "jquery.hoverIntent.min.js", "dnn.jquery.js", "jquery-migrate.js", "knockout.mapping.js", "knockout.js", "query.tokeninput.js", "dnn.DropDownList.js", "dnn.DynamicTreeView.js", "dnn.TreeView.js", "dnn.jScrollBar.js", "jquery.mousewheel.js", "dnn.DataStructures.js", "dnn.jquery.extensions.js", "dnn.extensions.js" };
        private readonly string[] CSSExcludes = new string[] { "dnndefault/7.0.0/default.css", "dnndefault/8.0.0/default.css", "portals/" + PortalSettings.Current.PortalId + "/home.css", "portals/_default/admin.css", "dnn.dragdrop.css", "dnn.DropDownList.css", "dnn.jScrollBar.css", "token-input-facebook.css" };
        private readonly string[] JSExcludesContains = new string[] { "dnn.editbar" };
        private readonly string[] CSSExcludesContains = new string[] { "dnn.editbar" };

        private readonly string[] JSLExcludes = new string[] { CommonJs.KnockoutMapping };
        private Pages page = null;
        private bool? m2v = null;
        private dynamic ModulesDictObj;
        private readonly Dictionary<int, dynamic> ModulesDict = new Dictionary<int, dynamic>();
        bool HasReviewPermission = false;

#if DEBUG
        private const bool ShowMissingKeys = true;
#else
        private const bool ShowMissingKeys = false;
#endif

        protected override void OnPreRender(EventArgs e)
        {
            RemoveDNNDependencies();
        }

        private void RemoveDNNDependencies()
        {
            if (string.IsNullOrEmpty(Request.QueryString["ctl"]))
            {
                RemoveDependencies("ClientDependencyHeadJs");
                RemoveDependencies("ClientDependencyHeadCss");
                RemoveDependencies("BodySCRIPTS");
                RemoveDependencies("ClientResourcesFormBottom");
                RemoveDependencies("ClientResourceIncludes");
            }
        }


        protected override void OnInit(EventArgs e)
        {
            RenderGoogleTagManagerScripts();
            ResetTheme();
            if (Request.QueryString["m2v"] != null)
                m2v = Convert.ToBoolean(Request.QueryString["m2v"]);

            HandleEditMode();

            if (m2v.HasValue || !string.IsNullOrEmpty(Request.QueryString["SkinSrc"]))
            {
                ToggleUserMode();
            }

            MigratePage();

            InjectViewport();
            HomeLogoAndSiteIcons();

            GetCookieConsentMarkup();

            AddModulePanes();

            if (!m2v.HasValue || (m2v.HasValue && m2v.Value && !string.IsNullOrEmpty(Request.QueryString["SkinSrc"])))
            {
                LoadModuleInFrame();
            }

            base.OnInit(e);

            InjectExtensionControl();

            RenderLocalizedMetaData();

        }

        private void HandleEditMode()
        {
            ///Code to support IsEditable pending a solution to prevent Edit Bar from appearing. 
#pragma warning disable CS0162 // Unreachable code detected
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["vj_IsPageEdit"] != null && HttpContext.Current.Request.Cookies["vj_IsPageEdit"].Value == "true")
#pragma warning restore CS0162 // Unreachable code detected
            {
                ToggleUserMode("EDIT");
            }
            else
            {
                ToggleUserMode("VIEW");
            }

            if (m2v.HasValue || !string.IsNullOrEmpty(Request.QueryString["SkinSrc"]))
            {
                ToggleUserMode();
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery, null, SpecificVersion.Latest);

            if (TabPermissionController.HasTabPermission("EDIT") && PortalSettings.UserInfo.IsInRole("Administrators"))
                JavaScript.RequestRegistration(CommonJs.jQueryUI);

            InjectThemeJS();

            PageManager.Init(Page, PortalSettings);

            if (string.IsNullOrEmpty(Request.QueryString["mid"]) || (!string.IsNullOrEmpty(Request.QueryString["icp"]) && bool.Parse(Request.QueryString["icp"]) && (!string.IsNullOrEmpty(Request.QueryString["mid"]) && Request.QueryString["mid"] != "0")))
            {
                if (page != null && page.StateID.HasValue)
                    HasReviewPermission = WorkflowManager.HasReviewPermission(page.StateID.Value, PortalSettings.UserInfo);

                if (PageManager.InjectThemeCSS(PortalSettings))
                    WebForms.LinkCSS(Page, "ThemeCSS", Page.ResolveUrl("~/Portals/" + PortalSettings.PortalId + "/vThemes/" + Core.Managers.ThemeManager.CurrentTheme.Name + "/Theme.css"), true, "DnnPageHeaderProvider", 0);

                WebForms.LinkCSS(Page, "SkinCSS", Page.ResolveUrl("~/Portals/_default/Skins/Vanjaro/Resources/css/skin.css"), true, "DnnPageHeaderProvider", 1);

                //Skin js requried because using for openpopup update memeber Profile when user is registered user 
                //VjDefaultPath used in Skin.js for loading icon.
                if (PageManager.InjectEditor(PortalSettings))
                    WebForms.RegisterClientScriptBlock(Page, "DefaultPath", "var VjThemePath='" + Page.ResolveUrl("~/Portals/_default/vThemes/" + Core.Managers.ThemeManager.CurrentTheme.Name) + "'; var VjDefaultPath='" + Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Images/") + "'; var VjSitePath='" + Page.ResolveUrl("~/DesktopModules/Vanjaro/") + "';", true);
                WebForms.RegisterClientScriptInclude(Page, "VJ-Skin-JS", Page.ResolveUrl("~/Portals/_default/Skins/Vanjaro/Resources/js/skin.min.js"), true, "DnnFormBottomProvider", 2, WebForms.Execution.defer);
                WebForms.RegisterClientScriptInclude(Page, "VJ-Bootstrap-JS", FrameworkManager.Request("Bootstrap", "js/bootstrap.bundle.min.js"), true, "DnnBodyProvider", WebForms.Execution.defer);
            }
            else
            {
                WebForms.LinkCSS(Page, "EditCSS", Page.ResolveUrl("~/Portals/_default/Skins/Vanjaro/Resources/css/edit.css"));
            }
            HandleUXM();
            ResetModulePanes();
            AccessDenied();
            InjectAnalyticsScript();
        }

        private void RenderGoogleTagManagerScripts()
        {
            string Site_Head = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleTagManager.Site_Head", true);
            string Site_Body = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleTagManager.Site_Body", true);
            string Host_Head = SettingManager.GetHostSetting("Vanjaro.Integration.GoogleTagManager.Host_Head", true);
            string Host_Body = SettingManager.GetHostSetting("Vanjaro.Integration.GoogleTagManager.Host_Body", true);

            Literal HeadScript = new Literal();
            Literal BodyScript = new Literal();

            if (!string.IsNullOrEmpty(Site_Head) || !string.IsNullOrEmpty(Site_Body))
            {
                if (!string.IsNullOrEmpty(Site_Head))
                {
                    HeadScript.Text = Site_Head;
                    Page.FindControl("Head").Controls.AddAt(0, HeadScript);
                }
                if (!string.IsNullOrEmpty(Site_Body))
                {
                    BodyScript.Text = Site_Body;
                    Page.FindControl("Body").Controls.AddAt(0, BodyScript);
                }
            }
            else if (!string.IsNullOrEmpty(Host_Head) || !string.IsNullOrEmpty(Host_Body))
            {
                if (!string.IsNullOrEmpty(Host_Head))
                {
                    HeadScript.Text = Host_Head;
                    Page.FindControl("Head").Controls.AddAt(0, HeadScript);
                }
                if (!string.IsNullOrEmpty(Host_Body))
                {
                    BodyScript.Text = Host_Body;
                    Page.FindControl("Body").Controls.AddAt(0, BodyScript);
                }
            }
            else
                RenderGoogleAnalyticsScript();

        }
        private void RenderGoogleAnalyticsScript()
        {
            string MeasurementID = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleAnalytics.MeasurementID", true);
            if (!string.IsNullOrEmpty(MeasurementID))
            {
                Literal GoogleAnalyticsScript = new Literal();
                GoogleAnalyticsScript.Text = "<script async src=\"https://www.googletagmanager.com/gtag/js?id=" + MeasurementID + "\"></script><script>window.dataLayer = window.dataLayer || [];  function gtag() { dataLayer.push(arguments); } gtag('js', new Date());gtag('config', '" + MeasurementID + "');</script>";
                Page.Header.Controls.AddAt(0, GoogleAnalyticsScript);
            }

            string TrackingID = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleAnalytics.TrackingID", true);
            if (!string.IsNullOrEmpty(TrackingID))
            {
                Literal UniversalAnalyticsScript = new Literal();
                UniversalAnalyticsScript.Text = "<script async src=\"https://www.googletagmanager.com/gtag/js?id=" + TrackingID + "\"></script><script>window.dataLayer = window.dataLayer || [];  function gtag() { dataLayer.push(arguments); } gtag('js', new Date());gtag('config', '" + TrackingID + "');</script>";
                Page.Header.Controls.AddAt(0, UniversalAnalyticsScript);
            }
        }

        private void AccessDenied()
        {
            string Message = null;
            Guid messageGuid;
            var guidText = this.Request.QueryString["message"];
            if (!string.IsNullOrEmpty(guidText) && Guid.TryParse(guidText, out messageGuid))
                Message = HttpUtility.HtmlEncode(DataProvider.Instance().GetRedirectMessage(messageGuid));

            if (!string.IsNullOrEmpty(Message))
            {
                Control ContentPane = FindControlRecursive(this, "ContentPane");
                Literal lt = new Literal();
                lt.Text = "<div class=\"alert alert-warning\" role=\"alert\">" + Message + "</div>";
                ContentPane.Controls.Add(lt);
            }
        }

        private void InjectThemeJS()
        {
            if (string.IsNullOrEmpty(Request.QueryString["mid"]) || (!string.IsNullOrEmpty(Request.QueryString["icp"]) && bool.Parse(Request.QueryString["icp"]) && (!string.IsNullOrEmpty(Request.QueryString["mid"]) && Request.QueryString["mid"] != "0")))
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals/" + PortalSettings.PortalId + "/vThemes/" + ThemeManager.CurrentTheme.Name + "/theme.editor.js")))
                    WebForms.RegisterClientScriptInclude(Page, "ThemeEditorJs", Page.ResolveUrl("~/Portals/" + PortalSettings.PortalId + "/vThemes/" + ThemeManager.CurrentTheme.Name + "/theme.editor.js"), WebForms.Execution.defer);

                if (!string.IsNullOrEmpty(ThemeManager.CurrentTheme.Assembly) && !string.IsNullOrEmpty(ThemeManager.CurrentTheme.ClientScript))
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ThemeClientScript", "<script type=\"text/javascript\" src=\"" + Page.ClientScript.GetWebResourceUrl(Type.GetType(ThemeManager.CurrentTheme.Assembly), ThemeManager.CurrentTheme.ClientScript) + "\"></script>", false);
            }
        }
        private void HandleUXM()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["uxm"]))
            {
                if (Request.QueryString["uxm"] == "open")
                {
                    CookieManager.AddValue("vj_IsPageEdit", "true", new DateTime());
                    CookieManager.AddValue("vj_InitUX", "true", new DateTime());
                }
                else if (Request.QueryString["uxm"] == "close")
                {
                    CookieManager.AddValue("vj_IsPageEdit", "false", new DateTime());
                    CookieManager.AddValue("vj_InitUX", "false", new DateTime());
                }
                Response.Redirect(URLManager.RemoveQueryStringByKey(Request.Url.AbsoluteUri, "uxm"), true);
            }
        }

        private void InjectAnalyticsScript()
        {
            if (HostController.Instance.GetBoolean("VJImprovementProgram", true))
            {
                AnalyticsManager.Post();

                if (Analytics.RegisterScript)
                {
                    string PostEvent = Analytics.PostEvent();
                    if (!string.IsNullOrEmpty(PostEvent))
                    {
                        WebForms.RegisterClientScriptBlock(Page, "GTagManager", "<script type='text/javascript' async src='https://www.googletagmanager.com/gtag/js?id=" + Analytics.MeasurementID + "'></script>", false);
                        //WebForms.RegisterStartupScript(Page, "GTagManagerScript", "window.dataLayer = window.dataLayer || []; function gtag() { dataLayer.push(arguments); } gtag('js', new Date()); gtag('config', '" + Analytics.MeasurementID + "', { 'send_page_view': false }); " + PostEvent, true);
                        string script = @"$(document).ready(function () {window.dataLayer = window.dataLayer || []; function gtag() { dataLayer.push(arguments); } gtag('js', new Date()); gtag('config', '" + Analytics.MeasurementID + "', { 'send_page_view': false }); " + PostEvent + "});";
                        WebForms.RegisterStartupScript(Page, "AnalyticsPostEventScript", script, true);
                    }
                }
            }
        }

        private void InjectViewport()
        {
            HtmlMeta tagKeyword = new HtmlMeta
            {
                ID = "Vanjaro_Viewport",
                Name = "viewport",
                Content = "width=device-width, initial-scale=1, minimum-scale=1",

            };
            //if (Viewport)
            //tagKeyword.Content += " user-scalable=1";
            //else
            tagKeyword.Content += " user-scalable=0";

            if (Page.Header.FindControl("Vanjaro_Viewport") == null)
            {
                Page.Header.Controls.Add(tagKeyword);
            }
        }

        private static Control FindControlRecursive(Control rootControl, string controlId)
        {
            if (rootControl.ID == controlId)
            {
                return rootControl;
            }

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn = FindControlRecursive(controlToSearch, controlId);
                if (controlToReturn != null)
                {
                    return controlToReturn;
                }
            }
            return null;
        }

        private void AddModulePanes()
        {
            //Get the Page Markup
            if (!IsAllowed())
            {
                if (FindControlRecursive(this, "ContentPane") == null)
                {
                    Controls.Add(ParseControl("<div id=\"ContentPane\" runat=\"server\" />"));
                }

                BuildPanes();
                return;
            }
            if (string.IsNullOrEmpty(Request.QueryString["ctl"]))
            {
                if (FindControlRecursive(this, "ContentPane") == null)
                {
                    Controls.Add(ParseControl("<div id=\"ContentPane\" runat=\"server\" />"));
                }

                Control ContentPane = FindControlRecursive(this, "ContentPane");

                StringBuilder sb = new StringBuilder();
                page = GetPage();
                if (page != null)
                {
                    InjectStyle(page);
                    sb.Append(page.Content.ToString());
                }

                if (page != null && page.StateID.HasValue)
                    HasReviewPermission = WorkflowManager.HasReviewPermission(page.StateID.Value, PortalSettings.UserInfo);

                if (PortalSettings.Current.UserInfo.UserID > 0)
                    FrameworkManager.Load(this, "jQuerySweetAlert");

                if (TabPermissionController.HasTabPermission("EDIT") || (page != null && page.StateID.HasValue && HasReviewPermission))
                    FrameworkManager.Load(this, "Toastr");

                if ((TabPermissionController.HasTabPermission("EDIT") || HasReviewPermission))
                    WebForms.RegisterClientScriptBlock(Page, "ReviewGlobalVariable", GetReviewGlobalVariable(PortalSettings, HasReviewPermission), true);

                if (!TabPermissionController.HasTabPermission("EDIT") && HasReviewPermission)
                {
                    FrameworkManager.Load(this, "FontAwesome");
                }

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(sb.ToString());
                InjectBlocks(page, html);
                CheckPermission(html);
                if (!PageManager.InjectEditor(PortalSettings) || !string.IsNullOrEmpty(Request.QueryString["pv"]))
                    RemoveDataBlocks(html);

                string FinalMarkup = InjectModules(html.DocumentNode.OuterHtml);

                if (page != null && page.ReplaceTokens)
                    FinalMarkup = new TokenReplace().ReplaceEnvironmentTokens(FinalMarkup);

                string ClassName = "vj-wrapper";
                if (!string.IsNullOrEmpty(Request.QueryString["m2v"]) && string.IsNullOrEmpty(Request.QueryString["pv"]))
                {
                    ClassName += " m2vDisplayNone";
                }

                try
                {
                    ContentPane.Controls.Add(ParseControl("<div class=\"" + ClassName + "\"><div id=\"vjEditor\">" + FinalMarkup + "</div></div>"));
                }
                catch (Exception ex)
                {
                    string Message = string.Empty;
                    if (!UserController.Instance.GetCurrentUserInfo().IsAdmin)
                        Message = DotNetNuke.Services.Localization.Localization.GetString("Non_Admin_Users_Message", Constants.LocalResourcesFile);
                    if (UserController.Instance.GetCurrentUserInfo().IsAdmin || UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                        Message = DotNetNuke.Services.Localization.Localization.GetString("Admin_Super_Users_Message", Constants.LocalResourcesFile);

                    if (!string.IsNullOrEmpty(Message))
                    {
                        Literal lt = new Literal();
                        lt.Text = "<div class=\"alert alert-danger\" role=\"alert\">" + Message + "</div>";
                        ContentPane.Controls.Add(lt);
                    }

                    ExceptionManager.LogException(ex);
                }

                InjectLoginAuthentication();
            }
            else
            {
                string Content = Core.Managers.ThemeManager.CurrentTheme.EditLayout;
                Controls.Add(ParseControl(Content));
            }
            BuildPanes();
        }

        private void InjectLoginAuthentication()
        {
            if (FindControlRecursive(this, "vLoginControls") is HtmlContainerControl VLoginControls)
            {

                bool IsAuth = false;
                HtmlGenericControl createDiv = new HtmlGenericControl("div")
                {
                    ID = "TabDiv"
                };

                HtmlGenericControl TabContent = new HtmlGenericControl("div")
                {
                    ID = "TabContent"
                };
                TabContent.Attributes["class"] = "tab-content";

                //HtmlGenericControl SocialRegistration = new HtmlGenericControl("div");
                //SocialRegistration.Attributes["class"] = "dnnSocialRegistration";

                //HtmlGenericControl socialControls = new HtmlGenericControl("div");
                //socialControls.Attributes["id"] = "socialControls";

                //HtmlGenericControl Authdiv = new HtmlGenericControl("ul");
                //Authdiv.Attributes["class"] = "buttonList";

                int tab = 1;
                foreach (DotNetNuke.Services.Authentication.AuthenticationInfo authSystem in DotNetNuke.Services.Authentication.AuthenticationController.GetEnabledAuthenticationServices())
                {
                    if (authSystem.AuthenticationType != "Vanjaro")
                    {
                        var authLoginControl = (DotNetNuke.Services.Authentication.AuthenticationLoginBase)LoadControl("~/" + authSystem.LoginControlSrc);
                        if (authLoginControl.Enabled)
                        {
                            BindLoginControl(authLoginControl, authSystem);
                            if (authLoginControl is DotNetNuke.Services.Authentication.OAuth.OAuthLoginBase oAuthLoginControl)
                            {
                                //Authdiv.Controls.Add(oAuthLoginControl);
                            }
                            else
                            {
                                if (!IsAuth)
                                {
                                    string ResourceFile = Page.ResolveUrl("~/DesktopModules/AuthenticationServices") + "/Vanjaro/" + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc);
                                    createDiv.InnerHtml += "<ul class=\"vj_authenticationtab nav nav-tabs\" id=\"nav-tab\" role=\"tablist\">";
                                    createDiv.InnerHtml += "<li class=\"nav-item\"><a class=\"nav-link active\" id=\"nav-tab-" + tab + "\" data-bs-toggle=\"tab\" href=\"#dnn_navtab_" + tab + "\" role=\"tab\" aria-controls=\"dnn_navtab_" + tab + "\" aria-selected=\"true\">" + DotNetNuke.Services.Localization.Localization.GetString("Title", ResourceFile) + "</a></li>";

                                    HtmlGenericControl homediv = new HtmlGenericControl("div");
                                    homediv.Attributes["class"] = "tab-pane fade show active vj_authenticationcontrol";
                                    homediv.Attributes["role"] = "tabpanel";
                                    homediv.Attributes["aria-labelledby"] = "nav-tab-" + tab;
                                    homediv.Attributes["data-bs-toggle"] = "tab";
                                    homediv.ID = "navtab_" + tab;

                                    Control Login = FindControl("LoginControl");
                                    if (Login != null)
                                    {
                                        homediv.Controls.Add(Login);
                                    }

                                    TabContent.Controls.Add(homediv);
                                    tab++;
                                    IsAuth = true;
                                }

                                createDiv.InnerHtml += "<li class=\"nav-item\"><a class=\"nav-link\" id=\"nav-tab-" + tab + "\" data-bs-toggle=\"tab\" href=\"#dnn_navtab_" + tab + "\" role=\"tab\" aria-controls=\"dnn_navtab_" + tab + "\" aria-selected=\"false\">" + DotNetNuke.Services.Localization.Localization.GetString("Title", authLoginControl.LocalResourceFile) + "</a></li>";
                                HtmlGenericControl tabdiv = new HtmlGenericControl("div");
                                tabdiv.Attributes["class"] = "tab-pane fade vj_authenticationcontrol";
                                tabdiv.Attributes["role"] = "tabpanel";
                                tabdiv.Attributes["aria-labelledby"] = "nav-tab-" + tab;
                                tabdiv.Attributes["data-bs-toggle"] = "tab";
                                tabdiv.ID = "navtab_" + tab;
                                tabdiv.Controls.Add(authLoginControl);
                                TabContent.Controls.Add(tabdiv);
                                tab++;
                            }
                        }
                    }
                }

                if (IsAuth)
                {
                    createDiv.InnerHtml += "</ul>";
                    createDiv.Controls.Add(TabContent);

                    VLoginControls.Controls.Add(createDiv);
                }

                //socialControls.Controls.Add(Authdiv);
                //SocialRegistration.Controls.Add(socialControls);
                //VLoginControls.Controls.Add(SocialRegistration);
                WebForms.LinkCSS(Page, "AuthModuelCSS", Page.ResolveUrl("~/DesktopModules/Admin/Authentication/module.css"));

            }
        }

        private void BindLoginControl(AuthenticationLoginBase authLoginControl, AuthenticationInfo authSystem)
        {
            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            authLoginControl.AuthenticationType = authSystem.AuthenticationType;
            authLoginControl.ID = Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc) + "_" + authSystem.AuthenticationType;
            authLoginControl.LocalResourceFile = authLoginControl.TemplateSourceDirectory + "/" + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" +
                                                 Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc);

        }

        private void ResetModulePanes()
        {
            foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
            {
                if (ModulesDict.ContainsKey(m.ModuleID))
                {
                    dynamic item = ModulesDict[m.ModuleID];
                    try
                    {
                        if (item != null && item.PaneName != null)
                        {
                            m.PaneName = item.PaneName;
                        }
                    }
                    catch { }
                    try
                    {
                        if (item != null && item.IsDeleted != null)
                        {
                            m.IsDeleted = item.IsDeleted;
                        }
                    }
                    catch { }
                }
            }
        }

        private static string GetReviewGlobalVariable(PortalSettings PortalSettings, bool HasReviewPermission)
        {
            string Script = string.Empty;
            if (TabPermissionController.HasTabPermission("EDIT") || HasReviewPermission)
            {
                ReviewSettings ReviewSetting = Core.Managers.PageManager.GetPageReviewSettings(PortalSettings);
                Script = "var VJIsPageDraft='" + ReviewSetting.IsPageDraft + "';  var VJIsContentApproval='" + ReviewSetting.IsContentApproval + "'; var VJNextStateName='" + ReviewSetting.NextStateName + "'; var VJIsLocked='" + ReviewSetting.IsLocked + "'; var VJIsModeratorEditPermission='" + ReviewSetting.IsModeratorEditPermission + "'; var VJLocal='" + Core.Managers.PageManager.GetCultureCode(PortalSettings) + "';";
            }
            return Script;
        }

        private void BuildPanes()
        {
            if (FindControlRecursive(this, "ContentPane") is HtmlContainerControl contentPane)
            {
                base.PortalSettings.ActiveTab.Panes.Add(contentPane.ID);
                Panes.Add(contentPane.ID.ToLowerInvariant(), new Pane(contentPane));
            }

            if (!m2v.HasValue || m2v.HasValue && m2v.Value && !string.IsNullOrEmpty(Request.QueryString["SkinSrc"]))
            {
                foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
                {
                    if (FindControl("vj_" + m.ModuleID.ToString()) is HtmlContainerControl paneCtrl)
                    {
                        base.PortalSettings.ActiveTab.Panes.Add(paneCtrl.ID);
                        Panes.Add(paneCtrl.ID.ToLowerInvariant(), new Pane(paneCtrl));
                    }
                }
            }

        }
        private void CheckPermission(HtmlDocument html)
        {
            IEnumerable<HtmlNode> query = html.DocumentNode.SelectNodes("//*[@perm]");
            if (query != null)
            {
                foreach (HtmlNode item in query.ToList())
                {
                    if (!string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "perm").FirstOrDefault().Value))
                    {
                        int EntityID = int.Parse(item.Attributes.Where(a => a.Name == "perm").FirstOrDefault().Value);
                        bool Inherit = true;
                        BlockSection blockSection = SectionPermissionManager.GetBlockSection(EntityID);
                        if (blockSection != null && blockSection.Inherit.HasValue)
                            Inherit = blockSection.Inherit.Value;
                        if (!Inherit && !SectionPermissionManager.HasViewPermission(EntityID))
                            item.Remove();
                    }
                }
            }
        }
        private void RemoveDataBlocks(HtmlDocument html)
        {
            IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
            foreach (HtmlNode item in query.ToList())
            {
                if (item.Attributes == null)
                    continue;

                List<dynamic> AttributesToRemove = new List<dynamic>();
                if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value == "Language" && item.Attributes.Where(a => a.Name == "id").FirstOrDefault() != null && string.IsNullOrEmpty(item.InnerHtml))
                    AttributesToRemove.Add(item.Attributes.Where(a => a.Name == "id").FirstOrDefault());

                if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value == "Search Result")
                {
                    foreach (var Attribute in item.Attributes)
                    {
                        if (!Attribute.Name.StartsWith("data-block-global") && !Attribute.Name.StartsWith("data-block-guid"))
                            AttributesToRemove.Add(Attribute);
                    }
                }
                else
                {
                    foreach (var Attribute in item.Attributes)
                    {

                        if (Attribute.Name.StartsWith("data-block-"))
                            AttributesToRemove.Add(Attribute);
                    }
                }
                foreach (var Attribute in AttributesToRemove)
                    item.Attributes.Remove(Attribute);
            }

            IEnumerable<HtmlNode> sectionquery = html.DocumentNode.Descendants("section");
            foreach (HtmlNode item in sectionquery.ToList())
            {
                if (item.Attributes == null)
                    continue;
                List<dynamic> AttributesToRemove = new List<dynamic>();
                foreach (var Attribute in item.Attributes)
                {
                    if (Attribute.Name == "perm")
                        AttributesToRemove.Add(Attribute);
                }
                foreach (var Attribute in AttributesToRemove)
                    item.Attributes.Remove(Attribute);
            }
        }
        private void InjectBlocks(Pages page, HtmlDocument html)
        {
            IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
            foreach (HtmlNode item in query.ToList())
            {
                if (item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault().Value))
                {
                    Dictionary<string, string> Attributes = new Dictionary<string, string>();
                    foreach (HtmlAttribute attr in item.Attributes)
                    {
                        Attributes.Add(attr.Name, attr.Value);
                    }

                    ThemeTemplateResponse response = Core.Managers.BlockManager.Render(Attributes);
                    if (response != null)
                    {
                        if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value == "Logo" && page != null)
                        {
                            BuildLogoBlock(response, item);
                        }
                        else
                        {
                            item.InnerHtml = response.Markup;
                        }

                        if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                        {
                            HtmlDocument globalhtml = new HtmlDocument();
                            if (item.ChildNodes != null && item.ChildNodes.Count > 0 && item.ChildNodes[0].Attributes != null && item.ChildNodes[0].Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null && item.ChildNodes[0].Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                            {
                                globalhtml.LoadHtml(item.ChildNodes[0].InnerHtml);
                            }
                            else
                            {
                                globalhtml.LoadHtml(item.InnerHtml);
                            }

                            InjectBlocks(page, globalhtml);
                            item.InnerHtml = globalhtml.DocumentNode.OuterHtml;
                        }
                        else
                        {
                            HtmlDocument localhtml = new HtmlDocument();
                            localhtml.LoadHtml(item.InnerHtml);
                            if (localhtml.DocumentNode.ChildNodes.Count > 0)
                            {
                                item.InnerHtml = localhtml.DocumentNode.ChildNodes[0].InnerHtml;
                            }
                            else
                            {
                                item.InnerHtml = localhtml.DocumentNode.InnerHtml;
                            }
                        }

                        if (response.Scripts != null)
                        {
                            foreach (string script in response.Scripts)
                            {
                                if (!string.IsNullOrEmpty(script))
                                {
                                    WebForms.RegisterClientScriptInclude(Page, script, Page.ResolveUrl(script), WebForms.Execution.defer);
                                }
                            }
                        }

                        if (response.Styles != null)
                        {
                            foreach (string style in response.Styles)
                            {
                                if (!string.IsNullOrEmpty(style))
                                {
                                    WebForms.LinkCSS(Page, style, Page.ResolveUrl(style));
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(response.Script))
                        {
                            WebForms.RegisterClientScriptBlock(Page, "BlocksScript" + item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value, response.Script, true);
                        }

                        if (!string.IsNullOrEmpty(response.Style))
                        {
                            if (item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value))
                                WebForms.RegisterClientStyleBlock(Page, "BlocksStyle" + item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value, "<style type=\"text/css\" vj=\"true\" vjdataguid=\"" + item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value + "\">" + response.Style + "</style>", false);
                            else
                                WebForms.RegisterClientStyleBlock(Page, "BlocksStyle" + item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value, "<style type=\"text/css\">" + response.Style + "</style>", false);
                        }
                    }

                }
            }
        }

        private static void BuildLogoBlock(ThemeTemplateResponse Response, HtmlNode Node)
        {
            HtmlDocument LogoHtml = new HtmlDocument();
            LogoHtml.LoadHtml(Response.Markup);
            IEnumerable<HtmlNode> LogoImg = LogoHtml.DocumentNode.Descendants("img");
            if ((Node.Attributes["data-style"] == null || LogoImg == null || LogoImg.FirstOrDefault<HtmlNode>() == null ? false : LogoImg.FirstOrDefault<HtmlNode>().Attributes != null))
            {
                if ((
                    from a in LogoImg.FirstOrDefault<HtmlNode>().Attributes
                    where a.Name == "style"
                    select a).FirstOrDefault<HtmlAttribute>() == null)
                {
                    LogoImg.FirstOrDefault<HtmlNode>().Attributes.Add("style", Node.Attributes["data-style"].Value);
                }
                else
                {
                    LogoImg.FirstOrDefault<HtmlNode>().Attributes["style"].Value = Node.Attributes["data-style"].Value;
                }
            }
            Node.InnerHtml = LogoHtml.DocumentNode.OuterHtml;
        }

        private string InjectModules(string sb)
        {
            if (!string.IsNullOrEmpty(sb))
            {
                foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
                {
                    if (sb.Contains("<app id=\"" + m.ModuleID + "\"></app>"))
                    {
                        ModulesDictObj = new ExpandoObject();
                        ModulesDictObj.PaneName = m.PaneName;
                        ModulesDictObj.IsDeleted = m.IsDeleted;
                        ModulesDict.Add(m.ModuleID, ModulesDictObj);
                        //Replace Tokens w/ID's
                        sb = sb.Replace("<app id=\"" + m.ModuleID + "\"></app>", "<div id=\"vj_" + m.ModuleID.ToString() + "\" runat=\"server\" />");
                        m.PaneName = "vj_" + m.ModuleID.ToString();
                        m.IsDeleted = false;
                        InjectModuleActions(m);
                    }
                    else
                    {
                        ModulesDictObj = new ExpandoObject();
                        ModulesDictObj.IsDeleted = m.IsDeleted;
                        ModulesDict.Add(m.ModuleID, ModulesDictObj);
                        m.IsDeleted = true;
                    }
                }
            }
            else
            {
                foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
                {
                    ModulesDictObj = new ExpandoObject();
                    ModulesDictObj.IsDeleted = m.IsDeleted;
                    ModulesDict.Add(m.ModuleID, ModulesDictObj);
                    m.IsDeleted = true;
                }
            }
            return sb;
        }
        private string InjectModuleActions(ModuleInfo ModuleInfo)
        {
            try
            {
                if (LoadControl(Page.ResolveUrl("~/" + ModuleInfo.ModuleControl.ControlSrc)) is IActionable)
                {
                    IActionable ctl = LoadControl(Page.ResolveUrl("~/" + ModuleInfo.ModuleControl.ControlSrc)) as IActionable;
                    if (ctl != null && ctl.ModuleActions != null)
                    {
                        foreach (ModuleAction rootAction in ctl.ModuleActions)
                        {
                            //Process Children
                            List<ModuleAction> actions = new List<ModuleAction>();
                            foreach (ModuleAction action in rootAction.Actions)
                            {
                                if (action.Visible)
                                {
                                    if ((Globals.IsAdminControl() == false) ||
                                        (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                    {
                                        if (!action.Icon.Contains("://")
                                                && !action.Icon.StartsWith("/")
                                                && !action.Icon.StartsWith("~/"))
                                        {
                                            action.Icon = "~/images/" + action.Icon;
                                        }
                                        if (action.Icon.StartsWith("~/"))
                                        {
                                            action.Icon = Globals.ResolveUrl(action.Icon);
                                        }

                                        actions.Add(action);
                                    }
                                }

                            }

                            //var oSerializer = new JavaScriptSerializer();
                            //if (rootAction.Title == Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile))
                            //{
                            //    AdminActionsJSON = oSerializer.Serialize(actions);
                            //}
                            //else
                            //{
                            //    if (rootAction.Title == Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile))
                            //    {
                            //        CustomActionsJSON = oSerializer.Serialize(actions);
                            //    }
                            //}
                        }
                    }
                }
            }
            catch { }

            return string.Empty;

        }

        private void InjectStyle(Pages page)
        {
            HtmlGenericControl style = new HtmlGenericControl
            {
                TagName = "style"
            };
            style.Attributes.Add("type", "text/css");
            style.Attributes.Add("vj", "true");
            style.InnerHtml = page.Style.Trim('"');
            Page.Header.Controls.Add(style);
        }

        private Pages GetPage()
        {
            Pages page;
            if (!string.IsNullOrEmpty(Request.QueryString["revisionversion"]) && TabPermissionController.HasTabPermission("EDIT"))
            {
                page = PageManager.GetByVersion(PortalSettings.ActiveTab.TabID, Convert.ToInt32(Request.QueryString["revisionversion"]), PageManager.GetCultureCode(PortalSettings));
            }
            else if ((WorkflowManager.HasReviewPermission(PortalSettings.UserInfo) && !TabPermissionController.HasTabPermission("EDIT")) || (Request.Cookies["vj_IsPageEdit"] != null && Request.Cookies["vj_IsPageEdit"].Value == "true") || (m2v.HasValue && Request.Cookies["vj_InitUX"] != null && Request.Cookies["vj_InitUX"].Value == "true"))
            {
                page = PageManager.GetLatestVersion(PortalSettings.ActiveTab.TabID, PageManager.GetCultureCode(PortalSettings));
            }
            else
            {
                page = PageManager.GetLatestVersion(PortalSettings.ActiveTab.TabID, true, PageManager.GetCultureCode(PortalSettings), true);
            }

            return page;
        }

        private void InjectExtensionControl()
        {
            //For menu
            if (!string.IsNullOrEmpty(Request.QueryString["mid"]) && int.Parse(Request.QueryString["mid"]) == 0)
            {
                UserControl ctl = (UserControl)Page.LoadControl(Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Extension.ascx"));
                Page.Form.Controls.Add(ctl);
            }
        }

        private bool IsAllowed()
        {
            if (string.IsNullOrEmpty(Request.QueryString["icp"]) || Convert.ToBoolean(Request.QueryString["icp"]) == false)
            {
                return true;
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["icp"]) && Convert.ToBoolean(Request.QueryString["icp"]) == true && !string.IsNullOrEmpty(Request.QueryString["pv"]) && Convert.ToBoolean(Request.QueryString["pv"]) == true)
            {
                HtmlGenericControl body = (HtmlGenericControl)this.Page.FindControl("Body");
                body.Attributes.Add("class", "preview");
                return true;
            }

            return false;
        }

        private void RemoveDependencies(string ResourceControl)
        {
            Control loader = Page.FindControl(ResourceControl);

            if (loader != null)
            {
                for (int i = loader.Controls.Count - 1; i >= 0; i--)
                {
                    DnnCssInclude cssInclude = loader.Controls[i] as DnnCssInclude;
                    DnnJsInclude jsInclude = loader.Controls[i] as DnnJsInclude;

                    if (cssInclude != null && (CSSExcludes.Any(c => cssInclude.FilePath.ToLower().EndsWith(c.ToLower())) || CSSExcludesContains.Any(c => cssInclude.FilePath.ToLower().Contains(c.ToLower()))))
                    {
                        loader.Controls.Remove(cssInclude);
                        continue;
                    }
                    else if (jsInclude != null && (JSExcludes.Any(j => jsInclude.FilePath.ToLower().EndsWith(j.ToLower())) || JSExcludesContains.Any(j => jsInclude.FilePath.ToLower().Contains(j.ToLower()))))
                    {
                        loader.Controls.Remove(jsInclude);
                        continue;
                    }
                }
            }
        }

        private void LoadModuleInFrame()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["mid"]) && TabPermissionController.HasTabPermission("EDIT"))
            {
                int mid = int.Parse(Request.QueryString["mid"]);
                List<ModuleInfo> ModulesToRemove = new List<ModuleInfo>();
                foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
                {
                    if (m.ModuleID != mid)
                    {
                        ModulesToRemove.Add(m);
                    }
                }
                foreach (ModuleInfo m in ModulesToRemove)
                {
                    PortalSettings.ActiveTab.Modules.Remove(m);
                }
            }
        }
        private void ResetTheme()
        {
            try
            {
                string ThemeName = Core.Managers.ThemeManager.GetCurrent(PortalSettings.Current.PortalId).Name;
                string BaseEditorFolder = HttpContext.Current.Server.MapPath("~/Portals/" + PortalSettings.Current.PortalId + "/vThemes/" + ThemeName + "/editor");
                string ThemeCss = HttpContext.Current.Server.MapPath("~/Portals/" + PortalSettings.Current.PortalId + "/vThemes/" + ThemeName + "/Theme.css");
                if (!File.Exists(ThemeCss))
                {
                    ThemeManager.ProcessScss(PortalSettings.Current.PortalId, false);
                    Response.Redirect(Request.RawUrl, true);
                }
            }
            catch (Exception ex) { ExceptionManager.LogException(ex); }
        }


        private void HomeLogoAndSiteIcons()
        {
            string PortalRoot = PageManager.GetPortalRoot(PortalSettings.Current.PortalId);
            PortalRoot = PortalRoot + "/";
            string SocialSharingLogo = PortalRoot + PortalController.GetPortalSetting("SocialSharingLogo", PortalSettings.Current.PortalId, "", PortalSettings.CultureCode);
            string HomeScreenIcon = PortalRoot + PortalController.GetPortalSetting("HomeScreenIcon", PortalSettings.Current.PortalId, "", PortalSettings.CultureCode);
            if (!string.IsNullOrEmpty(SocialSharingLogo))
            {
                HtmlMeta Link = new HtmlMeta();
                Link.ID = "SocialSharingLogo";
                Link.Attributes.Add("property", "og:image");
                Link.Attributes.Add("content", SocialSharingLogo);
                Page.Header.Controls.Add(Link);
            }
            if (!string.IsNullOrEmpty(HomeScreenIcon))
            {
                HtmlGenericControl Link = new HtmlGenericControl("link");
                Link.ID = "HomeScreenIcon";
                Link.Attributes["rel"] = "apple-touch-icon";
                Link.Attributes["href"] = HomeScreenIcon;
                Link.Attributes["vj"] = "true";
                Page.Header.Controls.Add(Link);
            }
        }

        private void RenderLocalizedMetaData()
        {
            DotNetNuke.Framework.CDefault basePage = (DotNetNuke.Framework.CDefault)Page;
            if (page != null)
            {
                basePage.Title = !string.IsNullOrEmpty(page.Title) ? page.Title : (!string.IsNullOrEmpty(page.Name) ? PortalSettings.Current.PortalName + " > " + page.Name : basePage.Title);
                basePage.Description = !string.IsNullOrEmpty(page.Description) ? page.Description : basePage.Description;
            }

            // Set Default Description if page description empty or not given
            if (string.IsNullOrEmpty(basePage.Description))
            {
                string Vanjaro_Description = DotNetNuke.Services.Localization.Localization.GetString("Vanjaro_Description", DotNetNuke.Services.Localization.Localization.GlobalResourceFile);
                basePage.Description = !string.IsNullOrEmpty(Vanjaro_Description) ? Vanjaro_Description : "Vanjaro is built on DNN Platform";
            }

        }


        private void GetCookieConsentMarkup()
        {
            try
            {
                if (string.IsNullOrEmpty(Request.QueryString["mid"]) && string.IsNullOrEmpty(Request.QueryString["icp"]))
                {
                    if (Request.Cookies["cookieconsent_status"] == null)
                    {
                        string d = PortalController.GetPortalSetting("Vanjaro_CookieConsent", PortalSettings.PortalId, "False");
                        if (d == bool.TrueString)
                        {
                            if (FindControlRecursive(this, "CookieConsentPane") == null)
                            {
                                Controls.Add(ParseControl("<div id=\"CookieConsentPane\" runat=\"server\" />"));
                            }

                            if (FindControlRecursive(this, "CookieConsentPane") != null)
                            {

                                IDictionary<string, object> Objects = new ExpandoObject() as IDictionary<string, object>;
                                Objects.Add("CookieConsentUrl", PortalSettings.CookieMoreLink);
                                string Template = RazorEngineManager.RenderTemplate("", BlockPath + "/Cookie Consent/", "Default", Objects);
                                Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                                FindControlRecursive(this, "CookieConsentPane").Controls.Add(ParseControl(Template));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
            }
        }

        public string ResouceFilePath
        {
            get
            {
                return Globals.ApplicationMapPath + @"\Portals\_default\" + Core.Managers.BlockManager.GetTheme(PortalSettings.PortalId) + "App_LocalResources\\Shared.resx";
            }
        }

        public string BlockPath
        {
            get
            {
                return Core.Managers.BlockManager.GetVirtualPath() + Core.Managers.BlockManager.GetTheme(PortalSettings.PortalId) + "templates/design";
            }
        }


        #region Private Method
        // Edit Page in DNN Persona Bar. Lock Edit Bar. Open a Vanjaro page is new window.
        public static void ToggleUserMode(string mode = "VIEW")
        {
            DotNetNuke.Services.Personalization.PersonalizationController personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            DotNetNuke.Services.Personalization.PersonalizationInfo personalization = personalizationController.LoadProfile(PortalSettings.Current.UserInfo.UserID, PortalSettings.Current.PortalId);
            personalization.Profile["Usability:UserMode" + PortalSettings.Current.PortalId] = mode.ToUpper();
            personalization.IsModified = true;
            personalizationController.SaveProfile(personalization);
        }

        #region Migrate Page
        private void MigratePage()
        {
            if (m2v.HasValue || !string.IsNullOrEmpty(Request.QueryString["m2vsetup"]))
            {
                WebForms.RegisterClientScriptBlock(Page, "m2vStyle", "<div class='style-wrapper'><style type=\"text/css\">#dnn_ContentPane >.DnnModule{display:none !important;}</style></div>", false);
            }

            //Only work when call is not from Iframe like page,role etc extension,
            if (string.IsNullOrEmpty(Request.QueryString["mid"]) && string.IsNullOrEmpty(Request.QueryString["icp"]))
            {
                if (m2v.HasValue && !string.IsNullOrEmpty(Request.QueryString["SkinSrc"]) && Request.QueryString["SkinSrc"].ToLower() == "[G]Skins/Vanjaro/Base".ToLower() && PortalSettings.UserInfo.IsInRole("Administrators"))
                {
                    Pages page = GetPage();
                    if (page != null && !page.IsPublished)
                    {
                        if (!m2v.Value && !page.Content.Contains("data-m2v"))
                        {
                            HtmlDocument html = new HtmlDocument();
                            html.LoadHtml(page.Content.ToString());
                            page.Content = MigrateManager.MigrateInjectBlocks(PortalSettings, html);
                            PageManager.UpdatePage(page, PortalSettings.UserInfo.UserID);
                            Response.Redirect(NavigationManager.NavigateURL(PortalSettings.ActiveTab.TabID, "", "m2v=true", "skinsrc=[g]skins/vanjaro/base", "containersrc=[g]containers/vanjaro/base"));
                        }

                        // Redirect if Page already migrated 
                        if (page.Content.ToString().Contains("data-m2v") && !m2v.Value)
                        {
                            Response.Redirect(NavigationManager.NavigateURL(PortalSettings.ActiveTab.TabID, "", "m2v=true", "skinsrc=[g]skins/vanjaro/base", "containersrc=[g]containers/vanjaro/base"));
                        }
                        else
                        {
                            if (!(!string.IsNullOrEmpty(Request.QueryString["pv"]) && Request.QueryString["pv"] == "true"))
                            {
                                WebForms.RegisterStartupScript(Page, "MigratePage", "<script type=\"text/javascript\" vanjarocore=\"true\">" + MigrateManager.GetMigratePageToastMarkup(Page) + "</script> <style type=\"text/css\">" + MigrateManager.GetMigratePageCSS(Page) + "</style>", false);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}

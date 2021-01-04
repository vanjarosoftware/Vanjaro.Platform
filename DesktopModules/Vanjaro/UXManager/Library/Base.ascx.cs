using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Url.FriendlyUrl;
using DotNetNuke.UI.ControlPanels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vanjaro.Common;
using Vanjaro.Common.ASPNET;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Manager;
using Vanjaro.UXManager.Library.Entities;
using Vanjaro.UXManager.Library.Entities.Enum;
using static Vanjaro.UXManager.Library.Managers;
using System.Net;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Library
{
    public partial class Base : ControlPanelBase
    {
        private string TemplateLibraryURL = string.Empty;
        private string ExtensionStoreURL = string.Empty;
        private string ExtensionURL = string.Empty;
        private bool? m2v = null;

#if DEBUG
        private const bool ShowMissingKeys = true;
#else
        private const bool ShowMissingKeys = false;
#endif
        protected override void OnInit(EventArgs e)
        {
#if RELEASE
#else
#endif
#if RELEASE
            TemplateLibraryURL = "~" + Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/placeholder.html");
            ExtensionStoreURL = "~" + Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/placeholder.html");                       
#else
            TemplateLibraryURL = "http://library.vanjaro.local/templates/tid/49A70BA1-206B-471F-800A-679799FF09DF";
            ExtensionStoreURL = "http://store.vanjaro.local/store";
#endif
            ExtensionURL= ServiceProvider.NavigationManager.NavigateURL().ToLower().Replace(PortalSettings.Current.DefaultLanguage.ToLower(), PortalSettings.Current.CultureCode.ToLower()).TrimEnd('/') + MenuManager.GetURL() + "mid=0&icp=true&guid=54caeff2-9fac-42ae-8594-40312867d56a#/installpackage";

            base.OnInit(e);
            if (CanShowUXManager())
            {
                ServicesFramework.Instance.RequestAjaxScriptSupport();
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["m2v"] == null)
            {
                m2v = null;
            }
            else
            {
                m2v = Convert.ToBoolean(Request.QueryString["m2v"]);
            }

            if (CanShowUXManager())
            {
                if (string.IsNullOrEmpty(Request.QueryString["ctl"]) && (string.IsNullOrEmpty(Request.QueryString["icp"]) || Convert.ToBoolean(Request.QueryString["icp"]) == false))
                {
                    if (string.IsNullOrEmpty(Request.QueryString["uxmode"]))
                    {
                        Literal lt = new Literal();
                        IDictionary<string, object> dynObjects = new ExpandoObject() as IDictionary<string, object>;
                        dynObjects.Add("Setting", GetBaseModel());
                        string Template = RazorEngineManager.RenderTemplate("VanjaroUXManager", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/"), "Base", dynObjects);
                        Template = new DNNLocalizationEngine(null, Server.MapPath("~/DesktopModules/Vanjaro/UXManager/Library/App_LocalResources/Shared.resx"), ShowMissingKeys).Parse(Template);
                        lt.Text = Template;
                        Controls.Add(lt);
                    }

                    string DirectoryPath = System.Web.Hosting.HostingEnvironment.MapPath("~/DesktopModules/Vanjaro/UXManager/Library/Resources/tui/");
                    if (Directory.Exists(DirectoryPath))
                    {
                        foreach (string file in Directory.GetFiles(DirectoryPath))
                        {
                            string FileName = Path.GetFileName(file);
                            if (!string.IsNullOrEmpty(FileName))
                            {
                                if (FileName.EndsWith(".js"))
                                {
                                    WebForms.RegisterClientScriptInclude(Page, FileName.Replace(".", ""), Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/tui/" + FileName), false);
                                }
                                else
                                {
                                    WebForms.LinkCSS(Page, FileName.Replace(".", ""), Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/tui/" + FileName), false);
                                }
                            }
                        }
                    }

                    WebForms.LinkCSS(Page, "UXManagerAppCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/StyleSheets/app.css"));


                    WebForms.LinkCSS(Page, "GrapesJsCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJs/css/grapes.min.css"));
                    WebForms.LinkCSS(Page, "GrapickJsCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJs/css/grapick.min.css"));
                    WebForms.LinkCSS(Page, "GrapesJsPanelCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/jsPanel/jspanel.min.css"));


                    WebForms.RegisterClientScriptInclude(Page, "GrapesJsJs", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/uxmanager.min.js"), false);
                    WebForms.LinkCSS(Page, "GrapesJspluginCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJsManagers/css/uxmanager.css"));
                    WebForms.LinkCSS(Page, "FontawesomeV4Css", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJs/css/fontawesome/v4.css"));

                    LocalizeGrapeJS();

                    FrameworkManager.Load(this, "FontAwesome");
                }
                string NavigateURL = PageManager.GetCurrentTabUrl(PortalSettings);
                string ClientScript = "var AppMenus=" + JsonConvert.SerializeObject(AppManager.GetAll(AppType.Module)) + "; var m2vPageTabUrl='" + NavigateURL + "'; var CurrentTabUrl ='" + NavigateURL + "'; var IsAdmin=" + PortalSettings.UserInfo.IsInRole("Administrators").ToString().ToLower() + ";var vjProductSKU='" + Core.Components.Product.SKU + "';";
                WebForms.RegisterClientScriptBlock(Page, "GrapesJsAppsExts", ClientScript, true);

                if (TabPermissionController.HasTabPermission("EDIT") && !Request.QueryString.AllKeys.Contains("mid"))
                {
                    string OpenPopup = string.Empty;
                    if (m2v.HasValue && !m2v.Value && (Vanjaro.Core.Managers.PageManager.GetPages(PortalSettings.ActiveTab.TabID).Count == 0))
                    {
                        OpenPopup = "#/choosetemplate";
                    }
                    else if (Request.QueryString["m2vsetup"] != null && Request.QueryString["m2vsetup"] == "page")
                    {
                        OpenPopup = "#detail";
                    }

                    if (!string.IsNullOrEmpty(OpenPopup))
                    {
                        NavigateURL = PageManager.GetCurrentTabUrl(PortalSettings, "&mid=0&icp=true&guid=10E56C75-548E-4A10-822E-52E6AA2AB45F" + OpenPopup);
                        WebForms.RegisterStartupScript(Page, "m2v", "<script type=\"text/javascript\" vanjarocore=\"true\">OpenPopUp(event, 800,'right','Choose Template', '" + NavigateURL + "')</script>", false);
                    }
                }
            }
            else
                WebForms.RegisterClientScriptBlock(Page, "MenuSettingsBlocks", "$(document).ready(function(){$('[href=\"#MenuSettings\"]').click();$('#mode-switcher').remove();setTimeout(function(){$('.gjs-cv-canvas__frames').css('pointer-events','none');}, 100); });", true);


        }


        protected void Page_PreRender(object sender, EventArgs e)
        {

            if (InjectEditor())
                WebForms.RegisterClientScriptBlock(Page, "EditorInit", "var TemplateLibraryURL = \"" + TemplateLibraryURL + "\"; var ExtensionStoreURL = \"" + ExtensionStoreURL + "\"; var ExtensionURL = \"" + ExtensionURL + "\"; $(document).ready(function(){ if(typeof GrapesjsInit !='undefined') GrapesjsInit(" + JsonConvert.SerializeObject(Editor.Options) + "); });", true);
        }

        private BaseModel GetBaseModel()
        {
            BaseModel item = new BaseModel
            {
                LoadingImage = Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Images/loading.gif"),
                AboutUrl = AppManager.GetAboutUrl(),
                Logo = Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Images/Vanjaro.png"),
                MenuMarkUp = MenuManager.RenderMenu(MenuManager.ParseMenuCategoryTree(null), null),
                NotificationCount = Core.Managers.NotificationManager.RenderNotificationsCount(PortalSettings.PortalId),
                ToolbarMarkUp = ToolbarManager.RenderMenu(),
                LanguageMarkUp = LanguageManager.RenderLanguages(),
                HasShortcut = (ShortcutManager.GetShortcut().Where(x => x.Shortcut.Visibility).Count() > 0)
            };
            item.ShortcutMarkUp = item.HasShortcut ? ShortcutManager.RenderShortcut() : string.Empty;

            if (!Editor.Options.EditPage)
                item.HasTabEditPermission = true;
            else
                item.HasTabEditPermission = TabPermissionController.HasTabPermission("EDIT");

            item.EditPage = Editor.Options.EditPage;
            item.ShowUXManager = string.IsNullOrEmpty(Core.Managers.CookieManager.GetValue("InitGrapejs")) ? false : Convert.ToBoolean(Core.Managers.CookieManager.GetValue("InitGrapejs"));
            return item;
        }

        private void LocalizeGrapeJS()
        {
            string FilePath = Server.MapPath("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/Localization/Localization.js");
            string FileText = File.ReadAllText(FilePath);
            string SharedResourceFile = Server.MapPath("~/DesktopModules/Vanjaro/UXManager/Library/App_LocalResources/Shared.resx");
            FileText = new DNNLocalizationEngine(null, SharedResourceFile, Extension.ShowMissingKeysStatic).Parse(FileText);
            WebForms.RegisterStartupScript(Page, "LocalizeGrapeJS", FileText, true);
        }

        private string GetAttributes(dynamic attribute)
        {
            StringBuilder sb = new StringBuilder();
            string Attribute = string.Empty;
            sb.Append("{");
            foreach (dynamic attr in attribute)
            {
                Attribute += attr.Name + ":" + attr.Value.ToString() + ",";
            }
            Attribute = Attribute.Remove(Attribute.Length - 1, 1);
            sb.Append(Attribute);
            sb.Append("}");
            return sb.ToString();
        }

        private bool CanShowUXManager()
        {
            if (PortalSettings.UserId > 0 && TabPermissionController.CanViewPage() && (TabPermissionController.HasTabPermission("EDIT") || MenuManager.GetExtentions().Count > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool InjectEditor()
        {
            if (PortalSettings.UserId > 0 && TabPermissionController.CanViewPage() && (TabPermissionController.HasTabPermission("EDIT") || !Editor.Options.EditPage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class StringExtension
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
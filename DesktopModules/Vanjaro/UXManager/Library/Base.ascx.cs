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
            TemplateLibraryURL = "https://library.vanjaro.cloud/templates/tid/" + Core.Managers.ThemeManager.CurrentTheme.GUID.ToLower() + "/type/block";
            ExtensionStoreURL = "https://store.vanjaro.com";
# else
            TemplateLibraryURL = "http://library.vanjaro.local/templates/tid/" + Core.Managers.ThemeManager.CurrentTheme.GUID.ToLower()+"/type/block";
            ExtensionStoreURL = "http://store.vanjaro.local/store";
#endif
            ExtensionURL = ServiceProvider.NavigationManager.NavigateURL().ToLower().Replace(PortalSettings.Current.DefaultLanguage.ToLower(), PortalSettings.Current.CultureCode.ToLower()).TrimEnd('/') + MenuManager.GetURL() + "mid=0&icp=true&guid=54caeff2-9fac-42ae-8594-40312867d56a#!/installpackage";

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

                    try
                    {
                        string Fonts = JsonConvert.SerializeObject(Core.Managers.ThemeManager.GetDDLFonts("all"));
                        WebForms.RegisterClientScriptBlock(Page, "VJThemeFonts", "var VJFonts=" + Fonts + ";", true);
                    }
                    catch (Exception) { }

                    WebForms.LinkCSS(Page, "UXManagerAppCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/StyleSheets/app.css"), true, "DnnBodyProvider");
                    WebForms.LinkCSS(Page, "GrapesJsCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJs/css/grapes.min.css"), false);
                    WebForms.LinkCSS(Page, "GrapickJsCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJs/css/grapick.min.css"), false);
                    WebForms.LinkCSS(Page, "GrapesJsPanelCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/jsPanel/jspanel.min.css"), false);
                    WebForms.RegisterClientScriptInclude(Page, "CustomCodeJs", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/Blocks/custom-code.js"), false);
                    WebForms.RegisterClientScriptInclude(Page, "GrapesJsJs", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/uxmanager.min.js"), false);
                    WebForms.LinkCSS(Page, "GrapesJspluginCss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJsManagers/css/uxmanager.css"), true, "DnnBodyProvider");
                    WebForms.LinkCSS(Page, "FontawesomeV4Css", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Scripts/GrapesJs/css/fontawesome/v4.css"), false);

                    LocalizeThemeJS();

                    if (!string.IsNullOrEmpty(Core.Managers.ThemeManager.CurrentTheme.Assembly) && !string.IsNullOrEmpty(Core.Managers.ThemeManager.CurrentTheme.DesignScript) && !Core.Entities.Editor.Options.Blocks)
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ThemeDesignScript", "<script type=\"text/javascript\" src=\"" + Page.ClientScript.GetWebResourceUrl(Type.GetType(Core.Managers.ThemeManager.CurrentTheme.Assembly), Core.Managers.ThemeManager.CurrentTheme.DesignScript) + "\"></script>", false);

                    LocalizeGrapeJS();

                    FrameworkManager.Load(this, "Bootstrap", false);
                    FrameworkManager.Load(this, "FontAwesome", false);
                }
                string NavigateURL = PageManager.GetCurrentTabUrl(PortalSettings);
                string ClientScript = "var AppMenus=" + JsonConvert.SerializeObject(AppManager.GetAll(AppType.Module)) + "; var m2vPageTabUrl='" + NavigateURL + "'; var CurrentTabUrl ='" + NavigateURL + "'; var IsAdmin=" + PortalSettings.UserInfo.IsInRole("Administrators").ToString().ToLower() + ";var vjProductSKU='" + Vanjaro.Common.Components.Product.SKU + "';";
                WebForms.RegisterClientScriptBlock(Page, "GrapesJsAppsExts", ClientScript, true);

                if (TabPermissionController.HasTabPermission("EDIT") && !Request.QueryString.AllKeys.Contains("mid"))
                {
                    string OpenPopup = string.Empty;
                    if (m2v.HasValue && !m2v.Value && (Vanjaro.Core.Managers.PageManager.GetPages(PortalSettings.ActiveTab.TabID).Count == 0))
                    {
                        OpenPopup = "#!/choosetemplate";
                    }
                    else if (Request.QueryString["m2vsetup"] != null && Request.QueryString["m2vsetup"] == "page")
                    {
                        OpenPopup = "#!/detail";
                    }

                    if (!string.IsNullOrEmpty(OpenPopup))
                    {
                        NavigateURL = PageManager.GetCurrentTabUrl(PortalSettings, "&mid=0&icp=true&guid=10E56C75-548E-4A10-822E-52E6AA2AB45F" + OpenPopup);
                        WebForms.RegisterStartupScript(Page, "m2v", "<script type=\"text/javascript\" vanjarocore=\"true\">$(document).ready(function(){OpenPopUp(event, 800,'right','Choose Template', '" + NavigateURL + "'); });</script>", false);
                    }
                }
            }
            else
                WebForms.RegisterClientScriptBlock(Page, "MenuSettingsBlocks", "$(document).ready(function(){$('[href=\"#MenuSettings\"]').click();$('#mode-switcher').remove();setTimeout(function(){$('.gjs-cv-canvas__frames').css('pointer-events','none');}, 100); });", true);


        }


        protected void Page_PreRender(object sender, EventArgs e)
        {

            if (Core.Managers.PageManager.InjectEditor(PortalSettings))
                WebForms.RegisterClientScriptBlock(Page, "EditorInit", "var vjEditorSettings =" + JsonConvert.SerializeObject(Core.Entities.Editor.Options) + "; var  TemplateLibraryURL = \"" + TemplateLibraryURL + "\"; var ExtensionStoreURL = \"" + ExtensionStoreURL + "\"; var ExtensionURL = \"" + ExtensionURL + "\"; $(document).ready(function(){ if(typeof GrapesjsInit !='undefined' && getCookie('vj_InitUX') == 'true') GrapesjsInit(); });", true);

        }

        private BaseModel GetBaseModel()
        {
            BaseModel item = new BaseModel
            {
                LoadingImage = Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Images/loading.svg"),
                AboutUrl = AppManager.GetAboutUrl(),
                Logo = Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/Images/Vanjaro.png"),
                MenuMarkUp = MenuManager.RenderMenu(MenuManager.ParseMenuCategoryTree(null), null),
                NotificationCount = Core.Managers.NotificationManager.RenderNotificationsCount(PortalSettings.PortalId),
                ToolbarMarkUp = ToolbarManager.RenderMenu(),
                LanguageMarkUp = Core.Entities.Editor.Options.Language == true ? LanguageManager.RenderLanguages() : string.Empty,
                HasShortcut = (ShortcutManager.GetShortcut().Where(x => x.Shortcut.Visibility).Count() > 0)
            };
            item.ShortcutMarkUp = item.HasShortcut ? ShortcutManager.RenderShortcut() : string.Empty;

            if (!Core.Entities.Editor.Options.EditPage)
                item.HasTabEditPermission = true;
            else
                item.HasTabEditPermission = TabPermissionController.HasTabPermission("EDIT");

            item.EditPage = Core.Entities.Editor.Options.EditPage;
            item.CustomBlocks = Core.Entities.Editor.Options.CustomBlocks;
            item.Library = Core.Entities.Editor.Options.Library;
            item.ShowUXManager = string.IsNullOrEmpty(Core.Managers.CookieManager.GetValue("vj_InitUX")) ? false : Convert.ToBoolean(Core.Managers.CookieManager.GetValue("vj_InitUX"));
            return item;
        }

        private void LocalizeThemeJS()
        {
            string FilePath = Server.MapPath("~/Portals/_default/vThemes/" + Core.Managers.ThemeManager.CurrentTheme.Name + "/Localization.js");
            if (File.Exists(FilePath))
            {
                string FileText = File.ReadAllText(FilePath);
                string SharedResourceFile = Server.MapPath("~/Portals/_default/vThemes/" + Core.Managers.ThemeManager.CurrentTheme.Name + "/App_LocalResources/Shared.resx");
                FileText = new DNNLocalizationEngine(null, SharedResourceFile, Extension.ShowMissingKeysStatic).Parse(FileText);
                WebForms.RegisterStartupScript(Page, "LocalizeThemeJS", FileText, true);
            }
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
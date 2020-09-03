using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common;
using Vanjaro.Common.ASPNET;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Foundation;
using Vanjaro.UXManager.Library.Entities.Enum;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Library
{
    public partial class Extension : AngularModuleBase
    {
        private dynamic ext;
        private int ModuleID;
        protected override void OnInit(EventArgs e)
        {
            if (int.TryParse(Request.QueryString["mid"], out ModuleID) && ModuleID == 0 && Request.QueryString.Get("guid") != null && !string.IsNullOrEmpty(Request.QueryString.Get("guid")))
            {
                ext = MenuManager.GetExtentions(false).Where(x => x.SettingGuid == Guid.Parse(Request.QueryString.Get("guid"))).FirstOrDefault();
                if (ext == null)
                {
                    ext = ToolbarManager.GetExtentions().Where(x => x.SettingGuid == Guid.Parse(Request.QueryString.Get("guid"))).FirstOrDefault();
                }

                if (ext == null)
                {
                    ext = AppManager.GetExtentions(AppType.None).Where(x => x.SettingGuid == Guid.Parse(Request.QueryString.Get("guid"))).FirstOrDefault();
                }

                if (ext == null)
                {
                    ext = Core.Managers.BlockManager.GetExtentions().Where(x => x.Guid == Guid.Parse(Request.QueryString.Get("guid"))).FirstOrDefault();
                }

                if (ext == null)
                {
                    ext = Core.Managers.ExtensionManager.Extentions.Where(x => x.SettingGuid == Guid.Parse(Request.QueryString.Get("guid"))).FirstOrDefault();
                }
            }
            base.OnInit(e);
            WebForms.LinkCSS(Page, "extensioncommoncss", Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/Resources/StyleSheets/common.css"));
            FrameworkManager.Load(this, "FontAwesome");

            //For ThemeBuilder
            if (!string.IsNullOrEmpty(Request.QueryString["guid"]) && Request.QueryString["guid"].ToLower() == "726c5619-e193-4605-acaf-828576ba095a")
                FrameworkManager.Load(this, "SpectrumColorPicker");
        }

        public override AppInformation App => ext != null ? ext.App : null;

        public override string AppCSSPath => ext != null ? ext.AppCssPath : string.Empty;
        public override string AppJSPath => ext != null ? ext.AppJsPath : string.Empty;
        public override string AppTemplatePath => ext != null ? ext.UIPath : string.Empty;
        public override string UIEngineAngularBootstrapPath => ext != null ? ext.UIEngineAngularBootstrapPath : string.Empty;
        public override string[] Dependencies => ext != null ? ext.Dependencies : new string[] { "Bootstrap" };

        public override bool ShowMissingKeys => ShowMissingKeysStatic;

        public override string AccessRoles => ext != null ? ext.AccessRoles(UserInfo) : string.Empty;

        public override List<AngularView> AngularViews => ext != null ? ext.AngularViews : string.Empty;

        public static bool ShowMissingKeysStatic =>
#if DEBUG
                true;
#else
                false;
#endif


    }
}

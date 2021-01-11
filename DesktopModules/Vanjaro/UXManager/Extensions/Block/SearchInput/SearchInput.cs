using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.SearchInput.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Extensions.Block.SearchInput
{
    public class SearchInput : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string Name => "Search Input";
        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fas fa-search";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);
        public bool Visible { get; set; } = true;

        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                return result;
            }
        }

        public ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
        {
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ThemeTemplateResponse response = new ThemeTemplateResponse
            {
                Markup = GenerateMarkup(Attributes),
                Scripts = new string[3] { ResourcesFolderPath + "Scripts/SearchInput.js", "~/DesktopModules/Vanjaro/Common/Frameworks/jQuery/Plugins/Autocomplete/1.0.7/js/jquery.auto-complete.min.js", "~/DesktopModules/Vanjaro/Common/Frameworks/Toastr/2.1.4/js/toastr.min.js" },
                Styles = new string[3] { ResourcesFolderPath + "Stylesheets/SearchInput.css", "~/DesktopModules/Vanjaro/Common/Frameworks/jQuery/Plugins/Autocomplete/1.0.7/css/jquery.auto-complete.css", "~/DesktopModules/Vanjaro/Common/Frameworks/Toastr/2.1.4/css/toastr.min.css" }
            };
            return response;
        }

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Stylesheets/SearchInput.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Scripts/SearchInput.js";

        public string ResourcesFolderPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/";

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "search input");
            }
        }

        public string ResouceFilePath
        {
            get
            {

                return Globals.ApplicationMapPath + @"\Portals\_default\" + Core.Managers.BlockManager.GetTheme() + "App_LocalResources//Shared.resx";
            }
        }

        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap" };

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }

        private string GenerateMarkup(Dictionary<string, string> Attributes)
        {
            try
            {
                Entities.SearchInput searchInput = new Entities.SearchInput();
                IDictionary<string, object> Objects = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                Objects.Add("SearchInput", searchInput);
                string Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], Objects);
                Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                return Template;
            }
            catch (Exception ex)
            {
                Core.Managers.ExceptionManage.LogException(ex);
                return ex.Message;
            }
        }
    }
}
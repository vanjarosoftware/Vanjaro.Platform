using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.BlockLanguage.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Extensions.Block.BlockLanguage
{
    public class Language : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Language";

        public string Name => "Language";
        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fa fa-language";

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
            ThemeTemplateResponse response = new ThemeTemplateResponse
            {
                Markup = GenerateMarkup(Attributes),
                Scripts = new string[1] { ResourcesFolderPath + "Scripts/app.js" }
            };
            return response;
        }

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string TemplatePath => "~/vThemes/" + ThemeManager.CurrentTheme.Name + "/Blocks/Language/Templates/";

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string ResourcesFolderPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/";

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "Language");
            }
        }

        public string ResouceFilePath
        {
            get
            {

                return Globals.ApplicationMapPath + @"\Portals\_default\" + Core.Managers.BlockManager.GetTheme() + "App_LocalResources\\Shared.resx";
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
                IDictionary<string, object> dynObjects = new ExpandoObject() as IDictionary<string, object>;
                dynObjects.Add("SelectedLanguage", PortalSettings.Current.CultureCode);
                dynObjects.Add("DefaultLanguage", LanguageManager.GetCultureListItems(true).Where(a => a.Code == PortalSettings.Current.DefaultLanguage).FirstOrDefault());
                dynObjects.Add("Languages", Managers.LanguageManager.GetCultureListItems(false));
                string InitGrapejs = "false";
                if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["InitGrapejs"] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["InitGrapejs"].Value))
                {
                    InitGrapejs = HttpContext.Current.Request.Cookies["InitGrapejs"].Value;
                }

                dynObjects.Add("InitGrapejs", InitGrapejs);
                string Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], dynObjects);
                Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                return Template;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message;
            }
        }
    }
}
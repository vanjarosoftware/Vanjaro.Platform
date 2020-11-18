using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.Menu.Entities;
using Vanjaro.UXManager.Extensions.Block.Menu.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Extensions.Block.Menu
{
    public class Menu : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string Name => "Menu";
        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fas fa-bars";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);
        public bool Visible { get; set; } = true;

        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                //result.Add("data-block-global", "true");
                //result.Add("data-block-type", Name);
                //result.Add("data-block-guid", "" + Guid.ToString().ToLower() + "");
                //result.Add("data-block-template", "Default");
                //result.Add("data-block-nodeselector", "*");
                //result.Add("data-block-includehidden", "false");
                //result.Add("data-block-allow-customization", "false");
                return result;
            }
        }

        public ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
        {
            ThemeTemplateResponse response = new ThemeTemplateResponse
            {
                Markup = GenerateMarkup(Attributes)
            };
            return response;
        }

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";


        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "menu");
            }
        }

        public string ResouceFilePath
        {
            get
            {

                return Globals.ApplicationMapPath + @"\Portals\_default\" + Core.Managers.BlockManager.GetTheme() + "/App_LocalResources/Shared.resx";
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
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;

                if (ps != null && ps.ActiveTab != null && ps.ActiveTab.BreadCrumbs == null)
                {
                    ps.ActiveTab.BreadCrumbs = new System.Collections.ArrayList();
                }

                Dictionary<string, string> BaseAttributes = Core.Managers.BlockManager.GetGlobalConfigs(ps, "menu");
                if (Attributes["data-block-global"] == "true")
                {
                    Attributes = BaseAttributes;
                }
                else
                {
                    //Loop on base attributes and add missing attribute
                    foreach (KeyValuePair<string, string> attr in BaseAttributes)
                    {
                        if (!Attributes.ContainsKey(attr.Key))
                        {
                            Attributes.Add(attr.Key, attr.Value);
                        }
                    }
                }

                MenuSetting menuSetting = new MenuSetting
                {
                    NodeSelector = Attributes["data-block-nodeselector"],
                    IncludeHidden = Convert.ToBoolean(Attributes["data-block-includehidden"]),
                };

                MenuNode rootNode = new MenuNode(
                                     Localiser.LocaliseDNNNodeCollection(
                                     Navigation.GetNavigationNodes(
                                     ExtensionInfo.GUID,
                                     Navigation.ToolTipSource.None,
                                     -1,
                                     -1,
                                     MenuBase.GetNavNodeOptions(true))));

                MenuBase menu = new MenuBase();
                menu.ApplySetting(menuSetting);
                menu.RootNode = rootNode;
                menu.Initialize();

                IDictionary<string, object> dynObjects = new ExpandoObject() as IDictionary<string, object>;
                dynObjects.Add("Menu", menu);
                string Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], dynObjects);
                Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                return Template;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return ex.Message;
            }
        }
    }
}
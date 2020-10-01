using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.RegisterLink.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.RegisterLink
{
    public class RegisterLink : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string Name => "Register Link";

        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fas fa-sign-in-alt";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);

        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                //result.Add("data-block-type", Name);
                //result.Add("data-block-template", "Default");
                //result.Add("data-block-guid", "" + Guid.ToString().ToLower() + "");                
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

        public string TemplatePath => "~/vThemes/" + Core.Managers.ThemeManager.GetCurrent().Name + "/Blocks/Register Link/Templates/";

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "register link");
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
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;

                if (ps != null && ps.ActiveTab != null && ps.ActiveTab.BreadCrumbs == null)
                {
                    ps.ActiveTab.BreadCrumbs = new System.Collections.ArrayList();
                }

                Dictionary<string, string> BaseAttributes = Core.Managers.BlockManager.GetGlobalConfigs(ps, "register link");
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
                Entities.RegisterLink rl = new Entities.RegisterLink
                {
                    Url = Globals.RegisterURL(HttpUtility.UrlEncode(ServiceProvider.NavigationManager.NavigateURL()), Null.NullString),
                    IsAuthenticated = HttpContext.Current.Request.IsAuthenticated,
                    ShowRegisterLink = Convert.ToBoolean(Attributes["data-block-showregisterlink"]),
                    ShowNotification = Convert.ToBoolean(Attributes["data-block-shownotification"]),
                    ShowAvatar = Convert.ToBoolean(Attributes["data-block-showavatar"])
                };
                IDictionary<string, object> dynObjects = new ExpandoObject() as IDictionary<string, object>;
                dynObjects.Add("RegisterLink", rl);
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
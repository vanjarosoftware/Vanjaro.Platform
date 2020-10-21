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
using Vanjaro.UXManager.Extensions.Block.Register.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Extensions.Block.Register
{
    public class Register : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string Name => "Register";

        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fas fa-user-plus";

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
                Scripts = new string[3] { "~/DesktopModules/Vanjaro/Common/Frameworks/ValidationJS/1.14.0/validate.min.js", ResourcesFolderPath + "Scripts/Register.js", "~/DesktopModules/Vanjaro/Common/Frameworks/ValidationJS/1.14.0/mnValidationService.min.js" },
                Styles = new string[1] { ResourcesFolderPath + "Stylesheets/app.css" }
            };
            return response;
        }

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string ResourcesFolderPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/";

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "register");
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

                Dictionary<string, string> BaseAttributes = Core.Managers.BlockManager.GetGlobalConfigs(ps, "register");
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
                DotNetNuke.Security.RegistrationSettings RegistrationSettings = (PortalController.Instance.GetCurrentSettings() as PortalSettings).Registration;
                int UserRegistration = (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserRegistration;
                IDictionary<string, object> dynObjects = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                Entities.Register register = new Entities.Register
                {
                    ButtonAlign = Attributes["data-block-buttonalign"],
                    ShowLabel = Convert.ToBoolean(Attributes["data-block-showlabel"]),
                    TermsPrivacy = Convert.ToBoolean(Attributes["data-block-termsprivacy"]),
                    ShowGoogleReCaptcha = Convert.ToBoolean(Attributes["data-block-showgooglerecaptcha"])
                };
                dynObjects.Add("Register", register);
                dynObjects.Add("RegistrationSettings", RegistrationSettings);
                dynObjects.Add("UserRegistration", UserRegistration);

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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.Core.Services;
using Vanjaro.UXManager.Extensions.Block.Login.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;
using static Vanjaro.UXManager.Extensions.Block.Login.Managers;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.Login
{
    public class Login : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);

        public string Name => "Login";

        public string Icon => "fas fa-user-circle";

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
            LoginManager.OAuthUserLogin();

            PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ThemeTemplateResponse response = new ThemeTemplateResponse
            {
                Markup = GenerateMarkup(Attributes),
                Scripts = new string[3] { "~/DesktopModules/Vanjaro/Common/Frameworks/ValidationJS/1.14.0/validate.min.js", ResourcesFolderPath + "Scripts/Login.js", "~/DesktopModules/Vanjaro/Common/Frameworks/ValidationJS/1.14.0/mnValidationService.min.js" },
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
                return Core.Managers.BlockManager.GetTemplateDir(ps, "login");
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
                    "Bootstrap"};

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

                Dictionary<string, string> BaseAttributes = Core.Managers.BlockManager.GetGlobalConfigs(ps, "login");
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

                Entities.Login login = new Entities.Login
                {
                    ButtonAlign = Attributes["data-block-buttonalign"],
                    ShowLabel = Convert.ToBoolean(Attributes["data-block-showlabel"]),
                    ShowResetPassword = Convert.ToBoolean(Attributes["data-block-showresetpassword"]),
                    ShowRememberPassword = Convert.ToBoolean(Attributes["data-block-showrememberpassword"]),
                    ResetPassword = Convert.ToBoolean(Attributes["data-block-resetpassword"]),
                    ShowRegister = Convert.ToBoolean(Attributes["data-block-showregister"]),
                    CaptchaEnabled = Captcha.IsEnabled()
                };

                login.RegisterUrl = Globals.RegisterURL(HttpUtility.UrlEncode(ServiceProvider.NavigationManager.NavigateURL()), Null.NullString);
                
                IDictionary<string, object> Objects = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                Objects.Add("Login", login);
                Objects.Add("UseEmailAsUserName", (PortalController.Instance.GetCurrentSettings() as PortalSettings).Registration.UseEmailAsUserName);
                Objects.Add("OAuthClients", Core.Managers.LoginManager.GetOAuthClients().Where(c => c.Enabled));

                string Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], Objects);
                Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                Captcha.Request();

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
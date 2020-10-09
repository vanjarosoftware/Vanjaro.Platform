using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.Logo.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.UXManager.Extensions.Block.Logo
{
    public class Logo : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string Name => "Logo";

        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fa fa-info-circle";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);
        public bool Visible { get; set; } = true;

        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                //result.Add("data-block-type", Name);
                //result.Add("data-block-guid", Guid.ToString().ToLower());
                //result.Add("data-block-setting-guid", "FAB10F7F-F1A9-46E4-A9E1-BAF7DEED1D8A".ToLower());                
                //result.Add("data-block-allow-customization", "false");
                //result.Add("data-block-width", "900");
                //result.Add("data-gjs-resizable", "true");
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

        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap" };

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "logo");
            }
        }

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }

        private string GenerateMarkup(Dictionary<string, string> Attributes)
        {
            try
            {
                PortalInfo portal = PortalController.Instance.GetPortal(PortalSettings.Current.PortalId, PortalSettings.Current.CultureCode);
                IFileInfo logoFile = string.IsNullOrEmpty(portal.LogoFile) ? null : FileManager.Instance.GetFile(PortalSettings.Current.PortalId, portal.LogoFile);
                Entities.Logo logo = new Entities.Logo
                {
                    NavigateURL = ServiceProvider.NavigationManager.NavigateURL("")
                };
                if (logoFile != null && logoFile.FileId > 0)
                {
                    logo.Path = FileManager.Instance.GetUrl(FileManager.Instance.GetFile(logoFile.FileId));
                }

                logo.NavigateURL = ServiceProvider.NavigationManager.NavigateURL(PortalSettings.Current.HomeTabId);

                IDictionary<string, object> dynObjects = new ExpandoObject() as IDictionary<string, object>;
                dynObjects.Add("Logo", logo);
                string uIPath = HttpContext.Current.Server.MapPath(UIPath);
                string Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], dynObjects);
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
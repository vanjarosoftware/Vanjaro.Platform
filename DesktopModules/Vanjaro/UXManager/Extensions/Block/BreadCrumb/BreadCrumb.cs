using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Manager;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.BreadCrumb.Components;

namespace Vanjaro.UXManager.Extensions.Block.Custom
{
    public class BreadCrumb : IBlock
    {
        public string Category => "BreadCrumb";

        public string Name => "Breadcrumb";

        public string DisplayName => DotNetNuke.Services.Localization.Localization.GetString("DisplayName", Constants.LocalResourcesFile);
        public string Icon => "fas fa-angle-double-right";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);

        public string LocalResourcesFile => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/App_LocalResources/Shared.resx";

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
                Markup = GenerateMarkup(Attributes)
            };
            return response;
        }

        public string ResouceFilePath
        {
            get
            {

                return Globals.ApplicationMapPath + @"\Portals\_default\" + Core.Managers.BlockManager.GetTheme() + "App_LocalResources\\Shared.resx";
            }
        }

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "breadcrumb");
            }
        }

        private string GenerateMarkup(Dictionary<string, string> Attributes)
        {
            try
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                IDictionary<string, object> Objects = new ExpandoObject() as IDictionary<string, object>;
                if (ps != null && ps.ActiveTab != null && ps.ActiveTab.BreadCrumbs == null)
                {
                    PortalSettingsController controller = new PortalSettingsController();
                    controller.ConfigureActiveTab(ps);
                }
                if (ps != null && ps.ActiveTab != null && ps.ActiveTab.BreadCrumbs != null && ps.ActiveTab.BreadCrumbs.Count > 0)
                {
                    Objects.Add("ActiveTab", ps.ActiveTab);
                    Objects.Add("Request", HttpContext.Current.Request);
                    string Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], Objects);
                    Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                    return Template;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return ex.Message;
            }
        }
    }
}
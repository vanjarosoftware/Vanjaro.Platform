using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Entities.Theme;
using Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Factories;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class EditController : UIEngineController
    {
        internal static List<IUIData> GetData(Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();

            string CatGuid = string.Empty;
            string Guid = string.Empty;
            string Category = string.Empty;
            string Type = string.Empty;
            if (parameters.Count > 0)
            {
                if (parameters.ContainsKey("catguid"))
                {
                    CatGuid = parameters["catguid"];
                }

                if (parameters.ContainsKey("guid"))
                {
                    Guid = parameters["guid"];
                }

                if (parameters.ContainsKey("cat"))
                {
                    Category = parameters["cat"];
                }

                if (parameters.ContainsKey("type"))
                {
                    Type = parameters["type"];
                }
            }

            ThemeEditor te = Core.Managers.ThemeManager.GetThemeEditor(CatGuid, Guid);
            Settings.Add("ThemeEditor", new UIData { Name = "ThemeEditor", Options = te ?? new ThemeEditor() });
            Settings.Add("ControlTypes", new UIData { Name = "ControlTypes", Options = Core.Managers.ThemeManager.GetControlTypes() });
            Settings.Add("IsNew", new UIData { Name = "IsNew", Value = string.IsNullOrEmpty(Guid) ? "true" : "false" });
            Settings.Add("Fonts", new UIData { Name = "Fonts", OptionsText = "Name", OptionsValue = "Value", Options = Core.Managers.ThemeManager.GetDDLFonts(CatGuid), Value = "0" });
            Settings.Add("Category", new UIData { Name = "Category", Value = Category });
            Settings.Add("Type", new UIData { Name = "Type", Value = Type });

            bool DeveloperMode = true;
            ThemeEditorWrapper themeEditorWrapper = Core.Managers.ThemeManager.GetThemeEditors(PortalSettings.Current.PortalId, CatGuid);
            if (themeEditorWrapper != null)
            {
                DeveloperMode = themeEditorWrapper.DeveloperMode;
            }

            Settings.Add("DeveloperMode", new UIData { Name = "DeveloperMode", Value = DeveloperMode.ToString().ToLower() });
            Settings.Add("CatGuid", new UIData { Name = "CatGuid", Value = CatGuid });
            return Settings.Values.ToList();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Update(string Catguid, ThemeEditor themeEditor)
        {
            Core.Managers.ThemeManager.BuildThemeEditor(themeEditor);
            if (Core.Managers.ThemeManager.Update(Catguid, themeEditor))
            {
                return Core.Managers.ThemeManager.GetMarkUp(AppFactory.Identifier.setting_settings.ToString(), Catguid);
            }
            else
            {
                return "Failed";
            }
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
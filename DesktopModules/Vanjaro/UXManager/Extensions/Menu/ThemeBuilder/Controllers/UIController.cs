using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Factories;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case AppFactory.Identifier.setting_settings:
                    return SettingsController.GetAllData(Identifier, Parameters);
                case AppFactory.Identifier.setting_manage:
                    return SettingsController.GetAllData(Identifier, Parameters);
                case AppFactory.Identifier.setting_edit:
                    return EditController.GetData(Parameters);
                case AppFactory.Identifier.setting_categories:
                    return CategoriesController.GetData(PortalSettings, Parameters);
                default:
                    break;
            }
            return base.GetData(Identifier, Parameters);
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }

        public override string AllowedAccessRoles(string Identifier)
        {
            return AppFactory.GetAllowedRoles(Identifier);
        }
    }
}
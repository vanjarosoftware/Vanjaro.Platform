using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Languages.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case AppFactory.Identifier.setting_languages:
                    return LanguagesController.GetData(PortalSettings, UserInfo);
                case AppFactory.Identifier.setting_add:
                    return AddController.GetData(PortalSettings.PortalId);
                case AppFactory.Identifier.setting_translator:
                    return TranslatorController.GetData(PortalSettings, UserInfo, Parameters);
                case AppFactory.Identifier.setting_resources:
                    return ResourcesController.GetData(PortalSettings, Parameters, UserInfo);
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
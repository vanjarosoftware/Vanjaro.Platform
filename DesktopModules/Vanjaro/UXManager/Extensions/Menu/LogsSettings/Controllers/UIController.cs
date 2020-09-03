using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Apps.LogsSettings.Factories;

namespace Vanjaro.UXManager.Extensions.Apps.LogsSettings.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case Factories.AppFactory.Identifier.setting_logSetting:
                    return LogSettingController.GetData(Identifier, Parameters, UserInfo, PortalSettings);
                case Factories.AppFactory.Identifier.setting_editlogsetting:
                    return LogSettingController.GetData(Identifier, Parameters, UserInfo, PortalSettings);
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
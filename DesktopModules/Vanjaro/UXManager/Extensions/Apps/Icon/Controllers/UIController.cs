using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Apps.Block.Icon.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((Factories.AppFactory.Identifier)Enum.Parse(typeof(Factories.AppFactory.Identifier), Identifier))
            {
                case Factories.AppFactory.Identifier.settings_setting:
                    return IconController.GetData(PortalSettings.PortalId, UserInfo);
                default:
                    break;
            }
            return base.GetData(Identifier, Parameters);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }

        public override string AllowedAccessRoles(string Identifier)
        {
            return Factories.AppFactory.GetAllowedRoles(Identifier);
        }
    }
}
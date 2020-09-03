using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Apps.Image.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((Factories.AppFactory.Identifier)Enum.Parse(typeof(Factories.AppFactory.Identifier), Identifier))
            {
                case Factories.AppFactory.Identifier.settings_image:
                    return ImageController.GetData(PortalSettings.PortalId, Parameters, Identifier);
                case Factories.AppFactory.Identifier.settings_imageonline:
                    return ImageController.GetData(PortalSettings.PortalId, Parameters, Identifier);
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
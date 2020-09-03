using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Pages.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Pages.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case Factories.AppFactory.Identifier.setting_pages:
                    return PagesController.GetData(Identifier, Parameters,  UserInfo);
                case Factories.AppFactory.Identifier.setting_detail:
                    return PagesController.GetData(Identifier, Parameters,  UserInfo);
                case Factories.AppFactory.Identifier.setting_permissions:
                    return PagesController.GetData(Identifier, Parameters,  UserInfo);
                case Factories.AppFactory.Identifier.setting_recyclebin:
                    return PagesController.GetData(Identifier, Parameters,  UserInfo);
                case Factories.AppFactory.Identifier.setting_savetemplateas:
                    return PagesController.GetData(Identifier, Parameters,  UserInfo);
                case Factories.AppFactory.Identifier.setting_choosetemplate:
                    return PagesController.GetData(Identifier, Parameters,  UserInfo);
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
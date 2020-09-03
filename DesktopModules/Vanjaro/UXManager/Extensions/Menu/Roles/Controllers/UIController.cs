using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Roles.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Roles.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case AppFactory.Identifier.setting_roles:
                    return RoleController.GetData(UserInfo, Identifier, Parameters);
                case AppFactory.Identifier.setting_add:
                    return RoleController.GetData(UserInfo, Identifier, Parameters);
                case AppFactory.Identifier.setting_addgroup:
                    return RoleGroupController.GetData(UserInfo, Identifier, Parameters);
                case AppFactory.Identifier.setting_adduser:
                    return RoleController.GetData(UserInfo, Identifier, Parameters);
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
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Users.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case AppFactory.Identifier.setting_setting:
                    return UserController.GetData(Identifier, UIEngineInfo, UserInfo, Parameters);
                case AppFactory.Identifier.setting_users:
                    return UserController.GetData(Identifier, UIEngineInfo, UserInfo, Parameters);
                case AppFactory.Identifier.setting_adduser:
                    return UserController.GetData(Identifier, UIEngineInfo, UserInfo, Parameters);
                case AppFactory.Identifier.setting_changepassword:
                    return UserController.GetData(Identifier, UIEngineInfo, UserInfo, Parameters);
                case AppFactory.Identifier.setting_updateprofile:
                    return UpdateProfileController.GetData(Identifier, UIEngineInfo, UserInfo, Parameters);
                case AppFactory.Identifier.setting_import:
                    return ImportController.GetData(Identifier, UIEngineInfo, UserInfo, Parameters);
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
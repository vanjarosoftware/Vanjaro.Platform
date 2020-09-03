using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.MemberProfile.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.MemberProfile.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case AppFactory.Identifier.memberprofile_settings:
                    return MemberProfileController.GetData(Identifier, Parameters, PortalSettings, UserInfo);
                case AppFactory.Identifier.memberprofile_memberprofile:
                    return MemberProfileController.GetData(Identifier, Parameters, PortalSettings, UserInfo);
                case AppFactory.Identifier.memberprofile_memberprofilesettings:
                    return MemberProfileController.GetData(Identifier, Parameters, PortalSettings, UserInfo);
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
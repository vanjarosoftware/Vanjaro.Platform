using System;
using System.Collections.Generic;
using System.Web;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Apps.Video.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((Factories.AppFactory.Identifier)Enum.Parse(typeof(Factories.AppFactory.Identifier), Identifier))
            {
                case Factories.AppFactory.Identifier.settings_video:
                    return VideoController.GetData(PortalSettings.PortalId, Parameters, Identifier, IsSupportBackground(HttpContext.Current));
                case Factories.AppFactory.Identifier.settings_videoonline:
                    return VideoController.GetData(PortalSettings.PortalId, Parameters, Identifier, IsSupportBackground(HttpContext.Current));
                default:
                    break;
            }
            return base.GetData(Identifier, Parameters);
        }

        private static bool IsSupportBackground(HttpContext Context)
        {
            if (HttpContext.Current.Request.UrlReferrer.ToString().Contains("issupportbackground"))
            {
                return false;
            }
            else
            {
                return true;
            }
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
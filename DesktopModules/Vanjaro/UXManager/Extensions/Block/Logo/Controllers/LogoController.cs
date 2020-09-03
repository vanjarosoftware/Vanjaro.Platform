using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Block.Logo.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class LogoController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
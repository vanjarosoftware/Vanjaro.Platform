using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;


namespace Vanjaro.UXManager.Extensions.Menu.Help.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class HelpController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
    }
}
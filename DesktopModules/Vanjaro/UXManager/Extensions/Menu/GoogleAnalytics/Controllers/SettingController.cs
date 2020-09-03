using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Settings", new UIData { Name = "Settings", Options = Managers.ConnectorManager.GetConfig() } },
                { "HasConfig", new UIData { Name = "HasConfig", Options = Managers.ConnectorManager.HasConfig() } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Save(Dictionary<string, dynamic> Data)
        {
            return Managers.ConnectorManager.SaveConfig(Data);
        }

        [HttpGet]
        public dynamic Delete()
        {
            if (Managers.ConnectorManager.HasConfig())
            {
                Managers.ConnectorManager.DeleteConfig();
            }

            return Managers.ConnectorManager.GetConfig();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
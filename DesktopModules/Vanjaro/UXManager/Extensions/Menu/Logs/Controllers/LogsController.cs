using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Extensions.Menu.Logs.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class LogsController : UIEngineController
    {
        internal static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo, UserInfo UserInfo, PortalSettings PortalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "GetLogTypes", new UIData { Name = "GetLogTypes", Options = Managers.LogsManager.GetLogTypes(PortalSettings.PortalId, UserInfo), OptionsText = "LogTypeFriendlyName", OptionsValue = "LogTypeKey", Value = "*" } }
            };
            string LogSettingsUrl = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=86710658-7b26-4cf2-84b1-d0797d939aa4";
            Settings.Add("LogSettingsUrl", new UIData { Name = "LogSettingsUrl", Value = LogSettingsUrl });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public dynamic GetLogItems(string logType, int pageSize, int pageIndex)
        {
            return Managers.LogsManager.GetLogItems(PortalSettings.ActiveTab.PortalID, logType, pageSize, pageIndex, UserInfo);
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "host")]
        public string ClearLog()
        {
            return Managers.LogsManager.ClearLog();
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "host")]
        public dynamic DeleteLogItems([FromBody]List<string> LogGuids)
        {
            return Managers.LogsManager.DeleteLogItems(LogGuids);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
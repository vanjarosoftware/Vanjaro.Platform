using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.SQLConsole.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class SQLQueryController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            List<string> Connections = Managers.SQLConsoleManager.GetConnections();
            string ConnectionValue = string.Empty;
            if (Connections.Count > 0)
            {
                ConnectionValue = Connections.FirstOrDefault();
            }

            Settings.Add("Connections", new UIData { Name = "Connections", Options = Connections, Value = ConnectionValue });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Run(string sqlConnection, dynamic query)
        {
            return Managers.SQLConsoleManager.RunQuery(sqlConnection, query);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
using Dnn.PersonaBar.AdminLogs.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Menu.Logs.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class EmailController : UIEngineController
    {
        internal static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo, UserInfo UserInfo, PortalSettings PortalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }

        [HttpPost]
        public dynamic EmailLogItems(EmailLogItemsRequest EmailLog)
        {
            return Managers.LogsManager.EmailLogItems(EmailLog, UserInfo, PortalSettings);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
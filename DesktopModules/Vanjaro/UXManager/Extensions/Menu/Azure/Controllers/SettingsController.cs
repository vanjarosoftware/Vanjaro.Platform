using DotNetNuke.Services.Connections;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Azure.Components;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Azure.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingsController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Connectors", new UIData { Name = "Connectors", Options = Manager.ConnectorsManager.GetAll(portalId) } }
            };
            return Settings.Values.ToList();
        }

        [HttpGet]
        public ActionResult GetAllConnector()
        {
            ActionResult actionResult = new ActionResult
            {
                IsSuccess = true,
                Data = Manager.ConnectorsManager.GetAll(PortalSettings.PortalId)
            };
            return actionResult;
        }

        [HttpGet]
        public ActionResult DeleteConnection(string Id)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                AzureConnector AzureConnector = new AzureConnector();
                IEnumerable<IConnector> connectors = AzureConnector.GetConnectors(PortalSettings.PortalId);

                IConnector connector = connectors.FirstOrDefault(c => c.Id == Id);
                if (connector != null)
                {
                    connector.DeleteConnector(PortalSettings.PortalId);
                    actionResult.IsSuccess = true;
                    actionResult.Data = Manager.ConnectorsManager.GetAll(PortalSettings.PortalId);
                    return actionResult;
                }
                actionResult.AddError("ErrConnectorNotFound", Localization.GetString("ErrConnectorNotFound.Text", Constants.LocalResourceFile));
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("InternalServerError", "InternalServerError", ex);
                return actionResult;
            }
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
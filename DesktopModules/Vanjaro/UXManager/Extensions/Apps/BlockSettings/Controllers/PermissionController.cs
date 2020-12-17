using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Apps.BlockSettings.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class PermissionController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings portalSettings, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            int EntityID = 0; string Entity = string.Empty;
            try { EntityID = int.Parse(parameters["entityid"]); Entity = parameters["entity"]; } catch { }
            Settings.Add("Permissions", new UIData { Name = "Permissions", Options = Managers.CustomPermissionManager.GetPermissions(EntityID) });
            Settings.Add("EntityID", new UIData { Name = "EntityID", Value = EntityID.ToString() });
            Settings.Add("Entity", new UIData { Name = "Entity", Value = Entity });
            return Settings.Values.ToList();
        }
        [HttpPost]
        public ActionResult Update(int EntityID, string Entity, dynamic Data)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                if (Data.PermissionsInherit.Value)
                {
                    Managers.CustomPermissionManager.Delete(EntityID);
                    actionResult.Data = 0;
                }
                else
                {
                    if (EntityID == 0 && !string.IsNullOrEmpty(Entity))
                        EntityID = Managers.CustomPermissionManager.AddCustomPermissionEntity(Entity, Data.PermissionsInherit.Value);
                    else if (EntityID > 0)
                        Managers.CustomPermissionManager.UpdateInherit(EntityID, Data.PermissionsInherit.Value);
                    Managers.CustomPermissionManager.Update(EntityID, Data);
                    actionResult.Data = EntityID;
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("CustomPermissionSave_Error", ex.Message);
            }
            return actionResult;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
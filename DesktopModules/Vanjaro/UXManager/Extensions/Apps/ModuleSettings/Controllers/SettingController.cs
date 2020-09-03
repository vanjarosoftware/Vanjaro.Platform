using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Apps.ModuleSettings.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            int ModuleID = -1;
            try
            {
                ModuleID = int.Parse(Parameters["appid"]);
            }
            catch { }
            ModuleInfo moduleinfo = ModuleController.Instance.GetModule(ModuleID, PortalSettings.ActiveTab.TabID, false);
            if (moduleinfo != null)
            {
                Settings.Add("StartDate", new UIData { Name = "StartDate", Options = moduleinfo.StartDate != null ? moduleinfo.StartDate : Null.NullDate });
                Settings.Add("EndDate", new UIData { Name = "EndDate", Options = moduleinfo.EndDate != null ? moduleinfo.EndDate : Null.NullDate });
                Settings.Add("chkAllTabs", new UIData { Name = "chkAllTabs", Options = moduleinfo.AllTabs });
            }
            Settings.Add("ModuleID", new UIData { Name = "ModuleID", Options = ModuleID });
            Settings.Add("Permissions", new UIData { Name = "Permissions", Options = Managers.SettingManager.GetPermission(ModuleID, PortalSettings.PortalId) });
            Settings.Add("AppSettingUrl", new UIData { Name = "AppSettingUrl", Value = new ModuleInstanceContext().NavigateUrl(PortalSettings.Current.ActiveTab.TabID, "Module", false, "ModuleId=" + ModuleID) });
            return Settings.Values.ToList();
        }
        [HttpPost]
        public ActionResult Save(int ModuleID, dynamic Data)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                ModuleInfo moduleinfo = ModuleController.Instance.GetModule(ModuleID, PortalSettings.ActiveTab.TabID, false);
                if (moduleinfo != null)
                {
                    Managers.SettingManager.UpdateModule(moduleinfo, Data);
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("ModuleSettingSave_Error", ex.Message);
            }
            return actionResult;

        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
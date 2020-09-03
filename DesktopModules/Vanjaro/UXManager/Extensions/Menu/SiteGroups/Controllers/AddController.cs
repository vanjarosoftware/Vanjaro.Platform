using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.SiteGroups.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.SiteGroups.Controllers
{
    public class AddController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            int PortalGroupId = -1;
            try
            {
                PortalGroupId = int.Parse(Parameters["id"]);
            }
            catch { }

            Settings.Add("AvailablePortals", new UIData { Name = "AvailablePortals", Options = SiteGroupManager.GetAvailablePortals(), OptionsText= "PortalName", OptionsValue= "PortalID", Value= "-1"});
            Settings.Add("PortalGroupInfo", new UIData { Name = "PortalGroupInfo", Options = SiteGroupManager.GetSiteGroup(PortalGroupId) });

            return Settings.Values.ToList();
        }

        [HttpPost]
        public void Update(Components.PortalGroupInfo groupInfo)
        {
            SiteGroupManager.Save(groupInfo);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
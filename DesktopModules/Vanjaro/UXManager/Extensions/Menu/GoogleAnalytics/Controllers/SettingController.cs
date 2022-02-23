using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData()
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            Settings.Add("MeasurementID", new UIData { Name = "MeasurementID", Value = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleAnalytics.MeasurementID", true) });
            Settings.Add("TrackingID", new UIData { Name = "TrackingID", Value = SettingManager.GetPortalSetting("Vanjaro.Integration.GoogleAnalytics.TrackingID", true) });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public bool Save(dynamic Data)
        {
            SettingManager.UpdatePortalSetting("Vanjaro.Integration.GoogleAnalytics.TrackingID", Data.TrackingID.ToString(), true);
            SettingManager.UpdatePortalSetting("Vanjaro.Integration.GoogleAnalytics.MeasurementID", Data.MeasurementID.ToString(), true);
            return true;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
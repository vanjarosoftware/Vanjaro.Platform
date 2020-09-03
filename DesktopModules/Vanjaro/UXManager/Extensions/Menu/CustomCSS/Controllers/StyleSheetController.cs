using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.CustomCSS.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]
    public class StyleSheetController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "StyleSheet", new UIData { Name = "StyleSheet", Value = Managers.StyleSheetManager.LoadStyleSheet(PortalID) } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Save(dynamic Data)
        {
            return Managers.StyleSheetManager.Update(PortalSettings.PortalId, Data);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
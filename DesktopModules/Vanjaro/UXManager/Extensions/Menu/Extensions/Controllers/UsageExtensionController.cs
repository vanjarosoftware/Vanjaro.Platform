using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]
    public class UsageExtensionController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, UserInfo UserInfo, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            int PackageID = 0;
            try
            {
                PackageID = int.Parse(Parameters["pid"]);
            }
            catch { }
            Settings.Add("Portals", new UIData { Name = "Portals", Options = Managers.ExtensionsManager.GetPortals(PortalID, UserInfo), OptionsText = "Value", OptionsValue = "Key", Value = PortalID.ToString() });
            Settings.Add("Pages", new UIData { Name = "Pages", Options = Managers.ExtensionsManager.GetPages(PortalID, PackageID, UserInfo) });
            Settings.Add("PackageID", new UIData { Name = "PackageID", Options = PackageID });
            return Settings.Values.ToList();
        }
        [HttpGet]
        public ActionResult GetPages(int PortalID, int PackageID)
        {
            ActionResult actionResult = new ActionResult
            {
                Data = Managers.ExtensionsManager.GetPages(PortalID, PackageID, UserInfo),
                IsSuccess = true
            };
            return actionResult;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
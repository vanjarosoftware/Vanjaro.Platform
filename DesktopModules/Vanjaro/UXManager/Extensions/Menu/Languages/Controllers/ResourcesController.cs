using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Languages.Entities;
using Vanjaro.UXManager.Extensions.Menu.Languages.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class ResourcesController : UIEngineController
    {

        internal static List<IUIData> GetData(PortalSettings PortalSettings, Dictionary<string, string> Parameters, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>(); int lid = -1;
            try
            {
                lid = int.Parse(Parameters["lid"]);
            }
            catch { }
            Settings.Add("LanguageID", new UIData { Name = "LanguageID", Options = lid });
            Settings.Add("PortalName", new UIData { Name = "PortalName", Options = PortalSettings.PortalName });
            Settings.Add("ResourcesFolders", new UIData { Name = "ResourcesFolders", Options = ResourcesManager.GetRootResourcesFolders() });
            Settings.Add("UpdateTransaltionsRequest", new UIData { Name = "UpdateTransaltionsRequest", Options = new UpdateTransaltionsRequest() });
            Settings.Add("IsSuperUser", new UIData { Name = "IsSuperUser", Options = UserInfo.IsSuperUser });

            return Settings.Values.ToList();
        }


        [HttpGet]
        public ActionResult GetSubRootResources(string currentFolder = null)
        {
            return ResourcesManager.GetSubRootResources(currentFolder);
        }


        [HttpPost]
        public ActionResult GetResxEntries(string Mode, int lid, dynamic Data)
        {
            string resourceFile = Data.resourceFile.ToString();
            if (Data.resourceFile.ToString().IndexOf("_/", StringComparison.Ordinal) == 0)
            {
                resourceFile = resourceFile.Substring(2);
            }
            return ResourcesManager.GetResxEntries(PortalSettings.PortalId, UserInfo, Mode, lid, resourceFile);
        }

        [HttpPost]
        public ActionResult SaveResxEntries(int lid, UpdateTransaltionsRequest request)
        {
            return ResourcesManager.SaveResxEntries(PortalSettings.PortalId, UserInfo, lid, request);
        }


        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }

}
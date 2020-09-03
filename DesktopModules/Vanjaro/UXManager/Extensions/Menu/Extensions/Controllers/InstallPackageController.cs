using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;
using Newtonsoft.Json.Linq;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]
    public class InstallPackageController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "PackageList", new UIData { Name = "PackageList", Options = Managers.InstallPackageManager.ParsePackage(PortalSettings, UserInfo) } }
            };
            return Settings.Values.ToList();
        }

        [HttpGet]
        public ActionResult Install()
        {
            ActionResult actionResult = new ActionResult();
            Managers.InstallPackageManager.InstallPackage(PortalSettings, UserInfo);
            return actionResult;
        }

        [HttpGet]
        public ActionResult Delete()
        {
            ActionResult actionResult = new ActionResult();
            Managers.InstallPackageManager.DeletePackage(PortalSettings, UserInfo);
            return actionResult;
        }

        [HttpGet]
        public ActionResult Download(string Data)
        {
            ActionResult actionResult = new ActionResult();
            JObject json = JObject.Parse(Data);
            foreach (string item in json["Uri"])
            {
                WebClient webClient = new WebClient();

                try
                {
                    webClient.DownloadFile(item, Globals.ApplicationMapPath + "\\Install\\Module\\" + Path.GetFileName(item));

                }
                catch (Exception ex)
                {
                    actionResult.HasErrors = true;
                    actionResult.Data = ex.Message;
                    return actionResult;
                }
            }
            actionResult.IsSuccess = true;
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
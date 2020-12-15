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
using System.Web;
using Vanjaro.Core.Components;
using Newtonsoft.Json;
using Dnn.PersonaBar.Extensions.Components.Dto;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]
    public class InstallPackageController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, UserInfo UserInfo)
        {
            List<ParseResultDto> ParseResults = Managers.InstallPackageManager.ParsePackage(PortalSettings, UserInfo, HttpContext.Current.Server.MapPath("\\DesktopModules\\Vanjaro\\Temp\\Install\\"));
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "PackageList", new UIData { Name = "PackageList", Options =ParseResults  } },
                { "PackageErrorList", new UIData { Name = "PackageErrorList", Options = ParseResults.Where(p=>p.Success==false).ToList() } }
            };
            return Settings.Values.ToList();
        }

        [HttpGet]
        public ActionResult Install()
        {
            ActionResult actionResult = new ActionResult();
            Managers.InstallPackageManager.InstallPackage(PortalSettings, UserInfo, HttpContext.Current.Server.MapPath("\\DesktopModules\\Vanjaro\\Temp\\Install\\"));
            return actionResult;
        }
        
        [HttpPost]
        public ActionResult Download()
        {
            ActionResult actionResult = new ActionResult();
            string installPackagePath = HttpContext.Current.Server.MapPath("\\DesktopModules\\Vanjaro\\Temp\\Install\\");
            if (Directory.Exists(installPackagePath))
                Directory.Delete(installPackagePath, true);
            if (!Directory.Exists(installPackagePath))
                Directory.CreateDirectory(installPackagePath);

            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Form != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["Packages"]))
            {
                List<StringValue> packages = JsonConvert.DeserializeObject<List<StringValue>>(HttpContext.Current.Request.Form["Packages"]);
                foreach (StringValue item in packages)
                {
                    WebClient webClient = new WebClient();

                    try
                    {
                        webClient.DownloadFile(item.Value, installPackagePath + Path.GetFileName(item.Text + ".zip"));
                    }
                    catch (Exception ex)
                    {
                        actionResult.HasErrors = true;
                        actionResult.Data = ex.Message;
                        return actionResult;
                    }
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
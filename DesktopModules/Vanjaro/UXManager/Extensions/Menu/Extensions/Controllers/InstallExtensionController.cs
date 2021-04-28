using Dnn.PersonaBar.Extensions.Components;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class InstallExtensionController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, UserInfo UserInfo, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Folders", new UIData { Name = "Folders", Options = null, Value = "0" } },
                { "Files", new UIData { Name = "Files", Options = null } },
                { "AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = "zip" } },
                { "MaxFileSize", new UIData { Name = "MaxFileSize", Value = Config.GetMaxUploadSize().ToString() } }
            };
            string FileName = string.Empty;
            string Type = string.Empty;
            if (Parameters.Keys.Contains("name"))
            {
                FileName = Parameters["name"];
            }

            if (Parameters.Keys.Contains("type"))
            {
                Type = Parameters["type"];
            }

            if (!string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(Type))
            {
                Settings.Add("ParsePackageFile", new UIData { Name = "ParsePackageFile", Options = Managers.ExtensionsManager.ParsePackageFile(PortalSettings, UserInfo, Type, FileName) });
                Settings.Add("FileName", new UIData { Name = "FileName", Value = FileName });
                Settings.Add("Type", new UIData { Name = "Type", Value = Type });
            }
            return Settings.Values.ToList();
        }

        [HttpGet]
        public ActionResult InstallAvailablePackage(string FileName, string Type)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                string installFolder = Managers.ExtensionsManager.GetPackageInstallFolder(Type);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(FileName))
                {
                    actionResult.AddError("InvalidPackage", "InvalidPackage");
                    return actionResult;
                }

                string packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, FileName);
                if (!File.Exists(packagePath))
                {
                    actionResult.AddError("NotFound", "NotFound");
                    return actionResult;
                }

                using (FileStream stream = new FileStream(packagePath, FileMode.Open))
                {
                    InstallResultDto InstallResultDto = InstallController.Instance.InstallPackage(PortalSettings, UserInfo, null, packagePath, stream);
                    actionResult.Data = InstallResultDto;
                    actionResult.IsSuccess = true;
                    return actionResult;
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }

        [HttpPost]
        public ActionResult ParsePackage()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                dynamic files = HttpContext.Current.Request.Files;
                HttpPostedFile file = files[0];
                ParseResultDto ParseResultDto = InstallController.Instance.ParsePackage(PortalSettings, UserInfo, file.FileName, file.InputStream);
                actionResult.Data = ParseResultDto;
                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("internalError", ex.Message, ex);
                return actionResult;
            }
        }

        [HttpPost]
        public ActionResult InstallPackage()
        {
            string legacySkin = null;
            bool isPortalPackage = false;
            ActionResult actionResult = new ActionResult();
            dynamic files = HttpContext.Current.Request.Files;
            HttpPostedFile file = files[0];
            InstallResultDto InstallResultDto = InstallController.Instance.InstallPackage(PortalSettings, UserInfo, legacySkin, file.FileName, file.InputStream, isPortalPackage);
            actionResult.Data = InstallResultDto;
            if (InstallResultDto != null)
            {
                PackageInfo packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.PackageID == InstallResultDto.NewPackageId);
                if (packageInfo != null && packageInfo.PackageType.ToLower() == "module")
                {
                    List<int> pids = new List<int>();
                    foreach (PortalInfo pi in PortalController.Instance.GetPortals())
                        pids.Add(pi.PortalID);

                    string ThemeCssFolder = HttpContext.Current.Server.MapPath("~/Portals");
                    Parallel.ForEach(pids,
                    new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.50) * 1.0)) },
                    pid =>
                    {
                        string ThemeCss = ThemeCssFolder + "/" + pid + "/vThemes/" + ThemeManager.GetCurrent(pid).Name + "/Theme.css";
                        if (File.Exists(ThemeCss))
                        {
                            File.Copy(ThemeCss, ThemeCss.Replace("Theme.css", "Theme.backup.css"), true);
                            File.Delete(ThemeCss.Replace("Theme.backup.css", "Theme.css"));
                        }
                    });
                }
            }
            DataCache.ClearCache();
            actionResult.IsSuccess = true;
            return actionResult;
        }


        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
using Dnn.PersonaBar.Extensions.Components;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class DashboardController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Extensions", new UIData { Name = "Extensions", Options = Managers.ExtensionsManager.GetAllExtensions(UserInfo, PortalID, true) } }
            };
            return Settings.Values.ToList();
        }

        [AuthorizeAccessRoles(AccessRoles = "host")]
        [HttpGet]
        public ActionResult Extensions(bool IsInstall)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                actionResult.IsSuccess = true;
                actionResult.Data = Managers.ExtensionsManager.GetAllExtensions(UserInfo, PortalSettings.PortalId, IsInstall);
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
            }
            return actionResult;
        }

        [AuthorizeAccessRoles(AccessRoles = "host")]
        [HttpGet]
        public HttpResponseMessage DownloadPackage(string FileName, string Type)
        {
            HttpResponseMessage Response = new HttpResponseMessage();
            try
            {
                string installFolder = Managers.ExtensionsManager.GetPackageInstallFolder(Type);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(FileName))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                string packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, FileName);
                if (!File.Exists(packagePath))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                if (FileName.EndsWith(".resources"))
                {
                    FileName = FileName.Replace(".resources", ".zip");
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                FileStream stream = new FileStream(packagePath, FileMode.Open);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentLength = stream.Length;
                response.Content.Headers.Add("x-filename", FileName);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                return response;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }

        [AuthorizeAccessRoles(AccessRoles = "host")]
        [HttpGet]
        public HttpResponseMessage DownloadLanguagePackage(string cultureCode)
        {
            try
            {
                const string packageFileName = "installlanguage.resources";
                string packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);

                ParseResultDto parsePackage = new ParseResultDto();

                using (FileStream stream = new FileStream(packagePath, FileMode.Open))
                {
                    parsePackage = InstallController.Instance.ParsePackage(PortalSettings, UserInfo, packagePath, stream);
                }

                bool invalidPackage = !parsePackage.Success
                                        || !parsePackage.PackageType.Equals("CoreLanguagePack")
                                        || !parsePackage.Name.EndsWith(cultureCode, StringComparison.InvariantCultureIgnoreCase);

                if (invalidPackage)
                {
                    DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                }

                return DownLoadFile(packagePath);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        private HttpResponseMessage DownLoadFile(string packagePath)
        {
            if (!File.Exists(packagePath))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "FileNotFound");
            }

            Stream stream = FileWrapper.Instance.OpenRead(packagePath);
            string fileName = Path.GetFileNameWithoutExtension(packagePath) + ".zip";

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            result.Content.Headers.Add("x-filename", fileName);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return result;
        }
        [AuthorizeAccessRoles(AccessRoles = "host")]
        [HttpGet]
        public ActionResult ParseLanguagePackage(string cultureCode)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                const string packageFileName = "installlanguage.resources";
                string packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);
                using (FileStream stream = new FileStream(packagePath, FileMode.Open))
                {
                    actionResult.Data = InstallController.Instance.ParsePackage(PortalSettings, UserInfo, packagePath, stream);
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

        [AuthorizeAccessRoles(AccessRoles = "host")]
        [HttpGet]
        public ActionResult GetAvailablePackages()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                ExtensionsController ExtensionsController = new ExtensionsController();
                actionResult.Data = ExtensionsController.GetAvailablePackages("CoreLanguagePack");
                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
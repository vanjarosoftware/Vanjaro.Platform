using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Editors;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class DeleteExtensionController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            int pid = 0;
            try
            {
                pid = int.Parse(Parameters["pid"]);
            }
            catch { }

            PackageSettingsDto packageSettings = new PackageSettingsDto
            {
                PortalId = PortalID
            };
            DeletePackageDto deletePackage = new DeletePackageDto();
            PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == pid);
            if (package != null)
            {
                Dnn.PersonaBar.Extensions.Components.Editors.IPackageEditor packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
                PackageInfoDto packageDetail = packageEditor?.GetPackageDetail(PortalID, package) ?? new PackageInfoDto(PortalID, package);
                Settings.Add("packageDetail", new UIData { Name = "packageDetail", Options = packageDetail });
                packageSettings.PackageId = packageDetail.PackageId;
                deletePackage.Id = packageDetail.PackageId;
            }
            Settings.Add("packageSettings", new UIData { Name = "packageSettings", Options = packageSettings });
            Settings.Add("deletePackage", new UIData { Name = "deletePackage", Options = deletePackage });
            return Settings.Values.ToList();
        }
        [HttpPost]
        public ActionResult DeletePackage(DeletePackageDto deletePackage)
        {
            return Managers.ExtensionsManager.DeletePackage(deletePackage);
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
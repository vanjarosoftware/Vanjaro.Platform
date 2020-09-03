using Dnn.PersonaBar.Extensions.Components;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System.IO;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Apps.About.Managers
{
    public class AboutManager
    {
        internal static string GetVersion(UserInfo UserInfo, int PortalId)
        {
            string LocalResourceFile = Path.Combine("~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + AboutInfo.Name + "/Views/App_LocalResources/Shared.resx");
            ExtensionsController ExtensionsController = new ExtensionsController();
            Dnn.PersonaBar.Extensions.Components.Dto.PackageInfoSlimDto Vanjaro = ExtensionsController.GetInstalledPackages((UserInfo.IsSuperUser ? -1 : PortalId), "Library").Where(x => x.Name.ToLower() == "vanjaro.core").FirstOrDefault();
            if (Vanjaro != null)
            {
                return Vanjaro.Version;
            }
            else
            {
                return Localization.GetString("VersionNotAvailable", LocalResourceFile);
            }
        }


    }
}
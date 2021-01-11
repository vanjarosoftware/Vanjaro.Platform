using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using System;
using System.IO;
using System.Web;
using Vanjaro.Common.Data.Scripts;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Services;

namespace Vanjaro.Core.Components
{
    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string Version)
        {
            Managers.AnalyticsManager.Update(Version);
            if (Version == "01.00.00")
            {
                PlatformCleanup();
                MoveFilesInRoot();
                Managers.SettingManager.ApplyingSettings(true);
                Services.Search.SearchEngineScheduler.Install();
                PageWorkflowPermission.InitTabPermissions();
                Managers.SettingManager.UpdateSettingWebConfig();
            }
            else
            {
                Managers.SettingManager.ApplyingSettings(Version);
            }

            return "Success";
        }

        private void PlatformCleanup()
        {
            if (Managers.SettingManager.IsDistribution(0))
            {
                using (VanjaroRepo vrepo = new VanjaroRepo())
                {
                    //--Platform Clean Up
                    vrepo.Execute("DELETE FROM " + CommonScript.DnnTablePrefix + "[Packages] WHERE PackageType IN('SkinObject', 'Skin', 'Container') OR (PackageType = 'Module' AND Name LIKE 'DotNetNuke%' AND Name != 'DotNetNuke.Authentication') OR (PackageType = 'Auth_System' AND Name = 'DefaultAuthentication')");
                    vrepo.Execute("UPDATE " + CommonScript.DnnTablePrefix + "[Packages] SET IsSystemPackage = 1 WHERE Name IN ('Dnn.PersonaBar.UI','Dnn.PersonaBar.Extensions')");
                }
            }
        }

        private void MoveFilesInRoot()
        {
            try
            {
                #region Copy Font Awesome
                string CoreFontAwesomePath = HttpContext.Current.Request.PhysicalApplicationPath + "DesktopModules\\Vanjaro\\Core\\Library\\Font Awesome";
                string PortalFonts = HttpContext.Current.Request.PhysicalApplicationPath + "\\Icons\\Font Awesome";
                Managers.SettingManager.Copy(CoreFontAwesomePath, PortalFonts);

                #endregion
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

        }

    }
}
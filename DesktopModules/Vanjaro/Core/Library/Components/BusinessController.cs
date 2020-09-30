using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using System;
using System.IO;
using System.Web;

namespace Vanjaro.Core.Components
{
    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string Version)
        {
            if (Version == "01.00.00")
            {
                MoveFilesInRoot();
                Managers.SettingManager.ApplyingSettings(true);
                Services.Search.SearchEngineScheduler.Install();
                Components.PageWorkflowPermission.InitTabPermissions();
            }
            else
            {
                Managers.SettingManager.ApplyingSettings(Version);
            }
            return "Success";
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
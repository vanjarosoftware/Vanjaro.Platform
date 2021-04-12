using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;

namespace Vanjaro.UXManager.Extensions.Menu.Security
{
    public static partial class Managers
    {
        public class SecurityManager
        {
            public const string LocalResourcesFile = "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx";
            #region Get SSL Settings
            internal static Entities.UpdateSslSettingsRequest GetSslSettings(PortalSettings portalSettings, UserInfo userInfo)
            {
                Entities.UpdateSslSettingsRequest UpdateSslSettingsRequest = new Entities.UpdateSslSettingsRequest()
                {
                    SSLEnabled = PortalController.GetPortalSettingAsBoolean("SSLEnabled", portalSettings.PortalId, false),
                    SSLEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", portalSettings.PortalId, false),
                    SSLURL = PortalController.GetPortalSetting("SSLURL", portalSettings.PortalId, Null.NullString),
                    STDURL = PortalController.GetPortalSetting("STDURL", portalSettings.PortalId, Null.NullString),
                    SSLOffloadHeader = userInfo.IsSuperUser ? HostController.Instance.GetString("SSLOffloadHeader", "") : Null.NullString
                };
                return UpdateSslSettingsRequest;
            }
            #endregion


            internal static string ValidateFileExtension(string Value)
            {
                #region Validate webp extension 
                if (!string.IsNullOrEmpty(Value) && !System.Text.RegularExpressions.Regex.IsMatch(Value, string.Format(@"\b{0}\b", System.Text.RegularExpressions.Regex.Escape("webp"))))
                {
                    Value += ",webp";
                    string[] extn = Value.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    Value = string.Join(",", extn);
                }
                return Value;
                #endregion
            }
        }
    }
}
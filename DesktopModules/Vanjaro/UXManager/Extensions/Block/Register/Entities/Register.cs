using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Extensions.Block.Register.Entities
{
    public class Register
    {
        private PortalSettings PortalSettings = null;
        public Register()
        {
            PortalSettings = (PortalController.Instance.GetCurrentSettings() as PortalSettings);
        }
        public bool ShowLabel { get; set; }
        public bool TermsPrivacy { get; set; }
        public string ButtonAlign { get; set; }
        public string LoginURL => Managers.LoginLinkManager.LoginURL("", false);
        public string TermsURL
        {
            get
            {
                if (PortalSettings.TermsTabId > 0)
                    return Globals.NavigateURL(PortalSettings.TermsTabId);
                else
                    return ServiceProvider.NavigationManager.NavigateURL("Terms");
            }
        }
        public string PrivacyURL
        {
            get
            {
                if (PortalSettings.PrivacyTabId > 0)
                    return Globals.NavigateURL(PortalSettings.PrivacyTabId);
                else
                    return ServiceProvider.NavigationManager.NavigateURL("Privacy");
            }
        }
        public int RegistrationMode
        {
            get
            {
                return PortalSettings.UserRegistration;
            }
        }
    }
}
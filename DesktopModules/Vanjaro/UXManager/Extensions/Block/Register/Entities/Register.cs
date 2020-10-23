using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Extensions.Block.Register.Entities
{
    public class Register
    {
        public bool ShowLabel { get; set; }
        public bool TermsPrivacy { get; set; }
        public string ButtonAlign { get; set; }
        public string LoginURL => Managers.LoginLinkManager.LoginURL("", false);
        public string TermsURL => ServiceProvider.NavigationManager.NavigateURL("Terms");
        public string PrivacyURL => ServiceProvider.NavigationManager.NavigateURL("Privacy");
        public int RegistrationMode => (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserRegistration;
    }
}
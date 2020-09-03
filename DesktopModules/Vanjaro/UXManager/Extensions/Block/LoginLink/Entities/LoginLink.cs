using DotNetNuke.Entities.Portals;

namespace Vanjaro.UXManager.Extensions.Block.LoginLink.Entities
{
    public class LoginLink
    {
        public string Url { get; set; }
        public bool IsAuthenticated { get; set; }
        public int RegistrationMode => (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserRegistration;
    }
}
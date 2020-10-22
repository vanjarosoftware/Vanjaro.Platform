using DotNetNuke.Entities.Portals;

namespace Vanjaro.UXManager.Extensions.Block.Login.Entities
{
    public class Login
    {
        public bool ShowResetPassword { get; set; }
        public bool ShowRememberPassword { get; set; }
        public bool ShowLabel { get; set; }
        public string ButtonAlign { get; set; }
        public bool ResetPassword { get; set; }
        public bool ShowRegister { get; set; }
        public string RegisterUrl { get; set; }
        public bool CaptchaEnabled { get; set; }

        public string LoginURL => Managers.LoginManager.LoginURL("", false);
        public int RegistrationMode => (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserRegistration;
        public DataConsent DataConsent => new DataConsent();
    }

    public class DataConsent
    {
        public bool DeleteMe => (PortalController.Instance.GetCurrentSettings() as PortalSettings).DataConsentUserDeleteAction != PortalSettings.UserDeleteAction.Off;
        public bool DataConsentActive => PortalSettings.Current.DataConsentActive == true;
    }
}
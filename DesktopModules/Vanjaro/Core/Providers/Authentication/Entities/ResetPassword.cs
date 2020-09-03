using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Core.Providers.Authentication.Entities
{
    public class ResetPassword
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ResetToken { get; set; }
        public string LoginURL => Managers.ResetPasswordManager.LoginURL("", false);

        private readonly IFileInfo logoFile = string.IsNullOrEmpty(PortalController.Instance.GetPortal(PortalSettings.Current.PortalId, PortalSettings.Current.CultureCode).LogoFile) ? null : FileManager.Instance.GetFile(PortalSettings.Current.PortalId, PortalController.Instance.GetPortal(PortalSettings.Current.PortalId, PortalSettings.Current.CultureCode).LogoFile);
        public string Path => FileManager.Instance.GetUrl(FileManager.Instance.GetFile(logoFile.FileId));

        public string NavigateURL => ServiceProvider.NavigationManager.NavigateURL(PortalSettings.Current.HomeTabId);
        public string SiteName => PortalSettings.Current.PortalName;
    }
}
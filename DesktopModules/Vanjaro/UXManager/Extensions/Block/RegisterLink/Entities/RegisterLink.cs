using Dnn.PersonaBar.Library.Common;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Extensions.Block.RegisterLink.Entities
{
    public class RegisterLink
    {
        public string Url { get; set; }
        public bool IsAuthenticated { get; set; }        

        public bool ShowSignInLink { get; set; }
        public bool ShowProfileLink { get; set; }        
        public bool ShowAvatar { get; set; }
        public bool ShowNotification { get; set; }

        public UserInfo UserInfo => UserController.Instance.GetCurrentUserInfo();

        public string UserProfileURL => Globals.UserProfileURL(UserInfo.UserID);

        public string NotificationExtensionURL => ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=cd8c127f-da66-4036-b107-90061361cf87");

        public string PhotoURL => UserInfo.Profile.PhotoURL.Contains("no_avatar.gif") ? Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, UserInfo.UserID, UserInfo.Email) : Utilities.GetProfileAvatar(UserInfo.UserID);
        public int NotificationCount => Vanjaro.Core.Managers.NotificationManager.RenderNotificationsCount(PortalSettings.Current.PortalId);

        public int RegistrationMode => (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserRegistration;
    }
}
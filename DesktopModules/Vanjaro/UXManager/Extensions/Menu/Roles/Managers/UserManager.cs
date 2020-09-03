using Dnn.PersonaBar.Roles.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System.Net;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Roles
{
    public static partial class Managers
    {
        public class UserManager
        {
            public static UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo userInfo, out ActionResult actionResult)
            {
                actionResult = new ActionResult();
                UserInfo user = UserController.Instance.GetUserById(portalSettings.PortalId, userId);
                if (user == null)
                {
                    actionResult.AddError("HttpStatusCode.NotFound_" + HttpStatusCode.NotFound, Localization.GetString("UserNotFound", Constants.LocalResourcesFile));
                    return null;
                }
                if (!IsAdmin(user, portalSettings))
                {
                    return user;
                }

                if ((user.IsSuperUser && !userInfo.IsSuperUser) || !IsAdmin(portalSettings))
                {
                    actionResult.AddError("HttpStatusCode.NotFound_" + HttpStatusCode.Unauthorized, Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
                    return null;
                }
                if (user.IsSuperUser)
                {
                    user = UserController.Instance.GetUserById(Null.NullInteger, userId);
                }

                return user;
            }

            internal static bool IsAdmin(UserInfo user, PortalSettings portalSettings)
            {
                return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
            }

            internal static bool IsAdmin(PortalSettings portalSettings)
            {
                UserInfo user = UserController.Instance.GetCurrentUserInfo();
                return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
            }
            internal static bool AllowExpired(int userId, int roleId, PortalSettings PortalSettings)
            {
                return userId != PortalSettings.AdministratorId || roleId != PortalSettings.AdministratorRoleId;
            }
        }
    }
}
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Menu.Roles
{
    public static partial class Managers
    {
        public class RoleManager
        {
            #region Public Method
            internal static object GetRoles(int PortalID)
            {
                List<RoleInfo> Roles = new List<RoleInfo>
                {
                    //retreive roles info
                    new RoleInfo { RoleID = int.Parse(Globals.glbRoleUnauthUser), RoleGroupID = -1, RoleName = Globals.glbRoleUnauthUserName },
                    new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers), RoleGroupID = -1, RoleName = Globals.glbRoleAllUsersName }
                };
                foreach (RoleInfo role in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(PortalID).OrderBy(r => r.RoleName))
                {
                    Roles.Add(new RoleInfo { RoleID = role.RoleID, RoleGroupID = role.RoleGroupID, RoleName = role.RoleName, });
                }

                return Roles;
            }


            internal static RoleDto GetRole(int PortalID, int RoleId)
            {
                return RoleDto.FromRoleInfo(DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(PortalID, RoleId));
            }

            public static UserRoleDto SaveUserRole(int portalId, UserInfo currentUserInfo, UserRoleDto userRoleDto, bool notifyUser, bool isOwner)
            {
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;

                if (portalSettings.PortalId != portalId)
                {
                    portalSettings = GetPortalSettings(portalId);
                }

                if (!AllowExpiredRole(portalSettings, userRoleDto.UserId, userRoleDto.RoleId))
                {
                    userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
                }

                UserInfo user = UserController.Instance.GetUserById(portalId, userRoleDto.UserId);
                RoleInfo role = RoleController.Instance.GetRoleById(portalId, userRoleDto.RoleId);
                if (role == null || role.Status != RoleStatus.Approved)
                {
                    throw new Exception(Localization.GetString("RoleIsNotApproved", Components.Constants.LocalResourcesFile));
                }

                if (currentUserInfo.IsSuperUser || currentUserInfo.Roles.Contains(portalSettings.AdministratorRoleName) ||
                    (!currentUserInfo.IsSuperUser && !currentUserInfo.Roles.Contains(portalSettings.AdministratorRoleName) &&
                     role.RoleType != RoleType.Administrator))
                {
                    if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                    {
                        isOwner = false;
                    }

                    RoleController.AddUserRole(user, role, portalSettings, RoleStatus.Approved, userRoleDto.StartTime,
                        userRoleDto.ExpiresTime, notifyUser, isOwner);
                    UserRoleInfo addedRole = RoleController.Instance.GetUserRole(portalId, userRoleDto.UserId, userRoleDto.RoleId);

                    return new UserRoleDto
                    {
                        UserId = addedRole.UserID,
                        RoleId = addedRole.RoleID,
                        DisplayName = addedRole.FullName,
                        StartTime = addedRole.EffectiveDate,
                        ExpiresTime = addedRole.ExpiryDate,
                        AllowExpired = AllowExpiredRole(portalSettings, user.UserID, role.RoleID),
                        AllowDelete = RoleController.CanRemoveUserFromRole(portalSettings, user.UserID, role.RoleID)
                    };
                }
                throw new Exception(Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
            }

            private static PortalSettings GetPortalSettings(int portalId)
            {
                PortalSettings portalSettings = new PortalSettings(portalId);
                IEnumerable<PortalAliasInfo> portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
                portalSettings.PrimaryAlias = portalAliases.FirstOrDefault(a => a.IsPrimary);
                portalSettings.PortalAlias = PortalAliasController.Instance.GetPortalAlias(portalSettings.DefaultPortalAlias);
                return portalSettings;
            }

            internal static bool AllowExpiredRole(PortalSettings portalSettings, int userId, int roleId)
            {
                return userId != portalSettings.AdministratorId || roleId != portalSettings.AdministratorRoleId;
            }
            #endregion

        }
    }
}
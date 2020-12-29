using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Permissions;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Engines.UIEngine.AngularBootstrap.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin,permissiongrid")]
    [ValidateAntiForgeryToken]
    public class PermissiosGridController : WebApiController
    {
        [HttpGet]
        public List<Suggestion> GetSuggestionUsers(string keyword, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return new List<Suggestion>();
                }

                string displayMatch = keyword + "%";
                int totalRecords = 0;
                int totalRecords2 = 0;
                System.Collections.ArrayList matchedUsers = UserController.GetUsersByDisplayName(PortalSettings.PortalId, displayMatch, 0, count,
                    ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(PortalSettings.PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                IEnumerable<Suggestion> finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => x.Membership.Approved)
                    .Select(u => new Suggestion()
                    {
                        Value = u.UserID,
                        Label = $"{u.DisplayName}",
                        UserName = u.Username,
                        Email = u.Email,
                        AvatarUrl = UserUtils.GetProfileImage(PortalSettings.PortalId, u.UserID, u.Email)
                    });

                return finalUsers.ToList().GroupBy(x => x.Value).Select(group => group.First()).ToList();
            }
            catch (Exception)
            {
                return new List<Suggestion>();
            }

        }
        [HttpGet]
        public List<Suggestion> GetSuggestionRoles(string keyword, int roleGroupId)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return new List<Suggestion>();
                }

                List<RoleInfo> roleInfos = RoleController.Instance.GetRoles(PortalSettings.PortalId).ToList();
                roleInfos.Add(new RoleInfo { RoleGroupID = -2, Status = RoleStatus.Approved, RoleID = int.Parse(DotNetNuke.Common.Globals.glbRoleUnauthUser), RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName });
                IEnumerable<Suggestion> matchedRoles = roleInfos
                    .Where(r => (roleGroupId == -2 || r.RoleGroupID == roleGroupId)
                                && r.RoleName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) > -1
                                   && r.Status == RoleStatus.Approved)
                    .Select(r => new Suggestion()
                    {
                        Value = r.RoleID,
                        Label = r.RoleName
                    });

                return matchedRoles.ToList();
            }
            catch (Exception)
            {
                return new List<Suggestion>();
            }

        }
        public override string AccessRoles()
        {
            return "admin,permissiongrid";
        }
        //public override string AllowedAccessRoles(string Identifier)
        //{
        //    return AppFactory.GetAllowedRoles(Identifier);
        //}
    }
}
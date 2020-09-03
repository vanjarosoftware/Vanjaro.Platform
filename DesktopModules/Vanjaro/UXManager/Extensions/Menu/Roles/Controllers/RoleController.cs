using Dnn.PersonaBar.Roles.Components;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Permissions;
using Vanjaro.UXManager.Extensions.Menu.Roles.Entities;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.UXManager.Extensions.Menu.Roles.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Roles.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class RoleController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            RoleController rc = new RoleController();
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            switch (identifier)
            {
                case "setting_roles":
                    {
                        dynamic RoleGroup = RoleGroupManager.GetRoleGroup(userInfo.PortalID);
                        string DefaultRoleGroup = "-2";
                        string DefaultRoleGroupName = string.Empty;
                        Settings.Add("RoleGroup", new UIData { Name = "RoleGroup", Options = RoleGroup, OptionsText = "Name", OptionsValue = "GroupId", Value = DefaultRoleGroup });
                        Settings.Add("Roles", new UIData { Name = "Roles", Value = "", Options = RoleGroupManager.GetGroupRoles(new RoleDto { GroupId = int.Parse(DefaultRoleGroup) }) });
                        Settings.Add("Working_RoleDto", new UIData { Name = "Working_RoleDto", Value = "", Options = new RoleDto() });
                        Settings.Add("Working_RoleGroupDto", new UIData { Name = "Working_RoleGroupDto", Value = "", Options = new RoleGroupDto() });
                        Settings.Add("GroupName", new UIData { Name = "GroupName", Value = DefaultRoleGroupName });

                        break;
                    }

                case "setting_add":
                    {
                        int? rid = null;
                        if (parameters.Count > 0)
                        {
                            rid = int.Parse(parameters["rid"]);
                        }

                        if (rid.HasValue)
                        {
                            Settings.Add("Working_RoleDto", new UIData { Name = "Working_RoleDto", Value = "", Options = RoleManager.GetRole(userInfo.PortalID, rid.Value) });
                        }
                        else
                        {
                            Settings.Add("Working_RoleDto", new UIData { Name = "Working_RoleDto", Value = "", Options = new RoleDto { Status = RoleStatus.Approved, GroupId = -1, SecurityMode = SecurityMode.SecurityRole } });
                        }

                        Settings.Add("RoleGroup", new UIData { Name = "RoleGroup", Options = RoleGroupManager.GetRoleGroup(userInfo.PortalID), OptionsText = "Name", OptionsValue = "GroupId", Value = "-1" });
                        Settings.Add("Status", new UIData { Name = "Status", Options = Factories.AppFactory.GetLocalizedEnumOption(typeof(RoleStatus)), OptionsText = "Key", OptionsValue = "Value", Value = "1" });
                        Settings.Add("SecurityMode", new UIData { Name = "SecurityMode", Options = Factories.AppFactory.GetLocalizedEnumOption(typeof(SecurityMode)), OptionsText = "Key", OptionsValue = "Value", Value = "0" });
                        Settings.Add("Working_RoleGroupDto", new UIData { Name = "Working_RoleGroupDto", Value = "", Options = new RoleGroupDto() });
                        break;
                    }
                case "setting_adduser":
                    {
                        string keyword = string.Empty;
                        int rid = 0;
                        if (parameters.Count > 0)
                            rid = int.Parse(parameters["rid"]);


                        if (rid > 0)
                            Settings.Add("Working_RoleDto", new UIData { Name = "Working_RoleDto", Value = "", Options = RoleManager.GetRole(userInfo.PortalID, rid) });
                        else
                            Settings.Add("Working_RoleDto", new UIData { Name = "Working_RoleDto", Value = "", Options = new RoleDto { Status = RoleStatus.Approved, GroupId = -1, SecurityMode = SecurityMode.SecurityRole } });

                        Settings.Add("Working_UserRole", new UIData { Name = "Working_UserRole", Value = "", Options = new UserRoleInfoDTO { } });
                        Settings.Add("UserRole", new UIData { Name = "UserRole", Value = "", Options = new List<UserRoleInfoDTO>() });
                        Settings.Add("AllUsers", new UIData { Name = "AllUsers", Value = "", Options = null });
                        break;
                    }
            }
            return Settings.Values.ToList();
        }

        [HttpGet]
        public List<Suggestion> GetSuggestionUsers(string keyword, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return new List<Suggestion>();
                }

                string displayMatch = "%" + keyword + "%";
                int totalRecords = 0;
                int totalRecords2 = 0;
                System.Collections.ArrayList matchedUsers = UserController.GetUsersByDisplayName(PortalSettings.PortalId, displayMatch, 0, count, ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(PortalSettings.PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                IEnumerable<Suggestion> finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => x.Membership.Approved)
                    .Select(u => new Suggestion()
                    {
                        Value = u.UserID,
                        Label = $"{u.DisplayName}"
                    });

                return finalUsers.ToList().GroupBy(x => x.Value).Select(group => group.First()).ToList();
            }
            catch (Exception)
            {
                return new List<Suggestion>();
            }
        }

        private static List<UserRoleInfoDTO> ConvertUserInfo(UserInfo userInfo, int RoleID)
        {
            List<UserRoleInfoDTO> ListUserInfo = new List<UserRoleInfoDTO>();
            if (RoleID > 0)
            {
                foreach (UserInfo uInfo in DotNetNuke.Security.Roles.RoleController.Instance.GetUsersByRole((PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId, RoleManager.GetRole(userInfo.PortalID, RoleID).Name))
                {
                    UserRoleInfo userInfoRole = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRole(PortalSettings.Current.PortalId, uInfo.UserID, RoleID);
                    UserRoleInfoDTO UserRoleDTO = new UserRoleInfoDTO
                    {
                        UserId = uInfo.UserID,
                        RoleId = RoleID,
                        DisplayName = uInfo.DisplayName,
                        StartTime = userInfoRole.EffectiveDate,
                        ExpiresTime = userInfoRole.ExpiryDate,
                        AllowExpired = UserManager.AllowExpired(uInfo.UserID, RoleID, PortalSettings.Current),
                        AllowDelete = DotNetNuke.Security.Roles.RoleController.CanRemoveUserFromRole(PortalSettings.Current, uInfo.UserID, RoleID)
                    };
                    ListUserInfo.Add(UserRoleDTO);
                }
            }
            return ListUserInfo;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRole(RoleDto roleDto, [FromUri] bool assignExistUsers = false)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Validate(roleDto);
                if (roleDto.Id == 0)
                {
                    roleDto.Id = Null.NullInteger;
                }
                RolesController.Instance.SaveRole(PortalSettings, roleDto, assignExistUsers, out KeyValuePair<HttpStatusCode, string> message);
                if (!string.IsNullOrEmpty(message.Value))
                {
                    actionResult.AddError(message.Key.ToString(), message.Value);
                }

                if (actionResult.IsSuccess)
                {
                    actionResult.Data = new { RoleGroupManager.GetGroupRoles(new RoleDto { GroupId = roleDto.GroupId }).Roles, RoleGroups = RoleGroupManager.GetRoleGroup(PortalSettings.Current.PortalId) };
                }
            }
            catch (ArgumentException ex)
            {
                actionResult.AddError("HttpStatusCode.BadRequest", ex.Message);
            }
            catch (SecurityException ex)
            {
                actionResult.AddError("HttpStatusCode.BadRequest", ex.Message);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                actionResult.AddError("HttpStatusCode.InternalServerError", ex.Message);
            }

            return actionResult;
        }

        [HttpGet]
        public ActionResult GetRoleUsers(string keyword, int roleId, int pageIndex, int pageSize)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int PortalID = (PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId;
                RoleInfo role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(PortalID, roleId);
                if (role == null && !actionResult.IsSuccess)
                {
                    actionResult.AddError(HttpStatusCode.NotFound.ToString(), Localization.GetString("RoleNotFound", Components.Constants.LocalResourcesFile));
                }

                if (actionResult.IsSuccess)
                {
                    if (role.RoleID == PortalSettings.AdministratorRoleId && !IsAdmin())
                    {
                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
                    }
                }
                if (actionResult.IsSuccess)
                {
                    IList<UserRoleInfo> users = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRoles(PortalID, Null.NullString, role.RoleName);
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        users = users.Where(u => u.FullName.ToLower().Contains(keyword.ToLower())).ToList();
                    }

                    int totalRecords = users.Count;
                    int startIndex = pageIndex * pageSize;
                    PortalInfo portal = PortalController.Instance.GetPortal(PortalID);
                    List<UserRoleInfoDTO> pagedData = new List<UserRoleInfoDTO>();
                    foreach (UserRoleInfo u in users.Skip(startIndex).Take(pageSize))
                    {
                        UserRoleInfoDTO uRoleInfo = new UserRoleInfoDTO();
                        UserInfo UserInfo = UserController.GetUserById((PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId, u.UserID);
                        uRoleInfo.UserId = u.UserID;
                        uRoleInfo.UserName = UserInfo.Username;
                        uRoleInfo.Email = UserInfo.Email;
                        uRoleInfo.Avatar = UserInfo.Profile.PhotoURL.Contains("no_avatar.gif") ? Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, u.UserID, u.Email) : UserInfo.Profile.PhotoURL;
                        uRoleInfo.FirstName = UserInfo.FirstName;
                        uRoleInfo.LastName = UserInfo.LastName;
                        uRoleInfo.RoleId = u.RoleID;
                        uRoleInfo.DisplayName = u.FullName;
                        uRoleInfo.StartTime = u.EffectiveDate;
                        uRoleInfo.ExpiresTime = u.ExpiryDate;
                        uRoleInfo.AllowExpired = AllowExpired(u.UserID, u.RoleID);
                        uRoleInfo.AllowDelete = DotNetNuke.Security.Roles.RoleController.CanRemoveUserFromRole(portal, u.UserID, u.RoleID);
                        pagedData.Add(uRoleInfo);
                    }

                    if (actionResult.IsSuccess)
                    {
                        actionResult.Data = new { users = pagedData, totalRecords };
                    }
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }
            return actionResult;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRole(RoleDto roleDto)
        {
            ActionResult actionResult = new ActionResult();
            string roleName = Dnn.PersonaBar.Roles.Components.RolesController.Instance.DeleteRole(PortalSettings, roleDto.Id, out KeyValuePair<HttpStatusCode, string> message);
            if (!string.IsNullOrEmpty(message.Key.ToString()) && !string.IsNullOrEmpty(message.Value))
            {
                actionResult.AddError(message.Key.ToString(), message.Value);
            }

            if (actionResult.IsSuccess)
            {
                actionResult.Data = new { RoleGroups = RoleGroupManager.GetRoleGroup(PortalSettings.Current.PortalId) };
            }

            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }

        #region Add User in Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUserToRole(UserRoleDto userRoleDto)
        {
            bool notifyUser = false;
            bool isOwner = false;
            ActionResult response = new ActionResult();
            try
            {
                Validate(userRoleDto);
                if (!UserManager.AllowExpired(userRoleDto.UserId, userRoleDto.RoleId, PortalSettings))
                {
                    userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
                }
                UserInfo user = UserManager.GetUser(userRoleDto.UserId, PortalSettings, UserInfo, out response);
                if (user == null)
                {
                    return response;
                }

                RoleInfo role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(PortalSettings.PortalId, userRoleDto.RoleId);
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                {
                    isOwner = false;
                }

                if (role.Status != RoleStatus.Approved)
                {
                    response.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, Localization.GetString("CannotAssginUserToUnApprovedRole", Dnn.PersonaBar.Roles.Components.Constants.LocalResourcesFile));
                }
                foreach (UserRoleInfoDTO ur in ConvertUserInfo(UserInfo, userRoleDto.RoleId))
                {
                    if (userRoleDto.UserId == ur.UserId)
                    {
                        response.AddError("UserAlreadyExist", Localization.GetString("UserAlreadyExist", Components.Constants.LocalResourcesFile));
                    }
                }

                if (response.IsSuccess)
                {
                    DotNetNuke.Security.Roles.RoleController.AddUserRole(user, role, PortalSettings, RoleStatus.Approved, userRoleDto.StartTime, userRoleDto.ExpiresTime, notifyUser, isOwner);
                    UserRoleInfo addedUser = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRole(PortalSettings.PortalId, userRoleDto.UserId, userRoleDto.RoleId);
                    PortalInfo portal = PortalController.Instance.GetPortal(PortalSettings.PortalId);

                    UserRoleInfoDTO UserRoleDTO = new UserRoleInfoDTO
                    {
                        UserId = addedUser.UserID,
                        RoleId = addedUser.RoleID,
                        DisplayName = addedUser.FullName,
                        StartTime = addedUser.EffectiveDate,
                        ExpiresTime = addedUser.ExpiryDate,
                        AllowExpired = UserManager.AllowExpired(addedUser.UserID, addedUser.RoleID, PortalSettings),
                        AllowDelete = DotNetNuke.Security.Roles.RoleController.CanRemoveUserFromRole(portal, addedUser.UserID, addedUser.RoleID)
                    };
                    response.Data = UserRoleDTO;
                }
            }
            catch (ArgumentException ex)
            {
                response.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                response.AddError("HttpStatusCode.InternalServerError" + HttpStatusCode.InternalServerError, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveUserRole(UserRoleDto userRoleDto, bool notifyUser, bool isOwner, int UserId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Validate(userRoleDto);
                UserInfo user = UserManager.GetUser(UserId, PortalSettings, UserInfo, out actionResult);
                if (actionResult.IsSuccess && user != null)
                {
                    userRoleDto.UserId = UserId;
                    actionResult.Data = RoleManager.SaveUserRole(PortalSettings.PortalId, UserInfo, userRoleDto, notifyUser, isOwner);
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("SaveUserRole_Exception", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUserFromRole(UserRoleDto userRoleDto)
        {
            ActionResult response = new ActionResult();
            try
            {
                Validate(userRoleDto);
                UserInfo user = UserManager.GetUser(userRoleDto.UserId, PortalSettings, UserInfo, out response);
                if (user == null)
                {
                    return response;
                }

                DotNetNuke.Security.Roles.RoleController.Instance.UpdateUserRole(PortalSettings.PortalId, userRoleDto.UserId, userRoleDto.RoleId,
                    RoleStatus.Approved, false, true);

                response.Data = new { userRoleDto.UserId, userRoleDto.RoleId };
            }
            catch (ArgumentException ex)
            {
                response.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                response.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                response.AddError("HttpStatusCode.BadRequest" + HttpStatusCode.BadRequest, ex.Message);
            }
            return response;
        }

        private bool IsAdmin()
        {
            UserInfo user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.AdministratorRoleName);
        }

        private bool AllowExpired(int userId, int roleId)
        {
            return userId != PortalSettings.AdministratorId || roleId != PortalSettings.AdministratorRoleId;
        }

        [HttpGet]
        public ActionResult GetSuggestUsers(int roleId, int count, string keyword)
        {
            ActionResult ActionResult = new ActionResult();
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return ActionResult.Data = new List<UserRoleDto>();
                }

                string displayMatch = keyword + "%";
                int totalRecords = 0;
                int totalRecords2 = 0;
                bool isAdmin = UserManager.IsAdmin(PortalSettings);

                System.Collections.ArrayList matchedUsers = UserController.GetUsersByDisplayName(PortalSettings.PortalId, displayMatch, 0, count,
                    ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(PortalSettings.PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                IEnumerable<UserRoleDto> finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => isAdmin || !x.Roles.Contains(PortalSettings.AdministratorRoleName))
                    .Select(u => new UserRoleDto()
                    {
                        UserId = u.UserID,
                        DisplayName = $"{u.DisplayName} ({u.Username})"
                    });

                ActionResult.Data = finalUsers.ToList().GroupBy(x => x.UserId).Select(group => group.First());
            }
            catch (Exception ex)
            {
                ActionResult.AddError("HttpStatusCode.InternalServerError" + HttpStatusCode.InternalServerError, ex.Message);

            }
            return ActionResult;

        }

        #endregion
    }
}
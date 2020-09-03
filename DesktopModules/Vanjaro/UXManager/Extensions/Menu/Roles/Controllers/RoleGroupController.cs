using Dnn.PersonaBar.Roles.Components;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.UXManager.Extensions.Menu.Roles.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Roles.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class RoleGroupController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            RoleGroupController rg = new RoleGroupController();
            switch (identifier)
            {
                case "setting_addgroup":
                    {
                        Settings.Add("Roles", new UIData { Name = "Roles", Value = "", Options = RoleManager.GetRoles(userInfo.PortalID) });
                        int gid = 0;
                        if (parameters.Count > 0)
                        {
                            gid = int.Parse(parameters["gid"]);
                        }

                        if (gid >= 0)
                        {
                            Settings.Add("Working_RoleGroupDto", new UIData { Name = "Working_RoleGroupDto", Value = "", Options = rg.GetGroup(gid) });
                        }

                        break;
                    }
            }
            return Settings.Values.ToList();
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public object GetRoleGroup()
        {
            return RoleGroupManager.GetRoleGroup(PortalSettings.PortalId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRoleGroup(RoleGroupDto roleGroupDto)
        {
            ActionResult actionResult = new ActionResult();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            try
            {
                Validate(roleGroupDto);

                RoleGroupInfo roleGroup = roleGroupDto.ToRoleGroupInfo();
                roleGroup.PortalID = PortalSettings.ActiveTab.PortalID;
                string Message = string.Empty;

                if (roleGroup.RoleGroupID < Null.NullInteger)
                {
                    try
                    {
                        DotNetNuke.Security.Roles.RoleController.AddRoleGroup(roleGroup);
                    }
                    catch (Exception ex)
                    {
                        actionResult.Errors.Add("DuplicateRoleGroup", ex);
                        actionResult.Message = Localization.GetString("DuplicateRoleGroup", Constants.LocalResourcesFile);
                    }
                }
                else
                {
                    try
                    {
                        DotNetNuke.Security.Roles.RoleController.UpdateRoleGroup(roleGroup);
                    }
                    catch (Exception ex)
                    {
                        actionResult.Errors.Add("DuplicateRoleGroup", ex);
                        actionResult.Message = Localization.GetString("DuplicateRoleGroup", Constants.LocalResourcesFile);
                    }
                }
                roleGroup = DotNetNuke.Security.Roles.RoleController.GetRoleGroups(PortalSettings.PortalId).Cast<RoleGroupInfo>()
                    .FirstOrDefault(r => r.RoleGroupName == roleGroupDto.Name?.Trim());

                data["FromRoleGroupInfo"] = RoleGroupDto.FromRoleGroupInfo(roleGroup);
                data["AllRoleGroup"] = RoleGroupManager.GetRoleGroup(PortalSettings.PortalId);
                actionResult.Data = data;
            }
            catch (Exception ex)
            {
                actionResult.AddError("SaveRoleGroup", ex.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleGroup(RoleGroupDto roleGroupDto)
        {
            ActionResult actionResult = new ActionResult();
            RoleGroupController rg = new RoleGroupController();
            RoleGroupDto roleGroup = rg.GetGroup(roleGroupDto.Id);
            if (roleGroup == null)
            {
                actionResult.AddError("RoleGroupNotFound", Constants.LocalResourcesFile);
            }
            if (actionResult.IsSuccess)
            {
                DotNetNuke.Security.Roles.RoleController.DeleteRoleGroup(PortalSettings.PortalId, roleGroup.Id);
                actionResult.Data = new { AllRoleGroup = RoleGroupManager.GetRoleGroup(PortalSettings.PortalId) };
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic GetGroupRoles(string keyword, int startIndex, int pageSize, RoleDto GroupRoles)
        {
            return RoleGroupManager.GetGroupRoles(GroupRoles, keyword, startIndex, pageSize);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
        private RoleGroupDto GetGroup(int groupId)
        {
            return RoleGroupDto.FromRoleGroupInfo(DotNetNuke.Security.Roles.RoleController.GetRoleGroup(PortalSettings.ActiveTab.PortalID, groupId));
        }
    }
}
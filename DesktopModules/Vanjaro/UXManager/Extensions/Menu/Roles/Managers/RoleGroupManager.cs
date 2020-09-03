using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Menu.Roles
{
    public static partial class Managers
    {
        public class RoleGroupManager
        {
            #region Public method           
            public static dynamic GetGroupRoles(RoleDto GroupRoles, string keyword, int startIndex, int pageSize)
            {
                int groupId = GroupRoles.GroupId;
                IEnumerable<RoleDto> Roles = Dnn.PersonaBar.Roles.Components.RolesController.Instance.GetRoles(PortalController.Instance.GetCurrentSettings() as PortalSettings, groupId, keyword, out int total, startIndex, pageSize).Select(RoleDto.FromRoleInfo);

                return new { Roles = ConvertGroupRoles(Roles), total };
            }
            public static dynamic GetGroupRoles(RoleDto GroupRoles)
            {
                return GetGroupRoles(GroupRoles, null, 0, 25);
            }
            public static object GetRoleGroup(int PortalId)
            {
                return GetRoleGroup(PortalId, null);
            }

            public static object GetRoleGroup(int PortalId, int? GroupID)
            {
                var data = new { Groups = new List<object>(), Roles = new List<object>() };
                PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                //retreive role groups info
                data.Groups.Add(new { GroupId = -2, Name = Localization.GetString("AllRoles", Components.Constants.LocalResourcesFile), Description = Localization.GetString("AllRoles", Components.Constants.LocalResourcesFile) });
                data.Groups.Add(new { GroupId = -1, Name = Localization.GetString("GlobalRoles", Components.Constants.LocalResourcesFile), Description = Localization.GetString("GlobalRoles", Components.Constants.LocalResourcesFile) });
                foreach (RoleGroupInfo group in DotNetNuke.Security.Roles.RoleController.GetRoleGroups(PortalId))
                {
                    IEnumerable<RoleDto> Roles = Dnn.PersonaBar.Roles.Components.RolesController.Instance.GetRoles(portalSettings, group.RoleGroupID, null, out int total, 0, 25).Select(RoleDto.FromRoleInfo);
                    data.Groups.Add(new { GroupId = group.RoleGroupID, Name = group.RoleGroupName, Description = group.Description, RolesCount = total });
                }

                if (GroupID.HasValue)
                {
                    foreach (dynamic g in data.Groups)
                    {
                        if (g.GroupId == GroupID)
                        {
                            data.Groups.Clear();
                            data.Groups.Add(new { GroupId = g.GroupId, Name = g.Name, Description = g.Description });
                            break;
                        }
                    }
                }
                return data.Groups;
            }

            public static List<dynamic> ConvertGroupRoles(IEnumerable<RoleDto> Roles)
            {
                List<dynamic> RoleDto = new List<dynamic>();
                try
                {
                    foreach (RoleDto r in Roles)
                    {
                        dynamic role = new ExpandoObject();
                        string RoleGroupName = string.Empty;
                        dynamic roleGroup = GetRoleGroup(PortalSettings.Current.PortalId, r.GroupId);
                        if (roleGroup.Count > 0)
                        {
                            RoleGroupName = roleGroup[0].Name;
                        }

                        role.TrialFee = r.TrialFee;
                        role.IsSystem = r.IsSystem;
                        role.SecurityMode = r.SecurityMode;
                        role.Status = r.Status;
                        role.Icon = r.Icon;
                        role.RsvpCode = r.RsvpCode;
                        role.AutoAssign = r.AutoAssign;
                        role.IsPublic = r.IsPublic;
                        role.TrialFrequency = r.TrialFrequency;
                        role.TrialPeriod = r.TrialPeriod;
                        role.AllowOwner = r.AllowOwner;
                        role.BillingFrequency = r.BillingFrequency;
                        role.BillingPeriod = r.BillingPeriod;
                        role.ServiceFee = r.ServiceFee;
                        role.Description = r.Description;
                        role.Name = r.Name;
                        role.GroupId = r.GroupId;
                        role.Id = r.Id;
                        role.UsersCount = r.UsersCount;
                        role.GroupName = RoleGroupName;

                        RoleDto.Add(role);
                    }
                }
                catch (Exception) { }

                return RoleDto;
            }
            #endregion
        }
    }
}
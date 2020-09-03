using DotNetNuke.Security.Roles;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Permissions;

namespace Vanjaro.Common.Factories
{
    public partial class Factory
    {
        public class RoleFactory
        {
            public static List<RoleGroup> GetAllRoleGroups(int PortalID, string ResourceFile)
            {
                List<RoleGroup> RoleGroups = new List<RoleGroup>
                {
                    new RoleGroup { Name = "All Roles", Id = -2 },
                    new RoleGroup { Name = "Global Roles", Id = -1 }
                };
                RoleGroups.AddRange(RoleController.GetRoleGroups(PortalID).Cast<RoleGroupInfo>().Select(RoleGroup.FromRoleGroupInfo).ToList());
                return RoleGroups;
            }

        }
    }
}
using DotNetNuke.Security.Roles;

namespace Vanjaro.Common.Permissions
{

    public class RoleGroup
    {
        public RoleGroup()
        {
            Id = -2;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int RolesCount { get; set; }

        public string Description { get; set; }

        public static RoleGroup FromRoleGroupInfo(RoleGroupInfo roleGroup)
        {
            return new RoleGroup()
            {
                Id = roleGroup.RoleGroupID,
                Name = roleGroup.RoleGroupName,
                Description = roleGroup.Description,
                RolesCount = roleGroup.Roles?.Count ?? 0
            };
        }

        public RoleGroupInfo ToRoleGroupInfo()
        {
            return new RoleGroupInfo()
            {
                RoleGroupID = Id,
                RoleGroupName = Name,
                Description = Description ?? ""
            };
        }
    }

    public class Suggestion
    {
        public int Value { get; set; }
        public string Label { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}
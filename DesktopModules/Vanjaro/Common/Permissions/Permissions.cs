
using System.Collections.Generic;
using Vanjaro.Common.Manager;

namespace Vanjaro.Common.Permissions
{
    public class Permissions
    {
        public bool Inherit { get; set; }

        public bool ShowInheritCheckBox { get; set; }

        public int InheritPermissionID { get; set; }

        public IList<Permission> PermissionDefinitions { get; set; }

        public IList<RolePermission> RolePermissions { get; set; }

        public IList<UserPermission> UserPermissions { get; set; }

        public Permissions() : this(false, false)
        {
        }

        public Permissions(bool needDefinitions, bool Locked)
        {
            RolePermissions = new List<RolePermission>();
            UserPermissions = new List<UserPermission>();

            if (needDefinitions)
            {
                PermissionDefinitions = new List<Permission>();
                this.EnsureDefaultRoles(Locked);
            }
        }


    }
}
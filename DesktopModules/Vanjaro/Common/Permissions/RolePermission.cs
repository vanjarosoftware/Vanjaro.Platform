using System.Collections.Generic;

namespace Vanjaro.Common.Permissions
{
    public class RolePermission
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public IList<Permission> Permissions { get; set; }

        public bool Locked { get; set; }

        public bool IsDefault { get; set; }

        public RolePermission()
        {
            Permissions = new List<Permission>();
        }
    }
}
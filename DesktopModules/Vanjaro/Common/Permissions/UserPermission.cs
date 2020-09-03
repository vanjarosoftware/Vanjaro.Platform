using System.Collections.Generic;

namespace Vanjaro.Common.Permissions
{
    public class UserPermission
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public IList<Permission> Permissions { get; set; }

        public UserPermission()
        {
            Permissions = new List<Permission>();
        }
    }
}
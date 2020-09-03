using System;

namespace Vanjaro.UXManager.Extensions.Menu.Roles.Entities
{
    public class UserRoleInfoDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpiresTime { get; set; }
        public bool AllowExpired { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowOwner { get; set; }
    }
}
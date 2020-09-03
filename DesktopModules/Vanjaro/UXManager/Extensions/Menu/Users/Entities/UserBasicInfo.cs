using System;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Entities
{
    public class UserBasicInfo
    {
        public UserBasicInfo() { }
        public int userId { get; set; }
        public string displayName { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime createdOnDate { get; set; }
        public bool isDeleted { get; set; }
        public bool authorized { get; set; }
        public bool isSuperUser { get; set; }
        public bool isAdmin { get; set; }
        public DateTime lastLogin { get; set; }
        public DateTime lastActivity { get; set; }
        public DateTime lastPasswordChange { get; set; }
        public DateTime lastLockout { get; set; }
        public bool IsOnline { get; set; }
        public bool isLocked { get; set; }
        public bool needUpdatePassword { get; set; }
        public int portalId { get; set; }
        public string profileUrl { get; }
        public string editProfileUrl { get; }
        public string userFolder { get; set; }
        public int userFolderId { get; set; }
        public bool hasUserFiles { get; set; }

    }
    public class UserItem
    {
        public UserItem() { }

        public int UserId { get; set; }
        public string Avatar { get; set; }
        public int PortalId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string LastModifiedOnDate { get; set; }
        public string FriendlyLastModifiedOnDate { get; set; }
    }
}



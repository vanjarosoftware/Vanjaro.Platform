namespace Vanjaro.Common.Permissions
{
    public class Permission
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; }

        public bool FullControl { get; set; }

        public bool View { get; set; }

        public bool AllowAccess { get; set; }
    }

    public class DNNModulePermissionInfo
    {
        public int ModuleDefID { get; set; }
        public string PermissionCode { get; set; }
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; }
        public string PermissionName { get; set; }

        public DNNModulePermissionInfo()
        {

        }
    }
}
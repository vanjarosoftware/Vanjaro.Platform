namespace Vanjaro.Core.Data.Entities
{
    public class WorkflowPermissionInfo
    {
        public int ModuleDefID { get; set; }
        public string PermissionCode { get; set; }
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; }
        public string PermissionName { get; set; }

        public WorkflowPermissionInfo()
        {

        }
    }

    public class SectionPermissionInfo
    {
        public int ModuleDefID { get; set; }
        public string PermissionCode { get; set; }
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; }
        public string PermissionName { get; set; }

        public SectionPermissionInfo()
        {

        }
    }
}
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Entities
{
    public class PageVersion : Core.Data.Entities.Pages
    {
        public string DisplayName { get; set; }
        public string PhotoURL { get; set; }
        public WorkflowState State { get; set; }
        public bool IsLogsExist { get; set; }
    }
}
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Data.Entities
{
    public partial class WorkflowState
    {
        public bool IsFirst => WorkflowManager.IsFirstState(WorkflowID, StateID);
        public bool IsLast => WorkflowManager.IsLastState(WorkflowID, StateID);
        public bool IsDeleted { get; set; }
    }
}
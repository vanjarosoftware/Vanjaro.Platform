using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class PortalFactory
        {
            public static void DeletePages(int PortalID)
            {
                Pages.Delete("Where PortalID=@0", PortalID);
            }

            public static void DeleteCustomBlocks(int PortalID)
            {
                CustomBlock.Delete("Where PortalID=@0", PortalID);
            }

            public static void DeleteSetting(int PortalID)
            {
                Setting.Delete("Where PortalID=@0", PortalID);
            }

            public static void DeleteWorkflows(int PortalID)
            {
                foreach (Workflow workflow in Factories.WorkflowFactory.GetAll(PortalID))
                {
                    Factories.WorkflowFactory.DeleteWorkflow(workflow);
                }
                WorkflowLog.Delete("Where PortalID=@0", PortalID);
            }
        }
    }
}
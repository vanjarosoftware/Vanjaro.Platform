using DotNetNuke.Abstractions;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Components;

namespace Vanjaro.Core.Data.Entities
{
    public class WorkflowContent
    {

        public int ID { get; set; }
        public int EntityID { get; set; }
        public int Version { get; set; }
        public int StateID { get; set; }
        public bool IsPublished { get; set; }
        public string EntityName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Workflow { get; set; }
        public string State { get; set; }

        public string EntityURL
        {
            get
            {

                if (EntityID > 0)
                {
                    if (EntityName.ToLower() == Enum.WorkflowLogType.VJPage.ToString().ToLower())
                    {
                        return ServiceProvider.NavigationManager.NavigateURL(EntityID);
                    }
                }
                return null;
            }
        }

    }
}
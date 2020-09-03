using DotNetNuke.Abstractions;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Core.Data.Entities
{
    public class WorkflowPage
    {

        public int PageID { get; set; }
        public int TabID { get; set; }
        public int Version { get; set; }
        public int StateID { get; set; }
        public bool IsPublished { get; set; }
        public string TabName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Workflow { get; set; }
        public string State { get; set; }

        public string TabURL
        {
            get
            {
                if (TabID > 0)
                {
                    return ServiceProvider.NavigationManager.NavigateURL(TabID);

                }
                return null;
            }
        }

    }
}
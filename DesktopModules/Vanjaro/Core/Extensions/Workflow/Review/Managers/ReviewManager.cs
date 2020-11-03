using System.Collections.Generic;
using Vanjaro.Core.Components;
using Vanjaro.Core.Extensions.Workflow.Review.Components;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Workflow.Review.Managers
{
    public class ReviewManager
    {

        public static List<StringValue> GetStatesforReview(int PortalID, int UserID, string ReviewType)
        {
            List<StringValue> DDLList = new List<StringValue>();
            StringValue st = new StringValue
            {
                Text = DotNetNuke.Services.Localization.Localization.GetString("All", Core.Components.Constants.LocalResourcesFile),
                Value = "0"
            };
            DDLList.Add(st);

            DDLList.AddRange(WorkflowManager.GetStatesforReview(PortalID, UserID, ReviewType));

            return DDLList;
        }
    }
}
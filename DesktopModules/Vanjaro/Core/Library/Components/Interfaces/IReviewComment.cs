using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Components.Interfaces
{
    public interface IReviewComment
    {
        void AddComment(string Entity, int EntityID, string Action, string Comment, PortalSettings PortalSettings);
    }
}
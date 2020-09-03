using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.UXManager.Extensions.Menu.SiteGroups.Components
{
    public class PortalGroupInfo
    {
        public int PortalGroupId { get; set; }
        public string PortalGroupName { get; set; }
        public string AuthenticationDomain { get; set; }
        public MasterPortal MasterPortal { get; set; }
        public List<MasterPortal> Portals { get; set; }
        public string Description { get; set; }
    }

    public class MasterPortal
    {
        public int PortalID { get; set; }
        public string PortalName { get; set; }
    }
}
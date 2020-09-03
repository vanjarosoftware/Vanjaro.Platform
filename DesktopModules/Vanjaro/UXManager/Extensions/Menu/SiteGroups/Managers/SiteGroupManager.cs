using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web;
using Vanjaro.UXManager.Extensions.Menu.SiteGroups.Components;

namespace Vanjaro.UXManager.Extensions.Menu.SiteGroups.Managers
{
    public class SiteGroupManager
    {
        public static List<MasterPortal> GetAvailablePortals()
        {
            List<MasterPortal> values = new List<MasterPortal>();
            MasterPortal t = new MasterPortal();
            t.PortalID = -1;
            t.PortalName = "Choose a Site";
            values.Add(t);
            PortalController Portals = new PortalController();

            foreach (PortalInfo item in Portals.GetPortals().Cast<PortalInfo>().Where(x => x.PortalGroupID == -1))
            {
                MasterPortal text = new MasterPortal();
                text.PortalID = item.PortalID;
                text.PortalName = item.PortalName;
                values.Add(text);
            }

            return values;
        }
        public static List<Components.PortalGroupInfo> GetSiteGroup(int PortalGroupId)
        {
            List<Components.PortalGroupInfo> info = new List<Components.PortalGroupInfo>();
            PortalGroupController portalGroupController = new PortalGroupController();
            List<MasterPortal> portals = new List<MasterPortal>();

            if (PortalGroupId == -1)
            {
                Components.PortalGroupInfo portalGroupInfo = new Components.PortalGroupInfo();
                portalGroupInfo.PortalGroupId = PortalGroupId;
                portalGroupInfo.Portals = Portals(PortalGroupId);
                info.Add(portalGroupInfo);
            }
            else
                info = SiteGroups().Where(p => p.PortalGroupId == PortalGroupId).ToList();

            return info;
        }
        public static List<Components.PortalGroupInfo> SiteGroups()
        {
            List<Components.PortalGroupInfo> info = new List<Components.PortalGroupInfo>();
            PortalGroupController portalGroupController = new PortalGroupController();
            foreach (var item in portalGroupController.GetPortalGroups())
            {
                Components.PortalGroupInfo portalGroupInfo = new Components.PortalGroupInfo();
                portalGroupInfo.PortalGroupId = item.PortalGroupId;
                portalGroupInfo.AuthenticationDomain = item.AuthenticationDomain;
                portalGroupInfo.Description = item.PortalGroupDescription;
                portalGroupInfo.PortalGroupName = item.PortalGroupName;

                MasterPortal masterPortal = new MasterPortal();
                masterPortal.PortalID = item.MasterPortalId;
                masterPortal.PortalName = item.MasterPortalName;
                portalGroupInfo.MasterPortal = masterPortal;
                
                portalGroupInfo.Portals = Portals(item.PortalGroupId);

                info.Add(portalGroupInfo);

            }
            return info;
        }

        public static List<MasterPortal> Portals(int PortalGroupId) 
        {
            PortalGroupController portalGroupController = new PortalGroupController();
            List<MasterPortal> portals = new List<MasterPortal>();
            foreach (var portalGroup in portalGroupController.GetPortalsByGroup(PortalGroupId).ToList())
            {
                MasterPortal mp = new MasterPortal();
                mp.PortalID = portalGroup.PortalID;
                mp.PortalName = portalGroup.PortalName;
                portals.Add(mp);
            }
            return portals;
        }



        public static int Save(Components.PortalGroupInfo portalGroup)
        {
            if (portalGroup.PortalGroupId == -1)
            {
                return AddPortalGroup(portalGroup);
            }
            else
            {
                return UpdatePortalGroup(portalGroup);
            }
        }

        public static int AddPortalGroup(Components.PortalGroupInfo portalGroup)
        {
            PortalGroupController portalGroupController = new PortalGroupController();
            UserCopiedCallback callback = delegate { };
            var group = new DotNetNuke.Entities.Portals.PortalGroupInfo
            {
                AuthenticationDomain = portalGroup.AuthenticationDomain,
                MasterPortalId = portalGroup.MasterPortal.PortalID,
                PortalGroupDescription = portalGroup.Description,
                PortalGroupName = portalGroup.PortalGroupName
            };
            portalGroupController.AddPortalGroup(@group);
            if (portalGroup.Portals != null)
            {
                foreach (var portal in portalGroup.Portals)
                {
                    var p = new PortalController().GetPortal(portal.PortalID);
                    portalGroupController.AddPortalToGroup(p, @group, callback);
                }
            }
            return @group.PortalGroupId;
        }

        public static int UpdatePortalGroup(Components.PortalGroupInfo portalGroup)
        {
            PortalGroupController portalGroupController = new PortalGroupController();
            UserCopiedCallback callback = delegate { };
            var g = portalGroupController.GetPortalGroups().Single(pg => pg.PortalGroupId == portalGroup.PortalGroupId);
            g.PortalGroupName = portalGroup.PortalGroupName;
            g.AuthenticationDomain = portalGroup.AuthenticationDomain;
            g.PortalGroupDescription = portalGroup.Description;
            portalGroupController.UpdatePortalGroup(g);
            var currentPortals = PortalsOfGroup(portalGroup.PortalGroupId, portalGroup.MasterPortal.PortalID).ToList();
            foreach (var portal in currentPortals)
            {
                if (portalGroup.Portals == null || portalGroup.Portals.All(p => p.PortalID != portal.PortalID))
                    portalGroupController.RemovePortalFromGroup(portal, g, false, callback);
            }

            if (portalGroup.Portals != null)
                foreach (var portal in portalGroup.Portals)
                {
                    if (currentPortals.All(p => p.PortalID != portal.PortalID))
                    {
                        var p = new PortalController().GetPortal(portal.PortalID);
                        portalGroupController.AddPortalToGroup(p, g, callback);
                    }
                }
            return g.PortalGroupId;
        }

        public static List<PortalInfo> PortalsOfGroup(int groupId, int masterPortalId)
        {
            PortalGroupController portalGroupController = new PortalGroupController();
            return portalGroupController.GetPortalsByGroup(groupId).Where(x => x.PortalID != masterPortalId).ToList();
        }

        public static void Delete(int portalGroupId)
        {
            PortalGroupController portalGroupController = new PortalGroupController();
            UserCopiedCallback callback = delegate { };
            var group = portalGroupController.GetPortalGroups().Single(g => g.PortalGroupId == portalGroupId);
            List<Components.PortalGroupInfo> item = SiteGroups().Where(p => p.PortalGroupId == portalGroupId).ToList();

            foreach (var portal in PortalsOfGroup(item[0].PortalGroupId, item[0].MasterPortal.PortalID).ToList())
            {
                if (item[0].Portals != null || item[0].Portals.All(p => p.PortalID == portal.PortalID))
                    portalGroupController.RemovePortalFromGroup(portal, group, false, callback);
            }
            portalGroupController.DeletePortalGroup(group);
        }
    }
}
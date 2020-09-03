using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Block.Menu.Entities
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.WebControls;
    using System.Reflection;

    public class Generic : ILocalisation
    {
        private bool haveChecked;
        private object locApi;
        private MethodInfo locTab;
        private MethodInfo locNodes;

        public bool HaveApi()
        {
            if (!haveChecked)
            {
                Dictionary<int, DesktopModuleInfo> modules = DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId);
                foreach (KeyValuePair<int, DesktopModuleInfo> moduleKeyPair in modules)
                {
                    if (!string.IsNullOrEmpty(moduleKeyPair.Value.BusinessControllerClass))
                    {
                        try
                        {
                            locApi = Reflection.CreateObject(moduleKeyPair.Value.BusinessControllerClass, moduleKeyPair.Value.BusinessControllerClass);
                            locTab = locApi.GetType().GetMethod("LocaliseTab", new[] { typeof(TabInfo), typeof(int) });
                            if (locTab != null)
                            {
                                if (locTab.IsStatic)
                                {
                                    locApi = null;
                                }
                                break;
                            }

                            locNodes = locApi.GetType().GetMethod("LocaliseNodes", new[] { typeof(DNNNodeCollection) });
                            if (locNodes != null)
                            {
                                if (locNodes.IsStatic)
                                {
                                    locApi = null;
                                }
                                break;
                            }
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        {
                        }
                        // ReSharper restore EmptyGeneralCatchClause
                    }
                }
                haveChecked = true;
            }

            return (locTab != null) || (locNodes != null);
        }

        public TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
            return (locTab == null) ? null : (TabInfo)locTab.Invoke(locApi, new object[] { tab, portalId });
        }

        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
        {
            return (locNodes == null) ? null : (DNNNodeCollection)locNodes.Invoke(locApi, new object[] { nodes });
        }
    }
}
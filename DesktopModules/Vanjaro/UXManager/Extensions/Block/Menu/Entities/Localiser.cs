using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;
using System.Collections.Generic;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Block.Menu.Entities
{
    public class Localiser
    {
        private readonly int portalId;
        private static bool apiChecked;
        private static ILocalisation _LocalisationApi;
        private static ILocalisation LocalisationApi
        {
            get
            {
                if (!apiChecked)
                {
                    foreach (ILocalisation api in new ILocalisation[] { new Generic() })
                    {
                        if (api.HaveApi())
                        {
                            _LocalisationApi = api;
                            break;
                        }
                    }
                    apiChecked = true;
                }
                return _LocalisationApi;
            }
        }

        public static DNNNodeCollection LocaliseDNNNodeCollection(DNNNodeCollection nodes)
        {
            return (LocalisationApi == null) ? nodes : (LocalisationApi.LocaliseNodes(nodes) ?? nodes);
        }

        public Localiser(int portalId)
        {
            this.portalId = portalId;
        }

        public void LocaliseNode(MenuNode node)
        {
            TabInfo tab = (node.TabId > 0) ? TabController.Instance.GetTab(node.TabId, Null.NullInteger, false) : null;
            if (tab != null)
            {
                List<Core.Data.Entities.Localization> Localization = Core.Managers.LocalizationManager.GetLocaleProperties(PortalSettings.Current.CultureCode, "Page", tab.TabID, null);
                TabInfo localised = LocaliseTab(tab);
                tab = localised ?? tab;

                if (localised != null)
                {
                    node.TabId = tab.TabID;
                    node.Enabled = !tab.DisableLink;
                    if (!tab.IsVisible)
                    {
                        node.TabId = -1;
                    }
                }

                node.Text = (Localization.Count > 0 && Localization.Where(x => x.Name == "Name").FirstOrDefault() != null && !string.IsNullOrEmpty(Localization.Where(x => x.Name == "Name").FirstOrDefault().Value)) ? Localization.Where(x => x.Name == "Name").FirstOrDefault().Value : tab.TabName;
                node.Title = (Localization.Count > 0 && Localization.Where(x => x.Name == "Title").FirstOrDefault() != null && !string.IsNullOrEmpty(Localization.Where(x => x.Name == "Title").FirstOrDefault().Value)) ? Localization.Where(x => x.Name == "Title").FirstOrDefault().Value : tab.Title;
                node.Description = (Localization.Count > 0 && Localization.Where(x => x.Name == "Description").FirstOrDefault() != null && !string.IsNullOrEmpty(Localization.Where(x => x.Name == "Description").FirstOrDefault().Value)) ? Localization.Where(x => x.Name == "Description").FirstOrDefault().Value : tab.Description;
                node.Keywords = tab.KeyWords;
            }
            else
            {
                node.TabId = -1;
            }

            node.Children.ForEach(LocaliseNode);
        }

        private TabInfo LocaliseTab(TabInfo tab)
        {
            return (LocalisationApi == null) ? null : LocalisationApi.LocaliseTab(tab, portalId);
        }
    }
}
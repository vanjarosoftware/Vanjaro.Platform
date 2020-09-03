using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI;
using System;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Block.Menu.Entities
{
    public class MenuBase
    {
        private MenuSetting menuSetting;
        public MenuNode RootNode { get; set; }
        private readonly Dictionary<string, string> nodeSelectorAliases = new Dictionary<string, string>
                                                                          {
                                                                            {"rootonly", "*,0,0"},
                                                                            {"rootchildren", "+0"},
                                                                            {"currentchildren", "."}
                                                                          };
        private PortalSettings hostPortalSettings;
        internal PortalSettings HostPortalSettings => hostPortalSettings ?? (hostPortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings);

        internal void ApplySetting(MenuSetting _menuSetting)
        {
            menuSetting = _menuSetting;
        }

        internal void Initialize()
        {
            if (!string.IsNullOrEmpty(menuSetting.NodeSelector))
            {
                ApplyNodeSelector();
            }
            new Localiser(HostPortalSettings.PortalId).LocaliseNode(RootNode);
            if (!menuSetting.IncludeHidden)
            {
                FilterHiddenNodes(RootNode);
            }
        }

        private void ApplyNodeSelector()
        {
            if (!nodeSelectorAliases.TryGetValue(menuSetting.NodeSelector.ToLowerInvariant(), out string selector))
            {
                selector = menuSetting.NodeSelector;
            }

            List<string> selectorSplit = SplitAndTrim(selector);

            int currentTabId = HostPortalSettings.ActiveTab.TabID;

            MenuNode newRoot = RootNode;

            string rootSelector = selectorSplit[0];
            if (rootSelector != "*")
            {
                if (rootSelector.StartsWith("+"))
                {
                    int depth = Convert.ToInt32(rootSelector);
                    newRoot = RootNode;
                    for (int i = 0; i <= depth; i++)
                    {
                        newRoot = newRoot.Children.Find(n => n.Breadcrumb);
                        if (newRoot == null)
                        {
                            RootNode = new MenuNode();
                            return;
                        }
                    }
                }
                else if (rootSelector.StartsWith("-") || rootSelector == "0" || rootSelector == ".")
                {
                    newRoot = RootNode.FindById(currentTabId);
                    if (newRoot == null)
                    {
                        RootNode = new MenuNode();
                        return;
                    }

                    if (rootSelector.StartsWith("-"))
                    {
                        for (int n = Convert.ToInt32(rootSelector); n < 0; n++)
                        {
                            if (newRoot.Parent != null)
                            {
                                newRoot = newRoot.Parent;
                            }
                        }
                    }
                }
                else
                {
                    newRoot = RootNode.FindByNameOrId(rootSelector);
                    if (newRoot == null)
                    {
                        RootNode = new MenuNode();
                        return;
                    }
                }
            }

            // ReSharper disable PossibleNullReferenceException
            RootNode = new MenuNode(newRoot.Children);
            // ReSharper restore PossibleNullReferenceException

            if (selectorSplit.Count > 1)
            {
                for (int n = Convert.ToInt32(selectorSplit[1]); n > 0; n--)
                {
                    List<MenuNode> newChildren = new List<MenuNode>();
                    foreach (MenuNode child in RootNode.Children)
                    {
                        newChildren.AddRange(child.Children);
                    }
                    RootNode = new MenuNode(newChildren);
                }
            }

            if (selectorSplit.Count > 2)
            {
                List<MenuNode> newChildren = RootNode.Children;
                for (int n = Convert.ToInt32(selectorSplit[2]); n > 0; n--)
                {
                    List<MenuNode> nextChildren = new List<MenuNode>();
                    foreach (MenuNode child in newChildren)
                    {
                        nextChildren.AddRange(child.Children);
                    }
                    newChildren = nextChildren;
                }
                foreach (MenuNode node in newChildren)
                {
                    node.Children = null;
                }
            }
        }

        private static List<string> SplitAndTrim(string str)
        {
            return new List<string>(str.Split(',')).ConvertAll(s => s.Trim().ToLowerInvariant());
        }

        private void FilterHiddenNodes(MenuNode parentNode)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
            List<MenuNode> filteredNodes = new List<MenuNode>();
            filteredNodes.AddRange(
                parentNode.Children.FindAll(
                    n =>
                    {
                        TabInfo tab = TabController.Instance.GetTab(n.TabId, portalSettings.PortalId);
                        return tab == null || !tab.IsVisible;
                    }));

            parentNode.Children.RemoveAll(n => filteredNodes.Contains(n));

            parentNode.Children.ForEach(FilterHiddenNodes);
        }

        internal static int GetNavNodeOptions(bool includeHidden)
        {
            return (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf +
                   (includeHidden ? (int)Navigation.NavNodeOptions.IncludeHiddenNodes : 0);
        }
    }
}
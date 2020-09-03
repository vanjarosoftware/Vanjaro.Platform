using DotNetNuke.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Vanjaro.UXManager.Extensions.Block.Menu.Entities
{
    [Serializable]
    [XmlRoot("root", Namespace = "")]
    public class MenuNode
    {
        public static List<MenuNode> ConvertDNNNodeCollection(DNNNodeCollection dnnNodes, MenuNode parent)
        {
            List<MenuNode> result = new List<MenuNode>();
            foreach (DNNNode node in dnnNodes)
            {
                result.Add(new MenuNode(node, parent));
            }

            return result;
        }

        public int TabId { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool Enabled { get; set; }
        public bool Selected { get; set; }
        public bool Breadcrumb { get; set; }
        public bool Separator { get; set; }
        public string Icon { get; set; }
        public string LargeImage { get; set; }
        public string CommandName { get; set; }
        public string CommandArgument { get; set; }
        public bool First => (Parent == null) || (Parent.Children[0] == this);
        public bool Last => (Parent == null) || (Parent.Children[Parent.Children.Count - 1] == this);
        public string Target { get; set; }

        public int Depth
        {
            get
            {
                int result = -1;
                MenuNode current = this;
                while (current.Parent != null)
                {
                    result++;
                    current = current.Parent;
                }
                return result;
            }
        }

        public string Keywords { get; set; }
        public string Description { get; set; }

        private List<MenuNode> _Children;
        public List<MenuNode> Children { get => _Children ?? (_Children = new List<MenuNode>()); set => _Children = value; }

        public MenuNode Parent { get; set; }

        public MenuNode()
        {
        }

        public MenuNode(DNNNodeCollection dnnNodes)
        {
            Children = ConvertDNNNodeCollection(dnnNodes, this);
        }

        public MenuNode(List<MenuNode> nodes)
        {
            Children = nodes;
            Children.ForEach(c => c.Parent = this);
        }

        public MenuNode(DNNNode dnnNode, MenuNode parent)
        {
            TabId = Convert.ToInt32(dnnNode.ID);
            Text = dnnNode.Text;
            Url = (dnnNode.ClickAction == eClickAction.PostBack)
                    ? "postback:" + dnnNode.ID
                    : string.IsNullOrEmpty(dnnNode.JSFunction) ? dnnNode.NavigateURL : "javascript:" + dnnNode.JSFunction;
            Enabled = dnnNode.Enabled;
            Selected = dnnNode.Selected;
            Breadcrumb = dnnNode.BreadCrumb;
            Separator = dnnNode.IsBreak;
            Icon = dnnNode.Image;
            Target = dnnNode.Target;
            Title = null;
            Keywords = null;
            Description = null;
            Parent = parent;
            CommandName = dnnNode.get_CustomAttribute("CommandName");
            CommandArgument = dnnNode.get_CustomAttribute("CommandArgument");

            DNNNodeToMenuNode(dnnNode, this);

            if ((dnnNode.DNNNodes != null) && (dnnNode.DNNNodes.Count > 0))
            {
                Children = ConvertDNNNodeCollection(dnnNode.DNNNodes, this);
            }
        }

        public static void DNNNodeToMenuNode(DNNNode dnnNode, MenuNode menuNode)
        {
            menuNode.LargeImage = dnnNode.LargeImage;
        }

        public MenuNode FindById(int tabId)
        {
            if (tabId == TabId)
            {
                return this;
            }

            foreach (MenuNode child in Children)
            {
                MenuNode result = child.FindById(tabId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public MenuNode FindByNameOrId(string tabNameOrId)
        {
            if (tabNameOrId.Equals(Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return this;
            }

            if (tabNameOrId == TabId.ToString())
            {
                return this;
            }

            foreach (MenuNode child in Children)
            {
                MenuNode result = child.FindByNameOrId(tabNameOrId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
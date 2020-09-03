using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Entities
{
    public class TreeView
    {
        public TreeView()
        {
            children = new List<TreeView>();
        }
        public List<TreeView> children { get; set; }
        public int childrenCount { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
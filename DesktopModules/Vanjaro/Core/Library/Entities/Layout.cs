using System.Collections.Generic;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core.Entities
{
    public class Layout
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string SVG { get; set; }
        public string Content { get; set; }
        public string ContentJSON { get; set; }
        public string Style { get; set; }
        public string StyleJSON { get; set; }
        public List<GlobalBlock> Blocks { get; set; }
        public bool IsSystem { get; set; }
        public List<Layout> Children { get; set; }
        public int SortOrder { get; set; }
        public Dictionary<string, dynamic> Settings { get; set; }
    }
}
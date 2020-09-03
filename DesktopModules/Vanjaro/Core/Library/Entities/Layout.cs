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
        public List<CustomBlock> Blocks { get; set; }
        public bool IsSystem { get; set; }
    }
}
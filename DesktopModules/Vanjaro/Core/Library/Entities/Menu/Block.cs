using System;
using System.Collections.Generic;

namespace Vanjaro.Core.Entities.Menu
{
    public class Block
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public Guid Guid { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}
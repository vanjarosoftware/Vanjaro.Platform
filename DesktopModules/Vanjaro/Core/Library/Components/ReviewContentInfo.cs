using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Components
{
    public class ReviewContentInfo
    {
        public string Entity { get; set; }
        public int EntityID { get; set; }
        public bool IsPublished { get; set; }
        public int StateID { get; set; }
        public int Version { get; set; }
    }
}
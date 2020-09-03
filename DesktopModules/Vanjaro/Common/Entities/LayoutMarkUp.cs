using System.Collections.Generic;

namespace Vanjaro.Common.Entities
{
    public class LayoutMarkUp
    {
        public string tag { get; set; }
        public Dictionary<string, object> attr { get; set; }
        public string text { get; set; }
        public List<LayoutMarkUp> child { get; set; }
    }
}
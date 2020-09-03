using System.Collections.Generic;

namespace Vanjaro.Core.Entities.Theme
{
    public class ThemeEditor
    {
        public string Guid { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public List<dynamic> Controls { get; set; }
        public string Sass { get; set; }
    }
}
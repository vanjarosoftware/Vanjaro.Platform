using System;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Library.Entities.Menu
{
    public class AppExtension
    {
        public string Text { get; set; }
        public string Command { get; set; }
        public string Class { get; set; }
        public List<AppExtension> Children { get; set; }
        public Guid ItemGuid { get; set; }
        public int Width { get; set; }
    }
}
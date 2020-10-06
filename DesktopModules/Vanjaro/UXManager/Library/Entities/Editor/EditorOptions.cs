using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.UXManager.Library.Entities
{
    public class EditorOptions
    {
        public string GetContentUrl{ get; set; }
        public string UpdateContentUrl { get; set; }
        public string SettingsUrl { get; set; }
        public string ContainerID { get; set; }
        public bool EditPage { get; set; }

    }
}

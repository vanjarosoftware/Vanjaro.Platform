using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Entities
{
    public class EditorOptions
    {
        public string GetContentUrl { get; set; }
        public string UpdateContentUrl { get; set; }
        public string SettingsUrl { get; set; }
        public string ContainerID { get; set; }
        public bool EditPage { get; set; }
        public int ModuleId { get; set; }
        public int EntityID { get; set; }
        public string RevisionUrl { get; set; }
        public string RevisionGUID { get; set; }
    }
}
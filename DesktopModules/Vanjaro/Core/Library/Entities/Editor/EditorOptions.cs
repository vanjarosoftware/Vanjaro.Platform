using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Core.Entities.Menu;

namespace Vanjaro.Core.Entities
{
    public class EditorOptions
    {
        public string GetContentUrl { get; set; }
        public string UpdateContentUrl { get; set; }
        public string SettingsUrl { get; set; }
        public string AppName { get; set; }
        public string AppTitle { get; set; }
        public bool EditPage { get; set; }
        public int ModuleId { get; set; }
        public int EntityID { get; set; }
        public string RevisionUrl { get; set; }
        public string RevisionGUID { get; set; }
        public Dictionary<MenuAction, dynamic> AppLink { get; set; }
    }
}
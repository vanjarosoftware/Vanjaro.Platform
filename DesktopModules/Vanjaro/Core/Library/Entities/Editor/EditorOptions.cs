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
        public string SettingsTitle { get; set; }
        public string AppName { get; set; }
        public string AppTitle { get; set; }
        public bool EditPage { get; set; }
        public int ModuleId { get; set; }
        public int EntityID { get; set; }
        public string RevisionUrl { get; set; }
        public string RevisionGUID { get; set; }
        public bool Blocks { get; set; }
        public bool Language { get; set; }
        public bool CustomBlocks { get; set; }
        public bool Library { get; set; }
        public string PublishText { get; set; }
        public string PublishLink { get; set; }
        public string TemplateLibraryURL { get; set; }
        public bool InjectThemeCSS { get; set; }
        public bool ResponsiveStyling { get; set; }
        public Dictionary<MenuAction, dynamic> AppLink { get; set; }
    }
}
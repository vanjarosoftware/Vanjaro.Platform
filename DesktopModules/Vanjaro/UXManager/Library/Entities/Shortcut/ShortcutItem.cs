using System.Collections.Generic;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Library.Entities.Shortcut
{
    public class ShortcutItem
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public int? ViewOrder { get; set; }
        public bool Breakline { get; set; }
        public int? Width { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public bool Visibility { get; set; }
        public MenuAction Action { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}
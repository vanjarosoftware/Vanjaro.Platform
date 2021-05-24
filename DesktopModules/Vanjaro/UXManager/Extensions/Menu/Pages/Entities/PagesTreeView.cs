using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Menu.Pages.Entities
{
    public class PagesTreeView
    {
        public string label { get; set; }
        public int Value { get; set; }
        public bool selected { get; set; }
        public string PageUrl { get; set; }
        public bool IsRedirectPage { get; set; }
        public bool IsAnchorPage { get; set; }
        public bool FolderPage { get; set; }
        public bool LinkNewWindow { get; set; }
        public bool HasBeenPublished { get; set; }
        public bool HasEditPermission { get; set; }
        public bool IsVisible { get; set; }
        public List<PagesTreeView> children { get; set; }
        public int usedbyCount { get; set; }
        public bool HasContent { get; set; }
    }
}

using System.Collections.Generic;

namespace Vanjaro.Common.Components
{
    public class TreeView
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public List<TreeView> children { get; set; }
        public int childrenCount { get; set; }
        public bool uploadAllowed { get; set; }
        public string DateModified { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }
        public bool Lock { get; set; }
        public bool IsImage { get; set; }
        public string ProviderType { get; set; }
    }
}
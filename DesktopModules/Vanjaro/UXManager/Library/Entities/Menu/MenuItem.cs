using System;
using System.ComponentModel;

namespace Vanjaro.UXManager.Library.Entities.Menu
{
    public class MenuItem
    {
        public MenuItem Hierarchy { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public Guid ItemGuid { get; set; }
        [DefaultValue(null)]
        public int? ViewOrder { get; set; }

        public bool AboveBreakLine { get; set; }
        [DefaultValue(null)]

        public bool BelowBreakLine { get; set; }
        [DefaultValue(null)]
        public string URL { get; set; }
    }
}
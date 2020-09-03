using System.Collections.Generic;

namespace Vanjaro.Core.Entities.Theme
{
    public class ThemeEditorWrapper
    {
        public bool DeveloperMode { get; set; }
        public List<ThemeEditor> ThemeEditors { get; set; }
        public List<ThemeFont> Fonts { get; set; }
    }
}
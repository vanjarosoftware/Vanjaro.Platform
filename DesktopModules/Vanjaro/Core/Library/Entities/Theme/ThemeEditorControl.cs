namespace Vanjaro.Core.Entities.Theme
{
    public class ThemeEditorControl
    {
        public string Guid { get; set; }
        public string Title { get; set; }
        public string DefaultValue { get; set; }
        public string Suffix { get; set; }
        public string CustomCSS { get; set; }
        public string PreviewCSS { get; set; }
        public string LessVariable { get; set; }
        public string Type { get; set; }
    }

    public class Slider : ThemeEditorControl
    {
        public float RangeMin { get; set; }
        public float RangeMax { get; set; }
        public float Increment { get; set; }
    }

    public class Dropdown : ThemeEditorControl
    {
        public dynamic Options { get; set; }
    }

    public class ColorPicker : ThemeEditorControl
    {
    }

    public class Fonts : ThemeEditorControl
    {
    }
}
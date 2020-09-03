namespace Vanjaro.Common.Engines.UIEngine
{
    public class UIData : IUIData
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public dynamic Options { get; set; }
        public string OptionsText { get; set; }
        public string OptionsValue { get; set; }

        public bool DoNotTrackChanges { get; set; }
        public bool IsChanged { get; set; }
        public bool IsNew { get; set; }
        public bool IsDeleted { get; set; }
    }
}
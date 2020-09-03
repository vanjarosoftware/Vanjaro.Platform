namespace Vanjaro.Common.Engines.UIEngine
{
    public interface IUIData
    {
        string Name { get; set; }
        string Value { get; set; }

        dynamic Options { get; set; }
        string OptionsText { get; set; }
        string OptionsValue { get; set; }

        bool DoNotTrackChanges { get; set; }
        bool IsChanged { get; set; }
        bool IsNew { get; set; }
        bool IsDeleted { get; set; }
    }
}
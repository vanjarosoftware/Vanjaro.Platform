using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.Common.Data.Entities
{
    public partial class Setting : IUIData
    {
        public Setting()
        {

        }
        public Setting(string Name)
        {
            this.Name = Name;
        }
        public Setting(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }


        public dynamic Options
        {
            get;
            set;
        }

        public string OptionsText
        {
            get;
            set;
        }

        public string OptionsValue
        {
            get;
            set;
        }

        public bool DoNotTrackChanges { get; set; }

        public bool IsChanged
        {
            get;
            set;
        }

        new public bool IsNew
        {
            get;
            set;
        }

        public bool IsDeleted
        {
            get;
            set;
        }

    }
}
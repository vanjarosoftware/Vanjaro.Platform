using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Entities
{
    public class ImportField
    {
        public enum DataTypes
        {
            String = 0, Integer = 1, Decimal = 2, DateTime = 3
        }
        public ImportField()
        { 

        }
        public ImportField(string DisplayName, string Name, bool IsRequired, DataTypes DataType, string SubLabel)
        {
            this.DisplayName = DisplayName;
            this.Name = Name;
            this.IsRequired = IsRequired;
            this.DataType = DataType;
            this.SubLabel = SubLabel;
        }

        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string SubLabel { get; set; }
        public bool IsRequired { get; set; }
        public DataTypes DataType { get; set; }
        public bool IsHeader { get; set; }
    }
}
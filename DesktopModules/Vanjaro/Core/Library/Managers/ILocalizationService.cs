using System.Collections.Generic;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public interface ILocalizationService
        {
            int EntityID { get; }
            string Entity { get; }
            List<Data.Entities.Localization> LocaleProperties { get; set; }
            void RemoveLocaleProperty(string Language, string PropertyName);
            void AddLocaleProperty(int EntityID, string Language, string PropertyName, string PropertyValue);
        }
    }
}

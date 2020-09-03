using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;
using static Vanjaro.Core.Factories;
using DNNLocalization = DotNetNuke.Services.Localization;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public static class LocalizationManager
        {
            public static List<Locales> GetActiveLocale(int PortalID)
            {
                List<Locales> Locales = new List<Locales>();
                DNNLocalization.LocaleController Locale = new DNNLocalization.LocaleController();
                foreach (KeyValuePair<string, DNNLocalization.Locale> item in Locale.GetLocales(PortalID))
                {
                    Locales.Add(new Locales { Value = item.Key, Text = item.Value.NativeName });
                }

                return Locales;
            }
            public static void AddProperty(Localization localization)
            {
                List<Localization> Localization = new List<Localization>
                {
                    localization
                };
                AddProperty(Localization);
            }
            public static void AddProperty(List<Localization> localizations)
            {
                Factories.LocalizationFactory.AddUpdateProperty(localizations);
            }
            public static void RemoveProperty(string EntityName, int EntityID)
            {
                Factories.LocalizationFactory.RemoveProperty(EntityName, EntityID);
            }
            public static void RemoveProperty(string Language, string EntityName, int EntityID)
            {
                Factories.LocalizationFactory.RemoveProperty(Language, EntityName, EntityID);
            }
            public static List<Localization> RemoveLocaleProperty(List<Localization> LocaleProperties, string Language, string PropertyName)
            {
                Localization pl = LocaleProperties.Where(lp => lp.Language == Language && lp.Name == PropertyName).FirstOrDefault();
                if (pl != null)
                {
                    pl.Delete();
                }
                return LocaleProperties;
            }
            public static List<Localization> GetLocaleProperties(string Entity)
            {
                return Factories.LocalizationFactory.GetLocaleProperties(Entity);
            }
            public static List<Localization> GetLocaleProperties(string Entity, int EntityID)
            {
                return Factories.LocalizationFactory.GetLocaleProperties(Entity, EntityID);
            }
            public static List<Localization> GetLocaleProperties(string Language, string Entity, int EntityID, string PropertyName)
            {
                return Factories.LocalizationFactory.GetLocaleProperties(Language, Entity, EntityID, PropertyName);
            }
            public static List<Localization> AddLocaleProperty(List<Localization> LocaleProperties, string Entity, int EntityID, string Language, string PropertyName, string PropertyValue)
            {
                Localization pl = LocaleProperties.Where(lp => lp.Language == Language && lp.Name == PropertyName).FirstOrDefault();

                if (pl != null)
                {
                    pl.Value = PropertyValue;
                }
                else
                {
                    pl = new Localization() { EntityID = EntityID, EntityName = Entity, Language = Language, Name = PropertyName, Value = PropertyValue };
                    LocaleProperties.Add(pl);
                }

                return LocaleProperties;
            }
        }
    }
}

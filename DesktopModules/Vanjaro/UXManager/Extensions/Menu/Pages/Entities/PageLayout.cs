using Dnn.PersonaBar.Pages.Services.Dto;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Pages.Entities
{
    public class PageSettingLayout : ILocalizationService
    {
        public PageSettingLayout(int EntityID)
        {
            if (EntityID > 0)
            {
                LocaleProperties = LocalizationManager.GetLocaleProperties(Entity).Where(e => e.EntityID == EntityID).ToList();
            }
        }
        public PageSettings PageSettings { get; set; }
        public Layout PageLayout { get; set; }
        public int EntityID => PageSettings != null && PageSettings.TabId > 0 ? PageSettings.TabId : 0;
        public string Entity => "Page";
        private List<Localization> _LocaleProperties;
        public List<Localization> LocaleProperties
        {
            get
            {
                if (_LocaleProperties == null)
                {
                    _LocaleProperties = LocalizationManager.GetLocaleProperties(Entity).Where(e => e.EntityID == EntityID).ToList();
                }

                return _LocaleProperties;
            }
            set => _LocaleProperties = value;
        }
        public bool GetLocaleProperty(int TabId, string Language, string PropertyName)
        {
            return LocalizationManager.GetLocaleProperties(Language, Entity, TabId, PropertyName).Count > 0;
        }
        public void AddLocaleProperty(int EntityID, string Language, string PropertyName, string PropertyValue)
        {
            LocaleProperties = LocalizationManager.AddLocaleProperty(LocaleProperties, Entity, EntityID, Language, PropertyName, PropertyValue);
        }
        public void RemoveLocaleProperty(string Language, string PropertyName)
        {
            LocaleProperties = LocalizationManager.RemoveLocaleProperty(LocaleProperties, Language, PropertyName);
        }
        public bool ReplaceTokens { get; set; }
        public bool MakePublic { get; set; }
    }
}
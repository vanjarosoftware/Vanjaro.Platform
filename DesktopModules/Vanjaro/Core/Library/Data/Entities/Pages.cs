using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using System.Linq;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Data.Entities
{
    public partial class Pages
    {
        public bool ReplaceTokens
        {
            get
            {
                string Setting = Managers.SettingManager.GetValue(PortalID, TabID, "setting_detail", "ReplaceTokens", null);
                return !string.IsNullOrEmpty(Setting) ? bool.Parse(Setting) : false;
            }
        }

        public string Title
        {
            get
            {
                List<Localization> Localization = LocalizationManager.GetLocaleProperties(PortalSettings.Current.CultureCode, "Page", PortalSettings.Current.ActiveTab.TabID, null);
                return Localization.Where(x => x.Name == "Title").FirstOrDefault() != null && !string.IsNullOrEmpty(Localization.Where(x => x.Name == "Title").FirstOrDefault().Value) ? Localization.Where(x => x.Name == "Title").FirstOrDefault().Value : null;
            }
        }

        public string Description
        {
            get
            {
                List<Localization> Localization = LocalizationManager.GetLocaleProperties(PortalSettings.Current.CultureCode, "Page", PortalSettings.Current.ActiveTab.TabID, null);
                return Localization.Where(x => x.Name == "Description").FirstOrDefault() != null && !string.IsNullOrEmpty(Localization.Where(x => x.Name == "Description").FirstOrDefault().Value) ? Localization.Where(x => x.Name == "Description").FirstOrDefault().Value : null;
            }
        }
    }
}
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Components
{

    public class Theme
    {
        #region Private Member
        int? PortalID;
        #endregion

        public Theme(int PortalID)
        {
            this.PortalID = PortalID;
        }
        public string Name
        {
            get
            {
                int PortalId = -1;
                if (PortalID != null)
                    PortalId = PortalID.Value;
                else if (PortalSettings.Current != null)
                    PortalId = PortalSettings.Current.PortalId;
                string ThemeValue = "Basic";
                Data.Entities.Setting ThemeSetting = SettingManager.GetSettings(PortalId, -1, "setting_theme").Where(s => s.Name == "Theme").FirstOrDefault();
                if (ThemeSetting != null)
                {
                    ThemeValue = ThemeSetting.Value;
                }
                return ThemeValue;
            }
        }

        public string EditLayout
        {
            get
            {
                string CacheKey = Factories.CacheFactory.GetCacheKey(Factories.CacheFactory.Keys.Theme + "EditLayout", Name);
                string _EditLayout = Factories.CacheFactory.Get(CacheKey);
                if (string.IsNullOrEmpty(_EditLayout))
                {
                    string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + Name + "/Layout.Edit.html");
                    _EditLayout = System.IO.File.ReadAllText(FolderPath);
                    Factories.CacheFactory.Set(CacheKey, _EditLayout);
                }
                return _EditLayout;
            }
        }

        public string GUID
        {
            get
            {
                return "49A70BA1-206B-471F-800A-679799FF09DF";
            }
        }

        public string ScriptsPath
        {
            get
            {
                return "~/Portals/_default/vThemes/" + Name + "/js/";
            }
        }
        public string ThemeJS
        {
            get
            {
                return "~/Portals/_default/vThemes/" + Name + "/theme.js";
            }
        }
    }
}

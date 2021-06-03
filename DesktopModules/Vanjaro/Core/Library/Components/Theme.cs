using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using static Vanjaro.Core.Factories;
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
                return ThemeManager.GetGUID(Name);
            }
        }

        public string DesignScript
        {
            get
            {
                return ThemeManager.GetDesignScript(Name);
            }
        }

        public string ClientScript
        {
            get
            {
                return ThemeManager.GetClientScript(Name);
            }
        }

        public string Assembly
        {
            get
            {
                return ThemeManager.GetAssembly(Name);
            }
        }        
    }
}

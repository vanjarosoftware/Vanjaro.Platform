using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Theme.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Theme", new UIData { Name = "Theme", Options = GetAllThemes(PortalSettings) } }
            };
            string ThemeBuilderUrl = Library.Managers.PageManager.GetCurrentTabUrl(PortalSettings) + "?mid=0&icp=true&guid=726c5619-e193-4605-acaf-828576ba095a";
            Settings.Add("ThemeBuilderUrl", new UIData { Name = "ThemeBuilderUrl", Value = ThemeBuilderUrl });
            return Settings.Values.ToList();
        }

        private static List<Entities.Theme> GetAllThemes(PortalSettings PortalSettings)
        {
            string ThemeValue = "Basic";
            Core.Data.Entities.Setting ThemeSetting = Core.Managers.SettingManager.GetSettings(PortalSettings.PortalId, -1, "setting_theme").Where(s => s.Name == "Theme").FirstOrDefault();
            if (ThemeSetting != null)
            {
                ThemeValue = ThemeSetting.Value;
            }

            string strRoot = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/");
            string[] arrThemes = Directory.GetDirectories(strRoot);
            List<Entities.Theme> Themes = new List<Entities.Theme>();
            foreach (string Theme in arrThemes)
            {
                Entities.Theme th = new Entities.Theme
                {
                    Text = Theme.Replace(strRoot, ""),
                    Value = Theme.Replace(strRoot, "")
                };
                if (File.Exists(strRoot + th.Value + "\\Theme.jpg"))
                {
                    th.Thumbnail = (HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + th.Value + "/Theme.jpg"));
                }

                if (ThemeValue == th.Value)
                {
                    Themes.Insert(0, th);
                }
                else
                {
                    Themes.Add(th);
                }
            }
            return Themes;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Update(string Theme)
        {
            if (!string.IsNullOrEmpty(Theme))
            {
                string BaseEditorFolder = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + Theme + "/editor");
                string PortalThemeFolder = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + Theme + "/").Replace("_default", PortalSettings.PortalId.ToString());
                if (!File.Exists(PortalThemeFolder + "Theme.css"))
                {
                    Core.Managers.SettingManager.Copy(BaseEditorFolder, BaseEditorFolder.Replace("_default", PortalSettings.PortalId.ToString()));
                    try
                    {
                        ThemeManager.ProcessScss(PortalSettings.PortalId);
                    }
                    catch (System.Exception ex) { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); }
                }
                Core.Managers.SettingManager.UpdateValue(PortalSettings.PortalId, -1, "setting_theme", "Theme", Theme);
            }
        }
         
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
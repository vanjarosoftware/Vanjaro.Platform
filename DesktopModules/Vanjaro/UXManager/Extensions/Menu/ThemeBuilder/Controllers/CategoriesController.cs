using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class CategoriesController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string Theme = string.Empty;
            if (parameters.Count > 0)
            {
                if (parameters.ContainsKey("themename"))
                {
                    Theme = parameters["themename"];
                }
            }
            Settings.Add("Theme", new UIData { Name = "Theme", Value = Theme });
            bool CheckFeatureAccess = UserController.Instance.GetCurrentUserInfo().IsSuperUser ? false : true;
            Settings.Add("Categories", new UIData { Name = "Categories", Options = Core.Managers.ThemeManager.GetCategories(CheckFeatureAccess).OrderBy(o => o.ViewOrder).ToList() });
            string ThemeUrl;
            if (MenuManager.GetURL().ToLower().Contains("guid=726c5619-e193-4605-acaf-828576ba095a"))
                ThemeUrl = ServiceProvider.NavigationManager.NavigateURL() + MenuManager.GetURL().ToLower().Replace("guid=726c5619-e193-4605-acaf-828576ba095a", "guid=5fa3e7fb-bdcb-4b4b-9620-f6318fe95cc5").TrimEnd('&');
            else
                ThemeUrl = ServiceProvider.NavigationManager.NavigateURL() + MenuManager.GetURL() + "mid=0&icp=true&guid=5fa3e7fb-bdcb-4b4b-9620-f6318fe95cc5";
            Settings.Add("ThemeUrl", new UIData { Name = "ThemeUrl", Value = ThemeUrl });
            Settings.Add("ThemesCount", new UIData { Name = "ThemesCount", Value = GetThemesCount().ToString() });
            
            return Settings.Values.ToList();
        }

        private static int GetThemesCount()
        {
            string strRoot = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/");
            string[] arrThemes = Directory.GetDirectories(strRoot);
            return arrThemes.Length;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
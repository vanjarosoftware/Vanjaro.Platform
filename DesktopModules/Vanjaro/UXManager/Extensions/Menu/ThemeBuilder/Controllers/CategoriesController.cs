using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
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
            Settings.Add("Categories", new UIData { Name = "Categories", Options = Core.Managers.ThemeManager.GetCategories().OrderBy(o => o.ViewOrder).ToList() });
            string ThemeUrl = PageManager.GetCurrentTabUrl(PortalSettings) + "?mid=0&icp=true&guid=5fa3e7fb-bdcb-4b4b-9620-f6318fe95cc5";
            Settings.Add("ThemeUrl", new UIData { Name = "ThemeUrl", Value = ThemeUrl });
            return Settings.Values.ToList();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
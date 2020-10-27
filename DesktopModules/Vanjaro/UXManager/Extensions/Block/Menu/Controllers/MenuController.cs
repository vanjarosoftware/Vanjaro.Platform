using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Block.Menu.Entities;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Block.Menu.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class MenuController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            List<StringText> Rootpages = new List<StringText>
            {
                new StringText { Text = "All Root Items", Value = "0" },
                new StringText { Text = "Current Parent", Value = "1" },
                new StringText { Text = "Current Page", Value = "2" },
                new StringText { Text = "Individual Page", Value = "3" }
            };
            Settings.Add("RootPages", new UIData { Name = "RootPages", Options = Rootpages, OptionsText = "Text", OptionsValue = "Value", Value = "0" });
            List<StringText> pages = GetPages();
            Settings.Add("Pages", new UIData { Name = "Pages", Options = pages, OptionsText = "Text", OptionsValue = "Value", Value = pages.Count > 0 ? pages[0].Value : "0" });
            Settings.Add("DisplayAllPages", new UIData { Name = "DisplayAllPages", Value = "true" });
            Settings.Add("IncludeHiddenPageNo", new UIData { Name = "IncludeHiddenPageNo", Value = "false" });
            Settings.Add("LimitDepth", new UIData { Name = "LimitDepth", Value = "false" });
            Settings.Add("NoOfDepth", new UIData { Name = "NoOfDepth", Value = "0" });
            Settings.Add("SkipPages", new UIData { Name = "SkipPages", Value = "false" });
            Settings.Add("NoOfPagesSkip", new UIData { Name = "NoOfPagesSkip", Value = "0" });
            Settings.Add("Global", new UIData { Name = "Global", Value = "true" });
            Settings.Add("GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "menu") });
            Settings.Add("IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() });
            List<StringText> templates = GetTemplates();
            Settings.Add("Template", new UIData { Name = "Template", Options = templates, OptionsText = "Text", OptionsValue = "Value"});
            return Settings.Values.ToList();
        }

        [HttpPost]
        [DnnPageEditor]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public void Update(Dictionary<string, string> Attributes)
        {
            Core.Managers.BlockManager.UpdateDesignElement(PortalSettings, Attributes);
        }

        private static List<StringText> GetPages()
        {
            return Library.Managers.PageManager.GetParentPages(PortalSettings.Current).Where(p => p.TabID != -1).Select(a => new StringText() { Value = a.TabID.ToString(), Text = a.TabName }).ToList();
        }

        private static List<StringText> GetTemplates()
        {    
            string TemplatesPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.CurrentTheme.Name + "/blocks/Menu/Templates");
            List<StringText> Templates = new List<StringText>();
            if (Directory.Exists(TemplatesPath))
            {
                foreach (string file in Directory.GetFiles(TemplatesPath))
                {
                    string FileName = Path.GetFileName(file);
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        if (FileName.EndsWith(".cshtml"))
                        {
                            FileName = FileName.Replace(".cshtml", "");
                            Templates.Add( new StringText() { Value = FileName.ToString(), Text = FileName.ToString() });
                        }
                    }
                }
            }

            return Templates;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
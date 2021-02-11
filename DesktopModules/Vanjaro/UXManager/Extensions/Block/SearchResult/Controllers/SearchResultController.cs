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
using Vanjaro.Core.Components;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Block.SearchResult.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin,anonymous")]
    public class SearchResultController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters, PortalSettings portalSettings)
        {
            List<StringText> templates = GetTemplates();
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Global", new UIData { Name = "Global", Value = "true" } },
                { "LinkTarget", new UIData { Name = "LinkTarget", Value = "true" } },
                { "EnableWildSearch", new UIData { Name = "EnableWildSearch", Value = "true" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "search result") } },
                { "IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() } },
                { "Template", new UIData { Name = "Template", Options = templates, OptionsText = "Text", OptionsValue = "Value" } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        [DnnPageEditor]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public void Update(Dictionary<string, string> Attributes)
        {
            Core.Managers.BlockManager.UpdateDesignElement(PortalSettings, Attributes);
        }


        private static List<StringText> GetTemplates()
        {
            string TemplatesPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.CurrentTheme.Name + "/blocks/search result/Templates");
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
                            Templates.Add(new StringText() { Value = FileName.ToString(), Text = FileName.ToString() });
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
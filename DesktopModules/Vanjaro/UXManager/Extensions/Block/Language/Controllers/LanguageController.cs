using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Block.BlockLanguage.Entities;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Block.BlockLanguage.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class LanguageController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            List<StringText> templates = GetTemplates();
            Settings.Add("Template", new UIData { Name = "Template", Options = templates, OptionsText = "Text", OptionsValue = "Value" });
            Settings.Add("Global", new UIData { Name = "Global", Value = "true" });
            Settings.Add("GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "language") });
            Settings.Add("IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() });
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
            string TemplatesPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.CurrentTheme.Name + "/blocks/language/Templates");
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

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

namespace Vanjaro.UXManager.Extensions.Block.RegisterLink.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin,anonymous")]
    public class RegisterLinkController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            List<StringText> templates = GetTemplates();
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "ShowSignInLink", new UIData { Name = "ShowSignInLink", Value = "false" } },
                { "ShowAvatar", new UIData { Name = "ShowAvatar", Value = "false" } },
                { "ShowNotification", new UIData { Name = "ShowNotification", Value = "false" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "register link") } },
                { "Global", new UIData { Name = "Global", Value = "false" } },
                { "Template", new UIData { Name = "Template", Options = templates, OptionsText = "Text", OptionsValue = "Value"} },
                {"IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() } }
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
            string TemplatesPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.CurrentTheme.Name + "/blocks/register link/Templates");
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
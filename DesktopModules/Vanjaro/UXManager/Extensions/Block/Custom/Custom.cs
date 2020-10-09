using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Shortcut;

namespace Vanjaro.UXManager.Extensions.Block.Custom
{
    public class Custom : Core.Entities.Interface.IBlock, IShortcut
    {
        public string Category => "Custom";

        public string Name => "Custom";

        public string DisplayName => Name;

        public string Icon => "fas fa-bars";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);
        public bool Visible { get; set; } = true;

        public string LocalResourcesFile => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/App_LocalResources/Shared.resx";

        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                return result;
            }
        }

        public Core.Entities.Menu.ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
        {
            Core.Entities.Menu.ThemeTemplateResponse response = new Core.Entities.Menu.ThemeTemplateResponse();
            string blockguid = Attributes["data-guid"];
            Core.Data.Entities.CustomBlock block = Core.Managers.BlockManager.GetAll(PortalSettings.Current).Where(c => c.Guid.ToLower() == blockguid.ToLower()).FirstOrDefault();
            if (block != null)
            {
                response.Markup = block.Html;
                response.Style = block.Css;
            }
            return response;
        }


        #region IShortcut       
        public ShortcutItem Shortcut
        {
            get
            {
                Dictionary<string, string> Attr = new Dictionary<string, string>
                {
                    { "class", "add-custom-block" }
                };
                return new ShortcutItem()
                {
                    Text = Localization.Get("Shortcut_Text", "Text", LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                    ViewOrder = 100,
                    URL = ExtensionInfo.GUID,
                    Title = Localization.Get("Shortcut_Title", "Text", LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                    Breakline = false,
                    Attributes = Attr,
                    Visibility = GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin")
                };
            }

        }
        private static string GetAccessRoles(UserInfo UserInfo)
        {
            List<string> AccessRoles = new List<string>();
            if (UserInfo.UserID > 0)
            {
                AccessRoles.Add("user");
            }
            else
            {
                AccessRoles.Add("anonymous");
            }

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
            }

            if (UserInfo.IsSuperUser)
            {
                AccessRoles.Add("host");
            }

            if (TabPermissionController.HasTabPermission("EDIT"))
            {
                AccessRoles.Add("edit");
            }

            return string.Join(",", AccessRoles);
        }
        #endregion
    }
}
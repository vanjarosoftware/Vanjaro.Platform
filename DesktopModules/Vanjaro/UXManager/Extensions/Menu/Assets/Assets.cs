using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Menu.Assets.Components;
using Vanjaro.UXManager.Extensions.Menu.Assets.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using Vanjaro.UXManager.Library.Entities.Shortcut;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Assets
{
    public class Assets : IMenuItem, IShortcut
    {

        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get(ExtensionInfo.Name, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),

                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fa fa-folder",
                    ViewOrder = 500,
                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Site", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Hierarchy = null,
                        Icon = "fa fa-file",
                        ViewOrder = 0
                    }
                });
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public int? Width => 900;

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);


        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    AngularPlugins.Grid.ToString(),
                    JavaScriptPlugins.ValidationJS.ToString(),
                    AngularPlugins.FileUpload.ToString(),
                    JavaScriptPlugins.ContextMenu.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    "Bootstrap"
                                                        };

        public string Icon => "fa fa-cog";

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin");

        public MenuAction Event => MenuAction.RightOverlay;

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        #region IShortcut       
        public ShortcutItem Shortcut => new ShortcutItem()
        {
            Text = Localization.Get("Shortcut_Text", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
            ViewOrder = 80,
            URL = ExtensionInfo.GUID + "#!/detail",
            Title = Localization.Get("Shortcut_Title", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
            Breakline = false,
            Width = Width,
            Action = Event,
            Icon = "fa fa-folder",
            Visibility = Visibility
        };
        #endregion

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
    }
}
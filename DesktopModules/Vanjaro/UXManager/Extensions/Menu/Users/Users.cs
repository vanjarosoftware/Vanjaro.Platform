using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Users.Components;
using Vanjaro.UXManager.Extensions.Menu.Users.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using Vanjaro.UXManager.Library.Entities.Shortcut;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Users
{
    public class Users : IMenuItem, IShortcut
    {
        #region IMenu Item Settings
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {

                    Text = Localization.Get(ExtensionInfo.FriendlyName, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fa fa-users",
                    ViewOrder = 300,

                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Site", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Hierarchy = null,
                        ViewOrder = 0,
                        BelowBreakLine = true

                    }
                });
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);


        public int? Width => 450;

        public string Icon => "fa fa-cog";

        public bool Visibility => Factories.AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin");

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        #endregion

        #region Extension Settings

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.ValidationJS.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    AngularPlugins.FileUpload.ToString(),
                    AngularPlugins.Grid.ToString(),
                    AngularPlugins.InlineEditor.ToString(),
                    JavaScriptPlugins.BootstrapDatepicker.ToString(),
                    AngularPlugins.CKEditor.ToString(),
                    "Bootstrap"
                };



        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }

        public MenuAction Event => MenuAction.Inline;
        #endregion

        #region IShortcut         
        public ShortcutItem Shortcut => new ShortcutItem()
        {
            Text = Localization.Get("Shortcut_Text", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
            ViewOrder = 40,
            URL = ExtensionInfo.GUID + "#adduser",
            Title = Localization.Get("Shortcut_Title", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
            Breakline = false,
            Width = Width,
            Action = Event,
            Icon = "",
            Visibility = Visibility
        };
        #endregion
    }
}
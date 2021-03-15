using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Pages.Components;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using Vanjaro.UXManager.Library.Entities.Shortcut;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Pages
{
    public class Pages : IMenuItem, IShortcut
    {
        #region IMenu Setting
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get(ExtensionInfo.FriendlyName, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),

                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fa fa-file",
                    ViewOrder = 200,

                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Site", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Hierarchy = null,
                        Icon = "fa fa-file",
                        ViewOrder = 0,
                        BelowBreakLine = true
                    }
                });
                return ListItems;
            }
        }


        public int SortOrder => 20;

        public string Icon => "fa fa-cog";

        public bool Visibility => Factories.AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("edit");

        public MenuAction Event => MenuAction.Inline;

        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public int? Width
        {
            get;
        }

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        #endregion

        #region Basic Extension Setting

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.ValidationJS.ToString(),
                    AngularPlugins.FileUpload.ToString(),
                    AngularPlugins.Grid.ToString(),
                    Frameworks.FontAwesome.ToString(),
                    AngularPlugins.TreeView.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    JavaScriptPlugins.BootstrapDatepicker.ToString(),
                    "Bootstrap"
                };

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }
        #endregion

        #region IShortcut       
        public ShortcutItem Shortcut => new ShortcutItem()
        {
            Text = Localization.Get("Shortcut_Text", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
            ViewOrder = 20,
            URL = ExtensionInfo.GUID + "#detail",
            Title = Localization.Get("Shortcut_Title", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
            Breakline = false,
            Width = Width,
            Action = Event,
            Icon = "fa fa-file",
            Visibility = Factories.AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin")
        };
        #endregion
    }
}
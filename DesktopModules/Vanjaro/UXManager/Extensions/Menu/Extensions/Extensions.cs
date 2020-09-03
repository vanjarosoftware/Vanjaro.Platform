using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Extensions.Components;
using Vanjaro.UXManager.Extensions.Menu.Extensions.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions
{
    public class Extensions : IMenuItem
    {
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get("Extensions", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.LocalMissingPrefix),
                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fas fa-puzzle-piece",
                    ViewOrder = 800,
                    BelowBreakLine = true,
                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Tools", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Icon = "fas fa-wrench",
                    }
                });
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public int? Width
        {
            get;
        }

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    AngularPlugins.FileUpload.ToString(),
                    AngularPlugins.Grid.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    "Bootstrap"
                };


        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string Icon => "fa fa-cog";

        public bool Visibility => IsAuthorized();

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        public MenuAction Event => MenuAction.Inline;

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
        #region Public or Private Method

        public bool IsAuthorized()
        {
            return AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin");
        }
        #endregion

    }
}
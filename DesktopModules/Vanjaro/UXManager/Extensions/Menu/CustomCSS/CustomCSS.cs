using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.CustomCSS.Components;
using Vanjaro.UXManager.Extensions.Menu.CustomCSS.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.CustomCSS
{
    public class CustomCSS : IMenuItem
    {
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get("CustomCSS", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fas fa-paint-brush",
                    ViewOrder = 600,
                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Design", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Icon = "fas fa-palette",
                        ViewOrder = 500,
                        Hierarchy = null,
                        BelowBreakLine = true
                    }
                });
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";
        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.CodeMirror.ToString()
               };


        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string Icon => "fa fa-cog";

        public bool Visibility => IsAuthorized();

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        public int SortOrder => 1000;

        public int? Width => 900;
        public MenuAction Event => MenuAction.RightOverlay;
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
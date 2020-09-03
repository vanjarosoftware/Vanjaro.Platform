using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder
{
    public class ThemeBuilder : IAppExtension
    {
        public AppExtension Item => new AppExtension { Text = ThemeBuilderInfo.FriendlyName, Command = "tlb-app-setting", Class = "fa fa-cog", ItemGuid = SettingGuid, Width = Width };

        public Guid SettingGuid => Guid.Parse(ThemeBuilderInfo.GUID);

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ThemeBuilderInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ThemeBuilderInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ThemeBuilderInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ThemeBuilderInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    Frameworks.jQueryUI.ToString(),
                    AngularPlugins.Grid.ToString(),
                    JavaScriptPlugins.ValidationJS.ToString(),
                    JavaScriptPlugins.CodeMirror.ToString(),
                    "Bootstrap"
                   };

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string Icon => "fa fa-cog";

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Components.Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin");

        public int SortOrder => 1000;

        public int Width
        {
            get;
        }

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
    }
}
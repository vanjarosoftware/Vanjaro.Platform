using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.UXManager.Extensions.Apps.LogsSettings.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Apps.LogsSettings
{
    public class LogSetting : IAppExtension
    {
        public AppExtension Item => new AppExtension { Text = ExtensionInfo.FriendlyName, Command = "tlb-app-setting", Class = "fa fa-cog", ItemGuid = SettingGuid, Width = Width };

        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => string.Empty;

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    AngularPlugins.Grid.ToString(),
                    JavaScriptPlugins.ValidationJS.ToString(),
                    "Bootstrap"
                };

        public int SortOrder => 160;

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }


        public string Icon => "fa fa-cog";
        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("user");
        public int Width
        {
            get;
        }
    }
}
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.Core.Extensions.Notification.Notification.Factories;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.Core.Extensions.Notification.Notification
{
    public class Notification : ICoreExtension
    {
        public Guid SettingGuid => Guid.Parse(NotificationInfo.GUID);

        public int Width
        {
            get;
        }

        public string UIPath => "~/DesktopModules/Vanjaro/Core/Extensions/Notification/" + NotificationInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/Core/Extensions/Notification/" + NotificationInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => string.Empty;

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/Core/Extensions/Notification/" + NotificationInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.ValidationJS.ToString(),
                    AngularPlugins.Grid.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    "Bootstrap"
                };

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("setting");

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
    }
}
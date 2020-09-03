using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Apps.Video
{
    public class Video : IAppExtension
    {
        public AppExtension Item => new AppExtension { Text = VideoInfo.FriendlyName, Command = "tlb-app-setting", Class = "fa fa-cog", ItemGuid = SettingGuid, Width = Width };

        public int Width => 900;

        public string Icon => "fa fa-cog";

        public bool Visibility => Factories.AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("editpage");

        public int SortOrder => 1;

        public Guid SettingGuid => Guid.Parse(VideoInfo.GUID);

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + VideoInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + VideoInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + VideoInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + VideoInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    AngularPlugins.FileUpload.ToString(),
                    AngularPlugins.Grid.ToString(),
                    "Bootstrap"
                };

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }
    }
}
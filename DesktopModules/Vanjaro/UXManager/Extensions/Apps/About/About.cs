using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.UXManager.Extensions.Apps.About.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Apps.About
{
    public class About : IAppExtension
    {
        public AppExtension Item => new AppExtension { Text = AboutInfo.FriendlyName, Command = "tlb-app-setting", Class = "fa fa-cog", ItemGuid = SettingGuid, Width = Width };

        public int Width => 550;

        public string Icon => "fa fa-cog";

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("user");

        public int SortOrder => 1;

        public Guid SettingGuid => Guid.Parse(AboutInfo.GUID);

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + AboutInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + AboutInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + AboutInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Apps/" + AboutInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    "Bootstrap" };

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
    }
}
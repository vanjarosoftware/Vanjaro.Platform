using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Toolbar.VersionManagement
{
    public class VersionManagement : IToolbarItem
    {
        public ToolbarItem Item => new ToolbarItem
        {

            Text = Localization.Get(ExtensionInfo.Name, "Text", Components.Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix)
        };

        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public int? Width
        {
            get;
        }

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    "Bootstrap"
                };

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public int SortOrder => 60;

        public string Icon => "fa fa-history";

        public bool Visibility => TabPermissionController.CanManagePage(PortalSettings.Current.ActiveTab);

        public Dictionary<MenuAction, dynamic> ToolbarAction => new Dictionary<MenuAction, dynamic>();

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }
    }
}
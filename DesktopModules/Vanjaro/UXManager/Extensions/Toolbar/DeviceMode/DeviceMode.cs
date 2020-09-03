
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Toolbar.DeviceMode
{
    public class DeviceMode : IToolbarItem
    {
        public ToolbarItem Item => new ToolbarItem
        {

            Text = Localization.Get(ExtensionInfo.Name, "Text", Components.Constants.LocalResourcesFile, false, Localization.SharedMissingPrefix)
        };

        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);


        public int? Width
        {
            get;
        }

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "//Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap"
                };

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public int SortOrder => 90;

        public string Icon => "fa fa-desktop";

        public bool Visibility => true;

        public Dictionary<MenuAction, dynamic> ToolbarAction
        {
            get
            {
                Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>
                {
                    //Event.Add(MenuAction.OpenInNewWindow, "_blank");
                    { MenuAction.onClick, "$(\"#LanguageManager\").hide();$(\".ntoolbox\").hide(); $(\"#DeviceManager\").slideToggle(100); event.stopPropagation();" }
                };
                return Event;
            }
        }

        public bool ChangeViewMode => false;

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }


    }
}
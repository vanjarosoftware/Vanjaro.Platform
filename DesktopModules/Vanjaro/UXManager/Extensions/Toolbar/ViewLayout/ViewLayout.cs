
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Toolbar.ViewLayout
{
    public class ViewLayout : IToolbarItem
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
        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "//Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap"
                };

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public int SortOrder => 180;

        public string Icon => "fas fa-border-none";

        public bool Visibility => true;

        public Dictionary<MenuAction, dynamic> ToolbarAction
        {
            get
            {
                Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>
                {
                    //Event.Add(MenuAction.OpenInNewWindow, "_blank");
                    { MenuAction.onClick, "if($(this).hasClass(\"active\")){VjEditor.stopCommand(\"core:component-outline\");}else{VjEditor.runCommand(\"core:component-outline\");} $(this).toggleClass(\"active\");" }
                };
                return Event;
            }
        }

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }


    }
}
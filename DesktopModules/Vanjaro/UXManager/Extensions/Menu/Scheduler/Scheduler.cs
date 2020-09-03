using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Components;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler
{
    public class Scheduler : IMenuItem
    {
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get(ExtensionInfo.Name, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),

                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fa fa-folder",
                    ViewOrder = 500,
                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Tools", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Hierarchy = null,
                        Icon = "fa fa-file",
                        ViewOrder = 0,
                    }
                });
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public int? Width => 900;

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);


        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    Frameworks.jQueryUI.ToString(),
                    AngularPlugins.Grid.ToString(),
                    JavaScriptPlugins.ValidationJS.ToString(),
                    AngularPlugins.FileUpload.ToString(),
                    JavaScriptPlugins.ContextMenu.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    JavaScriptPlugins.BootstrapDatepicker.ToString(),
                    "Bootstrap"
                   };

        public string Icon => "fa fa-cog";

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("host");

        public MenuAction Event => MenuAction.RightOverlay;

        //public Dictionary<MenuAction, dynamic> Event
        //{
        //    get
        //    {
        //        Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>();
        //        //Event.Add(MenuAction.OpenInNewWindow, "_blank");
        //        Event.Add(MenuAction.onClick, "OpenPopUp(event, " + this.Width + ",\"right\",\"" + ExtensionInfo.Name + "\", \"" + ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + ExtensionInfo.GUID + "\")");
        //        return Event;
        //    }
        //}

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }

    }
}
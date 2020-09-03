using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Privacy.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Privacy
{
    public class Privacy : IMenuItem
    {
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Text = Localization.Get(ExtensionInfo.Name, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),

                        ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                        Icon = "fas fa-shield-alt",
                        ViewOrder = 800,
                        Hierarchy = new MenuItem
                        {
                            Text = Localization.Get("Settings", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                            Icon = "fa fa-file",
                        }
                    }
                };
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";
        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.ValidationJS.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    AngularPlugins.Grid.ToString(),
                    "Bootstrap"
               };


        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string Icon => "fa fa-cog";

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("host");

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        public MenuAction Event =>
                //Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>();
                //Event.Add(MenuAction.onClick, "OpenPopUp(event, " + this.Width + ",\"right\",\"" + Localization.Get("Title_" + ExtensionInfo.Name, "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix) + "\", \"" + ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + ExtensionInfo.GUID + "\")");
                MenuAction.RightOverlay;

        public int SortOrder => 1000;

        public int? Width => 800;

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }
    }
}
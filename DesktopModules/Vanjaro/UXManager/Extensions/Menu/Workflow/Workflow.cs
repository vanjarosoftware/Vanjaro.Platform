using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Workflow.Components;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.UXManager.Extensions.Menu.Workflow
{
    public class Workflow : IMenuItem
    {
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get("LiveEditing", "Text", Constants.LocalResourcesFile, Library.Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                    ItemGuid = Guid.Parse(WorkflowInfo.GUID),
                    Icon = "fas fa-edit",
                    ViewOrder = 300,
                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Settings", "Text", Constants.LocalResourcesFile, Library.Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Icon = "fas fa-cog",
                    }
                });
                return ListItems;
            }
        }


        public Guid SettingGuid => Guid.Parse(WorkflowInfo.GUID);

        public int? Width => 800;

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + WorkflowInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + WorkflowInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => string.Empty;

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + WorkflowInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.ValidationJS.ToString(),
                    Frameworks.jQueryUI.ToString(),
                    AngularPlugins.Grid.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    "Bootstrap"
                };

        public bool Visibility => Factories.AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin");

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        public int SortOrder => 160;

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public MenuAction Event => MenuAction.RightOverlay;

        //public Dictionary<MenuAction, dynamic> Event
        //{
        //    get
        //    {
        //        Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>();
        //        Event.Add(MenuAction.onClick, "OpenPopUp(event, \"900\",\"right\",\"" + WorkflowInfo.FriendlyName + "\", \"" + ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + WorkflowInfo.GUID + "\")");
        //        return Event;
        //    }

        //}

        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }
    }
}
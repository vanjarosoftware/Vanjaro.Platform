using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Components;
using Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Factories;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics
{
    public class GoogleAnalytics : IMenuItem
    {
        public List<MenuItem> Items
        {
            get
            {
                List<MenuItem> ListItems = new List<MenuItem>();
                ListItems.Add(new MenuItem
                {
                    Text = Localization.Get("GoogleAnalytics", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                    ItemGuid = Guid.Parse(ExtensionInfo.GUID),
                    Icon = "fas fa-plug",
                    ViewOrder = 600,
                    Hierarchy = new MenuItem
                    {
                        Text = Localization.Get("Settings", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                        Icon = "fas fa-cog",
                        Hierarchy = new MenuItem
                        {
                            Text = Localization.Get("Integration", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix),
                            Icon = "fas fa-plug"
                        }
                    }
                });
                return ListItems;
            }
        }



        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/UIEngine/AngularBootstrap";
        public string[] Dependencies => new string[] {
                    "Bootstrap"
               };


        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string Icon => "fa fa-cog";

        public bool Visibility => IsAuthorized();

        public string SearchKeywords => Localization.Get("SearchKeywords", "Text", Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix);

        public int SortOrder => 1000;

        public int? Width => 700;
        public MenuAction Event => MenuAction.RightOverlay;
        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }

        #region Public or Private Method

        public bool IsAuthorized()
        {
            return AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("admin");
        }
        #endregion

    }
}
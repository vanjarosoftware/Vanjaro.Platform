
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Web;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Toolbar.Preview
{
    public class Preview : IToolbarItem
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

        public string AppCssPath => "/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
        public string AppJsPath => "/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/" + ExtensionInfo.Name + "//Resources/Scripts/app.js";

        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap"
                };

        public AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => Factories.AppFactory.GetViews();

        public int SortOrder => 120;

        public string Icon => "far fa-eye";

        public bool Visibility => true;

        public Dictionary<MenuAction, dynamic> ToolbarAction
        {
            get
            {

                string Slug = HttpContext.Current.Request.QueryString[null];
                if (!string.IsNullOrEmpty(Slug))
                    Slug = "&" + Slug;
                else
                    Slug = string.Empty;

                Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>
                {
                    //Event.Add(MenuAction.OpenInNewWindow, "_blank");
                    { MenuAction.onClick, "window.open(CurrentTabUrl.replace(\""+Slug.Replace("&","/")+"\", \"\").replace(\""+Slug+"\", \"\") + (CurrentTabUrl.indexOf(\"?\")!=-1?\"&icp=true&pv=yes"+Slug+"\":\"?icp=true&pv=yes"+Slug+"\"), \"_blank\");" }
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
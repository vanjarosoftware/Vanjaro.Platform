using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Extensions.Toolbar.Undo
{
    public class Undo : IToolbarItem
    {
        public ToolbarItem Item => new ToolbarItem
        {

            Text = Localization.Get(ExtensionInfo.Name, "Text", Components.Constants.LocalResourcesFile, Extension.ShowMissingKeysStatic, Localization.SharedMissingPrefix)
        };

        public string Icon => "fas fa-undo";

        public bool Visibility => Factories.AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("edit");

        public int SortOrder => 140;
        public Guid SettingGuid => Guid.Parse(ExtensionInfo.GUID);

        public int? Width
        {
            get;
        }

        public Dictionary<MenuAction, dynamic> ToolbarAction
        {
            get
            {
                Dictionary<MenuAction, dynamic> Event = new Dictionary<MenuAction, dynamic>
                {
                    { MenuAction.onClick, "VjEditor.UndoManager.undo();" }
                };
                return Event;
            }
        }

        public Vanjaro.Common.Entities.Apps.AppInformation App => Factories.AppFactory.GetAppInformation();

        public List<Vanjaro.Common.Engines.UIEngine.AngularBootstrap.AngularView> AngularViews => null;
        public string UIPath => string.Empty;

        public string AppCssPath => string.Empty;

        public string AppJsPath => string.Empty;
        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap"
                };
        public string AccessRoles(UserInfo userInfo)
        {
            return Factories.AppFactory.GetAccessRoles(userInfo);
        }
    }
}
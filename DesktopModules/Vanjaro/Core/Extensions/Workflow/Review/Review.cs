using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Components;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.Core.Extensions.Workflow.Review.Components;
using Vanjaro.Core.Extensions.Workflow.Review.Factories;
using static Vanjaro.Common.FrameworkManager;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Workflow.Review
{
    public class Review : ICoreExtension, INotificationTask
    {
        public Guid SettingGuid => Guid.Parse(ReviewInfo.GUID);

        public int Width
        {
            get;
        }

        public string UIPath => "~/DesktopModules/Vanjaro/Core/Extensions/Workflow/" + ReviewInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/Core/Extensions/Workflow/" + ReviewInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => string.Empty;

        public string UIEngineAngularBootstrapPath => "~/DesktopModules/Vanjaro/Core/Extensions/Workflow/" + ReviewInfo.Name + "/Resources/UIEngine/AngularBootstrap";

        public string[] Dependencies => new string[] {
                    JavaScriptPlugins.ValidationJS.ToString(),
                    Frameworks.jQueryUI.ToString(),
                    AngularPlugins.Grid.ToString(),
                    AngularPlugins.AutoComplete.ToString(),
                    "Bootstrap"
                };

        public bool Visibility => AppFactory.GetAccessRoles(UserController.Instance.GetCurrentUserInfo()).Contains("review");

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public NotificationItem Hierarchy => new NotificationItem
        {
            NotificationName = DotNetNuke.Services.Localization.Localization.GetString("ReviewContent", Core.Components.Constants.LocalResourcesFile),
            NotificationCount = WorkflowManager.GetPagesbyUserID(PortalSettings.Current.PortalId, PortalSettings.Current.UserId).Count,
            URL = ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=33d8efed-0f1d-471e-80a4-6a7f10e87a42") + "#!/review/" + Core.Components.Enum.WorkflowType.Page.ToString().ToLower()
        };

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
    }
}
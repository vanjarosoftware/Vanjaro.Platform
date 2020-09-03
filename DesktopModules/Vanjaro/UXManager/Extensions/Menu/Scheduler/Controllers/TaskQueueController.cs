using Dnn.PersonaBar.TaskScheduler.Components;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Factories;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class TaskQueueController : UIEngineController
    {
        private static readonly TaskSchedulerController _controller = new TaskSchedulerController();
        internal static List<IUIData> GetData(int PortalID, UserInfo userinfo)
        {
            Dictionary<string, IUIData> Setting = new Dictionary<string, IUIData>
            {
                { "ScheduleStatus", new UIData { Name = "ScheduleStatus", Options = SchedulerFactory.GetScheduleStatus() } },
                { "LoadingImage", new UIData { Name = "LoadingImage", Value = LoadingImage } }
            };
            return Setting.Values.ToList();
        }

        [HttpDelete]
        public ActionResult StopSchedule()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                _controller.StopSchedule();
                actionResult.Message = Localization.GetString("SchedulerStopSuccess", Components.Constants.TaskSchedulerResourcesFile);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                actionResult.AddError("SchedulerStopError", Localization.GetString("SchedulerStopError", Components.Constants.TaskSchedulerResourcesFile));
            }
            return actionResult;
        }

        [HttpPost]
        public ActionResult StartSchedule()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                SchedulingProvider.Instance().StartAndWaitForResponse();
                actionResult.Message = Localization.GetString("SchedulerStartSuccess", Components.Constants.TaskSchedulerResourcesFile);
            }
            catch (Exception exc)
            {
                actionResult.AddError("SchedulerStartError", Localization.GetString("SchedulerStartError", Components.Constants.TaskSchedulerResourcesFile));
                Exceptions.LogException(exc);
            }
            return actionResult;
        }

        [HttpPost]
        public ActionResult UpdateSchedulerSettings(dynamic request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                SchedulerMode originalSchedulerMode = (SchedulerMode)Convert.ToInt32(HostController.Instance.GetString("SchedulerMode"));
                Enum.TryParse(request.SchedulerMode.Value.ToString(), true, out SchedulerMode newSchedulerMode);
                if (originalSchedulerMode != newSchedulerMode)
                {
                    switch (newSchedulerMode)
                    {
                        case SchedulerMode.DISABLED:
                            Thread newThread1 = new Thread(new ThreadStart(Halt)) { IsBackground = true };
                            newThread1.Start();
                            break;
                        case SchedulerMode.TIMER_METHOD:
                            Thread newThread2 = new Thread(SchedulingProvider.Instance().Start) { IsBackground = true };
                            newThread2.Start();
                            break;
                        default:
                            Thread newThread3 = new Thread(new ThreadStart(Halt)) { IsBackground = true };
                            newThread3.Start();
                            break;
                    }
                }

                HostController.Instance.Update("SchedulerMode", request.SchedulerMode.Value.ToString(), false);
                HostController.Instance.Update("SchedulerdelayAtAppStart", request.SchedulerdelayAtAppStart.Value.ToString());

                switch (newSchedulerMode)
                {
                    case SchedulerMode.DISABLED:
                        actionResult.Message = Localization.GetString("DisabledSuccess", Components.Constants.LocalResourcesFile);
                        break;
                    case SchedulerMode.TIMER_METHOD:
                        actionResult.Message = Localization.GetString("EnabledSuccess", Components.Constants.LocalResourcesFile);
                        break;
                    default:
                        actionResult.Message = Localization.GetString("EnabledSuccess", Components.Constants.LocalResourcesFile);
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                actionResult.AddError("SchedulerUpdateError", Localization.GetString("SchedulerUpdateError", Components.Constants.TaskSchedulerResourcesFile));
            }
            return actionResult;
        }

        private static void Halt()
        {
            SchedulingProvider.Instance().Halt("Host Settings");
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
        private static string LoadingImage => DotNetNuke.Common.Globals.ApplicationPath + "/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Image/Loading.gif";
    }
}
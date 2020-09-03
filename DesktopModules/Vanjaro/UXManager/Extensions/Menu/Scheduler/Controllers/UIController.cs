using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            switch ((Factories.AppFactory.Identifier)Enum.Parse(typeof(Factories.AppFactory.Identifier), Identifier))
            {
                case Factories.AppFactory.Identifier.setting_scheduler:
                    return SchedulerController.GetData(PortalSettings.PortalId, UserInfo);
                case Factories.AppFactory.Identifier.setting_taskqueue:
                    return TaskQueueController.GetData(PortalSettings.PortalId, UserInfo);
                case Factories.AppFactory.Identifier.setting_history:
                    return HistoryController.GetData(PortalSettings.PortalId, UserInfo);
                case Factories.AppFactory.Identifier.setting_settings:
                    return HistoryController.GetData(PortalSettings.PortalId, UserInfo);
                default:
                    break;
            }
            return base.GetData(Identifier, Parameters);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
        public override string AllowedAccessRoles(string Identifier)
        {
            return Factories.AppFactory.GetAllowedRoles(Identifier);
        }
    }
}
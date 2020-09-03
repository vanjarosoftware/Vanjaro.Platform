using Dnn.PersonaBar.TaskScheduler.Components;
using Dnn.PersonaBar.TaskScheduler.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Factories;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class SchedulerController : UIEngineController
    {
        private static readonly TaskSchedulerController _controller = new TaskSchedulerController();

        internal static List<IUIData> GetData(int PortalID, UserInfo userinfo)
        {
            Dictionary<string, IUIData> Setting = new Dictionary<string, IUIData>
            {
                { "FriendlyName", new UIData { Name = "FriendlyName", Value = "" } },
                { "ObjectDependencies", new UIData { Name = "ObjectDependencies", Value = "" } },
                { "ScheduleStatus", new UIData { Name = "ScheduleStatus", Options = SchedulerFactory.GetScheduleStatus() } },
                { "GetScheduleItems", new UIData { Name = "GetScheduleItems", Options = SchedulerManager.GetScheduleItems() } },
                { "FrequencyPeriod", new UIData { Name = "FrequencyPeriod", Options = SchedulerManager.TimePeriod(), OptionsText = "Text", OptionsValue = "Value", Value = "s" } },
                { "RunTimeLapsePeriod", new UIData { Name = "RunTimeLapsePeriod", Options = SchedulerManager.TimePeriod(), OptionsText = "Text", OptionsValue = "Value", Value = "s" } },
                { "CatchUpTasks", new UIData { Name = "CatchUpTasks", Options = SchedulerManager.CatchUpTask(), OptionsText = "Text", OptionsValue = "Value", Value = "false" } },
                { "RunOnEvent", new UIData { Name = "RunOnEvent", Options = SchedulerManager.RunOnEvent(), OptionsText = "Text", OptionsValue = "Value", Value = "0" } },
                { "RetainScheduleHistory", new UIData { Name = "RetainScheduleHistory", Options = SchedulerManager.RetainScheduleHistory(), OptionsText = "Text", OptionsValue = "Value", Value = "0" } },
                { "EnabledScheduling", new UIData { Name = "EnabledScheduling", Value = "false" } },
                { "ScheduleStartDate", new UIData { Name = "ScheduleStartDate", Value = "" } },
                { "RetryTimeLapse", new UIData { Name = "RetryTimeLapse", Value = "" } },
                { "Server", new UIData { Name = "Server", Value = "" } },
                { "Frequency", new UIData { Name = "Frequency", Value = "" } },
                { "ClassNameAssembly", new UIData { Name = "ClassNameAssembly", Value = "" } }
            };
            return Setting.Values.ToList();
        }

        [HttpGet]
        public ActionResult GetScheduleStatus()
        {
            ActionResult ActionResult = new ActionResult
            {
                Data = SchedulerFactory.GetScheduleStatus()
            };
            return ActionResult;
        }

        [HttpPost]
        public ActionResult RunSchedule(ScheduleDto scheduleDto)
        {
            ActionResult ActionResult = new ActionResult();
            string Data = SchedulerFactory.Run_Schedule(scheduleDto);
            if (Data == "Success")
            {
                ActionResult.Message = Localization.GetString("RunNow", Components.Constants.TaskSchedulerResourcesFile);
            }
            else
            {
                ActionResult.AddError("RunNowError", Localization.GetString("RunNowError", Components.Constants.TaskSchedulerResourcesFile));
            }

            return ActionResult;

        }

        [HttpPost]
        public ActionResult DirectRunSchedule(int ScheduleID)
        {
            ActionResult ActionResult = new ActionResult();
            //ScheduleDto scheduleDto = GetSchedule_Item(ScheduleID).Data as ScheduleDto;
            dynamic m = SchedulerManager.GetSchedule_Item(ScheduleID).Data;
            ScheduleDto scheduleDto = new ScheduleDto
            {
                ScheduleID = m.ScheduleID,
                TypeFullName = m.TypeFullName,
                FriendlyName = m.FriendlyName,
                TimeLapse = m.TimeLapse,
                TimeLapseMeasurement = m.TimeLapseMeasurement,
                RetryTimeLapse = m.RetryTimeLapse,
                RetryTimeLapseMeasurement = m.RetryTimeLapseMeasurement,
                RetainHistoryNum = m.RetainHistoryNum,
                AttachToEvent = m.AttachToEvent,
                CatchUpEnabled = m.CatchUpEnabled,
                Enabled = m.Enabled,
                ObjectDependencies = m.ObjectDependencies,
                ScheduleStartDate = m.ScheduleStartDate,
                Servers = m.Servers
            };

            string Data = SchedulerFactory.Run_Schedule(scheduleDto);
            if (Data == "Success")
            {
                ActionResult.Message = Localization.GetString("RunNow", Components.Constants.TaskSchedulerResourcesFile);
            }
            else
            {
                ActionResult.AddError("RunNowError", Localization.GetString("RunNowError", Components.Constants.TaskSchedulerResourcesFile));
            }

            return ActionResult;
        }

        [HttpGet]
        public ActionResult GetScheduleItem(int scheduleId)
        {
            ActionResult ActionResult = new ActionResult();
            dynamic Data = SchedulerManager.GetSchedule_Item(scheduleId);
            if (Data.Status == "Success")
            {
                ActionResult.Data = Data;
            }
            else
            {
                ActionResult.Message = Data;
            }

            return ActionResult;
        }

        [HttpPost]
        public ActionResult CreateScheduleItem(ScheduleDto scheduleDto)
        {
            dynamic Result = new ExpandoObject();
            ActionResult ActionResult = new ActionResult();
            try
            {
                if (scheduleDto.RetryTimeLapse == 0)
                {
                    scheduleDto.RetryTimeLapse = Null.NullInteger;
                }

                if (!SchedulerManager.VerifyValidTimeLapseRetry(scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement, scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement))
                {
                    ActionResult.AddError("InvalidFrequencyAndRetry", Localization.GetString("InvalidFrequencyAndRetry", Components.Constants.TaskSchedulerResourcesFile));
                }
                else
                {
                    ScheduleItem scheduleItem = _controller.CreateScheduleItem(scheduleDto.TypeFullName, scheduleDto.FriendlyName, scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement,
                                       scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement, scheduleDto.RetainHistoryNum, scheduleDto.AttachToEvent, scheduleDto.CatchUpEnabled,
                                       scheduleDto.Enabled, scheduleDto.ObjectDependencies, scheduleDto.ScheduleStartDate, scheduleDto.Servers);
                    SchedulingProvider.Instance().AddSchedule(scheduleItem);
                    Result.ScheduleItems = SchedulerManager.GetScheduleItems(0, 14);
                    Result.Status = "Success";
                    ActionResult.Data = Result;
                    ActionResult.Message = Localization.GetString("ScheduleItemCreateSuccess", Components.Constants.TaskSchedulerResourcesFile);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                Result.Status = exc.Message.ToString();
                ActionResult.AddError("ScheduleItemCreateError", Localization.GetString("ScheduleItemCreateError", Components.Constants.TaskSchedulerResourcesFile));
            }
            return ActionResult;

        }

        [HttpPost]
        public ActionResult UpdateScheduleItem(ScheduleDto scheduleDto)
        {
            ActionResult ActionResult = new ActionResult();
            dynamic Result = new ExpandoObject();
            try
            {
                if (scheduleDto.RetryTimeLapse == 0)
                {
                    scheduleDto.RetryTimeLapse = Null.NullInteger;
                }

                if (!SchedulerManager.VerifyValidTimeLapseRetry(scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement, scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement))
                {
                    ActionResult.AddError("InvalidFrequencyAndRetry", Localization.GetString("InvalidFrequencyAndRetry", Components.Constants.TaskSchedulerResourcesFile));
                }
                else
                {
                    ScheduleItem existingItem = SchedulingProvider.Instance().GetSchedule(scheduleDto.ScheduleID);

                    ScheduleItem updatedItem = _controller.CreateScheduleItem(scheduleDto.TypeFullName, scheduleDto.FriendlyName, scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement,
                                      scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement, scheduleDto.RetainHistoryNum, scheduleDto.AttachToEvent, scheduleDto.CatchUpEnabled,
                                      scheduleDto.Enabled, scheduleDto.ObjectDependencies, scheduleDto.ScheduleStartDate, scheduleDto.Servers);
                    updatedItem.ScheduleID = scheduleDto.ScheduleID;


                    if (updatedItem.ScheduleStartDate != existingItem.ScheduleStartDate ||
                        updatedItem.Enabled ||
                        updatedItem.Enabled != existingItem.Enabled ||
                        updatedItem.TimeLapse != existingItem.TimeLapse ||
                        updatedItem.RetryTimeLapse != existingItem.RetryTimeLapse ||
                        updatedItem.RetryTimeLapseMeasurement != existingItem.RetryTimeLapseMeasurement ||
                        updatedItem.TimeLapseMeasurement != existingItem.TimeLapseMeasurement)
                    {
                        SchedulingProvider.Instance().UpdateSchedule(updatedItem);
                        ActionResult.Message = Localization.GetString("ScheduleItemUpdateSuccess", Components.Constants.TaskSchedulerResourcesFile);
                    }
                    else
                    {
                        ActionResult.Message = Localization.GetString("ScheduleItemUpdateSuccess", Components.Constants.TaskSchedulerResourcesFile);
                        SchedulingProvider.Instance().UpdateScheduleWithoutExecution(updatedItem);
                    }
                    Result.Status = "Success";
                    Result.ScheduleItems = SchedulerManager.GetScheduleItems(0, 14);
                    ActionResult.Data = Result;
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                Result.Status = exc.Message.ToString();
                ActionResult.AddError("ScheduleItemUpdateError", Localization.GetString("ScheduleItemUpdateError", Components.Constants.TaskSchedulerResourcesFile));
            }
            return ActionResult;
        }

        [HttpPost]
        public ActionResult DeleteSchedule(int ScheduleID)
        {
            ActionResult ActionResult = new ActionResult();
            dynamic Result = new ExpandoObject();
            try
            {
                ScheduleItem objScheduleItem = new ScheduleItem { ScheduleID = ScheduleID };
                SchedulingProvider.Instance().DeleteSchedule(objScheduleItem);
                Result.Status = "Success";
                Result.ScheduleItems = SchedulerManager.GetScheduleItems(0, 14);
                ActionResult.Data = Result;
                ActionResult.Message = Localization.GetString("DeleteSuccess", Components.Constants.TaskSchedulerResourcesFile);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                Result.Status = exc.Message.ToString();
                ActionResult.AddError("DeleteError", Localization.GetString("DeleteError", Components.Constants.TaskSchedulerResourcesFile));
            }
            return ActionResult;
        }

        [HttpPost]
        public ActionResult GetSchedulerItembyPageing(dynamic Data)
        {
            ActionResult ActionResult = new ActionResult();
            dynamic Result = new ExpandoObject();
            try
            {
                Result.Status = "Success";
                Result.ScheduleItems = SchedulerManager.GetScheduleItems(int.Parse(Data.Skip.Value.ToString()), int.Parse(Data.Take.Value.ToString()));
                ActionResult.Data = Result;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                Result.Status = exc.Message.ToString();
            }
            return ActionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }

}
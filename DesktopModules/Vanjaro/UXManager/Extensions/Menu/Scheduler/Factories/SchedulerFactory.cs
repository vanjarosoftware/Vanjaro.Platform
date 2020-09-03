using Dnn.PersonaBar.TaskScheduler.Components;
using Dnn.PersonaBar.TaskScheduler.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Entities;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Factories
{
    public class SchedulerFactory
    {

        private static readonly TaskSchedulerController _controller = new TaskSchedulerController();
        public static dynamic GetScheduleStatus()
        {
            dynamic Result = new ExpandoObject();

            if (SchedulingProvider.Enabled)
            {
                Collection arrScheduleProcessing = SchedulingProvider.Instance().GetScheduleProcessing();
                var processing = from ScheduleHistoryItem item in arrScheduleProcessing
                                 select new
                                 {
                                     item.ScheduleID,
                                     item.TypeFullName,
                                     StartDate = !Null.IsNull(item.StartDate) ? item.StartDate.ToString() : "",
                                     ElapsedTime = Math.Round(item.ElapsedTime, 3),
                                     item.ObjectDependencies,
                                     ScheduleSource = item.ScheduleSource.ToString(),
                                     item.ThreadID,
                                     item.Servers
                                 };

                Collection arrScheduleQueue = SchedulingProvider.Instance().GetScheduleQueue();

                var queue = from ScheduleHistoryItem item in arrScheduleQueue
                            select new
                            {
                                item.ScheduleID,
                                item.FriendlyName,
                                NextStart = !Null.IsNull(item.NextStart) ? item.NextStart.ToString() : "",
                                item.Overdue,
                                RemainingTime = GetTimeStringFromSeconds(item.RemainingTime),
                                RemainingSeconds = item.RemainingTime,
                                item.ObjectDependencies,
                                ScheduleSource = item.ScheduleSource.ToString(),
                                item.ThreadID,
                                item.Servers
                            };


                Result.Status = "Success";
                Result.Data = new
                {
                    ServerTime = DateTime.Now.ToString(),
                    SchedulingEnabled = SchedulingProvider.Enabled.ToString(),
                    Status = SchedulingProvider.Instance().GetScheduleStatus().ToString(),
                    FreeThreadCount = SchedulingProvider.Instance().GetFreeThreadCount().ToString(),
                    ActiveThreadCount = SchedulingProvider.Instance().GetActiveThreadCount().ToString(),
                    MaxThreadCount = SchedulingProvider.Instance().GetMaxThreadCount().ToString(),
                    ScheduleProcessing = processing,
                    ScheduleMode = HostController.Instance.GetString("SchedulerMode"),
                    ScheduleQueue = queue.ToList().OrderBy(q => q.RemainingSeconds),
                    DelayAtAppStart = HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1)
                };
            }
            else
            {
                Result.Status = "Success";
                Result.Data = new
                {
                    SchedulingEnabled = "False",
                    Status = Localization.GetString("Disabled", Components.Constants.TaskSchedulerResourcesFile),
                    FreeThreadCount = SchedulingProvider.Instance().GetFreeThreadCount().ToString(),
                    ActiveThreadCount = SchedulingProvider.Instance().GetActiveThreadCount().ToString(),
                    MaxThreadCount = SchedulingProvider.Instance().GetMaxThreadCount().ToString(),
                    ScheduleMode = HostController.Instance.GetString("SchedulerMode"),
                    DelayAtAppStart = HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1),
                    ScheduleProcessing = new List<string>(),
                    ScheduleQueue = new List<string>()
                };
            }
            Result.SchedulerModes = GetSchedulerModes();
            return Result;
        }

        private static string GetTimeStringFromSeconds(double sec)
        {
            TimeSpan time = TimeSpan.FromSeconds(sec);
            if (time.Days > 0)
            {
                return $"{time.Days} {Localization.GetString(time.Days == 1 ? "DaySingular" : "DayPlural", Components.Constants.TaskSchedulerResourcesFile)}";
            }
            if (time.Hours > 0)
            {
                return $"{time.Hours} {Localization.GetString(time.Hours == 1 ? "HourSingular" : "HourPlural", Components.Constants.TaskSchedulerResourcesFile)}";
            }
            if (time.Minutes > 0)
            {
                return $"{time.Minutes} {Localization.GetString(time.Minutes == 1 ? "MinuteSingular" : "MinutePlural", Components.Constants.TaskSchedulerResourcesFile)}";
            }
            return Localization.GetString("LessThanMinute", Components.Constants.TaskSchedulerResourcesFile);
        }

        internal static List<StringInt> GetSchedulerModes()
        {
            List<StringInt> SchedulerModes = new List<StringInt>
            {
                new StringInt { String = Localization.GetString("Disabled", Components.Constants.TaskSchedulerResourcesFile), Int = 0 },
                new StringInt { String = Localization.GetString("TimerMethod", Components.Constants.TaskSchedulerResourcesFile), Int = 1 },
                new StringInt { String = Localization.GetString("RequestMethod", Components.Constants.TaskSchedulerResourcesFile), Int = 2 }
            };
            return SchedulerModes.ToList();
        }

        internal static string Run_Schedule(ScheduleDto scheduleDto)
        {
            try
            {
                ScheduleItem scheduleItem = _controller.CreateScheduleItem(scheduleDto.TypeFullName, scheduleDto.FriendlyName, scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement,
                                   scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement, scheduleDto.RetainHistoryNum, scheduleDto.AttachToEvent, scheduleDto.CatchUpEnabled,
                                   scheduleDto.Enabled, scheduleDto.ObjectDependencies, scheduleDto.ScheduleStartDate, scheduleDto.Servers);

                scheduleItem.ScheduleID = scheduleDto.ScheduleID;
                SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem, true);

                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                {
                    SchedulingProvider.Instance().ReStart("Change made to schedule.");
                }
                return "Success";
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                return exc.Message.ToString();
            }
        }

    }
}
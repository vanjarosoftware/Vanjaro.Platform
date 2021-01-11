using Dnn.PersonaBar.TaskScheduler.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using Vanjaro.UXManager.Extensions.Menu.Scheduler.Entities;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Managers
{
    public static class SchedulerManager
    {
        private static readonly TaskSchedulerController _controller = new TaskSchedulerController();
        internal static dynamic GetSchedule_Item(int scheduleId)
        {
            dynamic Result = new ExpandoObject();
            try
            {
                ScheduleItem scheduleItem = SchedulingProvider.Instance().GetSchedule(scheduleId);
                Result.Data = new
                {
                    scheduleItem.ScheduleID,
                    scheduleItem.FriendlyName,
                    scheduleItem.TypeFullName,
                    scheduleItem.Enabled,
                    ScheduleStartDate = !Null.IsNull(scheduleItem.ScheduleStartDate) ? scheduleItem.ScheduleStartDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern) : "",
                    Locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                    scheduleItem.TimeLapse,
                    scheduleItem.TimeLapseMeasurement,
                    scheduleItem.RetryTimeLapse,
                    scheduleItem.RetryTimeLapseMeasurement,
                    scheduleItem.RetainHistoryNum,
                    scheduleItem.AttachToEvent,
                    scheduleItem.CatchUpEnabled,
                    scheduleItem.ObjectDependencies,
                    scheduleItem.Servers
                };
                Result.Status = "Success";
                return Result;
            }
            catch (Exception exc)
            {
                Core.Managers.ExceptionManage.LogException(exc);
                return Result.Status = exc.Message.ToString();
            }
        }

        internal static bool VerifyValidTimeLapseRetry(int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement)
        {
            if (retryTimeLapse == 0)
            {
                return true;
            }

            DateTime frequency = CalculateTime(Convert.ToInt32(timeLapse), timeLapseMeasurement);
            DateTime retry = CalculateTime(Convert.ToInt32(retryTimeLapse), retryTimeLapseMeasurement);
            if (retry > frequency)
            {
                return false;
            }
            return true;
        }

        internal static DateTime CalculateTime(int lapse, string measurement)
        {
            DateTime nextTime = new DateTime();
            switch (measurement)
            {
                case "s":
                    nextTime = DateTime.Now.AddSeconds(lapse);
                    break;
                case "m":
                    nextTime = DateTime.Now.AddMinutes(lapse);
                    break;
                case "h":
                    nextTime = DateTime.Now.AddHours(lapse);
                    break;
                case "d":
                    nextTime = DateTime.Now.AddDays(lapse);
                    break;
                case "w":
                    nextTime = DateTime.Now.AddDays(lapse);
                    break;
                case "mo":
                    nextTime = DateTime.Now.AddMonths(lapse);
                    break;
                case "y":
                    nextTime = DateTime.Now.AddYears(lapse);
                    break;
            }
            return nextTime;
        }

        internal static void Halt()
        {
            SchedulingProvider.Instance().Halt("Host Settings");
        }


        internal static string GetTimeStringFromSeconds(double sec)
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

        internal static List<StringText> RetainScheduleHistory()
        {
            List<StringText> RetainScheduleItem = new List<StringText>
            {
                new StringText { Text = "None", Value = "0" },
                new StringText { Text = "1", Value = "1" },
                new StringText { Text = "5", Value = "2" },
                new StringText { Text = "10", Value = "3" },
                new StringText { Text = "25", Value = "4" },
                new StringText { Text = "50", Value = "5" },
                new StringText { Text = "60", Value = "6" },
                new StringText { Text = "100", Value = "7" },
                new StringText { Text = "250", Value = "8" },
                new StringText { Text = "500", Value = "9" },
                new StringText { Text = "All", Value = "-1" }
            };
            return RetainScheduleItem;
        }

        internal static List<StringText> RunOnEvent()
        {
            List<StringText> RunOnEvent = new List<StringText>
            {
                new StringText { Text = Localization.GetString("None", Components.Constants.TaskSchedulerResourcesFile), Value = "0" },
                new StringText { Text = Localization.GetString("APPLICATION_START", Components.Constants.TaskSchedulerResourcesFile), Value = "APPLICATION_START" }
            };
            return RunOnEvent;
        }

        internal static List<StringText> CatchUpTask()
        {
            List<StringText> CatchUpTask = new List<StringText>
            {
                new StringText() { Text = Localization.GetString("Disabled", Components.Constants.TaskSchedulerResourcesFile), Value = "false" },
                new StringText() { Text = "Enabled", Value = "true" }
            };
            return CatchUpTask;
        }

        internal static List<StringText> TimePeriod()
        {
            List<StringText> TimePeriods = new List<StringText>
            {
                new StringText() { Text = Localization.GetString("Seconds", Components.Constants.TaskSchedulerResourcesFile), Value = "s" },
                new StringText() { Text = Localization.GetString("Minutes", Components.Constants.TaskSchedulerResourcesFile), Value = "m" },
                new StringText() { Text = Localization.GetString("Hours", Components.Constants.TaskSchedulerResourcesFile), Value = "h" },
                new StringText() { Text = Localization.GetString("Days", Components.Constants.TaskSchedulerResourcesFile), Value = "d" },
                new StringText() { Text = Localization.GetString("Weeks", Components.Constants.TaskSchedulerResourcesFile), Value = "w" },
                new StringText() { Text = Localization.GetString("Months", Components.Constants.TaskSchedulerResourcesFile), Value = "mo" },
                new StringText() { Text = Localization.GetString("Years", Components.Constants.TaskSchedulerResourcesFile), Value = "y" }
            };
            return TimePeriods;
        }

        internal static dynamic GetScheduleItems(string serverName = "")
        {
            dynamic Result = new ExpandoObject();

            IEnumerable<ScheduleItem> scheduleviews = _controller.GetScheduleItems(null, serverName);
            ScheduleItem[] arrSchedule = scheduleviews.ToArray();

            Result = arrSchedule.Select(v => new
            {
                v.ScheduleID,
                v.FriendlyName,
                v.Enabled,
                RetryTimeLapse = _controller.GetTimeLapse(v.RetryTimeLapse, v.RetryTimeLapseMeasurement),
                NextStart = (v.Enabled && !Null.IsNull(v.NextStart)) ? v.NextStart.ToString() : "",
                Frequency = _controller.GetTimeLapse(v.TimeLapse, v.TimeLapseMeasurement)
            });

            return Result;
        }

        internal static dynamic GetScheduleItems(int Skip, int PageSize)
        {
            dynamic Result = new ExpandoObject();
            IEnumerable<ScheduleItem> scheduleviews = _controller.GetScheduleItems(null, "");
            ScheduleItem[] arrSchedule = scheduleviews.ToArray();

            int totalpins = arrSchedule.Count();
            int NumberOfPages = (int)Math.Ceiling((double)totalpins / PageSize);
            Result.numberOfPages = NumberOfPages;

            Result.ScheduledItems = arrSchedule.Select(v => new
            {
                v.ScheduleID,
                v.FriendlyName,
                v.Enabled,
                RetryTimeLapse = _controller.GetTimeLapse(v.RetryTimeLapse, v.RetryTimeLapseMeasurement),
                NextStart = (v.Enabled && !Null.IsNull(v.NextStart)) ? v.NextStart.ToString() : "",
                Frequency = _controller.GetTimeLapse(v.TimeLapse, v.TimeLapseMeasurement)
            }).Skip(Skip).Take(PageSize);

            return Result;
        }
    }
}
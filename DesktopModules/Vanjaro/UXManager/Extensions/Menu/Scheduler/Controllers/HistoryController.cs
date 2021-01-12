using Dnn.PersonaBar.TaskScheduler.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
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
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class HistoryController : UIEngineController
    {
        private readonly TaskSchedulerController _controller = new TaskSchedulerController();

        internal static List<IUIData> GetData(int PortalID, UserInfo userinfo)
        {
            Dictionary<string, IUIData> Setting = new Dictionary<string, IUIData>
            {
                { "ScheduleStatus", new UIData { Name = "ScheduleStatus", Options = SchedulerFactory.GetScheduleStatus() } }
            };
            return Setting.Values.ToList();
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public ActionResult GetScheduleItemHistory(int scheduleId = -1, int pageSize = 10, int pageIndex = 0)
        {
            ActionResult ActionResult = new ActionResult();
            dynamic Result = new ExpandoObject();
            try
            {
                System.Collections.ArrayList arrSchedule = SchedulingProvider.Instance().GetScheduleHistory(scheduleId);

                var query = from ScheduleHistoryItem history in arrSchedule
                            select new
                            {
                                history.FriendlyName,
                                history.LogNotes,
                                history.Server,
                                ElapsedTime = Math.Round(history.ElapsedTime, 3),
                                history.Succeeded,
                                StartDate = !Null.IsNull(history.StartDate) ? history.StartDate.ToString() : "",
                                EndDate = !Null.IsNull(history.EndDate) ? history.EndDate.ToString() : "",
                                NextStart = !Null.IsNull(history.NextStart) ? history.NextStart.ToString() : ""
                            };


                Result.Status = "Success";
                Result.Item = query.Skip(pageIndex * pageSize).Take(pageSize);

                double NoofPages = (double)query.Count() / pageSize;
                if ((int)NoofPages > 0)
                {
                    NoofPages = Math.Ceiling(NoofPages);
                }

                Result.NumberOfPages = NoofPages;
            }
            catch (Exception exc)
            {
                Result.Status = exc.Message.ToString();
                ExceptionManager.LogException(exc);
            }
            ActionResult.Data = Result;
            return ActionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
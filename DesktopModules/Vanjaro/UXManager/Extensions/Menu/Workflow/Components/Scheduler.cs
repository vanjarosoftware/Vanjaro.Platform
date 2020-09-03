using DotNetNuke.Services.Scheduling;

namespace Vanjaro.UXManager.Extensions.Menu.Workflow.Components
{
    public class Scheduler : SchedulerClient
    {
        public Scheduler(ScheduleHistoryItem HistoryItem)
           : base()
        {
            ScheduleHistoryItem = HistoryItem;
        }

        public override void DoWork()
        {
            bool Success = true;
            Progressing();
            //foreach (Notification Queue in Mandeeps.DNN.Library.Workflow.WorkflowController.WorkflowNotificationController.GetNofication())
            //{
            //    try
            //    {
            //        DotNetNuke.Services.Mail.Mail.SendMail(Queue.From, Queue.To, "", Queue.BCC, DotNetNuke.Services.Mail.MailPriority.Normal, Queue.Subject, DotNetNuke.Services.Mail.MailFormat.Html, System.Text.Encoding.UTF8, Queue.Body, "", "", "", "", "");
            //    }
            //    catch (Exception ex) { Exceptions.LogException(ex); Success = false; this.Errored(ref ex); }
            //    try
            //    {
            //        Mandeeps.DNN.Library.Workflow.WorkflowController.WorkflowNotificationController.DeleteNotification(Queue);
            //    }
            //    catch { }
            //}
            if (Success)
            {
                ScheduleHistoryItem.AddLogNote("VJ workflow comment mailed successfully !");
                ScheduleHistoryItem.Succeeded = true;
                Completed();
            }
            else
            {
                ScheduleHistoryItem.AddLogNote("VJ workflow comment Not mailed properly.");
                ScheduleHistoryItem.Succeeded = false;
            }
        }



        #region Install Scheduler
        public static void Install()
        {
            if (SchedulingProvider.Instance().GetSchedule("Vanjaro.UXManager.Extensions.Menu.Workflow.Components.Scheduler,Vanjaro.UXManager.Extensions.Menu.Workflow", string.Empty) == null)
            {
                ScheduleItem Manager = new ScheduleItem
                {
                    TypeFullName = "Vanjaro.UXManager.Extensions.Menu.Workflow.Components.Scheduler,Vanjaro.UXManager.Extensions.Menu.Workflow",
                    Enabled = true,
                    TimeLapse = 2,
                    TimeLapseMeasurement = "m",
                    RetryTimeLapse = -1,
                    RetryTimeLapseMeasurement = "s",
                    RetainHistoryNum = 5,
                    FriendlyName = "Vanjaro Workflow Notification"
                };
                SchedulingProvider.Instance().AddSchedule(Manager);
            }
        }
        #endregion
    }
}
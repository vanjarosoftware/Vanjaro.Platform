using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Social.Notifications;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using Vanjaro.Core.Entities;
using Vanjaro.Core.Entities.Interface;
using static Vanjaro.Core.Factories;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Notification.Notification.Managers
{

    public class TasksManager
    {
        public static BaseModel GetBaseModel(int PortalID)
        {
            BaseModel item = new BaseModel
            {
                NotificationMarkUp = RenderNotificationsTask(PortalID),
                NotificationCount = GetNotificationCount(PortalID)
            };
            return item;
        }

        public static string RenderNotificationsTask(int PortalID)
        {
            List<INotificationTask> NotificationTask = GetNotificationTask(PortalID);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<ul class=\"Notifications" + "\">");

            if (NotificationTask.Count > 0)
            {
                foreach (INotificationTask Task in NotificationTask)
                {

                    if (Task.Hierarchy.NotificationCount > 0)
                    {
                        sb.Append("<li><a onclick=\"parent.OpenPopUp('',600,'right','" + Localization.GetString("Review", Components.Constants.LocalResourcesFile) + "','" + Task.Hierarchy.URL + "')\"><span class='notificationname'>" + Task.Hierarchy.NotificationName + "</span><span class='badge badge-error errorcount'>" + Task.Hierarchy.NotificationCount + "</span></a></li>");
                    }
                }
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        internal static List<INotificationTask> GetNotificationTask(int PortalID)
        {
            string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.NotificationTask + "PortalID" + PortalID);
            List<INotificationTask> NotificationTasks = CacheFactory.Get(CacheKey);
            if (NotificationTasks == null)
            {
                List<INotificationTask> ServiceInterfaceAssemblies = new List<INotificationTask>();
                string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && (c.Contains("Vanjaro.UXManager.Extensions") || c.Contains("Vanjaro.Core.Extensions"))).ToArray();
                foreach (string Path in binAssemblies)
                {
                    try
                    {
                        //get all assemblies 
                        IEnumerable<INotificationTask> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                         where t != (typeof(INotificationTask)) && (typeof(INotificationTask).IsAssignableFrom(t))
                                                                         select Activator.CreateInstance(t) as INotificationTask;

                        ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<INotificationTask>());
                    }
                    catch { continue; }
                }
                NotificationTasks = ServiceInterfaceAssemblies;
                CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
            }
            return NotificationTasks;
        }

        internal static int GetNotificationCount(int PortalID)
        {
            int Count = 0;
            foreach (INotificationTask Task in GetNotificationTask(PortalID))
            {

                if (Task.Hierarchy.NotificationCount > 0)
                {
                    Count++;
                }
            }
            return Count;
        }

        public static dynamic GetNotifications(int Page, int PageSize)
        {
            dynamic Result = new ExpandoObject();
            try
            {
                UserInfo UserInfo = PortalSettings.Current.UserInfo;
                int PortalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                int TotalNotifications = NotificationsController.Instance.CountNotifications(UserInfo.UserID, PortalId);

                IEnumerable<DotNetNuke.Services.Social.Notifications.Notification> notificationsDomainModel = NotificationsController.Instance.GetNotifications(UserInfo.UserID, PortalId, -1, TotalNotifications).Skip(Page).Take(PageSize);

                NotificationsViewModel notificationsViewModel = new NotificationsViewModel
                {
                    TotalNotifications = Math.Ceiling(((double)TotalNotifications / PageSize)),
                    Notifications = new List<NotificationViewModel>(notificationsDomainModel.Count())
                };

                foreach (DotNetNuke.Services.Social.Notifications.Notification notification in notificationsDomainModel)
                {
                    UserInfo user = UserController.Instance.GetUser(PortalId, notification.SenderUserID);
                    string displayName = (user != null ? user.DisplayName : "");
                    StringBuilder sb = new StringBuilder();
                    foreach (string s in notification.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        string[] markup = s.Split(':');
                        if (!string.IsNullOrEmpty(s) && markup.Length > 1)
                        {
                            sb.Append("<div class=\"notification-details\"><span class=\"heading\">" + markup[0] + ":</span>");
                            sb.Append("<span class=\"description \">" + markup[1] + "</span></div>" + Environment.NewLine);
                        }
                    }
                    NotificationViewModel notificationViewModel = new NotificationViewModel
                    {
                        NotificationId = notification.NotificationID,
                        Subject = notification.Subject,
                        From = notification.From,
                        Body = sb.ToString(),
                        DisplayDate = DotNetNuke.Common.Utilities.DateUtils.CalculateDateForDisplay(notification.CreatedOnDate),
                        SenderAvatar = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalId, notification.SenderUserID),
                        SenderProfileUrl = Globals.UserProfileURL(notification.SenderUserID),
                        SenderDisplayName = displayName,
                        Actions = new List<NotificationActionViewModel>()
                    };

                    NotificationType notificationType = NotificationsController.Instance.GetNotificationType(notification.NotificationTypeID);
                    IList<NotificationTypeAction> notificationTypeActions = NotificationsController.Instance.GetNotificationTypeActions(notification.NotificationTypeID);

                    foreach (NotificationTypeAction notificationTypeAction in notificationTypeActions)
                    {
                        NotificationActionViewModel notificationActionViewModel = new NotificationActionViewModel
                        {
                            Name = LocalizeActionString(notificationTypeAction.NameResourceKey, notificationType.DesktopModuleId),
                            Description = LocalizeActionString(notificationTypeAction.DescriptionResourceKey, notificationType.DesktopModuleId),
                            Confirm = LocalizeActionString(notificationTypeAction.ConfirmResourceKey, notificationType.DesktopModuleId),
                            APICall = notificationTypeAction.APICall
                        };

                        notificationViewModel.Actions.Add(notificationActionViewModel);
                    }

                    //if (notification.IncludeDismissAction)
                    //{
                    //    notificationViewModel.Actions.Add(new NotificationActionViewModel
                    //    {
                    //        Name = Localization.GetString("Dismiss.Text"),
                    //        Description = Localization.GetString("DismissNotification.Text"),
                    //        Confirm = "",
                    //        APICall = "API/InternalServices/NotificationsService/Dismiss"
                    //    });
                    //}

                    notificationsViewModel.Notifications.Add(notificationViewModel);
                }
                Result = notificationsViewModel;
            }
            catch (Exception exc)
            {
                ExceptionManager.LogException(exc);
            }
            return Result;
        }

        private static string LocalizeActionString(string key, int desktopModuleId)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }

            string actionString;

            if (desktopModuleId > 0)
            {
                int PortalId = PortalSettings.Current.PortalId;
                DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, PortalId);

                string resourceFile = string.Format("~/DesktopModules/{0}/{1}/{2}",
                    desktopModule.FolderName.Replace("\\", "/"),
                    Localization.LocalResourceDirectory,
                    Localization.LocalSharedResourceFile);

                actionString = Localization.GetString(key, resourceFile);
            }
            else
            {
                actionString = Localization.GetString(key);
            }

            return string.IsNullOrEmpty(actionString) ? key : actionString;
        }
    }
}
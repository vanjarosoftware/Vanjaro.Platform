using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Social.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vanjaro.Core.Entities.Interface;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class NotificationTaskFactory
        {
            internal static int GetNotificationCount(int PortalID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.NotificationTask + "PortalID" + PortalID);
                List<INotificationTask> NotificationTasks = Core.Factories.CacheFactory.Get(CacheKey);
                if (NotificationTasks == null)
                {
                    List<INotificationTask> ServiceInterfaceAssemblies = new List<INotificationTask>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
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

                int NotificationCount = 0;
                if (NotificationTasks.Count > 0)
                {
                    foreach (INotificationTask Task in NotificationTasks)
                    {

                        if (Task.Hierarchy.NotificationCount > 0)
                        {
                            NotificationCount += Task.Hierarchy.NotificationCount;
                        }
                    }
                }


                UserInfo uinfo = UserController.Instance.GetCurrentUserInfo();
                if (uinfo != null)
                {
                    NotificationCount += NotificationsController.Instance.CountNotifications(uinfo.UserID, uinfo.PortalID);
                }
                return NotificationCount;
            }
        }
    }
}
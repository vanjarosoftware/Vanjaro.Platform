using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;

namespace Vanjaro.Common.Factories
{
    internal class AppFactory
    {
        internal enum Identifiers
        {
            admin_notifications_email
        }

        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>
            {
                new AngularView
                {
                    Identifier = Identifiers.admin_notifications_email.ToString(),
                    Defaults = new Dictionary<string, string>
                {
                  {"DNNHostSetting","false"},
                  {"Notification_Email","false"},
                  {"Server",""},
                  {"Port","25"},
                  {"Authentication","Basic"},
                  {"Username",""},
                  {"Password",""},
                  {"SSL","false"},
                  {"ReplyFromDisplayName",""},
                  {"ReplyFromEmail",""},
                  {"ReplyTo",""}
               }
                }
            };
            return Views;
        }
    }
}
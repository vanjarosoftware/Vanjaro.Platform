using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Security;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;

namespace Vanjaro.Common.Manager
{
    public class NotificationManager
    {
        public static List<IUIData> GetSettings(int ModuleID)
        {
            CacheFactory.ClearCache();
            Dictionary<string, IUIData> Settings = SettingFactory.GetDictionary(ModuleID, AppFactory.Identifiers.admin_notifications_email.ToString());
            List<dynamic> ServerTypes = new List<dynamic>
            {
                new { AuthenticationType = "Anonymous" },
                new { AuthenticationType = "NTLM" },
                new { AuthenticationType = "Basic" }
            };
            Settings["Authentication"].Options = ServerTypes;
            Settings["Authentication"].OptionsText = "AuthenticationType";
            Settings["Authentication"].OptionsValue = "AuthenticationType";
            Settings["Password"].Value = FIPSCompliant.DecryptAES(Settings["Password"].Value, Config.GetDecryptionkey(), Host.GUID, 1000);
            return Settings.Values.ToList();
        }

        public static void UpdateSettings(int ModuleID, List<IUIData> Settings)
        {
            if (Settings != null && Settings.Count > 0)
            {
                SettingFactory.Update(Settings.Cast<Setting>().ToList());
                if (Settings.Where(a => a.Name == "Password").FirstOrDefault() != null)
                {
                    SettingFactory.UpdateValue(ModuleID, AppFactory.Identifiers.admin_notifications_email.ToString(), "Password", FIPSCompliant.EncryptAES(Settings.Where(a => a.Name == "Password").Select(a => new { a.Value }).FirstOrDefault().Value, Config.GetDecryptionkey(), Host.GUID, 1000));
                }

                CacheFactory.ClearCache();
            }
        }


        public static void QueueMail(int PortalID, int ModuleID, string Subject, string Content, string ToEmail, string FromEmailPrefix = null)
        {
            QueueMail(PortalID, ModuleID, Subject, Content, ToEmail, null, FromEmailPrefix);
        }

        public static void QueueMail(int PortalID, int ModuleID, string Subject, string Content, string ToEmail, List<Attachment> Attachments, string FromEmailPrefix = null)
        {
            QueueMail(PortalID, ModuleID, Subject, Content, ToEmail, Attachments, string.Empty, string.Empty, FromEmailPrefix);
        }

        public static void QueueMail(int PortalID, int ModuleID, string Subject, string Content, string ToEmail, List<Attachment> Attachments, string FromName, string FromEmail, string FromEmailPrefix = null)
        {
            QueueMail(PortalID, ModuleID, Subject, Content, ToEmail, Attachments, FromName, FromEmail, string.Empty, FromEmailPrefix);
        }

        public static void QueueMail(int PortalID, int ModuleID, string Subject, string Content, string ToEmail, List<Attachment> Attachments, string FromName, string FromEmail, string ReplyEmail, string FromEmailPrefix = null)
        {
            NotificationFactory.QueueMail(PortalID, ModuleID, Subject, Content, ToEmail, Attachments, FromName, FromEmail, FromEmailPrefix, ReplyEmail);
        }

        public static void QueueMail(MailQueue MailQueue)
        {
            NotificationFactory.QueueMail(MailQueue);
        }

        public static void QueueMail(List<MailQueue> MailQueues)
        {
            if (MailQueues != null && MailQueues.Count > 0)
            {
                List<MailQueue> distinctMailQueues = MailQueues.GroupBy(x => new { x.ToEmail, x.Subject })
                         .Select(g => g.First())
                         .ToList();

                NotificationFactory.QueueMail(distinctMailQueues);
            }
        }

        public static void TestSmtp(SmtpServer smtp, string ToEmail, string FriendlyName, string FromEmail, string FromName, string ReplyTo, ref string SuccessfulMessage, ref string ExceptionsMessage)
        {
            NotificationFactory.TestSmtp(smtp, ToEmail, FriendlyName, FromEmail, FromName, ReplyTo, ref SuccessfulMessage, ref ExceptionsMessage);
        }
    }
}
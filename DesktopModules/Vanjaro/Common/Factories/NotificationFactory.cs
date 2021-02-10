using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Data.Scripts;

namespace Vanjaro.Common.Factories
{
    internal class NotificationFactory
    {

        internal static void QueueMail(int PortalID, int ModuleID, string Subject, string Content, string ToEmail, List<Data.Entities.Attachment> Attachments, string FromName, string FromEmail, string FromEmailPrefix, string ReplyEmail)
        {
            QueueMail(ProcessMailQueue(PortalID, ModuleID, Subject, Content, ToEmail, Attachments, FromName, FromEmail, FromEmailPrefix, ReplyEmail));
        }

        internal static void QueueMail(MailQueue MailQueue)
        {
            if (MailQueue != null)
            {
                MailQueue.Insert();
            }
        }

        internal static void QueueMail(List<MailQueue> MailQueues)
        {
            DataTable dt = ConvertMailQueueListToDataTable(MailQueues);
            if (dt != null && MailQueues.Count > 0)
            {
                using (SqlConnection destinationConnection = new SqlConnection(Config.GetConnectionString()))
                {
                    destinationConnection.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                    {
                        bulkCopy.DestinationTableName = "VJ_Common_MailQueue";
                        bulkCopy.WriteToServer(dt);
                    }
                }

            }
        }

        public static SmtpClient Connect(string Server, int? port, string Authentication, string Username, string Password, bool EnableSSL)
        {
            var smtpClient = new SmtpClient
            {
                Host = Server
            };


            if (port.HasValue && port.Value != 25)
            {
                smtpClient.Port = port.Value;
            }

            smtpClient.ServicePoint.MaxIdleTime = Host.SMTPMaxIdleTime;
            smtpClient.ServicePoint.ConnectionLimit = Host.SMTPConnectionLimit;

            switch (Authentication)
            {
                case "":
                case "0": //anonymous
                    break;
                case "1": //basic
                    if (!String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(Username, Password);
                    }
                    break;
                case "2": //NTLM
                    smtpClient.UseDefaultCredentials = true;
                    break;
            }

            smtpClient.EnableSsl = EnableSSL;

            return smtpClient;
        }

        public static void SendMail(SmtpClient client, MailQueue mail)
        {
            var msg = new MailMessage
            {
                Subject = mail.Subject,
                From = new MailAddress(mail.FromEmail.Trim().ToLower(), mail.FromName.Trim()),
                IsBodyHtml = true,
                Body = mail.Content
            };

            msg.To.Add(mail.ToEmail.Trim().ToLower());

            if (!string.IsNullOrEmpty(mail.ReplyEmail.Trim().ToLower()))
            {
                msg.ReplyToList.Add(mail.ReplyEmail.Trim().ToLower());
            }

            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mail.Content, null, "text/html"));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(ConvertToText(mail.Content), null, "text/plain"));

            if (!string.IsNullOrEmpty(mail.Attachment))
            {
                var attachments = SendAttachment.Attachments(mail.Attachment);

                foreach (var attachment in attachments)
                {
                    msg.Attachments.Add(attachment);
                }
            }

            client.Send(msg);

        }

        public static string ConvertToText(string sHTML)
        {
            var formattedHtml = HtmlUtils.FormatText(sHTML, true);
            var styleLessHtml = HtmlUtils.RemoveInlineStyle(formattedHtml);
            return HtmlUtils.StripTags(styleLessHtml, true);
        }
        internal static void TestSmtp(SmtpServer smtp, string ToEmail, string FriendlyName, string FromEmail, string FromName, string ReplyTo, ref string SuccessfulMessage, ref string ExceptionsMessage)
        {
            try
            {
                if (smtp != null && !string.IsNullOrEmpty(ToEmail) && !string.IsNullOrEmpty(FromEmail) && !string.IsNullOrEmpty(FromName))
                {
                    SmtpClient client = Connect(smtp.Server, smtp.Port, smtp.Authentication, smtp.Username, smtp.Password, smtp.SSL);

                    var msg = new MailQueue
                    {
                        FromName = FromEmail,
                        FromEmail = FromEmail,
                        ToEmail = ToEmail,
                        ReplyEmail = ReplyTo,
                        Subject = !string.IsNullOrEmpty(FriendlyName) ? FriendlyName + " Notification Email SMTP Configuration Test" : "Notification Email SMTP Configuration Test",
                    };

                    SendMail(client, msg);

                    SuccessfulMessage = "Email Sent Successfully from " + FromEmail.Trim().ToLower() + " to " + ToEmail.Trim().ToLower();
                }
            }
            catch (Exception ex)
            {
                ExceptionsMessage += "<br />" + ex.Message;
            }
        }

        internal static SmtpServer GetSMTP(int ModuleId, string Identifier)
        {
            SmtpServer smtp = new SmtpServer();
            CacheFactory.ClearCache();
            List<Setting> SmtpSettings = SettingFactory.GetSettings(ModuleId, Identifier);
            string Server = string.Empty, Port = string.Empty, Authentication = string.Empty, Username = string.Empty, Password = string.Empty, SSL = string.Empty;

            if (SmtpSettings.Count > 0)
            {
                string dnnhostsettings = SmtpSettings.Where(s => s.Name == "DNNHostSetting").FirstOrDefault().Value;
                if (dnnhostsettings.ToLower() == "false")
                {
                    Server = SmtpSettings.Where(s => s.Name == "Server").FirstOrDefault().Value;
                    if (!string.IsNullOrEmpty(Server))
                    {
                        smtp.Server = Server;
                    }

                    Port = SmtpSettings.Where(s => s.Name == "Port").FirstOrDefault().Value;
                    if (Port != null && int.Parse(Port) > -1)
                    {
                        smtp.Port = int.Parse(Port);
                    }

                    Authentication = SmtpSettings.Where(s => s.Name == "Authentication").FirstOrDefault().Value;
                    if (!string.IsNullOrEmpty(Authentication))
                    {
                        smtp.Authentication = Authentication;
                    }

                    Username = SmtpSettings.Where(s => s.Name == "Username").FirstOrDefault().Value;
                    if (!string.IsNullOrEmpty(Username))
                    {
                        smtp.Username = Username;
                    }

                    Password = SmtpSettings.Where(s => s.Name == "Password").FirstOrDefault().Value;
                    if (!string.IsNullOrEmpty(Password))
                    {
                        smtp.Password = Password;
                    }

                    SSL = SmtpSettings.Where(s => s.Name == "SSL").FirstOrDefault().Value;
                    if (!string.IsNullOrEmpty(SSL))
                    {
                        smtp.SSL = Convert.ToBoolean(SSL);
                    }
                }
                else
                {
                    smtp = GetDNNHostSettingsSmtpServer(ModuleId);
                }
            }
            return smtp;
        }

        internal static List<MailQueue> GetMailQueue(int moduleId)
        {
            return MailQueue.Query("Where ModuleID=@0", moduleId).Where(m => m.Status == "Queue" || (m.Status == "Retry" && m.RetryDateTime < DateTime.Now && m.RetryAttempt <= 3)).ToList();
        }

        internal static SmtpServer GetDNNHostSettingsSmtpServer(int ModuleId)
        {
            SmtpServer smInfo = new SmtpServer();
            try
            {
                ModuleInfo mod = new ModuleController().GetModule(ModuleId, Null.NullInteger, false);
                if (mod != null)
                {
                    Dictionary<string, string> portalsettings = PortalController.Instance.GetPortalSettings(mod.PortalID);
                    //NS Fix for DNN v922 case: when server mode type 'My Website' 
                    if (portalsettings.ContainsKey("SMTPmode") && portalsettings["SMTPmode"].ToLower() == "p")
                    {

                        string Server = portalsettings["SMTPServer"].ToString();
                        int Port = 25;
                        if (!string.IsNullOrEmpty(Server) && Server.Contains(":"))
                        {
                            try
                            {
                                Port = int.Parse(Server.Split(':')[1]);
                                Server = Server.Split(':')[0];
                            }
                            catch { }
                        }
                        smInfo.Server = Server;
                        smInfo.Port = Port;
                        smInfo.Username = portalsettings["SMTPUsername"].ToString();
                        smInfo.Password = portalsettings["SMTPPassword"].ToString();
                        smInfo.SSL = portalsettings["SMTPEnableSSL"].ToString().ToUpper() == "Y" ? true : false;
                        int Authentication = 0;
                        try
                        {
                            Authentication = int.Parse(portalsettings["SMTPAuthentication"].ToString());
                        }
                        catch { }

                        if (smInfo.SSL && smInfo.Port == 25)//SSL is Enable or true then assign Port number 465 else 25
                        {
                            smInfo.Port = 465;
                        }

                        if (Authentication == 0)
                        {
                            smInfo.Authentication = "Anonymous";
                        }
                        else if (Authentication == 1)
                        {
                            smInfo.Authentication = "Basic";
                        }
                        else
                        {
                            smInfo.Authentication = "NTLM";
                        }
                    }
                    //get dnn host and map it to smtpinfo and return the object
                    else
                    {

                        string Server = Host.SMTPServer;
                        int Port = 25;
                        if (!string.IsNullOrEmpty(Server) && Server.Contains(":"))
                        {
                            try
                            {
                                Port = int.Parse(Server.Split(':')[1]);
                                Server = Server.Split(':')[0];
                            }
                            catch { }
                        }
                        smInfo.Server = Server;
                        smInfo.Port = Port;
                        smInfo.Username = Host.SMTPUsername;
                        smInfo.Password = FIPSCompliant.EncryptAES(Host.SMTPPassword, Config.GetDecryptionkey(), Host.GUID, 1000);
                        smInfo.SSL = Host.EnableSMTPSSL;
                        int Authentication = 0;
                        try
                        {
                            Authentication = int.Parse(Host.SMTPAuthentication);
                        }
                        catch { }

                        if (smInfo.SSL && smInfo.Port == 25)//SSL is Enable or true then assign Port number 465 else 25
                        {
                            smInfo.Port = 465;
                        }

                        if (Authentication == 0)
                        {
                            smInfo.Authentication = "Anonymous";
                        }
                        else if (Authentication == 1)
                        {
                            smInfo.Authentication = "Basic";
                        }
                        else
                        {
                            smInfo.Authentication = "NTLM";
                        }
                    }
                    //smInfo.SmtpUniqueId = -1;
                }
            }
            catch (Exception ex) { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); }
            return smInfo;
        }

        private static MailQueue ProcessMailQueue(int PortalID, int ModuleID, string Subject, string Content, string ToEmail, List<Data.Entities.Attachment> Attachments, string FromName, string FromEmail, string FromEmailPrefix, string ReplyEmail)
        {
            MailQueue mailQueue = null;
            if (!string.IsNullOrEmpty(Subject) && !string.IsNullOrEmpty(Content) && !string.IsNullOrEmpty(ToEmail))
            {
                mailQueue = new MailQueue
                {
                    PortalID = PortalID,
                    ModuleID = ModuleID,
                    Subject = Subject,
                    Content = Content,
                    SmtpUniqueId = -1,
                    Status = "Queue",
                    RetryAttempt = 0,
                    RetryDateTime = DateTime.UtcNow
                };

                List<Setting> Settings = SettingFactory.GetSettings(ModuleID, AppFactory.Identifiers.admin_notifications_email.ToString());

                if (Settings.Count > 0)
                {
                    try
                    {
                        if (Settings.Where(s => s.Name == "DNNHostSetting").FirstOrDefault() != null && Settings.Where(s => s.Name == "DNNHostSetting").FirstOrDefault().Value.ToLower() == "false")
                        {
                            if (Settings.Where(s => s.Name == "ReplyFromDisplayName").FirstOrDefault() != null)
                            {
                                mailQueue.FromName = Settings.Where(s => s.Name == "ReplyFromDisplayName").FirstOrDefault().Value;
                            }

                            if (Settings.Where(s => s.Name == "ReplyFromEmail").FirstOrDefault() != null)
                            {
                                mailQueue.FromEmail = Settings.Where(s => s.Name == "ReplyFromEmail").FirstOrDefault().Value;
                            }

                            if (Settings.Where(s => s.Name == "ReplyTo").FirstOrDefault() != null)
                            {
                                mailQueue.ReplyEmail = Settings.Where(s => s.Name == "ReplyTo").FirstOrDefault().Value;
                            }
                        }
                        else
                        {
                            mailQueue.FromName = Host.HostEmail;
                            mailQueue.FromEmail = Host.HostEmail;
                            mailQueue.ReplyEmail = Host.HostEmail;
                        }
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(FromName))
                {
                    mailQueue.FromName = FromName;
                }

                if (!string.IsNullOrEmpty(FromEmail))
                {
                    mailQueue.FromEmail = FromEmail;
                }

                if (!string.IsNullOrEmpty(ReplyEmail))
                {
                    mailQueue.ReplyEmail = ReplyEmail;
                }

                if (string.IsNullOrEmpty(mailQueue.ReplyEmail))
                {
                    mailQueue.ReplyEmail = mailQueue.FromEmail;
                }

                mailQueue.ToEmail = ToEmail;

                if (Attachments != null && Attachments.Count > 0)
                {
                    mailQueue.Attachment = Newtonsoft.Json.JsonConvert.SerializeObject(Attachments);
                }
                else
                {
                    mailQueue.Attachment = "";
                }

                //Fix for 50310 and 50294  auto-no-reply-helpdesk@
                if (!string.IsNullOrEmpty(FromEmailPrefix) && !string.IsNullOrEmpty(mailQueue.FromEmail))
                {
                    mailQueue.FromEmail = FromEmailPrefix + mailQueue.FromEmail.Split('@')[1];
                }
            }

            return mailQueue;
        }

        private static DataTable ConvertMailQueueListToDataTable(List<MailQueue> MailQueues)
        {
            DataTable table = new DataTable();
            table.Columns.Add("MailQueueID", typeof(int));
            table.Columns.Add("ToEmail", typeof(string));
            table.Columns.Add("Subject", typeof(string));
            table.Columns.Add("Content", typeof(string));
            table.Columns.Add("FromName", typeof(string));
            table.Columns.Add("FromEmail", typeof(string));
            table.Columns.Add("ReplyEmail", typeof(string));
            table.Columns.Add("SmtpUniqueId", typeof(int));
            table.Columns.Add("ModuleID", typeof(int));
            table.Columns.Add("Attachment", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("RetryDateTime", typeof(DateTime));
            table.Columns.Add("RetryAttempt", typeof(int));
            foreach (MailQueue item in MailQueues)
            {
                MailQueue mq = ProcessMailQueue(item.PortalID, item.ModuleID, item.Subject, item.Content, item.ToEmail, null, item.FromName, item.FromEmail, null, item.ReplyEmail);
                if (mq != null)
                {
                    table.Rows.Add(0, mq.ToEmail, mq.Subject, mq.Content, mq.FromName, mq.FromEmail, mq.ReplyEmail, mq.SmtpUniqueId, mq.ModuleID, item.Attachment, mq.Status, mq.RetryDateTime, mq.RetryAttempt);
                }
            }
            return table;
        }


        #region MailQueue Log

        internal static void Log(MailQueue_Log Log)
        {
            if (Log != null)
            {
                Log.CreatedOn = DateTime.UtcNow;
                Log.Insert();
            }
        }

        internal static void SMTPPurgeLogs()
        {
            string Days = SettingFactory.GetHostSetting("SMTPPurgeLogsAfter", false, "60");
            MailQueue_Log.Delete("WHERE DATEDIFF(DAY, CAST(CreatedOn as DATETIME), GETUTCDATE())>@0", Days);
        }




        #endregion
    }
}
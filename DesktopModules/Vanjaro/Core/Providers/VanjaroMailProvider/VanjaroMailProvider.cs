using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Mail;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Manager;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Portals;

namespace Vanjaro.Core.Providers.Mail
{
    public class VanjaroMailProvider : MailProvider
    {
        public override string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null)
        {
            string To = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(mailInfo.To))
                    To = mailInfo.To.Replace(";", ",");

                // attachments
                List<Attachment> attachments = new List<Attachment>();
                if (mailInfo.Attachments != null)
                {
                    foreach (var attachment in mailInfo.Attachments.Where(attachment => attachment.Content != null))
                        attachments.Add(new Attachment() { BLOB = attachment.Content, Name = attachment.Filename });
                }

                string FromName = !string.IsNullOrEmpty(mailInfo.FromName) ? mailInfo.FromName : mailInfo.From;

                NotificationManager.QueueMail(PortalController.Instance.GetCurrentSettings().PortalId, 0, HtmlUtils.StripWhiteSpace(mailInfo.Subject, true), mailInfo.Body, To, attachments, FromName, mailInfo.From);
                return "Mail added in Queue";
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return ex.Message;
            }
        }
    }
}
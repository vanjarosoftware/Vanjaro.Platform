using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace Vanjaro.Common
{
    public class SendAttachment
    {
        public static List<Attachment> Attachments(string SerializedAttachments)
        {
            List<Attachment> AttachmentList = new List<Attachment>();
           
            List<Data.Entities.Attachment> attachments = JsonConvert.DeserializeObject<List<Data.Entities.Attachment>>(SerializedAttachments);
            
            if (attachments == null || attachments.Count == 0)
            {
                string[] Files = SerializedAttachments.Split(',');
                foreach (string fileUrl in Files)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            if (File.Exists(fileUrl))
                            {
                                AttachmentList.Add(new Attachment(fileUrl));
                            }
                            else if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(fileUrl)))
                            {
                                AttachmentList.Add(new Attachment(System.Web.Hosting.HostingEnvironment.MapPath(fileUrl)));
                            }
                        }
                    }
                    catch (Exception ex) { Exceptions.LogException(ex); }
                }
            }
            else
            {
                foreach (Data.Entities.Attachment file in attachments)
                {
                    if (!string.IsNullOrEmpty(file.Url))
                    {
                        if (File.Exists(file.Url))
                        {
                            AttachmentList.Add(new Attachment(file.Url));
                        }
                        else if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(file.Url)))
                        {
                            AttachmentList.Add(new Attachment(System.Web.Hosting.HostingEnvironment.MapPath(file.Url)));
                        }
                    }
                    else if (file.BLOB != null)
                    {
                        AttachmentList.Add(new Attachment(new MemoryStream(file.BLOB), file.Name));
                    }

                }
            }
            return AttachmentList;
        }
        
    }
}
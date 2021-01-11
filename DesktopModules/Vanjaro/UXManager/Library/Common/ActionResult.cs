using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Library.Common
{
    public class ActionResult
    {
        public ActionResult()
        {
            Errors = new ExceptionDictionary();
            Warnings = new List<string>();
        }

        public void AddError(string Key, string Message)
        {
            Errors.Add(Key, new Exception(Message));
            this.Message = Message;
        }

        public void AddError(string ResourceKey, string ResourceFile, Exception ex)
        {
            AddError(ResourceKey, ResourceFile, string.Empty, ex);
            if (ex != null && string.IsNullOrEmpty(Message))
            {
                Message = ex.Message;
            }
        }

        public void AddError(string ResourceKey, string ResourceFile, string Message, Exception ex)
        {
            if (!string.IsNullOrEmpty(ResourceKey) && !string.IsNullOrEmpty(ResourceFile) || !string.IsNullOrEmpty(Message))
            {
                string LocalizedMessage = Localization.GetString(ResourceKey, ResourceFile);

                if (string.IsNullOrEmpty(LocalizedMessage))
                {
                    LocalizedMessage = Message;
                }

                if (string.IsNullOrEmpty(LocalizedMessage) && ex != null)
                {
                    LocalizedMessage = ex.Message;
                }

                if (ResourceKey != null && !Errors.ContainsKey(ResourceKey))
                {
                    Errors.Add(ResourceKey, new Exception(LocalizedMessage));
                }

                this.Message = LocalizedMessage;
            }
            Core.Managers.ExceptionManage.LogException(ex);
        }


        public ExceptionDictionary Errors { get; set; }
        public List<string> Warnings { get; set; }
        public dynamic Data { get; set; }
        public bool IsSuccess { get => Errors.Count == 0; set { } }
        public bool HasErrors { get => Errors.Count > 0; set { } }
        public bool HasWarnings { get => Warnings.Count > 0; set { } }
        public string Message { get; set; }
        public string RedirectURL { get; set; }
        public bool IsRedirect => !string.IsNullOrEmpty(RedirectURL);
    }
}
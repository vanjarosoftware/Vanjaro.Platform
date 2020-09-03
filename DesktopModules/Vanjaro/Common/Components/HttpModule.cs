using DotNetNuke.Entities.Controllers;
using DotNetNuke.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Vanjaro.Common.Components.Interfaces;

namespace Vanjaro.Common.Components
{
    public class HttpModule : IHttpModule
    {
        private static volatile bool applicationStarted = false;
        private static readonly object applicationStartLock = new object();
        public void Init(HttpApplication context)
        {
            context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
        }
        private void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (app != null)
            {
                HttpContext context = app.Context;

                //Make sure we have a valid HttpContext and we're not dealing with static requests such as images etc...
                if (context != null && context.CurrentHandler is CDefault)
                {
                    if (!applicationStarted)
                    {
                        lock (applicationStartLock)
                        {
                            if (!applicationStarted)
                            {
                                // this will run only once per application start
                                OnStart(context);
                                applicationStarted = true;
                            }
                        }
                    }
                }
            }
        }
        public virtual void OnStart(HttpContext context)
        {
            UpdateLowerCaseUrlRegex();
            PreCompileRazorTemplates(context);
        }
        private static void PreCompileRazorTemplates(HttpContext context)
        {
            HttpContext.Current.Application.Add("vjPreCompileRazors", true);

            string url = string.Empty;
            if (context.Request.IsSecureConnection)
                url = string.Format("https://{0}", context.Request.Url.Authority);
            else
                url = string.Format("http://{0}", context.Request.Url.Authority);

            Task.Run(() => FetchPreCompileRazorAPI(url + "/DesktopModules/vjCommonAngularBootstrap/API/commonrazor/precompile"));
        }
        private static void FetchPreCompileRazorAPI(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadStringAsync(new Uri(url));
            }
        }
        private void UpdateLowerCaseUrlRegex()
        {
            string PreventLowerCaseUrlRegex = "popUp";
            Dictionary<string, string> Settings = HostController.Instance.GetSettingsDictionary();
            if (Settings.ContainsKey("AUM_PreventLowerCaseUrlRegex"))
            {
                PreventLowerCaseUrlRegex = Settings["AUM_PreventLowerCaseUrlRegex"];

                if (string.IsNullOrEmpty(PreventLowerCaseUrlRegex))
                {
                    PreventLowerCaseUrlRegex = "popUp";
                }
                else if (!string.IsNullOrEmpty(PreventLowerCaseUrlRegex) && !PreventLowerCaseUrlRegex.Contains("popUp"))
                {
                    PreventLowerCaseUrlRegex += "|popUp";
                }

                if (Settings["AUM_PreventLowerCaseUrlRegex"].ToString() != PreventLowerCaseUrlRegex)
                {
                    HostController.Instance.Update("AUM_PreventLowerCaseUrlRegex", PreventLowerCaseUrlRegex);
                }
            }
            else
            {
                HostController.Instance.Update("AUM_PreventLowerCaseUrlRegex", PreventLowerCaseUrlRegex);
            }
        }
        public void Dispose()
        {

        }
    }
}
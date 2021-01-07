using System;
using System.IO;
using System.Web;
using DotNetNuke.Framework;

namespace Vanjaro.Core.Components
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
            HttpContext.Current.Application.Add("PingAnalytics", true);
        }

        public void Dispose()
        {

        }
    }
}
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.Components.Interfaces;

namespace Vanjaro.Common.Engines.UIEngine.AngularBootstrap.Controllers
{
    public class CommonRazorController : DnnApiController
    {
        private readonly static ILog Logger = LoggerSource.Instance.GetLogger(typeof(CommonRazorController));

        [AllowAnonymous]
        [HttpGet]
        public void PreCompile()
        {
            HttpContext.Current.Server.ScriptTimeout = 600;

            //Only Run this if initialized by Common OnStart HttpModule
            if (HttpContext.Current.Application["vjPreCompileRazors"] != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Logger.ErrorFormat("Vanjaro Common Pre Compile Razor Templates Started at {0} .", DateTime.UtcNow.ToString());

                //Immediately remove this since this code can only be called once at startup
                HttpContext.Current.Application.Remove("vjPreCompileRazors");

                List<Type> ExtensionTypes = new List<Type>();
                string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                foreach (string Path in binAssemblies)
                {

                    try
                    {

                        IEnumerable<Type> AssembliesToAdd = from t in Assembly.LoadFrom(Path).GetTypes()
                                                            where typeof(ICommonRazor).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null
                                                            select t;

                        ExtensionTypes.AddRange(AssembliesToAdd.ToList());
                    }
                    catch { continue; }
                }

                foreach (Type t in ExtensionTypes)
                {
                    ICommonRazor RazorInstance = Activator.CreateInstance(t) as ICommonRazor;

                    try
                    {
                        RazorInstance.PreCompile();
                    }
                    catch { }
                }
                stopwatch.Stop();
                Logger.ErrorFormat("Vanjaro Common Pre Compile Razor Templates Completed In {0} Seconds at {1} .", (stopwatch.ElapsedMilliseconds / 1000), DateTime.UtcNow.ToString());
            }
        }
    }
}
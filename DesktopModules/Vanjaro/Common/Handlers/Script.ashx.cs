using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Vanjaro.Common.Engines;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Handlers
{
    /// <summary>
    /// Summary description for Script
    /// </summary>
    public class Script : IHttpHandler
    {

        private string AppName = null;
        private Dictionary<string, object> appProperties;
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.QueryString["appname"] != null)
            {
                AppName = context.Request.QueryString["appname"];
            }
            else
            {
                context.Response.StatusCode = 404;
            }

            //Handle Angular Apps
            if (!string.IsNullOrEmpty(AppName))
            {
                ProcessAngularApp(context);
            }
        }


        private void ProcessAngularApp(HttpContext context)
        {
            string Script = string.Empty;
            if (context != null && context.Application != null && !string.IsNullOrEmpty(AppName) && context.Application["app-" + AppName] != null)
            {
                appProperties = (Dictionary<string, object>)context.Application["app-" + AppName];

                AppName = (appProperties["AppName"] as AppInformation).Name;
                string AppTemplatePath = appProperties["AppTemplatePath"].ToString();
                string AppConfigJS = appProperties["AppConfigJS"].ToString();
                string AppJS = appProperties["AppJS"].ToString();
                string FrameworkTemplatePath = appProperties["FrameworkTemplatePath"].ToString();
                bool ShowMissingKeys = appProperties["ShowMissingKeys"].ToString() == "true" ? true : false;
                List<AngularView> Templates = appProperties["AngularTemplates"] as List<AngularView>;
                string[] Dependencies = appProperties["Dependencies"] as string[];

                Script = GetAngularAppScript(context, AppName, FrameworkTemplatePath, AppTemplatePath, Dependencies, Templates, ShowMissingKeys, AppConfigJS, AppJS);
            }
            context.Response.ContentType = "text/javascript";
            context.Response.Write(Script ?? string.Empty);
        }

        private string GetAngularAppScript(HttpContext context, string AppName, string FrameworkTemplatePath, string AppTemplatePath, string[] Dependencies, List<AngularView> Templates, bool ShowMissingKeys, string AppConfigJS, string AppJS)
        {
            string CacheKey = AppName + "_ScriptHandler_" + AppTemplatePath;
            string CachedScript = Utilities.DataCache.GetItemFromCache<string>(CacheKey);
            if (CachedScript == null)
            {
                string AppTemplateDir = context.Server.MapPath(AppTemplatePath);

                string FrameworkTemplateDir = context.Server.MapPath(FrameworkTemplatePath + "Views/");
                string SharedResourceFile = FrameworkTemplateDir + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "\\Shared.resx";
                string SharedAppTemplateFile = AppTemplateDir + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "\\Shared.resx";

                if (Directory.Exists(AppTemplateDir))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("(function() {");
                    string appJS = AngularBootstrapUIEngine.GetConfig(AppName, FrameworkTemplatePath, AppTemplatePath, Dependencies, Templates, AppConfigJS, AppJS, SharedAppTemplateFile, ShowMissingKeys);

                    //Angular bootstrap common
                    foreach (string file in Directory.EnumerateFiles(FrameworkTemplateDir, "*.js", SearchOption.AllDirectories))
                    {
                        string ResourceFile = file.Split('\\').Last();
                        string ResourcePath = file.TrimEnd(ResourceFile.ToCharArray());
                        ResourceFile = ResourcePath + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "\\" + ResourceFile.Substring(0, ResourceFile.LastIndexOf(".js")) + ".resx";

                        sb.Append(new DNNLocalizationEngine(ResourceFile, SharedResourceFile, ShowMissingKeys).Parse(File.ReadAllText(file) + Environment.NewLine));

                    }
                    //Add individual js files
                    foreach (string file in Directory.EnumerateFiles(AppTemplateDir, "*.js", SearchOption.AllDirectories))
                    {
                        string ResourceFile = file.Split('\\').Last();
                        string ResourcePath = file.TrimEnd(ResourceFile.ToCharArray());
                        ResourceFile = ResourcePath + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "\\" + ResourceFile.Substring(0, ResourceFile.LastIndexOf(".js")) + ".resx";

                        sb.Append(new DNNLocalizationEngine(ResourceFile, SharedAppTemplateFile, ShowMissingKeys).Parse(File.ReadAllText(file) + Environment.NewLine));

                    }
                    sb.Append("})();");
                    CachedScript = appJS + Environment.NewLine + sb.ToString();
                }
                DataCache.SetCache<string>(CachedScript, CacheKey);
            }
            return CachedScript;
        }

        public bool IsReusable => false;
    }
}
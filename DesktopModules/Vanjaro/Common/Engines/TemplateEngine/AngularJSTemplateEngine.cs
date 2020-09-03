
using System.IO;
using System.Web.Hosting;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Globals;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Engines.TemplateEngine
{
    public class AngularJSTemplateEngine : ITemplateEngine
    {
        /// <summary>
        /// Cache Key to be used as prefix for AngularJS Templates within DNN Cache
        /// </summary>
        private const string CacheKey = "AngTemplate-";
        private readonly DNNContext DNNContext;

        /// <summary>
        /// Initializes AngularJSTemplateEngine
        /// </summary>
        /// <param name="DNNContext"></param>
        public AngularJSTemplateEngine(DNNContext DNNContext)
        {
            this.DNNContext = DNNContext;
        }

        /// <summary>
        /// Loads the template, parses it for DNN Tokens, and returns a cached view
        /// </summary>
        /// <param name="DNNContext"></param>
        /// <param name="TemplatePath"></param>
        /// <returns></returns>
        public string RenderTemplatePath(string TemplatePath)
        {
            string Template = DataCache.GetItemFromCache<string>(CacheKey + DNNContext.ModuleInfo.ModuleID + TemplatePath);

            if (Template == null)
            {
                File.ReadAllText(HostingEnvironment.MapPath(TemplatePath));
            }
            else
            {
                return Template; //Return existing parsed and cached template
            }

            Template = new DNNTokenEngine(DNNContext).Parse(Template);

            DataCache.SetCache<string>(Template, CacheKey + TemplatePath);

            return Template;
        }


        public string Render(string Template)
        {
            if (Template == null)
            {
                return Template;
            }

            return new DNNTokenEngine(DNNContext).Parse(Template);
        }
    }
}
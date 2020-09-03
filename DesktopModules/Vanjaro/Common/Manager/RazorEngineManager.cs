using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;

namespace Vanjaro.Common.Manager
{
    public static class RazorEngineManager
    {

        public static string RenderTemplate(string Identifier, string TemplateDir, string TemplateName, object Model)
        {
            return RenderTemplate((TemplateDir + TemplateName) + ".cshtml", Model);
        }

        private static string RenderTemplate(string virtualPath, dynamic model)
        {
            var page = WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var pageContext = new WebPageContext(httpContext, page, model);

            using (var writer = new StringWriter())
            {

                if (page is WebPage)
                    page.ExecutePageHierarchy(pageContext, writer);
                else
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must inherit from System.Web.WebPages.WebPage", virtualPath));

                return writer.ToString();
            }
        }
    }
}
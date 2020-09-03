using System.Web;

namespace Vanjaro.Common.Handlers
{
    /// <summary>
    /// Summary description for Template
    /// </summary>
    public class Template : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //Must have Engine=AngularJSTemplate
            //Switch on Engine
            //Call AngularJSTemplateEngine.Render() and return
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable => false;
    }
}
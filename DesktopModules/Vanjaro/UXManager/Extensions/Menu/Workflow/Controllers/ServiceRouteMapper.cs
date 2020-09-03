using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Workflow.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Workflow", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Workflow.Controllers" });
        }
    }
}
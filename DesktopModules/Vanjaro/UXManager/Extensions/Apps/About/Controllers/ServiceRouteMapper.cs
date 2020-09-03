using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.About.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("About", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.About.Controllers" });
        }
    }
}
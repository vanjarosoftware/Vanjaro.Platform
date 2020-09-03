using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.YouTube.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("YouTube", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.YouTube.Controllers" });
        }
    }
}
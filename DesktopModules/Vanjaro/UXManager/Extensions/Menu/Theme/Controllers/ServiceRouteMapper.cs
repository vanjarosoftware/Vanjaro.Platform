using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Theme.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Theme", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Theme.Controllers" });
        }
    }
}
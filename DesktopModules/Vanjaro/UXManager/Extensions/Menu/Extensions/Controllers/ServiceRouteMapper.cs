using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Extensions", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers" });
        }
    }
}
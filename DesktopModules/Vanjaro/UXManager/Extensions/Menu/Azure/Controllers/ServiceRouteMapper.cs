using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Azure.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Azure", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Azure.Controllers" });
        }
    }
}
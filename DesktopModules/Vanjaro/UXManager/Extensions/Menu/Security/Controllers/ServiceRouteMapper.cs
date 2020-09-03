using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Security.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Security", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Security.Controllers" });
        }
    }
}
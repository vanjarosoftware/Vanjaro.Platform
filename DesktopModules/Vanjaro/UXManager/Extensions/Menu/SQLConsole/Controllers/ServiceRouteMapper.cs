using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.SQLConsole.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("SQLConsole", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.SQLConsole.Controllers" });
        }
    }
}
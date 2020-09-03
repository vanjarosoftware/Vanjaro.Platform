using DotNetNuke.Web.Api;


namespace Vanjaro.UXManager.Extensions.Menu.Help.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Help", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Help.Controllers" });
        }
    }
}


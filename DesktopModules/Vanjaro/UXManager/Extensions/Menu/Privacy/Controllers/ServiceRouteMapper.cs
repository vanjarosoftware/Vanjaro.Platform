using DotNetNuke.Web.Api;


namespace Vanjaro.UXManager.Extensions.Menu.Privacy.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Privacy", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Privacy.Controllers" });
        }
    }
}


using DotNetNuke.Web.Api;


namespace Vanjaro.UXManager.Extensions.Menu.Roles.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Roles", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Roles.Controllers" });
        }
    }
}


using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Users", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Users.Controllers" });
        }
    }
}
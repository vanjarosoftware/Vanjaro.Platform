using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Memberships.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Memberships", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Memberships.Controllers" });
        }
    }
}
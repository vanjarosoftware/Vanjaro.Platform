using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Domain.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Domain", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Domain.Controllers" });
        }
    }
}
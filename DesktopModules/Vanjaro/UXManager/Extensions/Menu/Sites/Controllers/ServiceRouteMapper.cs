using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Sites.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Sites", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Sites.Controllers" });
        }
    }
}
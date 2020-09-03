using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.CustomCSS.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("CustomCSS", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.CustomCSS.Controllers" });
        }
    }
}
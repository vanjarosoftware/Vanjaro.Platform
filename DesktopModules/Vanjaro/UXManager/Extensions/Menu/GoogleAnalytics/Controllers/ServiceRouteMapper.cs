using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("GoogleAnalytics", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.Controllers" });
        }
    }
}
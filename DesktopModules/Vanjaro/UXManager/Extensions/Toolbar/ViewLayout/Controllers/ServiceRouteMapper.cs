using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Toolbar.ViewLayout.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("ViewLayout", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Toolbar.ViewLayout.Controllers" });
        }
    }
}
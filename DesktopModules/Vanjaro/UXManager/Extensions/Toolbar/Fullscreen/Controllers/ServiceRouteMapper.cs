using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Toolbar.Fullscreen.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Fullscreen", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Toolbar.Fullscreen.Controllers" });
        }
    }
}
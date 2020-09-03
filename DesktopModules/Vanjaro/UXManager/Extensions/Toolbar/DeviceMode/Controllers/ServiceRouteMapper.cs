using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Toolbar.DeviceMode.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("DeviceMode", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Toolbar.DeviceMode.Controllers" });
        }
    }
}
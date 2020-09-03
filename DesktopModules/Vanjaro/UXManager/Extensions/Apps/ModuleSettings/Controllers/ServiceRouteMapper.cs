using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.ModuleSettings.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("ModuleSettings", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.ModuleSettings.Controllers" });
        }
    }
}
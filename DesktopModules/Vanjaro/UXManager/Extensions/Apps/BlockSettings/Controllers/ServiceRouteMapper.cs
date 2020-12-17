using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.BlockSettings.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BlockSettings", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.BlockSettings.Controllers" });
        }
    }
}
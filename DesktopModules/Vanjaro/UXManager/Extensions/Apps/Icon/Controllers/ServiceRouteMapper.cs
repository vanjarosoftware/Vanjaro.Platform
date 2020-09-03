using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.Block.Icon.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(AppInfo.Name, "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.Block.Icon.Controllers" });
        }
    }
}
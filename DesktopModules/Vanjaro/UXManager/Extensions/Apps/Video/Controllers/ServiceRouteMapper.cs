using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.Video.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Video", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.Video.Controllers" });
        }
    }
}
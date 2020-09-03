using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.Link.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Link", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.Link.Controllers" });
        }
    }
}
using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.Image.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Image", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.Image.Controllers" });
        }
    }
}
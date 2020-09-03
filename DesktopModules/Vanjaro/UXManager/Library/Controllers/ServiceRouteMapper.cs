using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Library.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Vanjaro", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Library.Controllers" });
        }
    }
}
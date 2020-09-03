using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Toolbar.Preview.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Preview", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Toolbar.Preview.Controllers" });
        }
    }
}
using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Toolbar.Navigator.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Navigator", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Toolbar.Navigator.Controllers" });
        }
    }
}
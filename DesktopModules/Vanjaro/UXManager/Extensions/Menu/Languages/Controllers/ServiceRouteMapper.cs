using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Languages", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Languages.Controllers" });
        }
    }
}
using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("ThemeBuilder", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Controllers" });
        }
    }
}
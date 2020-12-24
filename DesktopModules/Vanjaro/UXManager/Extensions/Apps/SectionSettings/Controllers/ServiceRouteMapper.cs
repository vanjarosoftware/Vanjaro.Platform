using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Apps.SectionSettings.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("SectionSettings", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.SectionSettings.Controllers" });
        }
    }
}
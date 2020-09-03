using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Apps.LogsSettings.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("LogsSettings", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Apps.LogsSettings.Controllers" });
        }
    }
}
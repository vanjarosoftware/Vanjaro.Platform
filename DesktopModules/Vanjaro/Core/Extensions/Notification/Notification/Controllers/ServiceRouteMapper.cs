using DotNetNuke.Web.Api;

namespace Vanjaro.Core.Extensions.Notification.Notification.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Notification", "default", "{controller}/{action}", new[] { "Vanjaro.Core.Extensions.Notification.Notification.Controllers" });
        }
    }
}
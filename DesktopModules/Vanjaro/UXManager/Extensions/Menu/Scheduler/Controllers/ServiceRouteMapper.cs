using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Scheduler", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Scheduler.Controllers" });
        }
    }
}
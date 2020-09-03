using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Menu.Logs.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Logs", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Logs.Controllers" });
        }
    }
}
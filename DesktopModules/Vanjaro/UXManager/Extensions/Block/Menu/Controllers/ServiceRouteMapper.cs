using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.Menu.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Menu", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.Menu.Controllers" });
        }
    }
}
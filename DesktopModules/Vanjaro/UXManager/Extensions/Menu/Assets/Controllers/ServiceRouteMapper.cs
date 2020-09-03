using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Menu.Assets.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Assets", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Assets.Controllers" });
        }
    }
}
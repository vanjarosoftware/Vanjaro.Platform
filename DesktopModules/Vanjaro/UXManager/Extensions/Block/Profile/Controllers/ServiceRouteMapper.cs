using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.Profile.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Profile", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.Profile.Controllers" });
        }
    }
}
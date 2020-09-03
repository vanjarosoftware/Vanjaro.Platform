using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.Logo.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Logo", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.Logo.Controllers" });
        }
    }
}
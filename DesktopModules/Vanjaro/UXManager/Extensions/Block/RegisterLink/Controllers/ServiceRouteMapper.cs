using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.RegisterLink.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("RegisterLink", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.RegisterLink.Controllers" });
        }
    }
}
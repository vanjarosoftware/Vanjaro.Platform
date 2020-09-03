using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.Login.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Login", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.Login.Controllers" });
        }
    }
}
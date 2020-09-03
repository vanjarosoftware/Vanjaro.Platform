using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.Register.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Register", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.Register.Controllers" });
        }
    }
}
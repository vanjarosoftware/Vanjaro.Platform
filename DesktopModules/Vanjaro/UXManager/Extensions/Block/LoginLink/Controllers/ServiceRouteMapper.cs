using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.LoginLink.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("LoginLink", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.LoginLink.Controllers" });
        }
    }
}
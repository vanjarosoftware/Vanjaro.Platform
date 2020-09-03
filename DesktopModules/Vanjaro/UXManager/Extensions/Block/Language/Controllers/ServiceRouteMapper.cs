using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.BlockLanguage.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Language", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.BlockLanguage.Controllers" });
        }
    }
}
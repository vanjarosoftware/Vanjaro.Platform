using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.SearchInput.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("SearchInput", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.SearchInput.Controllers" });
        }
    }
}
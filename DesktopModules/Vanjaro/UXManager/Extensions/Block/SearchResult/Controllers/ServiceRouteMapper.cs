using DotNetNuke.Web.Api;
namespace Vanjaro.UXManager.Extensions.Block.SearchResult.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("SearchResult", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Block.SearchResult.Controllers" });
        }
    }
}
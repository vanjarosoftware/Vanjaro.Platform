using DotNetNuke.Web.Api;
namespace Vanjaro.Core.Providers.Authentication.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Authentication", "default", "{controller}/{action}", new[] { "Vanjaro.Core.Providers.Authentication.Controllers" });
        }
    }
}
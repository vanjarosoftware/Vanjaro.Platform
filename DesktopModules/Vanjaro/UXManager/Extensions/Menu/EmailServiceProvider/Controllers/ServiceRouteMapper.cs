using DotNetNuke.Web.Api;


namespace Vanjaro.UXManager.Extensions.Menu.EmailServiceProvider.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("EmailServiceProvider", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.EmailServiceProvider.Controllers" });
        }
    }
}


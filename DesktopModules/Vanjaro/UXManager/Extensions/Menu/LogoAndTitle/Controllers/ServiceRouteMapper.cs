using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("LogoAndTitle", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.Controllers" });
        }
    }
}


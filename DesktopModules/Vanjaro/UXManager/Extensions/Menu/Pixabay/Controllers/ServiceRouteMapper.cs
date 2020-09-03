using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.Pixabay.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Pixabay", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.Pixabay.Controllers" });
        }
    }
}
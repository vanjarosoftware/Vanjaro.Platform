using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Menu.MemberProfile.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("MemberProfile", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.MemberProfile.Controllers" });
        }
    }
}
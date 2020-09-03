using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Extensions.Toolbar.PageSetting.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("VersionManagement", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Controllers" });
        }
    }
}
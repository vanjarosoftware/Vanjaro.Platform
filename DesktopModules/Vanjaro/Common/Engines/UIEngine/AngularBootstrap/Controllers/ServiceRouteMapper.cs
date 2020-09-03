using DotNetNuke.Web.Api;

namespace Vanjaro.Common.Engines.UIEngine.AngularBootstrap.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("vjCommonAngularBootstrap", "default", "{controller}/{action}", new[] { "Vanjaro.Common.Engines.UIEngine.AngularBootstrap.Controllers" });
        }

    }
}
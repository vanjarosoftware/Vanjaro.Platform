using DotNetNuke.Web.Api;

namespace Vanjaro.Core.Extensions.Workflow.Review.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Review", "default", "{controller}/{action}", new[] { "Vanjaro.Core.Extensions.Workflow.Review.Controllers" });
        }
    }
}
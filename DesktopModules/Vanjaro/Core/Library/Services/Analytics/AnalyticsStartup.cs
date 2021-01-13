using DotNetNuke.Web.Api;
using System.Web;

namespace Vanjaro.Core.Services
{
    public class AnalyticsStartup : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            HttpContext.Current.Application.Add("PingAnalytics", true);
        }
    }
}
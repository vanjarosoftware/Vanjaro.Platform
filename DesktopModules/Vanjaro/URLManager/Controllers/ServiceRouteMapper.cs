using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.URL.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
	{
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("URLManager", "default", "{controller}/{action}", new[] { "Vanjaro.URL.Controllers" });
        }
	}
}
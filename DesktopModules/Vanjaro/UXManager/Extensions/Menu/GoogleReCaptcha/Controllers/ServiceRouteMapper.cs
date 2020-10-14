using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.UXManager.Extensions.Menu.GoogleReCaptcha.Controllers
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("GoogleReCaptcha", "default", "{controller}/{action}", new[] { "Vanjaro.UXManager.Extensions.Menu.GoogleReCaptcha.Controllers" });
        }
    }
}
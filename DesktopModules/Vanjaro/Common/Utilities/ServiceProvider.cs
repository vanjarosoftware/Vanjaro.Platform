using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Common.Utilities
{
    public class ServiceProvider
    {
        public static INavigationManager NavigationManager
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var scope = HttpContext.Current.GetScope();

                    if (scope != null)
                    {
                        return scope.ServiceProvider.GetService(typeof(INavigationManager)) as INavigationManager;
                    }
                }

                Exceptions.LogException(new Exception("Navigation Manager is not available; returning dummy navigation manager"));
                return new DummyNavigationManager();
                
            }
        }
    }
    public class DummyNavigationManager : INavigationManager
    {
        public string NavigateURL()
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, bool isSuperTab)
        {
            return string.Empty;
        }

        public string NavigateURL(string controlKey)
        {
            return string.Empty;
        }

        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, string controlKey)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return string.Empty;
        }

        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return string.Empty;
        }
    }
}
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Vanjaro.Core.Data.Entities;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Library.Controllers
{
    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class PageController : DnnApiController
    {
        [HttpPost]
        [DnnPageEditor]
        public dynamic Save(dynamic Data)
        {
            return Vanjaro.Core.Managers.PageManager.Update(this.PortalSettings, Data);
        }

        [HttpGet]
        [DnnPageEditor]
        public Pages Get()
        {
            var page = Vanjaro.Core.Managers.PageManager.GetLatestVersion(PortalSettings.ActiveTab.TabID, PortalSettings.CultureCode, true);
            Core.Managers.PageManager.ApplyGlobalBlockJSON(page);
            return page;
        }

        [HttpPost]
        [DnnPageEditor]
        public int AddModule()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Form != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["UniqueID"]) && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["DesktopModuleId"]))
            {
                return PageManager.AddModule(PortalSettings, int.Parse(HttpContext.Current.Request.Form["UniqueID"]), int.Parse(HttpContext.Current.Request.Form["DesktopModuleId"]));
            }
            else
            {
                return 0;
            }
        }

        [HttpGet]
        [DnnPageEditor]
        public dynamic GetApps()
        {
            return PageManager.GetApps(PortalSettings.ActiveTab.PortalID);
        }

        [HttpGet]
        [DnnPageEditor]
        public List<PageItem> GetPages()
        {
            return PageManager.GetPages(PortalSettings.ActiveTab.PortalID);
        }

        [HttpGet]
        [DnnPageEditor]
        public string GetPageUrl(int TabID)
        {
            return PageManager.GetPageUrl(PortalSettings, TabID);
        }

        [HttpGet]
        [DnnPageEditor]
        public void Delete(bool m2v = false)
        {
            if (m2v)
            {
                Vanjaro.Core.Managers.PageManager.Delete(PortalSettings.ActiveTab.TabID);
                DotNetNuke.Common.Utilities.DataCache.ClearModuleCache(PortalSettings.ActiveTab.TabID);
            }
        }
    }
}
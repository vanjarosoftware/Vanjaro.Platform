using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System.Collections.Generic;
using System.Dynamic;
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
            double dataLength = (HttpContext.Current.Request.ContentLength / 1024f) / 1024f;
            if (dataLength < 2)
                return Vanjaro.Core.Managers.PageManager.Update(this.PortalSettings, Data);
            else
            {
                dynamic result = new ExpandoObject();
                result.IsSuccess = false;
                result.Message = "An error occurred(Code: 1001). Your changes were not saved.";
                return result;
            }
        }

        [HttpGet]
        [DnnPageEditor]
        public Pages Get()
        {
            var page = Vanjaro.Core.Managers.PageManager.GetLatestVersion(PortalSettings.ActiveTab.TabID, PortalSettings.CultureCode, true);
            Core.Managers.PageManager.ApplyGlobalBlockJSON(page);
            Core.Managers.PageManager.ApplyBlockJSON(page);
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
        public string GetPageUrl(int TbID, bool AbsolutelLink)
        {
            string result = PageManager.GetPageUrl(PortalSettings, TbID, AbsolutelLink);
            if (string.IsNullOrEmpty(result))
                result = "/";
            return result;
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
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Library.Controllers
{
    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class BlockController : DnnApiController
    {
        [HttpPost]
        [DnnAdmin]
        public dynamic AddCustomBlock(CustomBlock CustomBlock)
        {
            return Core.Managers.BlockManager.Add(PortalSettings, CustomBlock);
        }

        [HttpPost]
        [DnnAdmin]
        public dynamic EditCustomBlock(CustomBlock CustomBlock)
        {
            return Core.Managers.BlockManager.Edit(PortalSettings, CustomBlock);
        }

        [HttpPost]
        [DnnAdmin]
        public dynamic DeleteCustomBlock(string CustomBlockGuid)
        {
            return Core.Managers.BlockManager.Delete(PortalSettings.ActiveTab.PortalID, CustomBlockGuid);
        }

        [HttpGet]
        [DnnPageEditor]
        public List<CustomBlock> GetAllCustomBlock()
        {
            return Core.Managers.BlockManager.GetAll(PortalSettings);
        }

        [HttpGet]
        [DnnPageEditor]
        public List<Block> GetAll()
        {
            return Core.Managers.BlockManager.GetAll();
        }

        [HttpPost]
        [DnnPageEditor]
        public ThemeTemplateResponse Render()
        {
            Dictionary<string, string> Attributes = new Dictionary<string, string>();
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                Attributes.Add(key, HttpContext.Current.Request.Form[key]);
            }

            return Core.Managers.BlockManager.Render(Attributes);
        }

        [HttpPost]
        [AllowAnonymous]
        public List<ThemeTemplateResponse> RenderMarkup(dynamic mappedAttr)
        {
            Dictionary<string, string> Attributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(mappedAttr.ToString());

            HtmlDocument html = new HtmlDocument();
            Pages Pages = PageManager.GetLatestVersion(PortalSettings.ActiveTab.TabID, PageManager.GetCultureCode(PortalSettings));
            html.LoadHtml(Pages.Content.ToString());
            return Managers.BlockManager.FindBlocks(Pages, html, Attributes);
        }
    }
}
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.UXManager.Library.Common;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Library.Controllers
{
    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class UXManagerController : DnnApiController
    {
        [HttpPost]
        public ActionResult Search(string Keyword)
        {
            ActionResult ActionResult = new ActionResult();
            if (!string.IsNullOrEmpty(Keyword))
            {
                List<CategoryTree> Data = MenuManager.ParseMenuCategoryTree(Keyword).Where(x => !string.IsNullOrEmpty(x.GUID)).Select(s => { s.ParentID = null; s.Level = 0; return s; }).OrderBy(o => o.Name).ToList();
                ActionResult.Data = MenuManager.RenderMenu(Data, Keyword);
            }
            else
            {
                ActionResult.Data = "";// MenuManager.RenderMenu(MenuManager.ParseMenuCategoryTree(null), Keyword);
            }

            return ActionResult;
        }
    }
}
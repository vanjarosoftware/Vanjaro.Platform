using DotNetNuke.Entities.Host;
using DotNetNuke.Web.Api;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Menu.Extensions.Factories;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class UploadController : WebApiController
    {
        [HttpGet]
        public dynamic GetFiles(int folderid, string uid, int skip, int pagesize, string keyword)
        {
            if (string.IsNullOrEmpty(uid) || uid == "null")
            {
                return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, uid, "zip", skip, pagesize, keyword);
            }
            else
            {
                return BrowseUploadFactory.GetPagedFiles(0, new TreeView() { Value = folderid }, uid, null, skip, pagesize, keyword);
            }
        }

        [HttpPost]
        public string Files(string Identifier)
        {
            return Files(Identifier, null);
        }

        [HttpPost]
        public string Files(string Identifier, string uid)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(Identifier) && PortalSettings != null && UserInfo != null)
            {
                dynamic files = HttpContext.Current.Request.Files;
                HttpPostedFile file = files[0];
                result = file.FileName;
            }
            return result;
        }
        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
using Dnn.PersonaBar.Sites.Services.Dto;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Sites.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Sites.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class AddController : UIEngineController
    {
        internal static List<IUIData> GetData()
        {
            CreatePortalRequest request = new CreatePortalRequest
            {
                UseCurrentUserAsAdmin = true
            };
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "CreatePortalRequest", new UIData { Name = "CreatePortalRequest", Options = request } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Create(CreatePortalRequest Data)
        {
            CreatePortalRequest request = new CreatePortalRequest
            {
                SiteName = Data.SiteName,
                SiteAlias = Data.SiteAlias,
                IsChildSite = false,
                HomeDirectory = "Portals/[PortalID]",
                UseCurrentUserAsAdmin = Data.UseCurrentUserAsAdmin
            };
            if (!request.UseCurrentUserAsAdmin)
            {
                request.Username = Data.Email;
                request.Email = Data.Email;
                request.Firstname = Data.Firstname;
                request.Lastname = Data.Lastname;
                request.Password = Data.Password;
                request.PasswordConfirm = Data.PasswordConfirm;
            }
            return SitesManager.CreatePortal(request);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
using Dnn.PersonaBar.Sites.Services.Dto;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public ActionResult Create(dynamic Data)
        {
            ActionResult actionResult = new ActionResult();
            string SiteAlias = Data.PortalRequest.SiteAlias.Value;
            Regex regex = new Regex("(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]");
            if (!string.IsNullOrEmpty(SiteAlias) && regex.Match(Data.PortalRequest.SiteAlias.Value).Value != SiteAlias.Replace("http://", "").Replace("https://", "").TrimEnd('/'))
                actionResult.AddError("SiteDomainError", Localization.GetString("SiteDomainError", Components.Constants.LocalResourcesFile));

            if (actionResult.IsSuccess)
            {
                CreatePortalRequest request = new CreatePortalRequest
                {
                    SiteName = Data.PortalRequest.SiteName.Value,
                    SiteAlias = Data.PortalRequest.SiteAlias.Value,
                    IsChildSite = false,
                    HomeDirectory = "Portals/[PortalID]",
                    UseCurrentUserAsAdmin = Data.PortalRequest.UseCurrentUserAsAdmin.Value
                };
                if (!request.UseCurrentUserAsAdmin)
                {
                    request.Username = Data.PortalRequest.Email.Value;
                    request.Email = Data.PortalRequest.Email.Value;
                    request.Firstname = Data.PortalRequest.Firstname.Value;
                    request.Lastname = Data.PortalRequest.Lastname.Value;
                    request.Password = Data.PortalRequest.Password.Value;
                    request.PasswordConfirm = Data.PortalRequest.PasswordConfirm.Value;
                }
                actionResult = SitesManager.CreatePortal(request, Data.SiteTemplate);
            }

            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
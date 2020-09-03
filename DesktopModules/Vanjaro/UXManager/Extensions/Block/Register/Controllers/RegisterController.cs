using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Block.Register.Entities;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.UXManager.Extensions.Block.Register.Managers;

namespace Vanjaro.UXManager.Extensions.Block.Register.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class RegisterController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "ShowLabel", new UIData { Name = "ShowLabel", Value = "false" } },
                { "TermsPrivacy", new UIData { Name = "TermsPrivacy", Value = "false" } },
                { "ButtonAlign", new UIData { Name = "ButtonAlign", Value = "justify" } },
                { "Global", new UIData { Name = "Global", Value = "false" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "register") } },
                { "IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        [DnnPageEditor]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public void Update(Dictionary<string, string> Attributes)
        {
            Core.Managers.BlockManager.UpdateDesignElement(PortalSettings, Attributes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "anonymous")]
        public ActionResult Index(RegisterDetails RegisterDetails)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                RegisterManager.MapRegisterDetail(RegisterDetails);
                if (RegisterManager.Validate())
                {
                    if (PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                    {
                        actionResult = RegisterManager.CreateUser(RegisterDetails);
                    }
                    else
                    {
                        actionResult.AddError("Registration_NotAllowed", "User registration is not allowed.");
                    }
                }
                else
                {
                    if (RegisterManager.CreateStatus != UserCreateStatus.AddUser)
                    {
                        actionResult.AddError("Registration_Failed", UserController.GetUserCreateStatus(RegisterManager.CreateStatus));
                    }
                }
            }
            catch (Exception ex)
            {
                actionResult.AddError("Register_Error", ex.Message);
            }
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
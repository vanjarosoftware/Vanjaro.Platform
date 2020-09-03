using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Languages.Components;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class TranslatorController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, UserInfo UserInfo, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string cultureCode = string.Empty;
            int lid = -1;
            try
            {
                lid = int.Parse(Parameters["lid"]);
            }
            catch { }
            Locale language = LocaleController.Instance.GetLocale(lid);
            string SelectedRoles = "";
            if (language != null)
            {
                SelectedRoles = PortalController.GetPortalSetting($"DefaultTranslatorRoles-{language.Code}", PortalSettings.PortalId, "Administrators");
            }
            Settings.Add("SelectedRoles", new UIData { Name = "SelectedRoles", Options = new ArrayList(SelectedRoles.Split(';')) });
            Settings.Add("LanguageID", new UIData { Name = "LanguageID", Options = lid });
            Settings.Add("RoleGroups", new UIData { Name = "RoleGroups", Options = Vanjaro.Common.Factories.Factory.RoleFactory.GetAllRoleGroups(PortalSettings.PortalId, ""), OptionsValue = "Id", OptionsText = "Name", Value = "-2" });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public ActionResult GetRoles(int groupId, int lid)
        {
            string ResourcesFile = "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/Setting/App_LocalResources/Translator.resx";
            ActionResult actionResult = new ActionResult();
            try
            {
                if (!UserInfo.IsInRole("Administrators"))
                {
                    actionResult.AddError("AuthFailureMessage", Constants.AuthFailureMessage);
                    return actionResult;
                }

                Locale language = LocaleController.Instance.GetLocale(lid);
                if (language == null)
                {
                    actionResult.AddError("InvalidLocale.ErrorMessage", string.Format(Localization.GetString("InvalidLocale.ErrorMessage", ResourcesFile), language.Code));
                    return actionResult;
                }
                string defaultRoles = PortalController.GetPortalSetting($"DefaultTranslatorRoles-{language.Code}", PortalSettings.PortalId, "Administrators");
                ArrayList selectedRoleNames = new ArrayList(defaultRoles.Split(';'));

                var roles = (groupId < Null.NullInteger
                                    ? RoleController.Instance.GetRoles(PortalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                                    : RoleController.Instance.GetRoles(PortalSettings.PortalId, r => r.RoleGroupID == groupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved))
                                    .Select(r => new
                                    {
                                        r.RoleID,
                                        r.RoleName,
                                        Selected = selectedRoleNames.Contains(r.RoleName)
                                    });
                actionResult.Data = roles;
                actionResult.IsSuccess = true;
            }
            catch (Exception exc)
            {
                actionResult.AddError("", "", exc);
            }
            return actionResult;
        }

        [HttpPost]
        public ActionResult UpdateRoles(int lid, ArrayList Roles)
        {
            string ResourcesFile = "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/Setting/App_LocalResources/Translator.resx";
            ActionResult actionResult = new ActionResult();
            try
            {
                if (!UserInfo.IsInRole("Administrators"))
                {
                    actionResult.AddError("AuthFailureMessage", Constants.AuthFailureMessage);
                    return actionResult;
                }
                Locale language = LocaleController.Instance.GetLocale(lid);
                if (language == null)
                {
                    actionResult.AddError("InvalidLocale.ErrorMessage", string.Format(Localization.GetString("InvalidLocale.ErrorMessage", ResourcesFile), language.Code));
                    return actionResult;
                }
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, $"DefaultTranslatorRoles-{language.Code}", string.Join(";", Roles.Cast<string>().ToArray()));

                actionResult.IsSuccess = true;
            }
            catch (Exception exc)
            {
                actionResult.AddError("", "", exc);
            }
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
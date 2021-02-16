using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Languages.Components;
using Vanjaro.UXManager.Extensions.Menu.Languages.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    [ValidateAntiForgeryToken]
    public class LanguagesController : UIEngineController
    {
        internal static List<IUIData> GetData(PortalSettings PortalSettings, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Languages", new UIData { Name = "Languages", Options = LanguagesManager.GetLanguages(PortalSettings, UserInfo) } },
                { "IsSuperUser", new UIData { Name = "IsSuperUser", Value = UserInfo.IsSuperUser.ToString() } }
            };
            return Settings.Values.ToList();
        }
        [HttpGet]
        public ActionResult Enabled(int lid)
        {
            string ResourcesFile = "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/Setting/App_LocalResources/Languages.resx";
            ActionResult actionResult = new ActionResult();
            Locale language = LocaleController.Instance.GetLocale(lid);
            if (language == null)
            {
                actionResult.AddError("InvalidLocale.ErrorMessage", string.Format(Localization.GetString("InvalidLocale.ErrorMessage", ResourcesFile), language.Code));
                return actionResult;
            }
            if (PortalSettings.DefaultLanguage != language.Code)
            {
                if (LanguagesManager.IsLanguageEnabled(PortalSettings.PortalId, language.Code))
                {
                    //remove language from portal
                    Localization.RemoveLanguageFromPortal(PortalSettings.PortalId, language.LanguageId);
                    LanguagesManager.SetTabUrlsActiveToRedirect(language.LanguageId);

                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.PortalId);
                    actionResult.RedirectURL = Common.Utilities.ServiceProvider.NavigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID,
                                this.PortalSettings.ActiveTab.IsSuperTab,
                                this.PortalSettings, "", defaultLocale.Code);
                }
                else
                {
                    //Add language to portal
                    Localization.AddLanguageToPortal(PortalSettings.PortalId, language.LanguageId, true);
                    LanguagesManager.UpdateTabUrlsDefaultLocale();
                }
            }

            List<LanguageRequest> languageRequests = LanguagesManager.GetLanguages(PortalSettings, UserInfo);
            if (languageRequests.Where(x => x.Enabled).Count() > 1)
                actionResult.RedirectURL = Common.Utilities.ServiceProvider.NavigationManager.NavigateURL();
            actionResult.Data = languageRequests;
            actionResult.IsSuccess = true;
            return actionResult;

        }

        [HttpGet]
        public ActionResult GetLanguages()
        {
            ActionResult actionResult = new ActionResult();

            if (!UserInfo.IsSuperUser)
            {
                actionResult.AddError("AuthFailureMessage", Constants.AuthFailureMessage);
                return actionResult;
            }
            actionResult.Data = LanguagesManager.GetLanguages(PortalSettings, UserInfo);
            actionResult.IsSuccess = true;
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
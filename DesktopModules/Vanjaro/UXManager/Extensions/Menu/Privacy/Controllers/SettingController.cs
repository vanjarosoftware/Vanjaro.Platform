using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Privacy.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class SettingController : UIEngineController
    {
        private const string AuthFailureMessage = "Authorization has been denied for this request.";
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            List<KeyValuePair<string, int>> userFilters = new List<KeyValuePair<string, int>>();
            SettingController sc = new SettingController();
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            dynamic data = sc.GetPrivacySettings(PortalSettings.Current.PortalId).Data;
            Settings.Add("UpdatePrivacy", new UIData { Name = "UpdatePrivacy", Value = "", Options = new UpdatePrivacySettingsRequest { } });
            Settings.Add("GetPrivacy", new UIData { Name = "GetPrivacy", Value = "", Options = data });
            Settings.Add("HardDelete", new UIData { Name = "HardDelete", Options = BindHardDelete().Select(a => new { a.Key, a.Value }), OptionsText = "Value", OptionsValue = "Key", Value = string.IsNullOrEmpty(data.Settings.DataConsentDelayMeasurement.ToString()) ? "d" : data.Settings.DataConsentDelayMeasurement.ToString() });
            Settings.Add("UserDelete", new UIData { Name = "UserDelete", Options = GetUserDelete(), OptionsText = "Key", OptionsValue = "Value", Value = data.Settings.DataConsentUserDeleteAction.ToString() });
            Settings.Add("PageRedirect", new UIData { Name = "PageRedirect", Options = Library.Managers.PageManager.GetParentPages(PortalSettings.Current).Select(a => new { a.TabID, a.TabName }), OptionsText = "TabName", OptionsValue = "TabID", Value = data.Settings.DataConsentConsentRedirect.ToString() });
            return Settings.Values.ToList();
        }

        private static List<StringText> GetUserDelete()
        {
            PortalSettings portalSettings = new PortalSettings();
            List<StringText> Users = new List<StringText>();
            foreach (var item in portalSettings.DataConsentUserDeleteAction.ToKeyValuePairs())
            {
                StringText user = new StringText
                {
                    Key = item.Key
                };
                user.Value = Common.Utilities.Localization.Get(item.Value, "Text", Constants.LocalResourcesFile, Library.Extension.ShowMissingKeysStatic, Common.Utilities.Localization.SharedMissingPrefix);

                if (string.IsNullOrEmpty(user.Value))
                    user.Value = item.Value;
                Users.Add(user);
            }

            return Users;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrivacySettings(dynamic settingsData)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                UpdatePrivacySettingsRequest request = JsonConvert.DeserializeObject<UpdatePrivacySettingsRequest>(settingsData.PrivacySettingsRequest.ToString());
                int pid = request.PortalId ?? PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                {
                    actionResult.AddError("HttpStatusCode.Unauthorized", AuthFailureMessage);
                }

                PortalController.UpdatePortalSetting(pid, "ShowCookieConsent", "False", false);
                PortalController.UpdatePortalSetting(pid, "Vanjaro_CookieConsent", request.ShowCookieConsent.ToString(), false);
                //HostController.Instance.Update("Copyright", request.DisplayCopyright ? "Y" : "N", false);
                PortalController.UpdatePortalSetting(pid, "CookieMoreLink", request.CookieMoreLink, false, request.CultureCode);
                HostController.Instance.Update("CheckUpgrade", request.CheckUpgrade ? "Y" : "N", false);
                HostController.Instance.Update("VJImprovementProgram", settingsData.CustomSettingsRequest.VJImprovementProgram.Value.ToString(), true);
                PortalController.UpdatePortalSetting(pid, "DataConsentActive", request.DataConsentActive.ToString(), false);
                PortalController.UpdatePortalSetting(pid, "DataConsentUserDeleteAction", request.DataConsentUserDeleteAction.ToString(), false);
                PortalController.UpdatePortalSetting(pid, "DataConsentConsentRedirect", ValidateTabId(request.DataConsentConsentRedirect, pid).ToString(), false);
                PortalController.UpdatePortalSetting(pid, "DataConsentDelay", request.DataConsentDelay.ToString(), false);
                PortalController.UpdatePortalSetting(pid, "DataConsentDelayMeasurement", request.DataConsentDelayMeasurement, false);
                DataCache.ClearCache();

                actionResult.Data = true;
            }
            catch (Exception exc)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
            }
            return actionResult;
        }

        [HttpGet]
        public ActionResult GetPrivacySettings(int? portalId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = portalId ?? PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                {
                    actionResult.AddError("HttpStatusCode.Unauthorized", AuthFailureMessage);
                }

                PortalInfo portal = PortalController.Instance.GetPortal(pid);
                PortalSettings portalSettings = new PortalSettings(portal);

                //return Request.CreateResponse(HttpStatusCode.OK, new
                actionResult.Data = new
                {
                    Settings = new
                    {
                        PortalId = portal.PortalID,
                        portal.CultureCode,
                        ShowCookieConsent = Convert.ToBoolean(PortalController.GetPortalSetting("Vanjaro_CookieConsent", PortalSettings.PortalId, "False")),
                        portalSettings.CookieMoreLink,
                        CheckUpgrade = HostController.Instance.GetBoolean("CheckUpgrade", true),
                        //DisplayCopyright = HostController.Instance.GetBoolean("Copyright", true),
                        VJImprovementProgram = HostController.Instance.GetBoolean("VJImprovementProgram", true),
                        portalSettings.DataConsentActive,
                        DataConsentUserDeleteAction = (int)portalSettings.DataConsentUserDeleteAction,
                        DataConsentConsentRedirect = !string.IsNullOrEmpty(actionResult.Data) ? TabSanitizer(portalSettings.DataConsentConsentRedirect, pid)?.TabID : portalSettings.DataConsentConsentRedirect,
                        DataConsentConsentRedirectName = !string.IsNullOrEmpty(actionResult.Data) ? TabSanitizer(portalSettings.DataConsentConsentRedirect, pid)?.TabName : portalSettings.DataConsentConsentRedirect.ToString(),
                        portalSettings.DataConsentDelay,
                        portalSettings.DataConsentDelayMeasurement
                    }
                };
            }
            catch (Exception exc)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetTermsAgreement()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = PortalSettings.PortalId;
                if (!UserInfo.IsSuperUser && PortalSettings.PortalId != pid)
                {
                    actionResult.AddError("HttpStatusCode.Unauthorized", AuthFailureMessage);
                }
                UserController.ResetTermsAgreement(pid);
                PortalController.UpdatePortalSetting(pid, "DataConsentTermsLastChange", DateTime.Now.ToString("O", CultureInfo.InvariantCulture), true);
                actionResult.Data = true;
            }
            catch (Exception exc)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
            }
            return actionResult;
        }

        private TabInfo TabSanitizer(int tabId, int portalId)
        {
            TabInfo tab = TabController.Instance.GetTab(tabId, portalId);
            if (tab != null && !tab.IsDeleted)
            {
                return tab;
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, string> BindHardDelete()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            for (int i = 0; i <= 2; i++)
            {
                if (i == 0)
                {
                    keyValuePairs.Add("d", Localization.GetString("Days", Constants.DNNLocalResourcesFile));
                }
                else if (i == 1)
                {
                    keyValuePairs.Add("h", Localization.GetString("Hours", Constants.DNNLocalResourcesFile));
                }
                else
                {
                    keyValuePairs.Add("w", Localization.GetString("Weeks", Constants.DNNLocalResourcesFile));
                }
            }
            return keyValuePairs;
        }

        private int ValidateTabId(int tabId, int portalId)
        {
            TabInfo tab = TabController.Instance.GetTab(tabId, portalId);
            return tab != null && !tab.IsDeleted ? tab.TabID : Null.NullInteger;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";
    }

    public class StringText
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }
}
using Dnn.PersonaBar.AdminLogs.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Apps.LogsSettings.Entities;
using Vanjaro.UXManager.Extensions.Apps.LogsSettings.Factories;

namespace Vanjaro.UXManager.Extensions.Apps.LogsSettings.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class LogSettingController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> Parameter, UserInfo UserInfo, PortalSettings PortalSettings)
        {
            if (identifier == AppFactory.Identifier.setting_logSetting.ToString())
            {
                Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
                string LogsUrl = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=695f26f3-d4b0-4a69-9d30-d1954742c1e4";
                Settings.Add("LogsUrl", new UIData { Name = "LogsUrl", Value = LogsUrl });
                return Settings.Values.ToList();
            }
            else
            {
                dynamic SingleSetting = new ExpandoObject();
                SingleSetting.Isfound = false;
                Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
                if (Parameter != null && Parameter.Keys.Count > 0 && Parameter.ContainsKey("id"))
                {
                    SingleSetting = Managers.LogsManager.GetSingleLogSetting(UserInfo, Parameter["id"].ToString());
                    SingleSetting.Isfound = true;
                }
                Settings.Add("GetLogTypes", new UIData { Name = "GetLogTypes", Options = Managers.LogsManager.GetLogTypes(), OptionsText = "LogTypeFriendlyName", OptionsValue = "LogTypeKey", Value = SingleSetting.Isfound == true ? SingleSetting.LogTypeKey.ToString() : "*" });
                Settings.Add("GetKeepMostRecentOptions", new UIData { Name = "GetKeepMostRecentOptions", Options = Managers.LogsManager.GetKeepMostRecentOptions(), OptionsText = "Key", OptionsValue = "Value", Value = SingleSetting.Isfound == true ? SingleSetting.KeepMostRecent.ToString() : "*" });
                Settings.Add("NotificationThreshold", new UIData { Name = "NotificationThreshold", Options = Managers.LogsManager.GetNotificationThresholds(), OptionsText = "Text", OptionsValue = "Value", Value = SingleSetting.Isfound == true ? SingleSetting.NotificationThreshold.ToString() : "1" });
                Settings.Add("NotificationTimes", new UIData { Name = "NotificationTimes", Options = Managers.LogsManager.GetNotificationTimes(), OptionsText = "Key", OptionsValue = "Value", Value = SingleSetting.Isfound == true ? SingleSetting.NotificationThresholdTime.ToString() : "1" });
                Settings.Add("NotificationTimeType", new UIData { Name = "NotificationTimeType", Options = Managers.LogsManager.GetNotificationTimeTypes(), OptionsText = "Text", OptionsValue = "Value", Value = SingleSetting.Isfound == true ? ((int)System.Enum.Parse(typeof(TimeType), SingleSetting.NotificationThresholdTimeType.ToString())).ToString() : "1" });
                Settings.Add("MailToAddress", new UIData { Name = "MailToAddress", Value = SingleSetting.Isfound == true ? SingleSetting.MailToAddress.ToString() : "" });
                Settings.Add("EmailNotificationIsActive", new UIData { Name = "EmailNotificationIsActive", Value = SingleSetting.Isfound == true ? SingleSetting.EmailNotificationIsActive.ToString() : "false" });
                Settings.Add("LoggingIsActive", new UIData { Name = "LoggingIsActive", Value = SingleSetting.Isfound == true ? SingleSetting.LoggingIsActive.ToString() : "false" });
                Settings.Add("ID", new UIData { Name = "ID", Value = SingleSetting.Isfound == true ? SingleSetting.ID.ToString() : "0" });
                Settings.Add("MailFromAddress", new UIData { Name = "MailFromAddress", Value = SingleSetting.Isfound == true ? SingleSetting.MailFromAddress.ToString() : "" });
                Settings.Add("LogTypePortalID", new UIData { Name = "LogTypePortalID", Value = SingleSetting.Isfound == true ? SingleSetting.LogTypePortalID.ToString() : "-1" });
                Settings.Add("Email", new UIData { Name = "Email", Value = "" });
                string LogSettingUrl = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=86710658-7b26-4cf2-84b1-d0797d939aa4";
                Settings.Add("LogSettingUrl", new UIData { Name = "LogSettingUrl", Value = LogSettingUrl });
                return Settings.Values.ToList();
            }
        }

        [HttpGet]
        public dynamic GetLogSettings(int pageSize, int pageIndex, string search)
        {
            return Managers.LogsManager.GetLogSettings(pageSize, pageIndex, search, UserInfo);
        }

        [HttpGet]
        public dynamic DeleteLogSetting(string LogTypeConfigId)
        {
            return Managers.LogsManager.DeleteLogSetting(UserInfo, LogTypeConfigId);
        }

        [HttpPost]
        public dynamic AddLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
            return Managers.LogsManager.AddLogSetting(PortalSettings, UserInfo, request);
        }

        [HttpPost]
        public dynamic UpdateLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
            return Managers.LogsManager.UpdateLogSetting(request, UserInfo);
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
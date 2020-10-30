using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Component;
using Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Entities;
using Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Factories;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Managers
{
    public static partial class RevisionsManager
    {
        internal static Dictionary<string, IUIData> GetData(PortalSettings PortalSettings, string Locale)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            Locale = PortalSettings.DefaultLanguage == Locale ? null : Locale;
            int PortalID = PortalSettings.PortalId, TabID = PortalSettings.ActiveTab.TabID;
            Dictionary<string, object> Data = BindRevisionData(PortalID, TabID, Locale);
            Settings.Add("MaxVersion", new UIData { Name = "MaxVersion", Value = Data.Count > 0 && Data["MaxVersion"] != null ? Data["MaxVersion"].ToString() : "" });
            Settings.Add("Versions", new UIData { Name = "Versions", Options = Data.Count > 0 ? Data["Versions"] : new List<PageVersion>() });
            Settings.Add("Guid", new UIData { Name = "Guid", Value = ExtensionInfo.GUID.ToLower() });
            Settings.Add("PublicVersion", new UIData { Name = "PublicVersion", Value = GetPublicVersion(PortalSettings.ActiveTab.TabID, Locale).ToString() });
            return Settings;
        }

        internal static List<PageVersion> GetAllVersionByTabID(int PortalID, int TabID, string Locale)
        {
            List<Core.Data.Entities.Pages> pages = Core.Managers.PageManager.GetPages(TabID).Where(a => a.Locale == Locale).OrderByDescending(a => a.Version).ToList();
            List<PageVersion> versioncontent = new List<PageVersion>();
            string ResourceFilePath = AppFactory.SharedResourcePath();
            string _Pulished = Localization.GetString("Published", ResourceFilePath);
            string _Draft = Localization.GetString("Draft", ResourceFilePath);
            foreach (Core.Data.Entities.Pages item in pages)
            {
                UserInfo uInfo = item.UpdatedBy.HasValue ? UserController.GetUserById(PortalID, (int)item.UpdatedBy) : UserController.GetUserById(PortalID, item.CreatedBy);

                int UserID = uInfo != null ? uInfo.UserID : -1;
                versioncontent.Add(new PageVersion
                {
                    Version = item.Version,
                    Content = item.Content.ToString(),
                    DisplayName = uInfo != null ? uInfo.DisplayName : "System Generated",
                    CreatedBy = UserID,
                    CreatedOn = item.CreatedOn,
                    PhotoURL = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalID, UserID),
                    IsPublished = item.IsPublished,
                    State = item.StateID.HasValue ? Core.Managers.WorkflowManager.GetStateByID(item.StateID.Value) : null,
                    IsLogsExist = WorkflowManager.GetEntityWorkflowLogs(Core.Components.Enum.WorkflowLogType.VJPage.ToString(), TabID, item.Version).Count() > 0,
                    TabID = TabID
                }) ;
            }
            return versioncontent;
        }

        internal static Dictionary<string, object> BindRevisionData(int PortalID, int TabID, string Locale)
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            List<StringText> VersionLeft = new List<StringText>();
            List<StringText> VersionRight = new List<StringText>();
            List<StringText> VersionPreview = new List<StringText>();
            string ResourceFilePath = AppFactory.SharedResourcePath();
            string _Current = Localization.GetString("Current", ResourceFilePath);
            List<Core.Data.Entities.Pages> pages = Core.Managers.PageManager.GetPages(TabID).Where(a => a.Locale == Locale).OrderByDescending(a => a.Version).ToList();
            if (pages.Count > 0)
            {
                Core.Data.Entities.Pages MaxVersion = pages.OrderByDescending(a => a.Version).FirstOrDefault();
                VersionPreview.Add(new StringText { Text = _Current, Value = "-1", Content = MaxVersion.Content.ToString() });
                foreach (Core.Data.Entities.Pages item in pages)
                {
                    StringText str = new StringText
                    {
                        Text = Localization.GetString("Version", ResourceFilePath) + " " + item.Version.ToString(),
                        Content = item.Content.ToString(),
                        Value = item.Version.ToString()
                    };
                    VersionLeft.Add(str);
                    VersionRight.Add(str);
                    VersionPreview.Add(str);
                }

                Data.Add("MaxVersion", MaxVersion.Version);
                Data.Add("VersionLeft", VersionLeft);
                Data.Add("VersionPreview", VersionPreview);
                Data.Add("VersionRight", VersionRight);
                Data.Add("Versions", GetAllVersionByTabID(PortalID, TabID, Locale));
            }
            return Data;
        }

        internal static void Rollback(int TabID, int Version, string Locale, int UserID)
        {
            Core.Managers.PageManager.Rollback(TabID, Version, Locale, UserID);
        }

        private static int GetPublicVersion(int TabID, string Locale)
        {
            int result = 0;
            Core.Data.Entities.Pages page = Core.Managers.PageManager.GetPages(TabID).Where(v => v.IsPublished == true && v.Locale == Locale).OrderByDescending(a => a.Version).FirstOrDefault();
            if (page != null)
            {
                result = page.Version;
            }

            return result;
        }
    }
}
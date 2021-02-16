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
        internal static Dictionary<string, IUIData> GetData(PortalSettings PortalSettings, string Locale, string BlockGuid = null)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            Locale = PortalSettings.DefaultLanguage == Locale ? null : Locale;
            int PortalID = PortalSettings.PortalId, TabID = PortalSettings.ActiveTab.TabID;
            Dictionary<string, object> Data = BindRevisionData(PortalID, TabID, Locale, BlockGuid);
            Settings.Add("MaxVersion", new UIData { Name = "MaxVersion", Value = Data.Count > 0 && Data["MaxVersion"] != null ? Data["MaxVersion"].ToString() : "" });
            Settings.Add("Versions", new UIData { Name = "Versions", Options = Data.Count > 0 ? Data["Versions"] : new List<PageVersion>() });
            Settings.Add("Guid", new UIData { Name = "Guid", Value = ExtensionInfo.GUID.ToLower() });
            Settings.Add("PublicVersion", new UIData { Name = "PublicVersion", Value = GetPublicVersion(PortalSettings.ActiveTab.TabID, Locale).ToString() });
            return Settings;
        }

        internal static List<PageVersion> GetAllVersionByTabID(int PortalID, int TabID, string Locale, string BlockGuid)
        {

            List<PageVersion> versioncontent = new List<PageVersion>();
            string ResourceFilePath = AppFactory.SharedResourcePath();
            string _Pulished = Localization.GetString("Published", ResourceFilePath);
            string _Draft = Localization.GetString("Draft", ResourceFilePath);
            if (!string.IsNullOrEmpty(BlockGuid))
            {
                List<Core.Data.Entities.CustomBlock> Blocks = BlockManager.GetAllByGUID(PortalID, BlockGuid).OrderByDescending(a => a.Version).ToList();
                int DefaultWorkflow = WorkflowManager.GetDefaultWorkflow();
                Core.Data.Entities.WorkflowState FirstState = WorkflowManager.GetFirstStateID(DefaultWorkflow);
                Core.Data.Entities.WorkflowState LastState = WorkflowManager.GetLastStateID(DefaultWorkflow);
                foreach (Core.Data.Entities.CustomBlock Block in Blocks)
                {
                    UserInfo uInfo = Block.UpdatedBy > 0 ? UserController.GetUserById(PortalID, (int)Block.UpdatedBy) : UserController.GetUserById(PortalID, Block.CreatedBy);

                    int UserID = uInfo != null ? uInfo.UserID : -1;
                    versioncontent.Add(new PageVersion
                    {
                        Version = Block.Version,
                        Content = Block.Html,
                        DisplayName = uInfo != null ? uInfo.DisplayName : "System Generated",
                        CreatedBy = UserID,
                        CreatedOn = Block.CreatedOn,
                        PhotoURL = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalID, UserID),
                        IsPublished = Block.IsPublished,
                        State = Block.IsPublished ? LastState : FirstState,
                        IsLogsExist = false,
                    });
                }
            }
            else
            {
                List<Core.Data.Entities.Pages> pages = Core.Managers.PageManager.GetPages(TabID).Where(a => a.Locale == Locale).OrderByDescending(a => a.Version).ToList();
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
                        IsLogsExist = WorkflowManager.GetEntityWorkflowLogs(Core.Components.Enum.WorkflowType.Page.ToString(), TabID, item.Version).Count() > 0,
                        TabID = TabID
                    });
                }
            }
            return versioncontent;
        }

        internal static Dictionary<string, object> BindRevisionData(int PortalID, int TabID, string Locale, string BlockGuid = null)
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            string ResourceFilePath = AppFactory.SharedResourcePath();
            string _Current = Localization.GetString("Current", ResourceFilePath);
            int MaxVersion = 0;
            if (!string.IsNullOrEmpty(BlockGuid))
            {
                List<Core.Data.Entities.CustomBlock> CustomBlock = BlockManager.GetAllByGUID(PortalID, BlockGuid).OrderByDescending(a => a.Version).ToList();
                if (CustomBlock.Count > 0)
                    MaxVersion = CustomBlock.FirstOrDefault().Version;
                 
            }
            else
            {
                List<Core.Data.Entities.Pages> pages = Core.Managers.PageManager.GetPages(TabID).Where(a => a.Locale == Locale).OrderByDescending(a => a.Version).ToList();
                if (pages.Count > 0)
                    MaxVersion = pages.OrderByDescending(a => a.Version).FirstOrDefault().Version;
            }

            Data.Add("MaxVersion", MaxVersion);
            Data.Add("Versions", GetAllVersionByTabID(PortalID, TabID, Locale, BlockGuid));
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
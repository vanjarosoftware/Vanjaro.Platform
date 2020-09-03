using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Menu.Assets.Entities;

namespace Vanjaro.UXManager.Extensions.Menu.Assets.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class AssetsController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = Host.AllowedExtensionWhitelist.ToStorageString() } },
                { "MaxFileSize", new UIData { Name = "MaxFileSize", Value = Config.GetMaxUploadSize().ToString() } },
                { "Files", new UIData { Name = "Files", Options = null } }
            };

            List<TreeView> folders = BrowseUploadFactory.GetFoldersTree(PortalID);
            Settings.Add("Folders", new UIData { Name = "Folders", Options = folders, Value = folders.Count > 0 ? folders.FirstOrDefault().Value.ToString() : "0", });
            Settings.Add("AssetType", new UIData { Name = "AssetType", Value = "true" });
            List<StringText> FolderType = new List<StringText>();
            foreach (FolderMappingInfo item in FolderMappingController.Instance.GetFolderMappings(PortalID))
            {
                FolderType.Add(new StringText() { Text = item.MappingName, Value = item.FolderMappingID.ToString(), PortalID = item.PortalID });
            }

            foreach (FolderMappingInfo item in FolderMappingController.Instance.GetFolderMappings(-1))
            {
                FolderType.Add(new StringText() { Text = item.MappingName, Value = item.FolderMappingID.ToString(), PortalID = item.PortalID });
            }

            Settings.Add("FolderType", new UIData { Name = "FolderType", Options = FolderType, OptionsText = "Text", OptionsValue = "Value" });
            Settings.Add("IsFileManager", new UIData { Name = "IsFileManager", Value = "true" });
            Settings.Add("IsList", new UIData { Name = "IsList", Value = "true" });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public dynamic GetFolderAndFiles(bool IsGlobal)
        {
            if (IsGlobal)
            {
                return BrowseUploadFactory.GetFoldersTree(-1);
            }
            else
            {
                return BrowseUploadFactory.GetFoldersTree(PortalSettings.ActiveTab.PortalID);
            }
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
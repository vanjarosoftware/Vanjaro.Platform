using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Apps.Video.Entities;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Extensions.Apps.Video.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class VideoController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters, string Identifier, bool IsSupportBackground)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            if (Identifier == "settings_video")
            {
                List<Common.Components.TreeView> folders = BrowseUploadFactory.GetFoldersTree(PortalID, "video");
                Settings.Add("AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType });
                Settings.Add("MaxFileSize", new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() });
                Settings.Add("Files", new UIData { Name = "Files", Options = null });
                Settings.Add("Folders", new UIData { Name = "Folders", Options = folders, Value = folders.Count > 0 ? folders.FirstOrDefault().Value.ToString() : "0", });
            }
            List<VideoEntity> VideoProviders = VideoManager.GetVideoProviders(IsSupportBackground);
            Settings.Add("VideoProviders", new UIData { Name = "VideoProviders", Options = VideoProviders, OptionsText = "Text", OptionsValue = "Value", Value = VideoProviders.Count > 0 ? VideoProviders.FirstOrDefault().Value : "" });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public Task<string> Search(string source, string keyword, int PageNo, Dictionary<string, object> AdditionalData)
        {
            return VideoManager.GetVideos(source, keyword, PageNo, 20, AdditionalData);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
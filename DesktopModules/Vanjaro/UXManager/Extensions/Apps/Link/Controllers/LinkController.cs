using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Extensions.Apps.Link.Entities;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Extensions.Apps.Link.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class LinkController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters, string Identifier)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            if (Identifier == "settings_link")
            {
                List<Common.Components.TreeView> folders = BrowseUploadFactory.GetFoldersTree(PortalID);
                Settings.Add("AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType });
                Settings.Add("MaxFileSize", new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() });
                Settings.Add("Files", new UIData { Name = "Files", Options = null });
                Settings.Add("Folders", new UIData { Name = "Folders", Options = folders, Value = folders.Count > 0 ? folders.FirstOrDefault().Value.ToString() : "0", });
            }
            List<ImageEntity> ImageProviders = ImageManager.GetImageProviders();
            Settings.Add("ImageProviders", new UIData { Name = "ImageProviders", Options = ImageProviders, OptionsText = "Text", OptionsValue = "Value", Value = ImageProviders.Count > 0 ? ImageProviders.FirstOrDefault().Value : "" });
            return Settings.Values.ToList();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
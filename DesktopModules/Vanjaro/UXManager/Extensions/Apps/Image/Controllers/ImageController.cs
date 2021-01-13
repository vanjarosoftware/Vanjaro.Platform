using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Utilities;
using Vanjaro.Core;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.Entities;
using Vanjaro.UXManager.Extensions.Apps.Image.Entities;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Library.Managers;

namespace Vanjaro.UXManager.Extensions.Apps.Image.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class ImageController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters, string Identifier)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            if (Identifier == "settings_image")
            {
                List<Common.Components.TreeView> folders = BrowseUploadFactory.GetFoldersTree(PortalID, "image");
                Settings.Add("AllowedAttachmentFileExtensions", new UIData { Name = "AllowedAttachmentFileExtensions", Value = FileSetting.FileType });
                Settings.Add("MaxFileSize", new UIData { Name = "MaxFileSize", Value = FileSetting.FileSize.ToString() });
                Settings.Add("Files", new UIData { Name = "Files", Options = null });
                Settings.Add("Folders", new UIData { Name = "Folders", Options = folders, Value = folders.Count > 0 ? folders.FirstOrDefault().Value.ToString() : "0", });
            }
            List<ImageEntity> ImageProviders = ImageManager.GetImageProviders();
            Settings.Add("ImageProviders", new UIData { Name = "ImageProviders", Options = ImageProviders, OptionsText = "Text", OptionsValue = "Value", Value = ImageProviders.Count > 0 ? ImageProviders.FirstOrDefault().Value : "" });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public Task<string> Search(string source, string keyword, int PageNo)
        {
            return ImageManager.GetImages(source, keyword, PageNo, 35);
        }

        [HttpPost]
        public dynamic Save(string path, int id)
        {
            string result = "failed";
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(path);
                    webRequest.AllowWriteStreamBuffering = true;
                    webRequest.Timeout = 30000;
                    using (WebResponse webResponse = webRequest.GetResponse())
                    {
                        Stream stream = webResponse.GetResponseStream();
                        return ConvertSave(path, id, stream);
                    }
                }
            }
            catch (Exception ex) { ExceptionManager.LogException(ex); }
            return result;
        }

        [HttpPost]
        public dynamic Convert()
        {
            string result = "failed";
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Form != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["ImageByte"]) && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["PreviousFileName"]))
                {
                    byte[] ByteImage = System.Convert.FromBase64String(HttpContext.Current.Request.Form["ImageByte"].Split(',')[1]);

                    string PreviousFile = HttpContext.Current.Request.Form["PreviousFileName"];

                    using (MemoryStream stream = new MemoryStream(ByteImage))
                    {
                        Random random = new Random();
                        if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["ImageByte"].Split(',')[0]) && !string.IsNullOrEmpty(PreviousFile))
                        {
                            return ConvertSave(PreviousFile, -1, stream, "_ve_");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
            }
            return result;
        }

        #region Folder and File
        public dynamic ConvertSave(string FilePath, int Id, Stream stream, string Suffix = null)
        {
            dynamic result = "failed";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (stream != null)
                {
                    List<Setting> settings = Managers.SettingManager.GetSettings(PortalSettings.PortalId, 0, "security_settings");
                    string Picture_DefaultFolder = "-1";
                    if (settings != null && settings.Count > 0 && settings.Where(s => s.Name == "Picture_DefaultFolder").FirstOrDefault() != null)
                    {
                        Picture_DefaultFolder = settings.Where(s => s.Name == "Picture_DefaultFolder").FirstOrDefault().Value;
                    }

                    IFolderInfo folderInfo = FolderManager.Instance.GetFolder(Picture_DefaultFolder == "-1" ? BrowseUploadFactory.GetRootFolder(PortalSettings.PortalId).FolderID : int.Parse(Picture_DefaultFolder));
                    if (folderInfo != null && BrowseUploadFactory.HasPermission(folderInfo, "WRITE"))
                    {
                        string FileName = Security.ReplaceIllegalCharacters(Path.GetFileName(FilePath));
                        if (Id > -1)
                        {
                            FileName = Id + FileName.Substring(FileName.LastIndexOf('.'));
                        }

                        string FileExtension = FileName.Substring(FileName.LastIndexOf('.'));

                        if (!string.IsNullOrEmpty(Suffix))
                        {
                            //Force extention to be .png to maintain transparency
                            FileName = FileName.Remove(FileName.Length - FileExtension.Length) + "_edited.png";
                            FileExtension = ".png";
                        }

                        string TempFileName = FileName;

                        if (Security.IsAllowedExtension(FileExtension, FileSetting.FileType.Split(',')))
                        {
                            int count = 1;

                        Find:
                            if (FileManager.Instance.FileExists(folderInfo, TempFileName))
                            {
                                string FName = FileName.Remove(FileName.Length - FileExtension.Length);
                                if (!string.IsNullOrEmpty(Suffix))
                                {
                                    if (FileName.Contains(Suffix))
                                    {
                                        string[] FInfo = FileName.Split(new string[] { Suffix }, StringSplitOptions.None);
                                        if (FInfo.Length > 1)
                                        {
                                            FName = FInfo[0];
                                        }
                                    }
                                }
                                TempFileName = FName + (!string.IsNullOrEmpty(Suffix) ? Suffix : string.Empty) + count + FileExtension;
                                count++;
                                goto Find;
                            }
                            else
                            {
                                FileName = TempFileName;
                                IFileInfo fileInfo;

                                if (!string.IsNullOrEmpty(Suffix))
                                {
                                    byte[] photoBytes = ToByteArray(stream);

                                    using (MemoryStream inStream = new MemoryStream(photoBytes))
                                    {
                                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                                        {
                                            //Convert to png
                                            ISupportedImageFormat format = new PngFormat { };

                                            // Load, resize, set the format and quality and save an image.
                                            imageFactory.Load(inStream)
                                                        .Format(format)
                                                        .Save(memoryStream);
                                            fileInfo = FileManager.Instance.AddFile(folderInfo, FileName, memoryStream);
                                            memoryStream.Seek(0, SeekOrigin.Begin);
                                        }
                                    }
                                }
                                else
                                {
                                    stream.CopyTo(memoryStream);
                                    fileInfo = FileManager.Instance.AddFile(folderInfo, FileName, memoryStream);
                                    memoryStream.Seek(0, SeekOrigin.Begin);
                                }

                                if (Utils.IsImageVersionable(fileInfo))
                                {
                                    BrowseUploadFactory.CropImage(FileName, FileExtension, folderInfo, memoryStream);
                                }

                                if (fileInfo != null)
                                {
                                    result = BrowseUploadFactory.GetUrl(fileInfo.FileId);
                                }
                            }

                        }
                    }

                    else
                    {
                        throw new Exception("Error: You do not have write permission for folder " + (folderInfo != null ? folderInfo.FolderPath.TrimEnd('/') : string.Empty));
                    }
                }
            }
            return result;
        }
        private static byte[] ToByteArray(Stream inputStream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                inputStream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        #endregion

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
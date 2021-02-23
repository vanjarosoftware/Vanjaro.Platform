using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.Core.Managers;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using System.Text;

namespace Vanjaro.UXManager.Library.Controllers
{
    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class BlockController : UIEngineController
    {
        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public dynamic AddCustomBlock(GlobalBlock CustomBlock)
        {
            return BlockManager.Add(PortalSettings, CustomBlock);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public dynamic EditCustomBlock(CustomBlock CustomBlock)
        {
            return BlockManager.EditCustomBlock(PortalSettings, CustomBlock);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public dynamic EditGlobalBlock(GlobalBlock GlobalBlock)
        {
            return BlockManager.EditGlobalBlock(PortalSettings, GlobalBlock);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public dynamic DeleteCustomBlock(string CustomBlockGuid)
        {
            return BlockManager.DeleteCustom(PortalSettings.ActiveTab.PortalID, CustomBlockGuid);
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public dynamic DeleteGlobalBlock(string CustomBlockGuid)
        {
            return BlockManager.DeleteGlobal(PortalSettings.ActiveTab.PortalID, CustomBlockGuid);
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public HttpResponseMessage ExportCustomBlock(string CustomBlockGuid)
        {
            return BlockManager.ExportCustomBlock(PortalSettings.ActiveTab.PortalID, CustomBlockGuid);
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "pageedit")]
        public List<CustomBlock> GetAllCustomBlock()
        {
            return BlockManager.GetAllCustomBlocks(PortalSettings);
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "pageedit")]
        public List<GlobalBlock> GetAllGlobalBlock()
        {
            return BlockManager.GetAllGlobalBlocks(PortalSettings);
        }

        [HttpGet]
        [AuthorizeAccessRoles(AccessRoles = "pageedit")]
        public List<Block> GetAll()
        {
            return BlockManager.GetAll();
        }

        [HttpPost]
        [AuthorizeAccessRoles(AccessRoles = "pageedit")]
        public ThemeTemplateResponse RenderItem()
        {
            Dictionary<string, string> Attributes = new Dictionary<string, string>();
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                Attributes.Add(key, HttpContext.Current.Request.Form[key]);
            }

            return Core.Managers.BlockManager.Render(Attributes);
        }

        [HttpPost]
        [AllowAnonymous]
        public List<ThemeTemplateResponse> RenderMarkup(dynamic mappedAttr)
        {
            Dictionary<string, string> Attributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(mappedAttr.ToString());

            HtmlDocument html = new HtmlDocument();
            Pages Pages = PageManager.GetLatestVersion(PortalSettings.ActiveTab.TabID, PageManager.GetCultureCode(PortalSettings));
            html.LoadHtml(Pages.Content.ToString());
            return Managers.BlockManager.FindBlocks(Pages, html, Attributes);
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public CustomBlock ImportCustomBlock(string TemplateHash, string TemplatePath)
        //{
        //    CustomBlock Result = new CustomBlock();
        //    try
        //    {
        //        IFolderInfo fi = FolderManager.Instance.GetFolder(PortalSettings.ActiveTab.PortalID, "Images/");
        //        IFolderInfo foldersizeinfo = FolderManager.Instance.GetFolder(PortalSettings.ActiveTab.PortalID, fi.FolderPath + ".versions");
        //        if (foldersizeinfo == null)
        //            foldersizeinfo = FolderManager.Instance.AddFolder(PortalSettings.ActiveTab.PortalID, fi.FolderPath + ".versions");
        //        string path = HttpContext.Current.Server.MapPath("~/Portals/_default/ThemeLibrary/");
        //        if (!Directory.Exists(path))
        //            Directory.CreateDirectory(path);
        //        path = path + Path.GetFileName(TemplatePath).Replace(".zip", "");
        //        if (!Directory.Exists(path))
        //        {
        //            DownloadTemplate(TemplateHash, TemplatePath, path);
        //        }
        //        if (Directory.Exists(path))
        //        {
        //            if (TemplateHash != Path.GetFileNameWithoutExtension(Directory.GetFiles(path, "Hash*").FirstOrDefault()).Replace("Hash", ""))
        //            {
        //                Directory.Delete(path, true);
        //                DownloadTemplate(TemplateHash, TemplatePath, path);
        //            }
        //            if (File.Exists(path + "/Template.json"))
        //            {
        //                ExportTemplate exportTemplate = JsonConvert.DeserializeObject<ExportTemplate>(File.ReadAllText(path + "/Template.json", Encoding.Unicode));
        //                if (exportTemplate != null && exportTemplate.ThemeGuid.ToLower() == ThemeManager.CurrentTheme.GUID.ToLower())
        //                {
        //                    Layout pagelayout = exportTemplate.Templates.FirstOrDefault();
        //                    if (pagelayout != null)
        //                    {
        //                        if (string.IsNullOrEmpty(pagelayout.Content))
        //                        {
        //                            foreach (CustomBlock cb in pagelayout.Blocks)
        //                            {
        //                                Result.Html += cb.Html;
        //                                Result.Css += cb.Css;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Result.Html = PageManager.DeTokenizeLinks(pagelayout.Content.ToString(), PortalSettings.ActiveTab.PortalID);
        //                            Result.Css = PageManager.DeTokenizeLinks(pagelayout.Style.ToString(), PortalSettings.ActiveTab.PortalID);
        //                        }

        //                        Result.ScreenshotPath = TemplatePath.Replace(".zip", ".png");

        //                        if (Directory.Exists(path + "/Assets"))
        //                        {
        //                            List<string> assets = Directory.GetFiles(path + "/Assets", "*", SearchOption.AllDirectories).ToList();
        //                            if (assets != null && assets.Count > 0)
        //                            {
        //                                Parallel.ForEach(assets,
        //                                new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.50) * 1.0)) },
        //                                asset =>
        //                                {
        //                                    if (fi == null)
        //                                        fi = FolderManager.Instance.AddFolder(PortalSettings.ActiveTab.PortalID, "Images");
        //                                    if (fi != null)
        //                                    {
        //                                        string fileName = Path.GetFileName(asset);
        //                                        if (fileName.ToLower().EndsWith("w.webp") || fileName.ToLower().EndsWith("w.png") || fileName.ToLower().EndsWith("w.jpeg"))
        //                                        {
        //                                            using (FileStream fs = File.OpenRead(asset))
        //                                            {
        //                                                FileManager.Instance.AddFile(foldersizeinfo, fileName, fs);
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            using (FileStream fs = File.OpenRead(asset))
        //                                            {
        //                                                FileManager.Instance.AddFile(fi, fileName, fs);
        //                                            }
        //                                        }
        //                                    }
        //                                });
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        Result.Html = PageManager.DeTokenizeLinks(Result.Html, PortalSettings.ActiveTab.PortalID);
        //        Result.Css = PageManager.DeTokenizeLinks(Result.Css, PortalSettings.ActiveTab.PortalID);
        //        Dictionary<string, object> LayoutData = new Dictionary<string, object>();
        //        LayoutData.Add("gjs-html", Result.Html);
        //        PageManager.AddModules(PortalSettings, LayoutData, PortalSettings.UserInfo, path + "/PortableModules");
        //        Result.Html = LayoutData["gjs-html"].ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionManager.LogException(ex);
        //    }
        //    return Result;
        //}

        private static void DownloadTemplate(string TemplateHash, string TemplatePath, string path)
        {
            new WebClient().DownloadFile(TemplatePath, path + ".zip");
            ZipFile.ExtractToDirectory(path + ".zip", path);
            File.Create(path + "/Hash" + TemplateHash + ".txt").Dispose();
            File.Delete(path + ".zip");
        }

        public override string AccessRoles()
        {
            return Factories.AppExtensionFactory.GetAccessRoles(UserInfo);
        }
    }
}
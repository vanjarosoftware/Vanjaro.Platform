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
using System.Web;
using System.Web.Http;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Library.Controllers
{
    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class BlockController : DnnApiController
    {
        [HttpPost]
        [DnnAdmin]
        public dynamic AddCustomBlock(CustomBlock CustomBlock)
        {
            return Core.Managers.BlockManager.Add(PortalSettings, CustomBlock);
        }

        [HttpPost]
        [DnnAdmin]
        public dynamic EditCustomBlock(CustomBlock CustomBlock)
        {
            return Core.Managers.BlockManager.Edit(PortalSettings, CustomBlock);
        }

        [HttpPost]
        [DnnAdmin]
        public dynamic DeleteCustomBlock(string CustomBlockGuid)
        {
            return Core.Managers.BlockManager.Delete(PortalSettings.ActiveTab.PortalID, CustomBlockGuid);
        }

        [HttpGet]
        [DnnAdmin]
        public HttpResponseMessage ExportCustomBlock(string CustomBlockGuid)
        {
            return Core.Managers.BlockManager.ExportCustomBlock(PortalSettings.ActiveTab.PortalID, CustomBlockGuid);
        }

        [HttpGet]
        [DnnPageEditor]
        public List<CustomBlock> GetAllCustomBlock()
        {
            return Core.Managers.BlockManager.GetAll(PortalSettings);
        }

        [HttpGet]
        [DnnPageEditor]
        public List<Block> GetAll()
        {
            return Core.Managers.BlockManager.GetAll();
        }

        [HttpPost]
        [DnnPageEditor]
        public ThemeTemplateResponse Render()
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

        [HttpPost]
        [AllowAnonymous]
        public CustomBlock ImportCustomBlock(string TemplatePath)
        {
            CustomBlock Result = new CustomBlock();
            try
            {
                IFolderInfo fi = FolderManager.Instance.GetFolder(PortalSettings.ActiveTab.PortalID, "Images/");

                if (fi == null)
                    fi = FolderManager.Instance.GetFolder(PortalSettings.ActiveTab.PortalID, "assets/Images/");

                string path = HttpContext.Current.Server.MapPath("~/Portals/" + PortalSettings.ActiveTab.PortalID + "/temp/");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = path + Path.GetFileName(TemplatePath);
                WebClient webClient = new WebClient();
                webClient.DownloadFile(TemplatePath, path);

                using (ZipArchive archive = ZipFile.OpenRead(path))
                {
                    ZipArchiveEntry entry = archive.GetEntry("Template.json");
                    if (entry != null)
                    {
                        using (StreamReader reader = new StreamReader(entry.Open()))
                        {
                            ExportTemplate exportTemplate = JsonConvert.DeserializeObject<ExportTemplate>(reader.ReadToEnd());
                            if (exportTemplate != null && exportTemplate.Templates.FirstOrDefault() != null && exportTemplate.Templates.FirstOrDefault().Blocks.FirstOrDefault() != null)
                            {
                                foreach (CustomBlock block in exportTemplate.Templates.FirstOrDefault().Blocks)
                                {
                                    Result.Html += block.Html;
                                    Result.Css += block.Css;
                                }

                                List<ZipArchiveEntry> assets = archive.Entries.Where(e => e.FullName.StartsWith("Assets/")).ToList();

                                if (assets != null && assets.Count > 0)
                                {
                                    foreach (ZipArchiveEntry asset in assets)
                                    {
                                        if (fi == null)
                                            fi = FolderManager.Instance.AddFolder(PortalSettings.ActiveTab.PortalID, "Images");

                                        if (fi != null)
                                        {
                                            try
                                            {
                                                FileManager.Instance.AddFile(fi, asset.Name, asset.Open());
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return Result;
        }
    }
}
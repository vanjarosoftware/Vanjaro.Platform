using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
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
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.Core.Entities.Menu;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class BlockManager
        {
            public static List<IBlock> GetExtentions()
            {
                return Extentions.ToList();
            }

            internal static List<IBlock> Extentions
            {
                get
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Block_Extension);
                    List<IBlock> Items = CacheFactory.Get(CacheKey);
                    if (Items == null)
                    {
                        List<IBlock> ServiceInterfaceAssemblies = new List<IBlock>();
                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                        foreach (string Path in binAssemblies)
                        {
                            try
                            {
                                //get all assemblies 
                                IEnumerable<IBlock> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                      where t != (typeof(IBlock)) && (typeof(IBlock).IsAssignableFrom(t))
                                                                      select Activator.CreateInstance(t) as IBlock;

                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IBlock>());
                            }
                            catch { continue; }
                        }
                        Items = ServiceInterfaceAssemblies;
                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                    }
                    return Items;
                }
            }

            public static List<Block> GetAll()
            {
                return Extentions.Where(e => e.Name != "Custom" && e.Visible).Select(s => new Block()
                {
                    Category = s.Category,
                    Guid = s.Guid,
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Icon = s.Icon,
                    Attributes = (s.Attributes.ContainsKey("data-block-global") && s.Attributes["data-block-global"] == "true") ? GetGlobalConfigs(PortalSettings.Current, s.Name) : GetGlobalConfigs(PortalSettings.Current, s.Name, false)
                }).ToList();
            }

            public static ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
            {
                ThemeTemplateResponse result = null;
                if (Attributes.ContainsKey("data-block-guid") && !string.IsNullOrEmpty(Attributes["data-block-guid"]))
                {
                    dynamic ext = Extentions.Where(s => s.Guid == Guid.Parse(Attributes["data-block-guid"])).FirstOrDefault();
                    if (ext != null)
                    {
                        foreach (dynamic checkAttr in ext.Attributes)
                        {
                            if (!Attributes.ContainsKey(checkAttr.Key))
                            {
                                Attributes.Add(checkAttr.Key, checkAttr.Value);
                            }
                        }
                        result = ext.Render(Attributes);
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<div");
                        foreach (KeyValuePair<string, string> item in Attributes)
                        {
                            sb.Append(" ").Append(item.Key).Append("=\"" + item.Value + "\"");
                        }

                        sb.Append(">" + result.Markup + "</div>");
                        result.Markup = sb.ToString();
                    }
                }
                return result;
            }

            public static Dictionary<string, string> GetGlobalConfigs(PortalSettings PortalSettings, string Block, bool IsGlobal = true, string BlockDirectory = "Blocks")
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.GlobalConfig, PortalSettings.PortalId, Block);
                Dictionary<string, string> result = CacheFactory.Get(CacheKey);
                if (result == null)
                {
                    result = new Dictionary<string, string>();
                    string FolderPath = Globals.ApplicationMapPath + @"\portals\_default\" + GetTheme() + BlockDirectory + "\\" + Block + "\\";
                    if (Directory.Exists(FolderPath))
                    {
                        string markup = string.Empty;
                        if (File.Exists(FolderPath + Block + ".html"))
                        {
                            markup += File.ReadAllText(FolderPath + Block + ".html");
                        }

                        if (IsGlobal && File.Exists(FolderPath + Block + ".config.html"))
                        {
                            markup += File.ReadAllText(FolderPath + Block + ".config.html");
                        }

                        if (!string.IsNullOrEmpty(markup))
                        {
                            HtmlDocument html = new HtmlDocument();
                            html.LoadHtml(markup);
                            IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                            foreach (HtmlNode item in query.ToList())
                            {
                                if (item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault() != null)
                                {
                                    foreach (HtmlAttribute attr in item.Attributes)
                                    {
                                        if (result.ContainsKey(attr.Name))
                                        {
                                            result[attr.Name] = attr.Value;
                                        }
                                        else
                                        {
                                            result.Add(attr.Name, attr.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    CacheFactory.Set(CacheKey, result);
                }
                return result;
            }

            public static string GetTheme(string ThemeName = "Basic")
            {
                return "vThemes\\" + ThemeName + "\\";
            }
            public static string GetVirtualPath()
            {
                return Globals.ApplicationPath + VirtualPathUtility.ToAppRelative(@"\portals\_default\");
            }

            public static string GetTemplateDir(PortalSettings PortalSettings, string Block)
            {
                return GetVirtualPath() + GetTheme(ThemeManager.CurrentTheme.Name) + "Blocks\\" + Block + "\\Templates\\";
            }

            public static void UpdateDesignElement(PortalSettings PortalSettings, Dictionary<string, string> Attributes)
            {
                if (Attributes.ContainsKey("data-block-global") && Attributes["data-block-global"] == "true")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<div");
                    foreach (KeyValuePair<string, string> item in Attributes)
                    {
                        if (item.Key.StartsWith("data-block"))
                        {
                            sb.Append(" ").Append(item.Key).Append("=\"" + item.Value + "\"");
                        }
                    }
                    sb.Append("></div>");
                    string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.CurrentTheme.Name + "/Blocks/" + Attributes["data-block-type"] + "/");
                    if (Directory.Exists(FolderPath))
                    {
                        if (!File.Exists(FolderPath + Attributes["data-block-type"] + ".config.html"))
                        {
                            File.Create(FolderPath + Attributes["data-block-type"].ToLower() + ".config.html").Dispose();
                        }

                        if (File.Exists(FolderPath + Attributes["data-block-type"] + ".config.html"))
                        {
                            File.WriteAllText(FolderPath + Attributes["data-block-type"] + ".config.html", sb.ToString());
                            CacheFactory.Clear(CacheFactory.Keys.GlobalConfig);
                        }
                    }
                }
            }

            public static dynamic Add(PortalSettings PortalSettings, CustomBlock CustomBlock, int ForceCount = 0)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    if (!string.IsNullOrEmpty(CustomBlock.Html))
                        CustomBlock.Html = PageManager.DeTokenizeLinks(CustomBlock.Html, PortalSettings.PortalId);
                    if (!string.IsNullOrEmpty(CustomBlock.Css))
                        CustomBlock.Css = PageManager.DeTokenizeLinks(CustomBlock.Css, PortalSettings.PortalId);
                    if (BlockFactory.Get(PortalSettings.PortalId, CustomBlock.Name) == null)
                    {
                        if (ForceCount == 0)
                        {
                            CustomBlock.Guid = Guid.NewGuid().ToString().ToLower();
                        }

                        CustomBlock.PortalID = PortalSettings.PortalId;
                        CustomBlock.CreatedBy = PortalSettings.UserId;
                        CustomBlock.UpdatedBy = PortalSettings.UserId;
                        CustomBlock.CreatedOn = DateTime.UtcNow;
                        CustomBlock.UpdatedOn = DateTime.UtcNow;
                        CustomBlock.Locale = PageManager.GetCultureCode(PortalSettings);
                        BlockFactory.AddUpdate(CustomBlock);
                        CustomBlock cb = BlockFactory.GetAll(PortalSettings.PortalId).Where(b => b.Guid.ToLower() == CustomBlock.Guid.ToLower() && b.Locale == null).FirstOrDefault();
                        if (cb == null)
                        {
                            cb = CustomBlock;
                            cb.Locale = null;
                            cb.ID = 0;
                            BlockFactory.AddUpdate(cb);
                        }
                        Result.Status = "Success";
                        Result.Guid = CustomBlock.Guid;
                    }
                    else
                    {
                        if (ForceCount > 0)
                        {
                            CustomBlock.Name = CustomBlock.Name + ForceCount;
                            ForceCount++;
                            Result = Add(PortalSettings, CustomBlock, ForceCount);
                        }
                        else
                        {
                            Result.Status = "Exist";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message.ToString();
                    ExceptionManager.LogException(ex);
                }
                return Result;
            }

            public static dynamic Edit(PortalSettings PortalSettings, CustomBlock CustomBlock)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    CustomBlock cb = BlockFactory.GetAll(PortalSettings.PortalId).Where(b => b.Guid.ToLower() == CustomBlock.Guid.ToLower() && b.Locale == PageManager.GetCultureCode(PortalSettings)).FirstOrDefault();
                    if (cb == null)
                    {
                        cb = BlockFactory.GetAll(PortalSettings.PortalId).Where(b => b.Guid.ToLower() == CustomBlock.Guid.ToLower() && b.Locale == null).FirstOrDefault();
                        if (cb != null)
                        {
                            cb.Locale = PageManager.GetCultureCode(PortalSettings);
                            cb.ID = 0;
                            BlockFactory.AddUpdate(cb);
                        }
                    }
                    if (cb != null)
                    {
                        cb.Name = CustomBlock.Name;
                        cb.Category = CustomBlock.Category;

                        if (CustomBlock.IsGlobal && !string.IsNullOrEmpty(CustomBlock.Html))
                        {
                            HtmlDocument html = new HtmlDocument();
                            html.LoadHtml(CustomBlock.Html);
                            IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                            foreach (HtmlNode item in query.ToList())
                            {
                                if (item.Attributes.Where(a => a.Name == "dmid").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "mid").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "mid").FirstOrDefault().Value))
                                {
                                    int mid = int.Parse(item.Attributes.Where(a => a.Name == "mid").FirstOrDefault().Value);
                                    ModuleInfo minfo = ModuleController.Instance.GetModule(mid, Null.NullInteger, false);
                                    if (minfo != null && !minfo.AllTabs)
                                    {
                                        minfo.AllTabs = true;
                                        ModuleController.Instance.UpdateModule(minfo);
                                        List<TabInfo> listTabs = TabController.GetPortalTabs(minfo.PortalID, Null.NullInteger, false, true);
                                        foreach (TabInfo destinationTab in listTabs)
                                        {
                                            ModuleInfo module = ModuleController.Instance.GetModule(minfo.ModuleID, destinationTab.TabID, false);
                                            if (module != null)
                                            {
                                                if (module.IsDeleted)
                                                {
                                                    ModuleController.Instance.RestoreModule(module);
                                                }
                                            }
                                            else
                                            {
                                                if (!PortalSettings.Current.ContentLocalizationEnabled || (minfo.CultureCode == destinationTab.CultureCode))
                                                {
                                                    ModuleController.Instance.CopyModule(minfo, destinationTab, minfo.PaneName, true);
                                                }
                                            }
                                        }
                                    }
                                    item.InnerHtml = "<div vjmod=\"true\"><app id=\"" + mid + "\"></app>";
                                }
                                else if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null)
                                {
                                    if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() != "global")
                                    {
                                        item.InnerHtml = item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value;
                                    }
                                }
                            }
                            CustomBlock.Html = html.DocumentNode.OuterHtml;
                        }

                        cb.Html = CustomBlock.Html;
                        cb.Css = CustomBlock.Css;
                        cb.IsGlobal = CustomBlock.IsGlobal;
                        cb.UpdatedBy = PortalSettings.Current.UserId;
                        cb.UpdatedOn = DateTime.UtcNow;
                        BlockFactory.AddUpdate(cb);
                        Result.Status = "Success";
                        Result.Guid = cb.Guid;
                        Result.CustomBlock = cb;
                    }
                    else
                    {
                        Result.Status = "Not exist";
                    }
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message.ToString();
                    ExceptionManager.LogException(ex);
                }
                return Result;
            }
            public static HttpResponseMessage ExportCustomBlock(int PortalID, string GUID)
            {
                HttpResponseMessage Response = new HttpResponseMessage();
                CustomBlock customBlock = BlockManager.GetByLocale(PortalID, GUID, null);
                if (customBlock != null)
                {
                    Dictionary<int, string> ExportedModulesContent = new Dictionary<int, string>();
                    string Theme = Core.Managers.ThemeManager.CurrentTheme.Name;
                    ExportTemplate exportTemplate = new ExportTemplate
                    {
                        Name = customBlock.Name,
                        Type = TemplateType.Block.ToString(),
                        UpdatedOn = DateTime.UtcNow,
                        Templates = new List<Layout>(),
                        ThemeName = Theme,
                        ThemeGuid = ThemeManager.CurrentTheme.GUID
                    };
                    Dictionary<string, string> Assets = new Dictionary<string, string>();
                    Layout layout = new Layout();
                    layout.Blocks = new List<CustomBlock>() { customBlock };
                    if (layout.Blocks != null)
                    {
                        foreach (CustomBlock block in layout.Blocks)
                        {
                            if (!string.IsNullOrEmpty(block.Html))
                            {
                                block.Html = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.Html, PortalID), false, Assets);
                                PageManager.ProcessPortableModules(PortalID, block.Html, ExportedModulesContent);
                            }
                            if (!string.IsNullOrEmpty(block.Css))
                                block.Css = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.Css, PortalID), false, Assets);
                        }
                        CacheFactory.Clear(CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", PortalID));
                    }
                    layout.Name = customBlock.Name;
                    layout.Content = "";
                    layout.SVG = "";
                    layout.ContentJSON = "";
                    layout.Style = "";
                    layout.StyleJSON = "";
                    layout.Type = "";
                    exportTemplate.Templates.Add(layout);
                    string serializedExportTemplate = JsonConvert.SerializeObject(exportTemplate);
                    if (!string.IsNullOrEmpty(serializedExportTemplate))
                    {
                        byte[] fileBytes = null;
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                            {
                                AddZipItem("Template.json", Encoding.Unicode.GetBytes(serializedExportTemplate), zip);
                                foreach (var exportedModuleContent in ExportedModulesContent)
                                    AddZipItem("PortableModules/" + exportedModuleContent.Key + ".json", Encoding.Unicode.GetBytes(exportedModuleContent.Value), zip);
                                if (Assets != null && Assets.Count > 0)
                                {
                                    foreach (KeyValuePair<string, string> asset in Assets)
                                    {
                                        string FileName = asset.Key.Replace(PageManager.ExportTemplateRootToken, "");
                                        string FileUrl = asset.Value;
                                        if (FileUrl.StartsWith("/"))
                                        {
                                            FileUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileUrl);
                                        }
                                        try
                                        {
                                            AddZipItem("Assets/" + FileName, new WebClient().DownloadData(FileUrl), zip);
                                        }
                                        catch (Exception ex) { ExceptionManager.LogException(ex); }
                                    }
                                }
                            }
                            fileBytes = memoryStream.ToArray();
                        }
                        string fileName = customBlock.Name + "_Block.zip";
                        Response.Content = new ByteArrayContent(fileBytes.ToArray());
                        Response.Content.Headers.Add("x-filename", fileName);
                        Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileName
                        };
                        Response.StatusCode = HttpStatusCode.OK;
                    }
                }
                return Response;
            }
            private static void AddZipItem(string zipItemName, byte[] zipData, ZipArchive zip)
            {
                ZipArchiveEntry zipItem = zip.CreateEntry(zipItemName);
                using (MemoryStream originalFileMemoryStream = new MemoryStream(zipData))
                {
                    using (Stream entryStream = zipItem.Open())
                    {
                        originalFileMemoryStream.CopyTo(entryStream);
                    }
                }
            }
            public static dynamic Delete(int PortalID, string GUID)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    BlockFactory.Delete(PortalID, GUID);
                    Result.Status = "Success";
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message.ToString();
                    ExceptionManager.LogException(ex);
                }
                return Result;
            }

            public static CustomBlock GetByLocale(int PortalID, string GUID, string Locale)
            {
                CustomBlock cb = BlockFactory.GetAll(PortalID).Where(b => b.Guid.ToLower() == GUID.ToLower() && b.Locale == Locale).FirstOrDefault();
                if (cb == null)
                {
                    cb = BlockFactory.GetAll(PortalID).Where(b => b.Guid.ToLower() == GUID.ToLower() && b.Locale == null).FirstOrDefault();
                }

                return cb;
            }
            public static List<CustomBlock> GetAll(PortalSettings PortalSettings)
            {
                List<CustomBlock> CustomBlocks = BlockFactory.GetAll(PortalSettings.PortalId).Where(c => c.Locale == null).ToList();
                string Locale = PageManager.GetCultureCode(PortalSettings);
                if (!string.IsNullOrEmpty(Locale))
                {
                    List<CustomBlock> CustomBlocksByLocale = BlockFactory.GetAll(PortalSettings.PortalId).Where(c => c.Locale == Locale).ToList();
                    if (CustomBlocksByLocale == null || CustomBlocksByLocale.Count <= 0)
                    {
                        return CustomBlocks.OrderBy(o => o.Category).OrderBy(o => o.Name).ToList();
                    }
                    else
                    {
                        foreach (CustomBlock item in CustomBlocks)
                        {
                            if (CustomBlocksByLocale.Where(c => c.Guid.ToLower() == item.Guid.ToLower()).FirstOrDefault() == null)
                            {
                                CustomBlocksByLocale.Add(item);
                            }
                        }
                        return CustomBlocksByLocale.OrderBy(o => o.Category).OrderBy(o => o.Name).ToList();
                    }
                }
                else
                {
                    return CustomBlocks.OrderBy(o => o.Category).OrderBy(o => o.Name).ToList();
                }
            }
        }
    }
}
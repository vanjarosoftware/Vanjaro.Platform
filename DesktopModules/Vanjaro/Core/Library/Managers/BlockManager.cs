using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    string FolderPath = Globals.ApplicationMapPath + @"\portals\_default\" + GetTheme(PortalSettings.PortalId) + BlockDirectory + "\\" + Block + "\\";
                    string PortalFolderPath = FolderPath.Replace("_default", PortalSettings.PortalId.ToString());
                    if (Directory.Exists(FolderPath))
                    {
                        string markup = string.Empty;
                        if (File.Exists(FolderPath + Block + ".html"))
                        {
                            markup += File.ReadAllText(FolderPath + Block + ".html");
                        }

                        if (IsGlobal)
                        {
                            if (Directory.Exists(PortalFolderPath) && File.Exists(PortalFolderPath + Block + ".config.html"))
                                markup += File.ReadAllText(PortalFolderPath + Block + ".config.html");
                            else if (File.Exists(FolderPath + Block + ".config.html"))
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
            public static void AddCustom(CustomBlock _customBlock)
            {
                BlockFactory.AddUpdate(_customBlock);
            }
            public static string GetTheme(int PortalID)
            {
                return "vThemes\\" + ThemeManager.GetCurrent(PortalID).Name + "\\";
            }
            public static string GetVirtualPath()
            {
                return Globals.ApplicationPath + VirtualPathUtility.ToAppRelative(@"\portals\_default\");
            }
            public static string GetTemplateDir(PortalSettings PortalSettings, string Block)
            {
                return GetVirtualPath() + GetTheme(PortalSettings.PortalId) + "Blocks\\" + Block + "\\Templates\\";
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
                    FolderPath = FolderPath.Replace("_default", PortalSettings.PortalId.ToString());
                    if (!Directory.Exists(FolderPath))
                        Directory.CreateDirectory(FolderPath);
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
            public static List<GlobalBlock> GetAllGlobalBlocks(int PortalID, string Guid, string Locale)
            {
                return GlobalBlockFactory.GetAllByGUID(PortalID, Guid).Where(l => l.Locale == Locale).ToList();
            }
            public static dynamic Add(PortalSettings PortalSettings, GlobalBlock block, int ForceCount = 0)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    var aliases = from PortalAliasInfo pa in PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalSettings.PortalId)
                                  select pa.HTTPAlias;
                    if (!string.IsNullOrEmpty(block.Html))
                    {
                        string Css = block.Css;
                        block.Html = PageManager.DeTokenizeLinks(PageManager.AbsoluteToRelativeUrls(PageManager.ResetModuleMarkup(PortalSettings.PortalId, block.Html, ref Css, PortalSettings.UserId), aliases), PortalSettings.PortalId);
                        block.Css = Css;
                    }
                    if (!string.IsNullOrEmpty(block.Css))
                        block.Css = PageManager.DeTokenizeLinks(block.Css, PortalSettings.PortalId);
                    if (!string.IsNullOrEmpty(block.ContentJSON))
                    {
                        List<string> Ids = new List<string>();
                        Dictionary<string, List<string>> StyleIds = new Dictionary<string, List<string>>();
                        Dictionary<string, dynamic> GlobalKeyValuePairs = new Dictionary<string, dynamic>();
                        Dictionary<string, dynamic> GlobalStyleKeyValuePairs = new Dictionary<string, dynamic>();
                        var DeserializedContentJSON = JsonConvert.DeserializeObject(block.ContentJSON);
                        var DeserializedStyleJSON = block.StyleJSON != null ? JsonConvert.DeserializeObject(block.StyleJSON) : string.Empty;
                        PageManager.GetAllIds(DeserializedContentJSON, Ids);
                        PageManager.FilterStyle(DeserializedStyleJSON, Ids);
                        PageManager.RemoveGlobalBlockComponents(DeserializedContentJSON, StyleIds, GlobalKeyValuePairs, null);
                        PageManager.RemoveGlobalBlockStyles(DeserializedStyleJSON, StyleIds, GlobalStyleKeyValuePairs);
                        PageManager.BuildCustomBlocks(PortalSettings.PortalId, DeserializedContentJSON, DeserializedStyleJSON);
                        block.ContentJSON = PageManager.DeTokenizeLinks(PageManager.AbsoluteToRelativeUrls(JsonConvert.SerializeObject(DeserializedContentJSON), aliases), PortalSettings.PortalId);
                        if (!string.IsNullOrEmpty(block.Css))
                            block.Css = PageManager.FilterCss(block.Css, StyleIds);
                        if (!string.IsNullOrEmpty(block.StyleJSON))
                        {
                            List<string> lstStyleIds = new List<string>();
                            ExtractStyleIDs(DeserializedContentJSON, lstStyleIds);
                            FilterStyles(DeserializedStyleJSON, lstStyleIds);
                            block.StyleJSON = JsonConvert.SerializeObject(DeserializedStyleJSON);
                        }
                    }
                    if (string.IsNullOrEmpty(block.Html) && string.IsNullOrEmpty(block.Css))
                    {
                        CustomBlock _customBlock = new CustomBlock();
                        _customBlock.ID = block.ID;
                        _customBlock.Guid = block.Guid;
                        _customBlock.Name = block.Name;
                        _customBlock.Category = block.Category;
                        _customBlock.ContentJSON = block.ContentJSON;
                        _customBlock.StyleJSON = block.StyleJSON;
                        if (BlockFactory.Get(PortalSettings.PortalId, _customBlock.Name) == null)
                        {
                            if (ForceCount == 0)
                                _customBlock.Guid = Guid.NewGuid().ToString().ToLower();
                            _customBlock.PortalID = PortalSettings.PortalId;
                            _customBlock.CreatedBy = PortalSettings.UserId;
                            _customBlock.UpdatedBy = PortalSettings.UserId;
                            _customBlock.CreatedOn = DateTime.UtcNow;
                            _customBlock.UpdatedOn = DateTime.UtcNow;
                            BlockFactory.AddUpdate(_customBlock);
                            Result.Status = "Success";
                            Result.Guid = block.Guid;
                        }
                        else
                        {
                            if (ForceCount > 0)
                            {
                                block.Name = block.Name + ForceCount;
                                ForceCount++;
                                Result = Add(PortalSettings, block, ForceCount);
                            }
                            else
                            {
                                Result.Status = "Exist";
                            }
                        }
                    }
                    else
                    {
                        if (GlobalBlockFactory.Get(PortalSettings.PortalId, block.Name) == null)
                        {
                            if (ForceCount == 0)
                                block.Guid = Guid.NewGuid().ToString().ToLower();
                            block.PortalID = PortalSettings.PortalId;
                            block.CreatedBy = PortalSettings.UserId;
                            block.UpdatedBy = PortalSettings.UserId;
                            block.CreatedOn = DateTime.UtcNow;
                            block.UpdatedOn = DateTime.UtcNow;
                            block.Locale = PageManager.GetCultureCode(PortalSettings);
                            UpdateGlobalBlock(block);
                            Result.Status = "Success";
                            Result.Guid = block.Guid;
                        }
                        else
                        {
                            if (ForceCount > 0)
                            {
                                block.Name = block.Name + ForceCount;
                                ForceCount++;
                                Result = Add(PortalSettings, block, ForceCount);
                            }
                            else
                            {
                                Result.Status = "Exist";
                            }
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

            public static string RemovePermissions(HtmlDocument html, string contentJson)
            {
                if (html != null)
                {
                    IEnumerable<HtmlNode> query = html.DocumentNode.SelectNodes("//*[@perm]");
                    if (query != null)
                    {
                        foreach (HtmlNode item in query.ToList())
                        {
                            item.Attributes["perm"].Remove();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(contentJson))
                {
                    dynamic DeserializedContentJSON = JsonConvert.DeserializeObject(contentJson);
                    RemoveContentPermissions(DeserializedContentJSON);
                    if (DeserializedContentJSON != null)
                        contentJson = JsonConvert.SerializeObject(DeserializedContentJSON);
                }
                return contentJson;
            }

            private static void RemoveContentPermissions(dynamic DeserializedContentJSON)
            {
                if (DeserializedContentJSON != null)
                {
                    foreach (dynamic con in DeserializedContentJSON)
                    {
                        if (con.type != null && con.type.Value == "section")
                        {
                            if (con.attributes != null && con.attributes["perm"] != null)
                            {
                                (con.attributes as JObject).Remove("perm");
                            }
                        }
                        if (con.components != null)
                        {
                            RemoveContentPermissions(con.components);
                        }
                    }
                }
            }

            public static void UpdateGlobalBlock(GlobalBlock GlobalBlock)
            {
                GlobalBlockFactory.AddUpdate(GlobalBlock);
                GlobalBlock cb = GetGlobalByGuid(GlobalBlock.PortalID, GlobalBlock.Guid, null, false);
                if (cb == null)
                {
                    cb = GlobalBlock;
                    cb.Locale = null;
                    cb.ID = 0;
                    GlobalBlockFactory.AddUpdate(cb);
                }
                else if (cb != null && cb.Version < GlobalBlock.Version)
                {
                    cb.Locale = null;
                    cb.ID = 0;
                    GlobalBlockFactory.AddUpdate(cb);
                }
            }
            public static dynamic EditGlobalBlock(PortalSettings PortalSettings, GlobalBlock GlobalBlock)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    GlobalBlock cb = GlobalBlockFactory.GetAll(PortalSettings.PortalId, PageManager.GetCultureCode(PortalSettings)).Where(b => b.Guid.ToLower() == GlobalBlock.Guid.ToLower()).FirstOrDefault();
                    if (cb == null)
                        cb = GlobalBlockFactory.GetAll(PortalSettings.PortalId, null).Where(b => b.Guid.ToLower() == GlobalBlock.Guid.ToLower()).FirstOrDefault();
                    if (cb != null)
                    {
                        foreach (GlobalBlock _block in GlobalBlockFactory.GetAllByGUID(PortalSettings.PortalId, GlobalBlock.Guid.ToLower()))
                        {
                            _block.Name = GlobalBlock.Name;
                            _block.Category = GlobalBlock.Category;
                            _block.UpdatedBy = PortalSettings.Current.UserId;
                            _block.UpdatedOn = DateTime.UtcNow;
                            UpdateGlobalBlock(_block);
                        }
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
            public static dynamic DeleteGlobal(int PortalID, string GUID)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    GlobalBlockFactory.Delete(PortalID, GUID);
                    Result.Status = "Success";
                }
                catch (Exception ex)
                {
                    Result.Status = ex.Message.ToString();
                    ExceptionManager.LogException(ex);
                }
                return Result;
            }
            public static GlobalBlock GetGlobalByGuid(int PortalID, string GUID, string Locale, bool GetDefaultLocale, bool IsPublished = false)
            {
                GlobalBlock result;
                List<GlobalBlock> CustomBlocks = GlobalBlockFactory.GetAllByGUID(PortalID, GUID);
                if (IsPublished)
                    result = CustomBlocks.Where(b => b.Guid.ToLower() == GUID.ToLower() && b.IsPublished == true && b.Locale == Locale).OrderByDescending(a => a.Version).FirstOrDefault();
                else
                    result = CustomBlocks.Where(b => b.Guid.ToLower() == GUID.ToLower() && b.Locale == Locale).OrderByDescending(a => a.Version).FirstOrDefault();
                if (result == null && !string.IsNullOrEmpty(Locale) && GetDefaultLocale)
                    return GetGlobalByGuid(PortalID, GUID, null, false, IsPublished);
                return result;
            }
            public static GlobalBlock GetGlobalByLocale(int PortalID, string GUID, string Locale, bool IsPublished = false)
            {
                GlobalBlock cb = GlobalBlockFactory.GetAll(PortalID, Locale, IsPublished).Where(b => b.Guid.ToLower() == GUID.ToLower()).FirstOrDefault();
                if (cb == null)
                {
                    cb = GlobalBlockFactory.GetAll(PortalID, null, IsPublished).Where(b => b.Guid.ToLower() == GUID.ToLower()).FirstOrDefault();
                }
                return cb;
            }
            public static List<GlobalBlock> GetAllGlobalBlocks(PortalSettings PortalSettings, bool IsPublished = false)
            {
                List<GlobalBlock> GlobalBlocks = GlobalBlockFactory.GetAll(PortalSettings.PortalId, null, IsPublished).ToList();
                string Locale = PageManager.GetCultureCode(PortalSettings);
                if (!string.IsNullOrEmpty(Locale))
                {
                    List<GlobalBlock> CustomBlocksByLocale = GlobalBlockFactory.GetAll(PortalSettings.PortalId, Locale).ToList();
                    if (CustomBlocksByLocale == null || CustomBlocksByLocale.Count <= 0)
                    {
                        return GlobalBlocks.OrderBy(o => o.Category).OrderBy(o => o.Name).ToList();
                    }
                    else
                    {
                        foreach (GlobalBlock item in GlobalBlocks)
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
                    return GlobalBlocks.OrderBy(o => o.Category).OrderBy(o => o.Name).ToList();
                }
            }
            private static void FilterStyles(dynamic styleJSON, List<string> styleIds)
            {
                if (styleJSON != null)
                {
                    List<dynamic> itemsToRemove = new List<dynamic>();
                    foreach (dynamic con in styleJSON)
                    {
                        if (con.selectors != null)
                        {
                            List<string> selectors = new List<string>();
                            foreach (dynamic cons in con.selectors)
                            {
                                try
                                {
                                    if (cons.name != null)
                                        selectors.Add(cons.name.Value);
                                }
                                catch
                                {
                                    try { selectors.Add(cons.Value.ToString().Replace("#", "").Replace(".", "")); } catch { }
                                }
                            }
                            bool hasMatch = styleIds.Any(x => selectors.Any(y => y == x));
                            if (!hasMatch)
                                itemsToRemove.Add(con);
                        }
                    }
                    foreach (var item in itemsToRemove)
                        styleJSON.Remove(item);
                }
            }
            internal static void ExtractStyleIDs(dynamic contentJSON, List<string> styleIds)
            {
                if (contentJSON != null)
                {
                    if (contentJSON is JArray)
                    {
                        foreach (dynamic con in contentJSON)
                        {
                            if (con.attributes != null && con.attributes["id"] != null)
                                styleIds.Add(con.attributes["id"].Value);
                            if (con.components != null)
                                ExtractStyleIDs(con.components, styleIds);
                        }
                    }
                    else
                    {
                        if (contentJSON.attributes != null && contentJSON.attributes["id"] != null)
                            styleIds.Add(contentJSON.attributes["id"].Value);
                        if (contentJSON.components != null)
                            ExtractStyleIDs(contentJSON.components, styleIds);
                    }
                }
            }
            public static void UpdateCustomBlock(CustomBlock CustomBlock)
            {
                BlockFactory.AddUpdate(CustomBlock);
            }
            public static dynamic EditCustomBlock(PortalSettings PortalSettings, CustomBlock CustomBlock)
            {
                dynamic Result = new ExpandoObject();
                try
                {
                    CustomBlock cb = BlockFactory.GetAll(PortalSettings.PortalId).Where(b => b.Guid.ToLower() == CustomBlock.Guid.ToLower()).FirstOrDefault();
                    if (cb != null)
                    {
                        cb.Name = CustomBlock.Name;
                        cb.Category = CustomBlock.Category;
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
                CustomBlock customBlock = GetCustomByGuid(PortalID, GUID);
                if (customBlock != null)
                {
                    Dictionary<int, string> ExportedModulesContent = new Dictionary<int, string>();
                    string Theme = ThemeManager.CurrentTheme.Name;
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
                    GlobalBlock globalBlock = new GlobalBlock();
                    globalBlock.ID = customBlock.ID;
                    globalBlock.Guid = customBlock.Guid;
                    globalBlock.PortalID = customBlock.PortalID;
                    globalBlock.Name = customBlock.Name;
                    globalBlock.Category = customBlock.Category;
                    globalBlock.ContentJSON = customBlock.ContentJSON;
                    globalBlock.StyleJSON = customBlock.StyleJSON;
                    globalBlock.CreatedBy = customBlock.CreatedBy;
                    globalBlock.CreatedOn = customBlock.CreatedOn;
                    globalBlock.UpdatedBy = customBlock.UpdatedBy;
                    globalBlock.UpdatedOn = customBlock.UpdatedOn;
                    layout.Blocks = new List<GlobalBlock>() { globalBlock };
                    if (layout.Blocks != null)
                    {
                        foreach (GlobalBlock block in layout.Blocks)
                        {
                            if (!string.IsNullOrEmpty(block.ContentJSON))
                            {
                                block.ContentJSON = PageManager.TokenizeTemplateLinks(PortalID, PageManager.DeTokenizeLinks(block.ContentJSON, PortalID), true, Assets);
                                block.ContentJSON = RemovePermissions(null, block.ContentJSON);
                            }
                            if (!string.IsNullOrEmpty(block.StyleJSON))
                                block.StyleJSON = PageManager.TokenizeTemplateLinks(PortalID, PageManager.DeTokenizeLinks(block.StyleJSON, PortalID), true, Assets);
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
                                        catch (Exception ex) {}
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

            //public static HttpResponseMessage ExportGlobalBlock(int PortalID, string GUID)
            //{
            //    HttpResponseMessage Response = new HttpResponseMessage();
            //    CustomBlock customBlock = BlockManager.GetCustomByGuid(PortalID, GUID);
            //    if (customBlock != null)
            //    {
            //        Dictionary<int, string> ExportedModulesContent = new Dictionary<int, string>();
            //        string Theme = Core.Managers.ThemeManager.CurrentTheme.Name;
            //        ExportTemplate exportTemplate = new ExportTemplate
            //        {
            //            Name = customBlock.Name,
            //            Type = TemplateType.Block.ToString(),
            //            UpdatedOn = DateTime.UtcNow,
            //            Templates = new List<Layout>(),
            //            ThemeName = Theme,
            //            ThemeGuid = ThemeManager.CurrentTheme.GUID
            //        };
            //        Dictionary<string, string> Assets = new Dictionary<string, string>();
            //        Layout layout = new Layout();
            //        layout.Blocks = new List<CustomBlock>() { customBlock };
            //        if (layout.Blocks != null)
            //        {
            //            foreach (CustomBlock block in layout.Blocks)
            //            {
            //                if (!string.IsNullOrEmpty(block.Html))
            //                {
            //                    block.Html = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.Html, PortalID), false, Assets);
            //                    PageManager.ProcessPortableModules(PortalID, block.Html, ExportedModulesContent);
            //                }
            //                if (!string.IsNullOrEmpty(block.Css))
            //                    block.Css = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.Css, PortalID), false, Assets);
            //                if (!string.IsNullOrEmpty(block.ContentJSON))
            //                    block.ContentJSON = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.ContentJSON, PortalID), true, Assets);
            //                if (!string.IsNullOrEmpty(block.StyleJSON))
            //                    block.StyleJSON = PageManager.TokenizeTemplateLinks(PageManager.DeTokenizeLinks(block.StyleJSON, PortalID), true, Assets);
            //            }
            //            CacheFactory.Clear(CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", PortalID));
            //        }
            //        layout.Name = customBlock.Name;
            //        layout.Content = "";
            //        layout.SVG = "";
            //        layout.ContentJSON = "";
            //        layout.Style = "";
            //        layout.StyleJSON = "";
            //        layout.Type = "";
            //        exportTemplate.Templates.Add(layout);
            //        string serializedExportTemplate = JsonConvert.SerializeObject(exportTemplate);
            //        if (!string.IsNullOrEmpty(serializedExportTemplate))
            //        {
            //            byte[] fileBytes = null;
            //            using (MemoryStream memoryStream = new MemoryStream())
            //            {
            //                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            //                {
            //                    AddZipItem("Template.json", Encoding.Unicode.GetBytes(serializedExportTemplate), zip);
            //                    foreach (var exportedModuleContent in ExportedModulesContent)
            //                        AddZipItem("PortableModules/" + exportedModuleContent.Key + ".json", Encoding.Unicode.GetBytes(exportedModuleContent.Value), zip);
            //                    if (Assets != null && Assets.Count > 0)
            //                    {
            //                        foreach (KeyValuePair<string, string> asset in Assets)
            //                        {
            //                            string FileName = asset.Key.Replace(PageManager.ExportTemplateRootToken, "");
            //                            string FileUrl = asset.Value;
            //                            if (FileUrl.StartsWith("/"))
            //                            {
            //                                FileUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileUrl);
            //                            }
            //                            try
            //                            {
            //                                AddZipItem("Assets/" + FileName, new WebClient().DownloadData(FileUrl), zip);
            //                            }
            //                            catch (Exception ex) {}
            //                        }
            //                    }
            //                }
            //                fileBytes = memoryStream.ToArray();
            //            }
            //            string fileName = customBlock.Name + "_Block.zip";
            //            Response.Content = new ByteArrayContent(fileBytes.ToArray());
            //            Response.Content.Headers.Add("x-filename", fileName);
            //            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //            Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //            {
            //                FileName = fileName
            //            };
            //            Response.StatusCode = HttpStatusCode.OK;
            //        }
            //    }
            //    return Response;
            //}
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
            public static dynamic DeleteCustom(int PortalID, string GUID)
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
            public static CustomBlock GetCustomByGuid(int PortalID, string GUID, bool isLibrary = false)
            {
                return BlockFactory.GetByGUID(PortalID, GUID, isLibrary);
            }
            public static List<CustomBlock> GetAllCustomBlocks(PortalSettings PortalSettings)
            {
                return BlockFactory.GetAll(PortalSettings.PortalId).OrderBy(o => o.Category).OrderBy(o => o.Name).ToList(); ;
            }

        }
    }
}
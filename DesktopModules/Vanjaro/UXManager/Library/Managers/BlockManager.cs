using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities.Menu;

namespace Vanjaro.UXManager.Library
{
    public static partial class Managers
    {
        public class BlockManager
        {
            internal static List<ThemeTemplateResponse> FindBlocks(Pages page, HtmlDocument html, Dictionary<string, string> BlockAttributes)
            {
                List<ThemeTemplateResponse> responses = new List<ThemeTemplateResponse>();
                IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                foreach (HtmlNode item in query.ToList())
                {
                    if (item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault().Value))
                    {
                        string BlockGUID = item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault().Value;
                        if (BlockAttributes.Where(a => a.Key == "data-block-guid") != null && BlockAttributes.Where(a => a.Key == "data-block-guid").FirstOrDefault().Value != null && BlockAttributes.Where(a => a.Key == "data-block-guid").FirstOrDefault().Value.ToLower() == BlockGUID.ToLower())
                        {
                            Dictionary<string, string> Attributes = new Dictionary<string, string>();
                            foreach (HtmlAttribute attr in item.Attributes)
                            {
                                Attributes.Add(attr.Name, attr.Value);
                            }

                            foreach (KeyValuePair<string, string> attr in BlockAttributes)
                            {
                                if (!Attributes.ContainsKey(attr.Key))
                                {
                                    Attributes.Add(attr.Key, attr.Value);
                                }
                            }
                            responses.Add(Core.Managers.BlockManager.Render(Attributes));
                        }
                    }
                }
                return responses;
            }
                //    public static List<IBlock> GetExtentions()
                //    {
                //        return Extentions.ToList();
                //    }

                //    internal static List<IBlock> Extentions
                //    {
                //        get
                //        {
                //            string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Block_Extension);
                //            List<IBlock> Items = CacheFactory.Get(CacheKey);
                //            if (Items == null)
                //            {
                //                List<IBlock> ServiceInterfaceAssemblies = new List<IBlock>();
                //                string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                //                foreach (string Path in binAssemblies)
                //                {
                //                    try
                //                    {
                //                        //get all assemblies 
                //                        IEnumerable<IBlock> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                //                                                              where t != (typeof(IBlock)) && (typeof(IBlock).IsAssignableFrom(t))
                //                                                              select Activator.CreateInstance(t) as IBlock;

                //                        ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IBlock>());
                //                    }
                //                    catch { continue; }
                //                }
                //                Items = ServiceInterfaceAssemblies;
                //                CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                //            }
                //            return Items;
                //        }
                //    }

                //    internal static List<Block> GetAll()
                //    {
                //        return Extentions.Where(e => e.Name != "Custom").Select(s => new Block()
                //        {
                //            Category = s.Category,
                //            Guid = s.Guid,
                //            Name = s.Name,
                //            Icon = s.Icon,
                //            Attributes = (s.Attributes.ContainsKey("data-block-global") && s.Attributes["data-block-global"] == "true") ? GetGlobalConfigs(PortalSettings.Current, s.Name) : GetGlobalConfigs(PortalSettings.Current, s.Name, false)
                //        }).ToList();
                //    }

                //    public static ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
                //    {
                //        ThemeTemplateResponse result = null;
                //        if (Attributes.ContainsKey("data-block-guid") && !string.IsNullOrEmpty(Attributes["data-block-guid"]))
                //        {
                //            dynamic ext = Extentions.Where(s => s.Guid == Guid.Parse(Attributes["data-block-guid"])).FirstOrDefault();
                //            if (ext != null)
                //            {
                //                foreach (var checkAttr in ext.Attributes)
                //                {
                //                    if (!Attributes.ContainsKey(checkAttr.Key))
                //                        Attributes.Add(checkAttr.Key, checkAttr.Value);

                //                }
                //                result = ext.Render(Attributes);
                //                StringBuilder sb = new StringBuilder();
                //                sb.Append("<div");
                //                foreach (var item in Attributes)
                //                    sb.Append(" ").Append(item.Key).Append("=\"" + item.Value + "\"");
                //                sb.Append(">" + result.Markup + "</div>");
                //                result.Markup = sb.ToString();
                //            }
                //        }
                //        return result;
                //    }

                //    public static Dictionary<string, string> GetGlobalConfigs(PortalSettings PortalSettings, string Block, bool IsGlobal = true)
                //    {
                //        string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.GlobalConfig, PortalSettings.PortalId, Block);
                //        Dictionary<string, string> result = CacheFactory.Get(CacheKey);
                //        if (result == null)
                //        {
                //            result = new Dictionary<string, string>();
                //            string FolderPath = GetThemeDir(PortalSettings) + "Blocks\\" + Block + "\\";
                //            if (Directory.Exists(FolderPath))
                //            {
                //                string markup = string.Empty;
                //                if (File.Exists(FolderPath + Block + ".html"))
                //                    markup += File.ReadAllText(FolderPath + Block + ".html");
                //                if (IsGlobal && File.Exists(FolderPath + Block + ".config.html"))
                //                    markup += File.ReadAllText(FolderPath + Block + ".config.html");
                //                if (!string.IsNullOrEmpty(markup))
                //                {
                //                    HtmlDocument html = new HtmlDocument();
                //                    html.LoadHtml(markup);
                //                    var query = html.DocumentNode.Descendants("div");
                //                    foreach (var item in query.ToList())
                //                    {
                //                        if (item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault() != null)
                //                        {
                //                            foreach (var attr in item.Attributes)
                //                            {
                //                                if (result.ContainsKey(attr.Name))
                //                                    result[attr.Name] = attr.Value;
                //                                else
                //                                    result.Add(attr.Name, attr.Value);
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //            CacheFactory.Set(CacheKey, result);
                //        }
                //        return result;
                //    }


                //    public static string GetThemeDir(PortalSettings PortalSettings, string ThemeName = "Default")
                //    {
                //        return PortalSettings.HomeDirectoryMapPath + "vThemes\\" + ThemeName + "\\";
                //    }

                //    public static string GetTemplateDir(PortalSettings PortalSettings, string Block)
                //    {
                //        return GetThemeDir(PortalSettings) + "Blocks\\" + Block + "\\Templates\\";
                //    }

                //    public static void UpdateDesignElement(PortalSettings PortalSettings, Dictionary<string, string> Attributes)
                //    {
                //        if (Attributes.ContainsKey("data-block-global") && Attributes["data-block-global"] == "true")
                //        {
                //            StringBuilder sb = new StringBuilder();
                //            sb.Append("<div");
                //            foreach (var item in Attributes)
                //            {
                //                if (item.Key.StartsWith("data-block"))
                //                    sb.Append(" ").Append(item.Key).Append("=\"" + item.Value + "\"");
                //            }
                //            sb.Append("></div>");
                //            string FolderPath = PortalSettings.HomeDirectoryMapPath + "vThemes\\" + ThemeManager.GetCurrentThemeName() + "\\Blocks\\" + Attributes["data-block-type"] + "\\";
                //            if (Directory.Exists(FolderPath))
                //            {
                //                if (!File.Exists(FolderPath + Attributes["data-block-type"] + ".config.html"))
                //                    File.Create(FolderPath + Attributes["data-block-type"].ToLower() + ".config.html").Dispose();

                //                if (File.Exists(FolderPath + Attributes["data-block-type"] + ".config.html"))
                //                {
                //                    File.WriteAllText(FolderPath + Attributes["data-block-type"] + ".config.html", sb.ToString());
                //                    CacheFactory.Clear();
                //                }
                //            }
                //        }
                //    }
            }
    }
}
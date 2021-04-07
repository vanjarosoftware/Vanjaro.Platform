using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class GlobalBlockFactory
        {
            internal static void AddUpdate(GlobalBlock globalBlock)
            {
                if (string.IsNullOrEmpty(globalBlock.StyleJSON))
                    globalBlock.StyleJSON = "";
                if (string.IsNullOrEmpty(globalBlock.Css))
                    globalBlock.Css = "";
                if (string.IsNullOrEmpty(globalBlock.Html))
                    globalBlock.Html = "";
                globalBlock.Category = globalBlock.Category.ToLower();
                if (globalBlock.ID > 0)
                {
                    globalBlock.Update();
                    CacheFactory.Clear(CacheFactory.Keys.GlobalBlock);
                }
                else
                {
                    int Version = 1;
                    GlobalBlock _GlobalBlock = GetAllByGUID(globalBlock.PortalID, globalBlock.Guid).Where(a => a.IsPublished == true).OrderByDescending(a => a.Version).FirstOrDefault();
                    if (_GlobalBlock != null)
                        Version = _GlobalBlock.Version + 1;
                    globalBlock.Version = Version;
                    if (Version == 1)
                    {
                        globalBlock.IsPublished = true;
                        globalBlock.PublishedBy = globalBlock.CreatedBy;
                        globalBlock.PublishedOn = DateTime.Now;
                    }
                    else
                    {
                        globalBlock.IsPublished = false;
                        globalBlock.PublishedBy = null;
                        globalBlock.PublishedOn = null;
                    }
                    globalBlock.Insert();
                    CacheFactory.Clear(CacheFactory.Keys.GlobalBlock);
                    RemoveRevisions(globalBlock.PortalID, globalBlock.Guid);
                }
            }
            internal static GlobalBlock Get(int PortalID, string Name)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.GlobalBlock, PortalID, Name);
                GlobalBlock GlobalBlock = CacheFactory.Get(CacheKey);
                if (GlobalBlock == null)
                {
                    GlobalBlock = GlobalBlock.Query("where PortalID=@0 and Name=@1", PortalID, Name).FirstOrDefault();
                    CacheFactory.Set(CacheKey, GlobalBlock);
                }
                return GlobalBlock;
            }
            internal static List<GlobalBlock> GetAllByGUID(int PortalID, string Guid)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.GlobalBlock + "AllByGUID", PortalID, Guid);
                List<GlobalBlock> Global_Block = CacheFactory.Get(CacheKey);
                if (Global_Block == null)
                {
                    Global_Block = GlobalBlock.Query("where PortalID=@0 and Guid=@1", PortalID, Guid).ToList();
                    CacheFactory.Set(CacheKey, Global_Block);
                }
                return Global_Block;
            }
            internal static List<GlobalBlock> GetAll(int PortalID, string Locale, bool IsPublished = false)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.GlobalBlock + "ALL", PortalID, Locale, IsPublished);
                List<GlobalBlock> Global_Blocks = CacheFactory.Get(CacheKey);
                if (Global_Blocks == null)
                {
                    if (string.IsNullOrEmpty(Locale))
                    {
                        if (IsPublished)
                            Global_Blocks = GlobalBlock.Query("where PortalID=@0 and IsPublished=@1 and Locale is null", PortalID, IsPublished).GroupBy(a => a.Guid).Select(a => a.OrderByDescending(n => n.Version).FirstOrDefault()).ToList();
                        else
                            Global_Blocks = GlobalBlock.Query("where PortalID=@0 and Locale is null", PortalID).GroupBy(a => a.Guid).Select(a => a.OrderByDescending(n => n.Version).FirstOrDefault()).ToList();
                    }
                    else
                    {
                        if (IsPublished)
                            Global_Blocks = GlobalBlock.Query("where PortalID=@0 and IsPublished=@1 and Locale=@2", PortalID, IsPublished, Locale).GroupBy(a => a.Guid).Select(a => a.OrderByDescending(n => n.Version).FirstOrDefault()).ToList();
                        else
                            Global_Blocks = GlobalBlock.Query("where PortalID=@0 and Locale=@1", PortalID, Locale).GroupBy(a => a.Guid).Select(a => a.OrderByDescending(n => n.Version).FirstOrDefault()).ToList();
                    }
                    CacheFactory.Set(CacheKey, Global_Blocks);
                }
                return Global_Blocks;
            }
            internal static void Delete(int PortalID, string Guid)
            {
                GlobalBlock.Delete("where PortalID=@0 and Guid=@1", PortalID, Guid.ToLower());
                CacheFactory.Clear(CacheFactory.Keys.GlobalBlock);
            }
            private static void RemoveRevisions(int PortalID, string Guid)
            {
                int Version = 5;
                Setting setting = SettingFactory.GetSetting(PortalID, 0, "setting_workflow", "MaxRevisions");
                if (setting != null)
                {
                    Version = int.Parse(setting.Value);
                }
                List<int> GlobalBlocks = GetAllByGUID(PortalID, Guid).OrderByDescending(a => a.Version).Select(a => a.Version).Distinct().Take(Version).ToList();
                if (GlobalBlocks.Count > 0)
                {
                    GlobalBlock.Delete("Where PortalID=@0 and Guid=@1 and Version not in (" + string.Join(",", GlobalBlocks) + ")", PortalID, Guid);
                    CacheFactory.Clear(CacheFactory.Keys.GlobalBlock);
                }
            }
        }
    }
}
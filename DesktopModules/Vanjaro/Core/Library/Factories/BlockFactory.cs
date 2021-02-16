using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class BlockFactory
        {
            internal static void AddUpdate(CustomBlock CustomBlock)
            {
                if (string.IsNullOrEmpty(CustomBlock.Html))
                {
                    CustomBlock.Html = "";
                }

                CustomBlock.Category = CustomBlock.Category.ToLower();
                if (CustomBlock.ID > 0)
                {
                    CustomBlock.Update();
                }
                else
                {
                    int Version = GetAllByGUID(CustomBlock.PortalID, CustomBlock.Guid).Where(a => a.IsPublished == true).Count() + 1;
                    CustomBlock.Version = Version;
                    CustomBlock.IsPublished = false;
                    CustomBlock.PublishedBy = null;
                    CustomBlock.PublishedOn = null;
                    CustomBlock.Insert();
                    RemoveRevisions(CustomBlock.PortalID, CustomBlock.Guid);
                }

                CacheFactory.Clear(CacheFactory.Keys.CustomBlock);
            }

            private static void RemoveRevisions(int PortalID, string Guid)
            {
                int Version = 5;
                Setting setting = SettingFactory.GetSetting(PortalID, 0, "setting_workflow", "MaxRevisions");
                if (setting != null)
                {
                    Version = int.Parse(setting.Value);
                }

                List<CustomBlock> CustomBlocks = GetAllByGUID(PortalID, Guid).OrderByDescending(a => a.Version).Select(a => a.Version).Distinct().Take(Version).ToList();
                CustomBlock.Delete("Where PortalID=@0 Guid=@1 and Version not in (" + string.Join(",", CustomBlocks) + ")", PortalID, Guid);
                CacheFactory.Clear(CacheFactory.Keys.CustomBlock);
            }

            internal static void Delete(int PortalID, string Guid)
            {
                foreach (CustomBlock item in GetAll(PortalID).Where(p => p.Guid.ToLower() == Guid.ToLower()).ToList())
                {
                    CustomBlock.Delete("where ID=@0", item.ID);
                }

                CacheFactory.Clear(CacheFactory.Keys.CustomBlock);
            }

            internal static CustomBlock Get(int PortalID, string Name)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock, PortalID, Name);
                CustomBlock CustomBlock = CacheFactory.Get(CacheKey);
                if (CustomBlock == null)
                {
                    CustomBlock = CustomBlock.Query("where PortalID=@0 and Name=@1", PortalID, Name).FirstOrDefault();
                    CacheFactory.Set(CacheKey, CustomBlock);
                }
                return CustomBlock;
            }

            internal static List<CustomBlock> GetAll(int PortalID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", PortalID);
                List<CustomBlock> Custom_Block = CacheFactory.Get(CacheKey);
                if (Custom_Block == null)
                {
                    Custom_Block = CustomBlock.Query("where PortalID=@0", PortalID).ToList();
                    CacheFactory.Set(CacheKey, Custom_Block);
                }
                return Custom_Block;
            }

            internal static List<CustomBlock> GetAllByGUID(int PortalID, string Guid)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "AllByGUID", PortalID, Guid);
                List<CustomBlock> Custom_Block = CacheFactory.Get(CacheKey);
                if (Custom_Block == null)
                {
                    Custom_Block = CustomBlock.Query("where PortalID=@0 and Guid=@1", PortalID, Guid).ToList();
                    CacheFactory.Set(CacheKey, Custom_Block);
                }
                return Custom_Block;
            }


        }
    }
}
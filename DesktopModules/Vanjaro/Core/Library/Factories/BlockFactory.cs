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
                CustomBlock.Category = CustomBlock.Category.ToLower();
                if (CustomBlock.ID > 0)
                    CustomBlock.Update();
                else
                    CustomBlock.Insert();
                CacheFactory.Clear(CacheFactory.Keys.CustomBlock);
            }
            internal static void Delete(int PortalID, string Guid)
            {
                CustomBlock.Delete("where Guid=@0", Guid);
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
            internal static CustomBlock GetByGUID(int PortalID, string Guid)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "GetByGUID", PortalID, Guid);
                CustomBlock Custom_Block = CacheFactory.Get(CacheKey);
                if (Custom_Block == null)
                {
                    Custom_Block = CustomBlock.Query("where PortalID=@0 and Guid=@1", PortalID, Guid).FirstOrDefault();
                    CacheFactory.Set(CacheKey, Custom_Block);
                }
                return Custom_Block;
            }
        }
    }
}
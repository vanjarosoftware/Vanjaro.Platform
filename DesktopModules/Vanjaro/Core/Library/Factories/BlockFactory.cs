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
                if (string.IsNullOrEmpty(CustomBlock.StyleJSON))
                    CustomBlock.StyleJSON = "";
                CustomBlock.Category = CustomBlock.Category.ToLower();
                if (CustomBlock.ID > 0)
                    CustomBlock.Update();
                else
                    CustomBlock.Insert();
                CacheFactory.Clear(CacheFactory.Keys.CustomBlock);
            }
            internal static void Delete(int PortalID, string Guid, bool IsLibrary = false)
            {
                if (IsLibrary)
                    CustomBlock.Delete("where Guid=@0 and IsLibrary=@1", Guid, IsLibrary);
                else
                    CustomBlock.Delete("where Guid=@0", Guid);
                CacheFactory.Clear(CacheFactory.Keys.CustomBlock);
            }
            internal static CustomBlock Get(int PortalID, string Name, bool IsLibrary = false)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock, PortalID, Name, IsLibrary);
                CustomBlock CustomBlock = CacheFactory.Get(CacheKey);
                if (CustomBlock == null)
                {
                    CustomBlock = CustomBlock.Query("where PortalID=@0 and Name=@1 and IsLibrary=@2", PortalID, Name, IsLibrary).FirstOrDefault();
                    CacheFactory.Set(CacheKey, CustomBlock);
                }
                return CustomBlock;
            }
            internal static List<CustomBlock> GetAll(int PortalID, bool IsLibrary = false)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "ALL", PortalID, IsLibrary);
                List<CustomBlock> Custom_Block = CacheFactory.Get(CacheKey);
                if (Custom_Block == null)
                {
                    Custom_Block = CustomBlock.Query("where PortalID=@0 and IsLibrary=@1", PortalID, IsLibrary).ToList();
                    CacheFactory.Set(CacheKey, Custom_Block);
                }
                return Custom_Block;
            }
            internal static CustomBlock GetByGUID(int PortalID, string Guid, bool IsLibrary = false)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomBlock + "GetByGUID", PortalID, Guid, IsLibrary);
                CustomBlock Custom_Block = CacheFactory.Get(CacheKey);
                if (Custom_Block == null)
                {
                    Custom_Block = CustomBlock.Query("where PortalID=@0 and Guid=@1 and IsLibrary=@2", PortalID, Guid, IsLibrary).FirstOrDefault();
                    CacheFactory.Set(CacheKey, Custom_Block);
                }
                return Custom_Block;
            }
        }
    }
}
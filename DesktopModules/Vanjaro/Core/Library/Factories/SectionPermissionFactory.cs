using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class SectionPermissionFactory
        {
            internal static List<BlockSectionPermission> GetPermissionsByEntityID(int EntityID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.SectionPermission + "GetPermissionsByEntityID", EntityID);
                List<BlockSectionPermission> _SectionPerm = CacheFactory.Get(CacheKey) as List<BlockSectionPermission>;
                if (_SectionPerm == null)
                {
                    _SectionPerm = BlockSectionPermission.Query("where EntityID=@0", EntityID).ToList();
                    CacheFactory.Set(CacheKey, _SectionPerm);
                }
                return _SectionPerm;
            }
            internal static List<SectionPermissionInfo> GetPermissionByCode(string Code)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.SectionPermission + "GetPermissionByCode", Code);
                List<SectionPermissionInfo> _SectionPermInfo = CacheFactory.Get(CacheKey) as List<SectionPermissionInfo>;
                if (_SectionPermInfo == null)
                {
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        _SectionPermInfo = db.Query<SectionPermissionInfo>("SELECT p.* FROM " + Data.Scripts.CommonScript.DnnTablePrefix + "Permission AS p WHERE p.PermissionCode = @0", Code).ToList();
                    }
                    CacheFactory.Set(CacheKey, _SectionPermInfo);
                }
                return _SectionPermInfo;
            }
            internal static int AddBlockSection(int TabID, bool? Inherit)
            {
                BlockSection blockSection = new BlockSection();
                blockSection.EntityID = 0;
                blockSection.TabID = TabID;
                blockSection.Inherit = Inherit;
                blockSection.Insert();
                CacheFactory.Clear(CacheFactory.Keys.SectionPermission);
                return blockSection.EntityID;
            }

            internal static void Delete(int EntityID)
            {
                BlockSection.Delete("where EntityID=@0", EntityID);
                ClearAllPermissions(EntityID);
            }

            internal static BlockSection GetBlockSection(int EntityID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.SectionPermission + "GetBlockSection", EntityID);
                BlockSection _BlockSection = CacheFactory.Get(CacheKey) as BlockSection;
                if (_BlockSection == null)
                {
                    _BlockSection = BlockSection.Query("where EntityID=@0", EntityID).FirstOrDefault();
                    CacheFactory.Set(CacheKey, _BlockSection);
                }
                return _BlockSection;
            }

            internal static void UpdateInherit(int EntityID, bool? Inherit)
            {
                BlockSection en = BlockSection.Query("where EntityID=@0", EntityID).FirstOrDefault();
                if (en != null)
                {
                    en.Inherit = Inherit;
                    en.Update();
                    CacheFactory.Clear(CacheFactory.Keys.SectionPermission);
                }
            }

            internal static void ClearAllPermissions(int EntityID)
            {
                foreach (BlockSectionPermission permission in GetPermissionsByEntityID(EntityID))
                {
                    permission.Delete();
                }
                CacheFactory.Clear(CacheFactory.Keys.SectionPermission);
            }
            internal static void UpdatePermissions(List<BlockSectionPermission> Permissions)
            {
                foreach (BlockSectionPermission permission in Permissions)
                {
                    permission.Insert();
                }
                CacheFactory.Clear(CacheFactory.Keys.SectionPermission);
            }

            internal static void DeletePermissions(List<int> EntityIDs)
            {
                BlockSection.Delete("Where EntityID in (" + string.Join(",", EntityIDs) + ")");
                BlockSectionPermission.Delete("Where EntityID in (" + string.Join(",", EntityIDs) + ")");
                CacheFactory.Clear(CacheFactory.Keys.SectionPermission);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class CustomPermissionFactory
        {
            internal static List<CustomPermission> GetPermissionsByEntityID(int EntityID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomPermission + "GetPermissionsByEntityID", EntityID);
                List<CustomPermission> _CustomPerm = CacheFactory.Get(CacheKey) as List<CustomPermission>;
                if (_CustomPerm == null)
                {
                    _CustomPerm = CustomPermission.Query("where EntityID=@0", EntityID).ToList();
                    CacheFactory.Set(CacheKey, _CustomPerm);
                }
                return _CustomPerm;
            }
            internal static List<CustomPermissionInfo> GetPermissionByCode(string Code)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomPermission + "GetPermissionByCode", Code);
                List<CustomPermissionInfo> _CustomPermInfo = CacheFactory.Get(CacheKey) as List<CustomPermissionInfo>;
                if (_CustomPermInfo == null)
                {
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        _CustomPermInfo = db.Query<CustomPermissionInfo>("SELECT p.* FROM " + Data.Scripts.CommonScript.DnnTablePrefix + "Permission AS p WHERE p.PermissionCode = @0", Code).ToList();
                    }
                    CacheFactory.Set(CacheKey, _CustomPermInfo);
                }
                return _CustomPermInfo;
            }
            internal static int AddCustomPermissionEntity(string Entity, bool? Inherit)
            {
                CustomPermissionEntity customPermissionEntity = new CustomPermissionEntity();
                customPermissionEntity.EntityID = 0;
                customPermissionEntity.Entity = Entity;
                customPermissionEntity.Inherit = Inherit;
                customPermissionEntity.Insert();
                CacheFactory.Clear(CacheFactory.Keys.CustomPermission);
                return customPermissionEntity.EntityID;
            }

            internal static void Delete(int EntityID)
            {
                CustomPermissionEntity.Delete("where EntityID=@0", EntityID);
                ClearAllPermissions(EntityID);
            }

            internal static CustomPermissionEntity GetCustomPermissionEntity(int EntityID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.CustomPermission + "GetCustomPermissionEntity", EntityID);
                CustomPermissionEntity _CustomPerm = CacheFactory.Get(CacheKey) as CustomPermissionEntity;
                if (_CustomPerm == null)
                {
                    _CustomPerm = CustomPermissionEntity.Query("where EntityID=@0", EntityID).FirstOrDefault();
                    CacheFactory.Set(CacheKey, _CustomPerm);
                }
                return _CustomPerm;
            }

            internal static void UpdateInherit(int EntityID, bool? Inherit)
            {
                CustomPermissionEntity en = CustomPermissionEntity.Query("where EntityID=@0", EntityID).FirstOrDefault();
                if (en != null)
                {
                    en.Inherit = Inherit;
                    en.Update();
                    CacheFactory.Clear(CacheFactory.Keys.CustomPermission);
                }
            }

            internal static void ClearAllPermissions(int EntityID)
            {
                foreach (CustomPermission permission in GetPermissionsByEntityID(EntityID))
                {
                    permission.Delete();
                }
                CacheFactory.Clear(CacheFactory.Keys.CustomPermission);
            }
            internal static void UpdatePermissions(List<CustomPermission> Permissions)
            {
                foreach (CustomPermission permission in Permissions)
                {
                    permission.Insert();
                }
                CacheFactory.Clear(CacheFactory.Keys.CustomPermission);
            }
        }
    }
}
namespace Vanjaro.Common.Utilities
{
    public class DataCache
    {
        public static bool CacheExists(string key)
        {
            return DotNetNuke.Common.Utilities.DataCache.GetCache(key) != null;
        }

        public static void SetCache<T>(T toSet, string key)
        {
            DotNetNuke.Common.Utilities.DataCache.SetCache(key, toSet);
        }

        public static T GetItemFromCache<T>(string key)
        {
            return (T)DotNetNuke.Common.Utilities.DataCache.GetCache(key);
        }

        public static void ClearCache(string key)
        {
            DotNetNuke.Common.Utilities.DataCache.ClearCache(key);
        }

    }
}
using DotNetNuke.Common.Utilities;
using System;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class CacheFactory
        {

            public class Keys
            {
                internal static TimeSpan Cache_Time_Low = new TimeSpan(0, 2, 0);
                internal static TimeSpan Cache_Time_Medium = new TimeSpan(0, 5, 0);
                internal static TimeSpan Cache_Time_Heavy = new TimeSpan(0, 10, 0);
                internal const string Prefix = "Vanjaro-Core-";
                internal const string Page = Prefix + "Page";
                public const string CustomBlock = Prefix + "CustomBlock";
                internal const string Settings = Prefix + "Settings";
                internal const string Extensions = Prefix + "Extensions";
                internal const string Workflow = Prefix + "Workflow";
                public const string NotificationTask = Prefix + "NotificationTask";
                internal const string GlobalConfig = Prefix + "GlobalConfig";
                internal const string Block_Extension = Prefix + "BlockExtension";
                public const string ThemeCategory = Prefix + "ThemeCategory";
                public const string ThemeManager = Prefix + "ThemeManager";
                public const string Theme = Prefix + "Theme";
                internal const string OAuthClients = Prefix + "OAuthClients";
                internal const string IOAuthClient_Extension = Prefix + "IOAuthClient_Extension";                
            }

            public static string GetCacheKey(object extensions)
            {
                throw new NotImplementedException();
            }

            public static void Clear()
            {
                DataCache.ClearCache(Keys.Prefix);
            }

            public static void Clear(string Prefix)
            {
                DataCache.ClearCache(Prefix);
            }

            public static void Set(string Key, object Object)
            {
                Set(Key, Object, Keys.Cache_Time_Heavy, null);
            }

            public static void Set(string Key, object Object, params object[] AddtionalKeys)
            {
                Set(Key, Object, Keys.Cache_Time_Heavy, AddtionalKeys);
            }

            internal static void Set(string Key, object Object, TimeSpan Time, params object[] AddtionalKeys)
            {
                if (string.IsNullOrEmpty(Key))
                {
                    throw new ArgumentNullException("Key");
                }

                if (!Key.StartsWith(Keys.Prefix))
                {
                    throw new Exception("Key must start with Prefix");
                }

                if (AddtionalKeys != null)
                {
                    foreach (object obj in AddtionalKeys)
                    {
                        if (obj != null)
                        {
                            Key = Key + "-" + obj.ToString();
                        }
                    }
                }
                DataCache.SetCache(Key, Object, Time);
            }

            public static dynamic Get(string key)
            {
                return DataCache.GetCache(key);
            }

            public static string GetCacheKey(string Key, params object[] AddtionalKeys)
            {
                if (!Key.StartsWith(Keys.Prefix))
                {
                    throw new Exception("Key must start with Prefix");
                }

                if (AddtionalKeys != null)
                {
                    foreach (object obj in AddtionalKeys)
                    {
                        if (obj != null)
                        {
                            Key = Key + "-" + obj.ToString();
                        }
                    }
                }
                return Key;
            }
        }
    }
}
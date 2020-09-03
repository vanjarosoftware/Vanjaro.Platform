using DotNetNuke.Common.Utilities;
using System;

namespace Vanjaro.UXManager.Library
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
                public const string Prefix = "Vanjaro-UXManager-";
                internal const string Menu_Extension = Prefix + "MenuExtension";
                internal const string NotificationTask = Prefix + "NotificationTask";
                internal const string Shortcut = Prefix + "Shortcut";
                internal const string Toolbar_Extension = Prefix + "ToolbarExtension";
                internal const string DesktopModules = Prefix + "DesktopModules";
                internal const string Instance = Prefix + "Instance";
                internal const string App_Extension = Prefix + "AppExtension";
                internal const string ImageProvider = Prefix + "ImageProvider";
                internal const string VideoProvider = Prefix + "VideoProvider";
                internal const string Images = Prefix + "Images";
                internal const string Videos = Prefix + "Videos";
                public const string PageLayout = Prefix + "PageLayout";
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

            internal static void Set(string Key, object Object, params object[] AddtionalKeys)
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
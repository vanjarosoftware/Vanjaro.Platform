using DotNetNuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.URL.Common
{
    public class Cache
    {
        public class Keys
        {
            public const string Prefix = "vjURL-";
            public const string Slug = Prefix + "Slug-";
            public const string SlugIndex = Prefix + "SlugIndex-";
            public const string ModuleTabIndex = Prefix + "ModuleTabIndex-";
            public const string URLEntity = Prefix + "URLEntity-";
            public const string FriendlyUrls = Prefix + "FriendlyUrls-";
            public static TimeSpan Cache_Time_Low = new TimeSpan(0, 2, 0);
            public static TimeSpan Cache_Time_Medium = new TimeSpan(0, 5, 0);
            public static TimeSpan Cache_Time_Heavy = new TimeSpan(0, 10, 0);
        }

        public static void Clear()
        {
            DataCache.ClearCache(Keys.Prefix);
        }
    }
}
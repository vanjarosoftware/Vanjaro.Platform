using DotNetNuke.Common.Utilities;
using Vanjaro.Common.Components;

namespace Vanjaro.Common.Factories
{
    internal class CacheFactory
    {
        internal static void ClearCache()
        {
            DataCache.ClearCache(Constants.CachPrefix);
        }
    }
}
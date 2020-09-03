using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;

namespace Vanjaro.UXManager.Extensions.Apps.Link.Entities
{
    public static class FileSetting
    {
        public static long FileSize => Config.GetMaxUploadSize() / (1024 * 1024);
        public static string FileType => Host.AllowedExtensionWhitelist.ToStorageString();
    }
}
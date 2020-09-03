using System.Linq;
using System.Web;

namespace Vanjaro.Common.Utilities
{
    public class Localization
    {
        public const string LocalMissingPrefix = "[L] ";
        public const string SharedMissingPrefix = "[LS] ";

        public static string GetLocal(string Key, string Suffix, string TemplatePath, string Identifier, bool ShowMissingKeys)
        {
            return Get(Key, Suffix, GetLocalResourceFile(TemplatePath, Identifier), ShowMissingKeys, LocalMissingPrefix);
        }

        public static string GetShared(string Key, string Suffix, string TemplatePath, string Identifier, bool ShowMissingKeys)
        {
            return Get(Key, Suffix, GetLocalResourceFile(TemplatePath, null), ShowMissingKeys, SharedMissingPrefix);
        }

        public static string Get(string Key, string Suffix, string ResourceFile, bool ShowMissingKeys, string MissingPrefix)
        {
            try
            {
                if (ResourceFile.IndexOf("DesktopModules") > -1)
                {
                    ResourceFile = VirtualPathUtility.ToAbsolute("~/" + ResourceFile.Substring(ResourceFile.IndexOf("DesktopModules")));
                }
                else if (ResourceFile.IndexOf("Portals") > -1)
                {
                    ResourceFile = VirtualPathUtility.ToAbsolute("~/" + ResourceFile.Substring(ResourceFile.IndexOf("Portals")));
                }
            }
            catch { }
            string value = DotNetNuke.Services.Localization.Localization.GetString(Key + "." + Suffix, ResourceFile);
            if (ShowMissingKeys)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return MissingPrefix + Key + "." + Suffix;
                }
                else
                {
                    return MissingPrefix + value;
                }
            }

            return value;
        }
        public static string GetSharedResourceFile(string TemplatePath)
        {
            return GetLocalResourceFile(TemplatePath, null);
        }
        public static string GetLocalResourceFile(string TemplatePath, string Identifier)
        {
            if (!string.IsNullOrEmpty(Identifier) && Identifier.Contains('_'))
            {
                if (Identifier.StartsWith("common_"))
                {
                    TemplatePath = "DesktopModules/Vanjaro/Common/Engines/UIEngine/AngularBootstrap/Views/";
                }

                string ResourceFile = Identifier.Split('_').Last();
                string ResourcePath = Identifier.Replace("_", "/").TrimEnd(ResourceFile.ToCharArray());

                return TemplatePath + ResourcePath + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + ResourceFile + ".resx";
            }

            return TemplatePath + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/Shared.resx";
        }
    }
}
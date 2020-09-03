using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Entities
{
    public class FileSetting
    {
        public static long FileSize
        {
            get
            {
                List<Setting> settings = Core.Managers.SettingManager.GetSettings(PortalSettings.Current.PortalId, 0, "security_settings");
                long MaxUploadSize = Config.GetMaxUploadSize() / (1024 * 1024);
                if (settings != null && settings.Count > 0 && settings.Where(s => s.Name == "Picture_MaxUploadSize").FirstOrDefault() != null)
                {
                    return MaxUploadSize = Convert.ToInt64(Convert.ToDouble(settings.Where(s => s.Name == "Picture_MaxUploadSize").FirstOrDefault().Value) * 1024 * 1024);
                }
                else
                {
                    return MaxUploadSize;
                }
            }
        }
        public static string FileType
        {
            get
            {
                List<Setting> settings = Core.Managers.SettingManager.GetSettings(PortalSettings.Current.PortalId, 0, "security_settings");
                string AllowableFileExtensions = "jpg,jpeg,gif,png,svg,webp";
                if (settings != null && settings.Count > 0 && settings.Where(s => s.Name == "Picture_AllowableFileExtensions").FirstOrDefault() != null)
                {
                    return AllowableFileExtensions = settings.Where(s => s.Name == "Picture_AllowableFileExtensions").FirstOrDefault().Value.ToString();
                }
                else
                {
                    return AllowableFileExtensions;
                }
            }
        }
    }
}
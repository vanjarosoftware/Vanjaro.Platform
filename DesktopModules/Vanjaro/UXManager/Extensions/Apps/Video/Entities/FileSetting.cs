using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.UXManager.Extensions.Apps.Video.Entities
{
    public class FileSetting
    {
        public static long FileSize
        {
            get
            {
                List<Setting> settings = Managers.SettingManager.GetSettings(PortalSettings.Current.PortalId, 0, "security_settings");
                long MaxUploadSize = Config.GetMaxUploadSize() / (1024 * 1024);
                if (settings != null && settings.Count > 0 && settings.Where(s => s.Name == "Video_MaxUploadSize").FirstOrDefault() != null)
                {
                    return MaxUploadSize = Convert.ToInt64(Convert.ToDouble(settings.Where(s => s.Name == "Video_MaxUploadSize").FirstOrDefault().Value));
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
                List<Setting> settings = Managers.SettingManager.GetSettings(PortalSettings.Current.PortalId, 0, "security_settings");
                string AllowableFileExtensions = "webm,mp4";
                if (settings != null && settings.Count > 0 && settings.Where(s => s.Name == "Video_AllowableFileExtensions").FirstOrDefault() != null)
                {
                    return AllowableFileExtensions = settings.Where(s => s.Name == "Video_AllowableFileExtensions").FirstOrDefault().Value.ToString();
                }
                else
                {
                    return AllowableFileExtensions;
                }
            }
        }
    }
}
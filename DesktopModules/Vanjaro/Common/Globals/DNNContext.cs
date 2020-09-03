using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Vanjaro.Common.Globals
{
    public class DNNContext
    {
        public DNNContext(PortalSettings PortalSettings, UserInfo UserInfo, ModuleInfo ModuleInfo)
        {
            this.PortalSettings = PortalSettings;
            this.UserInfo = UserInfo;
            this.ModuleInfo = ModuleInfo;
        }

        public PortalSettings PortalSettings { get; set; }
        public UserInfo UserInfo { get; set; }
        public ModuleInfo ModuleInfo { get; set; }
    }
}
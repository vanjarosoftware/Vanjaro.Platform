using DotNetNuke.Entities.Portals;

namespace Vanjaro.UXManager.Extensions.Block.Logo.Entities
{
    public class Logo
    {
        public string Path { get; set; }

        public string NavigateURL { get; set; }

        public string SiteName => PortalSettings.Current.PortalName;
    }
}
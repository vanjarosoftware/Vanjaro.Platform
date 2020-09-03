using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Extensions.Block.SearchInput.Entities
{
    public class SearchInput
    {
        public string Url { get; set; }
        public string SearchResultUrl => ServiceProvider.NavigationManager.NavigateURL(PortalSettings.Current.SearchTabId);
    }
}
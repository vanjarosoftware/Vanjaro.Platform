using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

namespace Vanjaro.UXManager.Extensions.Block.Menu.Entities
{
    public interface ILocalisation
    {
        bool HaveApi();
        TabInfo LocaliseTab(TabInfo tab, int portalId);
        DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes);
    }
}
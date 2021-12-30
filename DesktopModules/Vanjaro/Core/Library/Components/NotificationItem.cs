
using Vanjaro.Core.Entities.Menu;

namespace Vanjaro.Core.Components
{
    public class NotificationItem
    {
        public NotificationItem Hierarchy { get; set; }
        public string NotificationName { get; set; }
        public int NotificationCount { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int? Width { get; set; }
        public MenuAction Event { get; set; }
    }
}
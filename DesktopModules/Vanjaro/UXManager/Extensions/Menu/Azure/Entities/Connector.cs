using DotNetNuke.Services.Connections;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Menu.Azure.Entities
{
    public class Connector
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ConnectorCategories Type { get; set; }
        public string DisplayName { get; set; }
        public bool Connected { get; set; }
        public string IconUrl { get; set; }
        public string PluginFolder { get; set; }
        public IDictionary<string, string> Configurations { get; set; }
        public bool SupportsMultiple { get; set; }
    }
}
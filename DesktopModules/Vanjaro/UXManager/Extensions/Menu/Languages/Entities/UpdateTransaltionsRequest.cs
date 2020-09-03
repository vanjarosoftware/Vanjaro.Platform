using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Entities
{
    public class UpdateTransaltionsRequest
    {
        public UpdateTransaltionsRequest()
        {
            Entries = new List<LocalizationEntry>();
        }
        public string Mode { get; set; }
        public string ResourceFile { get; set; }
        public List<LocalizationEntry> Entries { get; set; }
    }
}
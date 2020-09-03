using System.Collections.Generic;

namespace Vanjaro.Core.Entities
{
    public class VideoInfo
    {
        public string ID { get; set; }
        public string Url;
        public string Thumbnail;
        public string Title;
        public string Duration;
        public Dictionary<string, object> AdditionalData;
    }
}
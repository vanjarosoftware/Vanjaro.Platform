namespace Vanjaro.Common.Data.Entities
{
    public class Attachment
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public byte[] BLOB { get; set; }
        public string Size { get; set; }
    }
}
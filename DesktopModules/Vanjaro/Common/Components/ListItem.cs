namespace Vanjaro.Common.Components
{
    public class ListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class LinkItem
    {
        public string Anchor { get; set; }

        public string Href { get; set; }

        public string Text { get; set; }

        public LinkItem()
        {
        }
    }
}
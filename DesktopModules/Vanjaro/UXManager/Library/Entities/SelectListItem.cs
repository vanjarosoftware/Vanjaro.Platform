namespace Vanjaro.UXManager.Library.Entities
{
    public class SelectListItem
    {
        public bool Selected { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class PageItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public string Url { get; set; }
    }
}
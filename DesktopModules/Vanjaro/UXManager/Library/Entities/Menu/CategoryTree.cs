namespace Vanjaro.UXManager.Library.Entities.Menu
{
    public class CategoryTree
    {
        public int CID { get; set; }
        public string Name { get; set; }
        public int? ParentID { get; set; }
        public int Level { get; set; }
        public string GUID { get; set; }
        public int Width { get; set; }
        public string Icon { get; set; }
        public int? ViewOrder { get; set; }
        public MenuAction MenuAction { get; set; }
        public bool AboveBreakLine { get; set; }
        public bool BelowBreakLine { get; set; }
        public string URL { get; set; }
    }
}
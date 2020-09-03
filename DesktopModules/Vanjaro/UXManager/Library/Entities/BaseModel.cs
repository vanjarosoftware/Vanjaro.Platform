namespace Vanjaro.UXManager.Library.Entities
{
    public class BaseModel
    {
        public string AboutUrl { get; set; }
        public string LoadingImage { get; set; }
        public string Logo { get; set; }
        public string MenuMarkUp { get; set; }
        public string NotificationMarkUp { get; set; }
        public string ToolbarMarkUp { get; set; }
        public string ShortcutMarkUp { get; set; }
        public bool HasTabEditPermission { get; set; }
        public bool HasShortcut { get; set; }
        public int NotificationCount { get; set; }
        public object LanguageMarkUp { get; internal set; }
        public bool ShowUXManagerToolbar { get;  set; }
        public bool ShowUXManager { get; set; }
    }
}
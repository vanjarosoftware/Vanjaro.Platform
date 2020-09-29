using System.Web;
using Vanjaro.Core.Entities.Interface;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder
{
    public class ThemeEditorImpl : IThemeEditor
    {
        public string Guid => "921af5fa-3030-4bae-aaec-0e353b9489ff";

        public string Name => "Site";

        public int ViewOrder => 0;

        public string JsonPath => HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + Core.Managers.ThemeManager.GetCurrent().ThemeName + "/" + "theme.editor.json");
    }

    public class ThemeEditorCustomImpl : IThemeEditor
    {
        public string Guid => "be134fd2-3a3d-4460-8ee9-2953722a5ab2";

        public string Name => "Custom";

        public int ViewOrder => 1000;

        public string JsonPath
        {
            get
            {
                return HttpContext.Current.Server.MapPath("~/Portals/{{PortalID}}/vThemes/{{ThemeName}}/" + "theme.editor.custom.json");
            }
        }
    }
}
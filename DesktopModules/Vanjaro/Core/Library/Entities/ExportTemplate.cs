using System;
using System.Collections.Generic;

namespace Vanjaro.Core.Entities
{
    public class ExportTemplate
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ThemeGuid { get; set; }
        public string ThemeName { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<Layout> Templates { get; set; }
        public string LogoFile { get; set; }
        public string FavIcon { get; set; }
        public string SocialSharingLogo { get; set; }
        public string HomeScreenIcon { get; set; }
    }
    public enum TemplateType
    {
        Site,
        Page,
        Block
    }
}
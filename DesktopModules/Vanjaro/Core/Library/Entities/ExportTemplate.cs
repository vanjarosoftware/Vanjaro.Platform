using System;
using System.Collections.Generic;

namespace Vanjaro.Core.Entities
{
    public class ExportTemplate
    {
        public string Guid { get; set; }
        public string Type { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<Layout> Templates { get; set; }
    }
    public enum TemplateType
    {
        SiteTemplate,
        PageTemplate
    }
}
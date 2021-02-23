using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Data.Entities
{
    public partial class CustomBlock
    {
        public string ScreenshotPath { get; set; }
        public bool IsGlobal { get; set; }
    }
    public partial class GlobalBlock
    {
        public string ScreenshotPath { get; set; }
        public bool IsGlobal { get; set; }
    }
}
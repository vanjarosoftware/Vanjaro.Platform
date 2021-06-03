using System;
using Vanjaro.Core.Components.Interfaces;

namespace Vanjaro.Core.Themes.Basic
{
    public class Theme : ITheme
    {
        public Guid GUID => Guid.Parse("49A70BA1-206B-471F-800A-679799FF09DF");
        public string Name => "Basic";
        public string DesignScript => "";
        public string ClientScript => "";
        public string Assembly => "";
    }
}
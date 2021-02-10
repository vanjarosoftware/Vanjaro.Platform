using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Entities.Interface
{
    public interface IThemeEditor
    {
        string Guid { get; }
        string Name { get; }
        bool IsVisible { get; }
        int ViewOrder { get; }
        string JsonPath { get; }
    }
}
using System;
using System.Collections.Generic;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Library.Entities.Interface
{
    public interface IMenuItem : IExtension
    {
        List<MenuItem> Items { get; }
        bool Visibility { get; }
        Guid SettingGuid { get; }
        int? Width { get; }
        string SearchKeywords { get; }
        MenuAction Event { get; }
    }
}
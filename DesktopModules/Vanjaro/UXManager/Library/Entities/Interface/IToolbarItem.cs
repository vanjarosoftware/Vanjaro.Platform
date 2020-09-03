using System;
using System.Collections.Generic;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Library.Entities.Interface
{
    public interface IToolbarItem : IExtension
    {
        ToolbarItem Item { get; }
        string Icon { get; }
        bool Visibility { get; }
        int SortOrder { get; }
        Guid SettingGuid { get; }
        int? Width { get; }
        Dictionary<MenuAction, dynamic> ToolbarAction { get; }
    }
}
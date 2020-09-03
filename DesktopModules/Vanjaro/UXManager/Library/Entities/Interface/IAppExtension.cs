using System;
using Vanjaro.UXManager.Library.Entities.Menu;

namespace Vanjaro.UXManager.Library.Entities.Interface
{
    public interface IAppExtension : IExtension
    {
        AppExtension Item { get; }
        string Icon { get; }
        bool Visibility { get; }
        Guid SettingGuid { get; }
        int Width { get; }
    }
}
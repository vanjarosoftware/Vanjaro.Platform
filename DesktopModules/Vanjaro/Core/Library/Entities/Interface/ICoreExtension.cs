using System;

namespace Vanjaro.Core.Entities.Interface
{
    public interface ICoreExtension : IExtension
    {
        bool Visibility { get; }
        Guid SettingGuid { get; }
        int Width { get; }
    }
}
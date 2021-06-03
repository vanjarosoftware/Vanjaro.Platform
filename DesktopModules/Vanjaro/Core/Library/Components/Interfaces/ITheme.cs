using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Components.Interfaces
{
    public interface ITheme
    {
        Guid GUID { get; }
        string Name { get; }
        string DesignScript { get; }
        string ClientScript { get; }
        string Assembly { get; }
    }
}
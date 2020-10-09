using System;
using System.Collections.Generic;
using Vanjaro.Core.Entities.Menu;

namespace Vanjaro.Core.Entities.Interface
{
    public interface IBlock : IThemeTemplate
    {
        string Category { get; }
        string Name { get; }
        bool Visible { get; }
        string DisplayName { get; }
        string Icon { get; }

        Dictionary<string, string> Attributes { get; }

    }

    public interface IThemeTemplate
    {
        Guid Guid { get; }

        ThemeTemplateResponse Render(Dictionary<string, string> Attributes);
    }
}

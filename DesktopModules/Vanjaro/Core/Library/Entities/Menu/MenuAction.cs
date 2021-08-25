using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Entities.Menu
{
    [DefaultValue(Inline)]
    public enum MenuAction
    {
        Inline = 0,
        RightOverlay = 1,
        CenterOverlay = 2,

        Default = 3,
        OpenInNewWindow = 4,
        onClick = 5,
        FullScreen = 6
    }
}
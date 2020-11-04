using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vanjaro.URL.Common
{
    public interface IPropertyChanged
    {
        bool HasChanged { get; set; }
        bool HasDeleted { get; set; }
    }
}

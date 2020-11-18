using Vanjaro.URL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vanjaro.URL.Data.Entities
{
    public partial class URLEntity : IPropertyChanged
    {
        public bool HasChanged
        {
            get;
            set;
        }

        public bool HasDeleted
        {
            get;
            set;
        }
    }
}

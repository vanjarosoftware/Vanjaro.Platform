using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.URL.Data.Scripts
{
    public static class CommonScript
    {
        private static string _TablePrefix = null;

        public static string TablePrefix
        {
            get
            {
                if (_TablePrefix == null)
                    _TablePrefix = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                return _TablePrefix;
            }
        }
    }
}
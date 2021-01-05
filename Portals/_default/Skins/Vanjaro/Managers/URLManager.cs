using System;
using System.Web;
using System.Linq;
using System.Text;

namespace Vanjaro.Skin
{
    public static partial class Managers
    {
        public class URLManager
        {
            public static string RemoveQueryStringByKey(string url, string key)
            {
                var uri = new Uri(url);
                var newQueryString = HttpUtility.ParseQueryString(uri.Query);
                newQueryString.Remove(key);
                string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);
                return newQueryString.Count > 0
                     ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                     : pagePathWithoutQueryString;
            }
        }
    }
}
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Vanjaro.Common.Utilities
{
    public class Url
    {
        public static string ResolveUrl(string originalUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(originalUrl) && '~' == originalUrl[0])
                {
                    int index = originalUrl.IndexOf('?');
                    string queryString = (-1 == index) ? null : originalUrl.Substring(index);
                    if (-1 != index)
                    {
                        originalUrl = originalUrl.Substring(0, index);
                    }

                    originalUrl = VirtualPathUtility.ToAbsolute(originalUrl) + queryString;
                }
            }

            catch { }

            return originalUrl;
        }
        public static string GetAbsURL(string relativeUrl)
        {
            if (!IsAbsoluteUrl(relativeUrl))
            {
                Uri url = new Uri(relativeUrl);
                string path = string.Format("{0}{1}{2}{3}", url.Scheme, Uri.SchemeDelimiter, url.Authority, url.AbsolutePath);
                return path;
            }
            else
            {
                return relativeUrl;
            }
        }

        //public static bool IsAbsoluteUrl(string url)
        //{
        //    Uri result;
        //    return Uri.TryCreate(url, UriKind.Absolute, out result);
        //}

        public static string GetAbsURL(HttpContext context, string relativeUrl)
        {
            if (!IsAbsoluteUrl(relativeUrl))
            {
                if (context.Request.IsSecureConnection)
                {
                    return string.Format("https://{0}{1}", context.Request.Url.Authority, context.Server.UrlDecode(relativeUrl));
                }
                else
                {
                    return string.Format("http://{0}{1}", context.Request.Url.Authority, context.Server.UrlDecode(relativeUrl));
                }
            }
            else
            {
                return relativeUrl;
            }
        }
        public static bool IsAbsoluteUrl(string url)
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
        public string GetAbsURL(Page page, string relativeUrl)
        {
            if (!IsAbsoluteUrl(relativeUrl))
            {
                if (page.Request.IsSecureConnection)
                {
                    return string.Format("https://{0}{1}", page.Request.Url.Authority, relativeUrl);
                }
                else
                {
                    return string.Format("http://{0}{1}", page.Request.Url.Authority, relativeUrl);
                }
            }
            else
            {
                return relativeUrl;
            }
        }

        public static string Sanitize(string url, bool ReplaceSpace)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }

            // remove entities
            //url = Regex.Replace(url, @"&\w+;", "");

            // remove anything that is not letters, numbers, dash, or space
            //url = Regex.Replace(url, @"[^A-Za-z0-9\-\s]", "");

            // remove any leading or trailing spaces left over
            url = url.Trim();

            // replace spaces with single dash
            url = Regex.Replace(url, @"\s+", "-");

            // if we end up with multiple dashes, collapse to single dash            
            url = Regex.Replace(url, @"\-{2,}", "-");

            // make it all lower case and remove all trailing dot
            url = url.TrimEnd('.').ToLower();

            // remove trailing dash, if there is one
            if (url.EndsWith("-"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            if (!ReplaceSpace)
            {
                return url.Replace("-", " ");
            }

            return url;
        }
        public static string Sanitize(string url)
        {
            return Sanitize(url, true);
        }
    }
}

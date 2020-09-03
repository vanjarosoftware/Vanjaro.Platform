using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class CookieManager
        {
            public static void AddValue(string Key, string Value, DateTime Expires, bool HttpOnly = false)
            {
                if (HttpContext.Current != null && !string.IsNullOrEmpty(Key))
                {
                    HttpCookie myCookie = HttpContext.Current.Request.Cookies[Key];
                    if (myCookie == null)
                    {
                        myCookie = new HttpCookie(Key);
                    }

                    myCookie.Value = Value;
                    myCookie.Expires = Expires;
                    myCookie.HttpOnly = HttpOnly;
                    HttpContext.Current.Response.Cookies.Add(myCookie);
                    HttpContext.Current.Request.Cookies.Add(myCookie);
                }
            }

            public static void AddValues(string Key, NameValueCollection Values, DateTime Expires, bool HttpOnly = false)
            {
                if (HttpContext.Current != null && !string.IsNullOrEmpty(Key))
                {
                    HttpCookie myCookie = HttpContext.Current.Request.Cookies[Key];
                    if (myCookie == null)
                    {
                        myCookie = new HttpCookie(Key);
                    }

                    myCookie.Values.Clear();
                    myCookie.Values.Add(Values);
                    myCookie.Expires = Expires;
                    myCookie.HttpOnly = HttpOnly;
                    HttpContext.Current.Response.Cookies.Add(myCookie);
                    HttpContext.Current.Request.Cookies.Add(myCookie);
                }
            }

            public static string GetValue(string Key)
            {
                string result = string.Empty;
                if (HttpContext.Current != null && !string.IsNullOrEmpty(Key) && HttpContext.Current.Request.Cookies[Key] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies[Key].Value))
                {
                    result = HttpContext.Current.Request.Cookies[Key].Value;
                }

                return result;
            }

            public static HttpCookie Get(string Key)
            {
                HttpCookie result = null;
                if (HttpContext.Current != null && !string.IsNullOrEmpty(Key) && HttpContext.Current.Request.Cookies[Key] != null && HttpContext.Current.Request.Cookies[Key].Values.Count > 0)
                {
                    result = HttpContext.Current.Request.Cookies[Key];
                }

                return result;
            }

            public static void Clear(string Key)
            {
                if (HttpContext.Current != null && HttpContext.Current.Request.Cookies[Key] != null)
                {
                    HttpCookie myCookie = HttpContext.Current.Request.Cookies[Key];
                    if (myCookie == null)
                    {
                        myCookie = new HttpCookie(Key);
                    }

                    myCookie.Expires = DateTime.Now.AddDays(-1d);
                    HttpContext.Current.Response.Cookies.Add(myCookie);
                }
            }
        }
    }
}
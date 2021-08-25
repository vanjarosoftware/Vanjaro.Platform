using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.UXManager.Library.Factories;

namespace Vanjaro.UXManager.Library
{
    public static partial class Managers
    {
        public class ShortcutManager
        {
            internal static List<IShortcut> GetShortcut()
            {
                return ShortcutFactory.GetShortcut();
            }

            #region IShortcut
            internal static string RenderShortcut()
            {
                List<IShortcut> Shortcuts = GetShortcut();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<ul class=\"Shortcuts\">");

                if (Shortcuts.Count > 0)
                {
                    foreach (IShortcut sc in Shortcuts)
                    {
                        if (sc.Shortcut.Visibility)
                        {
                            string url = null;
                            string icon = string.Empty;
                            //Add Icon class if Icon is available
                            if (!string.IsNullOrEmpty(sc.Shortcut.Icon))
                            {
                                icon = "<em class=\"" + sc.Shortcut.Icon + "\"></em>";
                            }

                            //Create Extension URL if GUID not null Or found
                            if (!string.IsNullOrEmpty(sc.Shortcut.URL))
                            {
                                //url = ServiceProvider.NavigationManager.NavigateURL().ToLower().Replace(PortalSettings.Current.DefaultLanguage.ToLower(), PortalSettings.Current.CultureCode.ToLower()).TrimEnd('/') + "?mid=0&icp=true&guid=" + sc.Shortcut.URL;

                                #region Append Query in Iframe url because migrated time SkinSrc querystring required 
                                string AppendURL = string.Empty;
                                foreach (string q in HttpContext.Current.Request.QueryString.AllKeys)
                                {
                                    AppendURL = AppendURL + q + "=" + HttpContext.Current.Request.QueryString[q] + "&";
                                }
                                #endregion
                                url = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?" + AppendURL + "mid=0&icp=true&guid=" + sc.Shortcut.URL;
                            }

                            if (!string.IsNullOrEmpty(sc.Shortcut.URL))
                            {
                                int Width = sc.Shortcut.Width ?? 800;
                                if (sc.Shortcut.Attributes != null && sc.Shortcut.Attributes.Count > 0)
                                {
                                    sb.Append(@"<li><a href='#'");
                                    foreach (KeyValuePair<string, string> attr in sc.Shortcut.Attributes)
                                    {
                                        sb.Append(string.Format("{0}='{1}' ", attr.Key, attr.Value));
                                    }

                                    sb.Append(string.Format(@"data-width='{1}'>{0} {2}</a>", icon, Width, sc.Shortcut.Text));
                                }
                                else
                                {
                                    if (sc.Shortcut.Action == MenuAction.CenterOverlay)
                                    {
                                        string ClickFunction = "OpenPopUp(event, " + Width + ",\"center\",\"" + sc.Shortcut.Title + "\", \"" + ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + sc.Shortcut.URL + "\")";
                                        sb.Append(string.Format(@"<li><a href='#' onclick='{0}' data-width='{2}'>{1} {3}</a>", ClickFunction, icon, Width, sc.Shortcut.Text));
                                    }
                                    else
                                    {
                                        string ClickFunction = "OpenPopUp(event, " + Width + ",\"right\",\"" + sc.Shortcut.Title + "\", \"" + url + "\")";
                                        sb.Append(string.Format(@"<li><a href='#' onclick='{0}' data-width='{2}'>{1} {3}</a>", ClickFunction, icon, Width, sc.Shortcut.Text));
                                    }
                                }

                            }
                            sb.Append("</li>");

                            if (sc.Shortcut.Breakline)
                            {
                                sb.Append("<hr />");
                            }
                        }
                    }
                }
                sb.Append("</ul>");
                return sb.ToString();
            }
            #endregion
        }
    }
}

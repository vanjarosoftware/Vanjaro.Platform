using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public class MenuManager
        {
            #region Sync Menu Extensions
            public static List<IMenuItem> GetExtentions(bool CheckVisibilityPermission = true)
            {
                return MenuFactory.GetExtentions(CheckVisibilityPermission).ToList();
            }

            public static string RenderMenu(List<CategoryTree> tree, string SearchKeyword)
            {
                StringBuilder sb = new StringBuilder();
                tree.RemoveAll(x => string.IsNullOrEmpty(x.Name));
                GenerateUL(tree.Where(x => x.ParentID == null).ToList(), tree, sb, SearchKeyword);

                //Replace Multiple breakline <br > to single breakline.
                return Regex.Replace(sb.ToString(), @"(<br ?/?>)+", "<br />");
            }

            private static string GenerateUL(List<CategoryTree> menu, List<CategoryTree> table, StringBuilder sb, string SearchKeyword)
            {
                sb.AppendLine("<ul class=\"MenuItem\">");
                if (string.IsNullOrEmpty(SearchKeyword))
                {
                    sb.AppendLine("<a class='backbutton'><span class=\"back-icon\"><em class=\"fas fa-chevron-left\"></em></span><span class=\"back-title\"></span></a>");
                }

                if (menu.Count > 0)
                {
                    foreach (CategoryTree dr in menu)
                    {
                        string url = null;
                        string icon = string.Empty;

                        //Create Extension URL if GUID not null Or found
                        if (!string.IsNullOrEmpty(dr.GUID))
                        {
                            if (string.IsNullOrEmpty(dr.URL) && !string.IsNullOrEmpty(dr.ModuleDefinition))
                                url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/DesktopModules/Vanjaro/UXManager/Library/ModuleControl.aspx?guid=" + dr.GUID + "&moduledefinition=" + dr.ModuleDefinition + "&modulecontrol=" + dr.ModuleControl;
                            else
                                url = ServiceProvider.NavigationManager.NavigateURL().ToLower().Replace(PortalSettings.Current.DefaultLanguage.ToLower(), PortalSettings.Current.CultureCode.ToLower()).TrimEnd('/') + GetURL() + "mid=0&icp=true&guid=" + dr.GUID;
                        }
                        if (!string.IsNullOrEmpty(dr.URL))
                        {
                            url = dr.URL;
                        }
                        //url = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + dr.GUID;

                        //Add Icon class if Icon is available
                        if (!string.IsNullOrEmpty(dr.Icon))
                        {
                            icon = "<em class=\"" + dr.Icon + "\"></em>";
                        }




                        if (dr.AboveBreakLine)
                        {
                            sb.Append("<br />");
                        }

                        if (!string.IsNullOrEmpty(dr.GUID) && !(dr.MenuAction == MenuAction.Inline))
                        {
                            if (dr.MenuAction != MenuAction.RightOverlay && string.IsNullOrEmpty(dr.URL) && string.IsNullOrEmpty(dr.ModuleDefinition))
                            {
                                url = ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + dr.GUID;
                            }

                            string ClickFunction = dr.MenuAction == MenuAction.FullScreen ? ("OpenPopUp(event, \"" + dr.Width + "%" + "\",\"center\",\"" + dr.Name + "\", \"" + url + "\")") : dr.MenuAction == MenuAction.RightOverlay ? ("OpenPopUp(event, " + dr.Width + ",\"right\",\"" + dr.Name + "\", \"" + url + "\")") : ("OpenPopUp(event, " + dr.Width + ",\"center\",\"" + dr.Name + "\", \"" + url + "\")");
                            sb.Append(string.Format(@"<li><a href='javascript:void(0);' onclick='{0}' data-width='{2}'>{1} {3}</a>", ClickFunction, icon, dr.Width, dr.Name));
                        }
                        else
                        {
                            sb.Append(string.Format(@"<li><a href='{0}' data-url='{0}' data-width='{1}'>" + icon + " {2}</a>", url ?? "javascript:void(0);", dr.Width, dr.Name));
                        }

                        int Cid = dr.CID;
                        List<CategoryTree> subMenu = table.Where(x => x.ParentID == Cid).ToList();
                        if (subMenu.Count > 0)
                        {
                            StringBuilder subMenuBuilder = new StringBuilder();
                            sb.Append(GenerateUL(subMenu, table, subMenuBuilder, SearchKeyword));
                        }
                        sb.Append("</li>");

                        if (dr.BelowBreakLine)
                        {
                            sb.Append("<br />");
                        }
                    }
                }
                sb.Append("</ul>");
                return sb.ToString();
            }

            internal static List<CategoryTree> ParseMenuCategoryTree(string SearchKeywords)
            {
                List<CategoryTree> MenuTree = new List<CategoryTree>();
                int CID = 1;
                List<IMenuItem> filter = !string.IsNullOrEmpty(SearchKeywords) ? MenuManager.GetExtentions().Where(x => !string.IsNullOrEmpty(x.SearchKeywords) && x.SearchKeywords.ToLower().Contains(SearchKeywords.ToLower())).ToList() : MenuManager.GetExtentions();
                foreach (IMenuItem categories in filter)
                {
                    foreach (MenuItem category in categories.Items)
                    {
                        int Level = 0;
                        MenuItem Node = category.Hierarchy;
                        int? ParentID = null;
                        CategoryTree ParentNode = new CategoryTree();
                        if (Node != null)
                        {
                            while (Node != null)
                            {
                                CategoryTree ParentCategory = MenuTree.Where(t => !string.IsNullOrEmpty(t.Name) && t.Name.Trim() == Node.Text.Trim() && t.ParentID == ParentID && t.Level == Level && string.IsNullOrEmpty(t.GUID)).FirstOrDefault();

                                #region Getting ViewOrder or Icon, While ParentCategory vieworder or Icon is null if found then set value
                                if (ParentCategory != null)
                                {
                                    if (!ParentCategory.ViewOrder.HasValue && Node.ViewOrder.HasValue)
                                    {
                                        ParentCategory.ViewOrder = Node.ViewOrder.Value;
                                    }

                                    if (string.IsNullOrEmpty(ParentCategory.Icon) && !string.IsNullOrEmpty(Node.Icon))
                                    {
                                        ParentCategory.Icon = Node.Icon;
                                    }

                                    if (!ParentCategory.AboveBreakLine && Node.AboveBreakLine)
                                    {
                                        ParentCategory.AboveBreakLine = Node.AboveBreakLine;
                                    }

                                    if (!ParentCategory.BelowBreakLine && Node.BelowBreakLine)
                                    {
                                        ParentCategory.BelowBreakLine = Node.BelowBreakLine;
                                    }
                                }
                                #endregion

                                if (ParentCategory == null)
                                {
                                    MenuTree.Add(new CategoryTree
                                    {
                                        CID = CID,
                                        GUID = string.Empty,
                                        Name = Node.Text.Trim(),
                                        ParentID = ParentID,
                                        Level = Level,
                                        Icon = Node.Icon,
                                        ViewOrder = Node.ViewOrder,
                                        AboveBreakLine = !string.IsNullOrEmpty(SearchKeywords) ? false : Node.AboveBreakLine,
                                        BelowBreakLine = !string.IsNullOrEmpty(SearchKeywords) ? false : Node.BelowBreakLine,
                                        Width = categories.Width ?? 0,
                                        MenuAction = categories.Event,
                                        ModuleDefinition = category.ModuleDefinition,
                                        ModuleControl = category.ModuleControl
                                    });
                                }

                                CategoryTree hasParent = MenuTree.Where(t => t.Name == Node.Text.Trim() && t.ParentID == ParentID && t.Level == Level && string.IsNullOrEmpty(t.GUID)).FirstOrDefault();
                                if (hasParent != null)
                                {
                                    ParentID = hasParent.CID;
                                }

                                Node = Node.Hierarchy; CID++;

                                Level += 1;
                            }
                        }

                        if (Node == null)
                        {
                            MenuTree.Add(new CategoryTree
                            {
                                CID = CID,
                                GUID = category.ItemGuid.ToString(),
                                URL = category.URL,
                                Name = !string.IsNullOrEmpty(category.Text) ? category.Text : null,
                                ParentID = ParentID,
                                Level = Level,
                                Icon = category.Icon,
                                ViewOrder = category.ViewOrder,
                                AboveBreakLine = !string.IsNullOrEmpty(SearchKeywords) ? false : category.AboveBreakLine,
                                BelowBreakLine = !string.IsNullOrEmpty(SearchKeywords) ? false : category.BelowBreakLine,
                                Width = categories.Width ?? 0,
                                MenuAction = categories.Event,
                                ModuleDefinition = category.ModuleDefinition,
                                ModuleControl = category.ModuleControl
                            });
                            CID++;
                        }
                    }


                }
                foreach (CategoryTree m in MenuTree.Where(x => !x.ViewOrder.HasValue))
                {
                    m.ViewOrder = MenuTree.Max(x => x.ViewOrder) - 1;
                }

                return MenuTree.OrderBy(c => c.Level).ThenBy(n => n.ViewOrder).ToList();

            }
            #endregion           
            public static string GetURL(params string[] additionalparams)
            {
                #region Append Query in Iframe url because migrated time SkinSrc querystring required 
                string AppendURL = string.Empty;
                bool IsSkinSrc = false; //(its using as flag IsSkinSrc)

                //if found in query sting then not check in UrlReferrer
                if (HttpContext.Current.Request.QueryString.AllKeys.Count() > 0 && (HttpContext.Current.Request.QueryString.AllKeys.Contains("m2vsetup") || HttpContext.Current.Request.QueryString.AllKeys.Contains("m2v") || HttpContext.Current.Request.QueryString.AllKeys.Contains("migrate")))
                {
                    foreach (string q in HttpContext.Current.Request.QueryString.AllKeys)
                    {
                        if (q.ToLower() != "language" && q.ToLower() != "tabid" && q.ToLower() != "uxmode")
                        {
                            AppendURL = AppendURL + q + "=" + HttpContext.Current.Request.QueryString[q] + "&";
                        }

                        IsSkinSrc = true;
                    }
                }

                //if not found in query sting then check in UrlReferrer 
                if (!IsSkinSrc && HttpContext.Current.Request.UrlReferrer != null && (HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys.Contains("skinsrc") || HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys.Contains("m2vsetup") || HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys.Contains("m2v") || HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys.Contains("migrate")))
                {
                    foreach (string q in HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query).AllKeys)
                    {
                        if (q.ToLower() != "language" && q.ToLower() != "tabid" && q.ToLower() != "uxmode")
                        {
                            AppendURL = AppendURL + q + "=" + HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)[q] + "&";
                        }
                    }
                }
                return ("?" + AppendURL);
                #endregion
            }
        }
    }
}
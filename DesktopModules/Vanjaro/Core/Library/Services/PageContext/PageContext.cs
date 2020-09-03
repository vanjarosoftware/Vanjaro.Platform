using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Url.FriendlyUrl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Vanjaro.Core.Services
{
    public class PageContext
    {
        /// <summary>
        /// Initializes PageContext
        /// </summary>
        /// <param name="Objects"></param>
        /// 

        public PageContext()
        {
            PageLinks = new List<PageLink>();

        }

        public static PageContext Init(HttpRequest Request, int ModuleID, dynamic PagedSet)
        {
            string key = "pageno";
            PageContext pc = new PageContext();
            if (PagedSet != null)
            {
                pc.CurrentPage = PagedSet.CurrentPage;
                pc.TotalPages = PagedSet.TotalPages;
                pc.TotalItems = PagedSet.TotalItems;
                pc.PageSize = PagedSet.ItemsPerPage;
                pc.HasNextPage = PagedSet.CurrentPage < PagedSet.TotalPages;
                pc.HasPrevPage = PagedSet.CurrentPage > 1;
                pc.NextPage = PagedSet.CurrentPage + 1;
                pc.PrevPage = PagedSet.CurrentPage - 1;
                pc.Render = PagedSet.TotalPages > 1 ? true : false;

                if (Request != null && PortalSettings.Current != null)
                {
                    if (pc.HasNextPage)
                    {
                        pc.NextPageURL = GetURL(Request, ModuleID, key, pc.NextPage.ToString());
                    }

                    if (pc.HasPrevPage)
                    {
                        pc.PrevPageURL = GetURL(Request, ModuleID, key, pc.PrevPage.ToString());
                    }

                    #region List of Paggination   ex : 0 1 2 3 4 Where 2 is Current page

                    if (PagedSet.CurrentPage - 1 >= 2)
                    {
                        pc.PageLinks.Add(new PageLink { Name = (pc.PrevPage - 1).ToString(), Link = GetURL(Request, ModuleID, key, (pc.PrevPage - 1).ToString()), IsActive = false });
                    }

                    if (pc.HasPrevPage)
                    {
                        pc.PageLinks.Add(new PageLink { Name = pc.PrevPage.ToString(), Link = GetURL(Request, ModuleID, key, pc.PrevPage.ToString()), IsActive = false });
                    }

                    if (pc.TotalPages > 0)
                    {
                        pc.PageLinks.Add(new PageLink { Name = pc.CurrentPage.ToString(), Link = GetURL(Request, ModuleID, key, pc.CurrentPage.ToString()), IsActive = true });
                    }

                    if (pc.HasNextPage)
                    {
                        pc.PageLinks.Add(new PageLink { Name = pc.NextPage.ToString(), Link = GetURL(Request, ModuleID, key, pc.NextPage.ToString()), IsActive = false });
                    }

                    if (PagedSet.CurrentPage + 1 < PagedSet.TotalPages)
                    {
                        pc.PageLinks.Add(new PageLink { Name = (pc.NextPage + 1).ToString(), Link = GetURL(Request, ModuleID, key, pc.NextPage.ToString()), IsActive = false });
                    }
                    #endregion

                }
            }
            return pc;
        }

        /// <summary>
        /// Returns Total Items Count
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// Returns Next and Previous Two pages
        /// </summary>
        public List<PageLink> PageLinks { get; set; }

        /// <summary>
        /// Returns page size
        /// </summary>
        public long PageSize { get; set; }

        /// <summary>
        /// Returns current page
        /// </summary>
        public long CurrentPage { get; set; }

        /// <summary>
        /// Returns total pages
        /// </summary>
        public long TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Indicate if there is a previous page
        /// </summary>
        public bool HasPrevPage { get; set; }

        /// <summary>
        /// Next page number contained in this page of result set 
        /// </summary>
        public long NextPage { get; set; }


        /// <summary>
        /// Prev page number contained in this page of result set 
        /// </summary>
        public long PrevPage { get; set; }

        /// <summary>
        /// Indicates if pager should be rendered or not
        /// </summary>
        public bool Render { get; set; }

        /// <summary>
        /// Returns next page url
        /// </summary>
        public string NextPageURL { get; set; }

        /// <summary>
        /// Returns prev page url
        /// </summary>
        public string PrevPageURL { get; set; }

        public static string GetURL(HttpRequest Request, int ModuleID, string key, string Value)
        {
            return GetURL(Request, ModuleID, key, Value, "");
        }
        public static string GetURL(HttpRequest Request, int ModuleID, string key, string Value, string RemoveKey)
        {
            if (Request != null && PortalSettings.Current != null)
            {

                NameValueCollection query = new NameValueCollection(Request.QueryString);

                if (!string.IsNullOrEmpty(key))
                {
                    query[key] = Value;
                }

                StringBuilder newQuery = new StringBuilder();
                foreach (string k in query.Keys)
                {
                    if (k != RemoveKey || RemoveKey == key)
                    {
                        newQuery.AppendFormat("&{0}={1}", k, query[k]);
                    }
                }

                string qPath = "?" + newQuery.ToString().TrimStart('&');
                string Path = "~/Default.aspx" + qPath; //?TabId=" + PortalSettings.Current.ActiveTab.TabID;

                return FriendlyUrlProvider.Instance().FriendlyUrl(PortalSettings.Current.ActiveTab, Path);
            }
            else
            {
                throw new Exception("HttpRequest & PortalSettings.Current must be Non-Nullable to generate a paging url");
            }
        }
    }
    public class PageLink
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public bool IsActive { get; set; }
    }
}
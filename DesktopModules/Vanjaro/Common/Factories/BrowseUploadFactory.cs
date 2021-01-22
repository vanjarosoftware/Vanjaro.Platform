using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.UI;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Vanjaro.Common.Components;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Data.Scripts;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Entities.Enum;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Factories
{
    public class BrowseUploadFactory
    {
        public static string LocalResourceFile = "DesktopModules/Vanjaro/Common/Engines/UIEngine/AngularBootstrap/Views/App_LocalResources/Shared.resx";

        internal static List<IUIData> GetData(PortalSettings portalSettings, ModuleInfo moduleInfo, Dictionary<string, string> parameters)
        {
            int PortalID = portalSettings.PortalId;
            int ModuleID = 0;
            if (moduleInfo != null)
            {
                ModuleID = moduleInfo.ModuleID;
            }

            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            bool filebrowserBrowseUrl = true;
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.ToString().Contains("filebrowserImageBrowseUrl"))
            {
                filebrowserBrowseUrl = false;
            }

            string filter = null;
            string uid = parameters.ContainsKey("uid") ? parameters["uid"].ToString().Split('&').First() : string.Empty;
            HTMLEditorSetting HTMLEditorSetting = new HTMLEditorSetting();
            if (!string.IsNullOrEmpty(uid))
            {
                HTMLEditor Editor = HTMLEditor.Query("where UID=@0", uid + ModuleID).SingleOrDefault();
                if (Editor != null)
                {
                    HTMLEditorSetting = Json.Deserialize<HTMLEditorSetting>(Editor.Settings);
                }

                Settings.Add("Settings", new UIData() { Name = "Settings", Options = HTMLEditorSetting });
                if (filebrowserBrowseUrl)
                {
                    filter = HTMLEditorSetting.UploadFilesAllowedAttachmentFileExtensions;
                }
                else
                {
                    filter = HTMLEditorSetting.UploadImagesAllowedAttachmentFileExtensions;
                }
            }
            Settings.Add("FilebrowserBrowseUrl", new UIData() { Name = "FilebrowserBrowseUrl", Value = filebrowserBrowseUrl.ToString() });
            List<TreeView> folders = new List<TreeView>();
            if (filebrowserBrowseUrl)
            {
                if (HTMLEditorSetting.UploadFilesRootFolder > 0)
                {
                    folders = GetFoldersTree(PortalID, HTMLEditorSetting.UploadFilesRootFolder);
                }
                else
                {
                    folders = GetFoldersTree(PortalID);
                }
            }
            else
            {
                if (HTMLEditorSetting.UploadImagesRootFolder > 0)
                {
                    folders = GetFoldersTree(PortalID, HTMLEditorSetting.UploadImagesRootFolder);
                }
                else
                {
                    folders = GetFoldersTree(PortalID);
                }
            }
            Settings.Add("Folders", new UIData() { Name = "Folders", Options = folders, Value = folders.Count > 0 ? folders.FirstOrDefault().Value.ToString() : "0" });
            Settings.Add("Files", new UIData() { Name = "Files", Options = null });
            Settings.Add("Types", new UIData() { Name = "Types", Value = "true" });
            Settings.Add("TypesPages", new UIData() { Name = "TypesPages", Value = "true" });
            Settings.Add("Uid", new UIData() { Name = "Uid", Value = uid });
            Settings.Add("DnnPages", new UIData() { Name = "DnnPages", Value = "-1", Options = GetDnnPages(PortalID) });
            Settings.Add("chkHumanFriendly", new UIData() { Name = "chkHumanFriendly", Value = "true" });
            Settings.Add("PageAnchors", new UIData() { Name = "PageAnchors", Options = GetPageAnchors(null), Value = "0" });
            Settings.Add("Languages", new UIData() { Name = "Languages", Options = GetDnnLanguages(PortalID), Value = "0" });
            return Settings.Values.ToList();
        }

        internal static List<ListItem> GetPageAnchors(TabInfo selectedTab)
        {
            List<ListItem> result = new List<ListItem>();
            ListItem li = new ListItem
            {
                Text = "None",
                Value = "0"
            };
            result.Add(li);
            if (selectedTab != null)
            {
                WebClient webClient = new WebClient();
                string fullUrl = selectedTab.FullUrl;
                if (fullUrl.StartsWith("/"))
                {
                    fullUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, fullUrl);
                }

                foreach (LinkItem linkItem in from i in GetPageAnchorsListAll(webClient.DownloadString(fullUrl))
                                              where !string.IsNullOrEmpty(i.Anchor)
                                              select i)
                {
                    string Text = GetPageAnchorModuleTitle(linkItem.Anchor);
                    if (!string.IsNullOrEmpty(Text))
                    {
                        ListItem item = new ListItem
                        {
                            Text = Text,
                            Value = linkItem.Anchor
                        };
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        private static string GetPageAnchorModuleTitle(string Anchor)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(Anchor))
            {
                int mid = 0;
                try { mid = int.Parse(Anchor); } catch { }
                if (mid > 0)
                {
                    ModuleInfo minfo = new ModuleController().GetModule(mid, Null.NullInteger, false);
                    if (minfo != null && !minfo.ModuleTitle.StartsWith("Do Not Delete or Rename - Auto Generated By Live Visualizer"))
                    {
                        result = minfo.ModuleTitle + " (" + minfo.ModuleID + ")";
                    }
                }
                else
                {
                    result = Anchor;
                }
            }
            return result;
        }

        private static List<LinkItem> GetPageAnchorsListAll(string file)
        {
            List<LinkItem> linkItems = new List<LinkItem>();
            foreach (object obj in Regex.Matches(file, "(<a.*?>.*?</a>)", RegexOptions.IgnoreCase))
            {
                string value = ((Match)obj).Groups[1].Value;
                LinkItem linkItem = new LinkItem();
                Match match = Regex.Match(value, "href=\\\"(.*?)\\\"", RegexOptions.Singleline);
                if (match.Success)
                {
                    linkItem.Href = match.Groups[1].Value;
                }
                Match match1 = Regex.Match(value, "name=\\\"(.*?)\\\"", RegexOptions.Singleline);
                if (match1.Success)
                {
                    linkItem.Anchor = match1.Groups[1].Value;
                }
                linkItem.Text = Regex.Replace(value, "\\s*<.*?>\\s*", string.Empty, RegexOptions.Singleline);
                linkItems.Add(linkItem);
            }
            return linkItems;
        }

        private static List<ListItem> GetDnnLanguages(int PortalID)
        {
            List<ListItem> result = new List<ListItem>();
            foreach (ListItem listItem in from language in (new LocaleController()).GetLocales(PortalID).Values
                                          select new ListItem()
                                          {
                                              Text = language.Text,
                                              Value = language.Code
                                          })
            {
                result.Add(listItem);
                ListItem li = new ListItem
                {
                    Text = "None",
                    Value = "0"
                };
                result.Insert(0, li);
            }
            return result;
        }

        public static List<TreeView> GetDnnPages(int PortalID)
        {
            List<TreeView> result = new List<TreeView>();
            List<TabInfo> TabCollection = TabController.GetPortalTabs(PortalID, -1, false, null, true, false, true, true, false);
            if (TabCollection != null && TabCollection.Count > 0)
            {
                foreach (TabInfo Item in TabCollection.Where(t => t.ParentId == -1))
                {
                    TreeView tab = new TreeView
                    {
                        Value = Item.TabID,
                        Text = Item.TabName
                    };
                    if (Item.HasChildren)
                    {
                        tab.children = new List<TreeView>();
                        GetDnnPagesChildrensTree(TabCollection, Item, tab.children);
                    }
                    if (tab.children != null && tab.children.Count > 0)
                    {
                        tab.childrenCount = tab.children.Count;
                    }

                    result.Add(tab);
                }
            }
            return result;
        }

        private static void GetDnnPagesChildrensTree(List<TabInfo> tabs, TabInfo currentTab, List<TreeView> nestedTabs)
        {
            List<TabInfo> childTabs = tabs.Where(t => t.ParentId == currentTab.TabID).ToList();
            foreach (TabInfo Item in childTabs)
            {
                TreeView tab = new TreeView
                {
                    Value = Item.TabID,
                    Text = Item.TabName
                };
                if (Item.HasChildren)
                {
                    tab.children = new List<TreeView>();
                    GetDnnPagesChildrensTree(tabs, Item, tab.children);
                }
                if (tab.children != null && tab.children.Count > 0)
                {
                    tab.childrenCount = tab.children.Count;
                }

                nestedTabs.Add(tab);
            }
        }

        public static void ExtractFiles(int fileid)
        {
            IFileInfo file = FileManager.Instance.GetFile(fileid);
            if (file != null && file.Extension.ToLower() == "zip")
            {
                FileManager.Instance.UnzipFile(file);
                FileManager.Instance.DeleteFile(file);
            }
        }

        internal static UploadOptions Options { get; set; }

        public static List<TreeView> GetFoldersTree(int PortalId)
        {
            return GetFoldersTree(PortalId, null);
        }

        public static IFolderInfo GetRootFolder(int PortalID)
        {
            return FolderManager.Instance.GetFolder(PortalID, "");
        }

        public static List<TreeView> GetFoldersTree(int PortalId, string Type)
        {
            if (PortalId == -1)
            {
                return GetFoldersTree(PortalId, 1, Type);
            }
            else
            {
                return GetFoldersTree(PortalId, GetRootFolder(PortalId).FolderID, Type);
            }
        }

        public static List<TreeView> GetFoldersTree(int PortalId, int ParentFolderId)
        {
            return GetFoldersTree(PortalId, ParentFolderId, null);
        }

        public static List<TreeView> GetFoldersTree(int PortalId, int ParentFolderId, string Type)
        {
            List<TreeView> result = new List<TreeView>();
            IFolderInfo parentFolder = FolderManager.Instance.GetFolder(ParentFolderId);
            if (parentFolder != null)
            {
                bool hasPermission = (HasPermission(parentFolder, "BROWSE") || HasPermission(parentFolder, "READ"));
                if (hasPermission)
                {
                    TreeView FolderItem = new TreeView();

                    if (string.IsNullOrEmpty(parentFolder.FolderPath))
                    {
                        if (PortalId == -1)
                        {
                            FolderItem.Text = "Global Root";
                        }
                        else
                        {
                            FolderItem.Text = "Site Root";
                        }
                    }
                    else
                    {
                        int nameCharIndex = parentFolder.FolderPath.Substring(0, parentFolder.FolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1;
                        FolderItem.Text = parentFolder.FolderPath.Substring(nameCharIndex, parentFolder.FolderPath.Length - nameCharIndex - 1);
                    }
                    FolderItem.Value = parentFolder.FolderID;
                    FolderItem.children = GetFoldersChildrensTree(PortalId, parentFolder.FolderID, UserController.Instance.GetCurrentUserInfo(), Type);
                    FolderItem.childrenCount = FolderItem.children.Count;
                    FolderItem.uploadAllowed = (HasPermission(parentFolder, "WRITE") || HasPermission(parentFolder, "ADD"));
                    FolderItem.Type = Enum.GetName(typeof(FolderTypes), parentFolder.StorageLocation);
                    FolderItem.ProviderType = FolderMappingController.Instance.GetFolderMapping(parentFolder.FolderMappingID).FolderProviderType;
                    FolderItem.Lock = HasBrowseOpenPerm(FolderItem);
                    FolderItem.IsImage = IsImage(Type, parentFolder.FolderID, null);
                    result.Add(FolderItem);
                }
            }
            return result;
        }

        public static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            bool hasPermision = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        public static List<TreeView> GetFoldersChildrensTree(int PortalID, int ParentFolderID, UserInfo userInfo)
        {
            return GetFoldersChildrensTree(PortalID, ParentFolderID, userInfo, null);
        }

        public static List<TreeView> GetFoldersChildrensTree(int PortalID, int ParentFolderID, UserInfo userInfo, string Type)
        {
            List<TreeView> result = new List<TreeView>();
            if (userInfo != null)
            {
                using (CommonLibraryRepo db = new CommonLibraryRepo())
                {
                    IList<UserRoleInfo> Roles = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRoles(userInfo, true);
                    if (Roles.Count == 0 && userInfo.IsSuperUser)
                    {
                        Roles.Add(new UserRoleInfo() { RoleID = PortalID == -1 ? -2 : DotNetNuke.Security.Roles.RoleController.Instance.GetRoleByName(PortalID, "Administrators").RoleID });
                    }

                    List<dynamic> folders = db.Fetch<dynamic>(BrowseUploadScript.GetFolders(PortalID, userInfo, ParentFolderID, string.Join(",", Roles.Select(r => r.RoleID))));
                    foreach (dynamic folder in folders)
                    {
                        TreeView FolderItem = new TreeView();
                        dynamic nameCharIndex = folder.FolderPath.Substring(0, folder.FolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1;
                        FolderItem.Text = folder.FolderPath.Substring(nameCharIndex, folder.FolderPath.Length - nameCharIndex - 1);
                        FolderItem.Value = folder.FolderID;
                        FolderItem.childrenCount = folder.ChildCount ?? 0;
                        FolderItem.Type = Enum.GetName(typeof(FolderTypes), folder.StorageLocation);
                        FolderItem.ProviderType = FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID).FolderProviderType;
                        FolderItem.Lock = HasBrowseOpenPerm(FolderItem);
                        FolderItem.IsImage = IsImage(Type, folder.FolderID, db);
                        bool hasPermision = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
                        if (!hasPermision)
                        {
                            IFolderInfo fo = FolderManager.Instance.GetFolder(folder.FolderID);
                            if (fo != null)
                            {
                                hasPermision = (FolderPermissionController.HasFolderPermission(fo.FolderPermissions, "WRITE") || FolderPermissionController.HasFolderPermission(fo.FolderPermissions, "ADD"));
                            }
                        }
                        FolderItem.uploadAllowed = hasPermision;
                        result.Add(FolderItem);
                    }
                }
            }
            return result;
        }

        public static dynamic GetPagedFiles(int ModuleID, TreeView folder, string uid, string filter, int skip, int pageSize, string keyword)
        {
            dynamic result = new ExpandoObject();
            double NumberOfPages = 0;
            List<TreeView> files = new List<TreeView>();
            if (folder != null)
            {
                IFolderInfo folderInfo = FolderManager.Instance.GetFolder(folder.Value);
                if (folderInfo != null)
                {
                    bool hasPermission = (HasPermission(folderInfo, "BROWSE") || HasPermission(folderInfo, "READ"));
                    if (hasPermission)
                    {
                        string searchText = null;
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            searchText = keyword;
                        }

                        Func<IFileInfo, bool> searchFunc;
                        if (string.IsNullOrEmpty(filter))
                        {
                            bool filebrowserBrowseUrl = true;
                            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.ToString().Contains("filebrowserImageBrowseUrl"))
                            {
                                filebrowserBrowseUrl = false;
                            }

                            HTMLEditorSetting HTMLEditorSetting = new HTMLEditorSetting();
                            if (!string.IsNullOrEmpty(uid))
                            {
                                HTMLEditor Editor = HTMLEditor.Query("where UID=@0", uid + ModuleID).SingleOrDefault();
                                if (Editor != null)
                                {
                                    HTMLEditorSetting = Json.Deserialize<HTMLEditorSetting>(Editor.Settings);
                                }
                            }
                            if (filebrowserBrowseUrl)
                            {
                                filter = HTMLEditorSetting.UploadFilesAllowedAttachmentFileExtensions;
                            }
                            else
                            {
                                filter = HTMLEditorSetting.UploadImagesAllowedAttachmentFileExtensions;
                            }
                        }
                        List<string> filterList = string.IsNullOrEmpty(filter) ? null : filter.ToLowerInvariant().Split(',').ToList();
                        if (string.IsNullOrEmpty(searchText))
                        {
                            searchFunc = f => filterList == null || filterList.Contains(f.Extension.ToLowerInvariant());
                        }
                        else
                        {
                            searchFunc = f => f.FileName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1
                                                && (filterList == null || filterList.Contains(f.Extension.ToLowerInvariant()));
                        }

                        foreach (IFileInfo file in FolderManager.Instance.GetFiles(folderInfo).Where(f => searchFunc(f)).Skip(skip).Take(pageSize))
                        {
                            TreeView FileItem = new TreeView
                            {
                                Text = file.FileName,
                                Value = file.FileId,
                                DateModified = file.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm tt"),
                                Size = Math.Round(Convert.ToDecimal(file.Size) / 1000, 2).ToString() + " KB",
                                Type = GetFileType(file)
                            };
                            files.Add(FileItem);
                        }

                        NumberOfPages = (double)FolderManager.Instance.GetFiles(folderInfo).Where(f => searchFunc(f)).Count() / pageSize;
                        NumberOfPages = (int)Math.Ceiling(NumberOfPages);
                    }
                }
            }
            result.Files = files;
            result.Pages = NumberOfPages;
            return result;
        }

        public static string GetLink(PortalSettings PortalSettings, ModuleInfo ActiveModule, string fileurl, int urltype)
        {
            string result = "";
            IFileInfo file = null;
            if (HttpContext.Current != null && ActiveModule != null && !string.IsNullOrEmpty(fileurl))
            {
                if (fileurl.StartsWith("pages"))
                {
                    int PageID = 0;
                    try
                    {
                        PageID = int.Parse(fileurl.Replace("pages", ""));
                    }
                    catch { }

                    TabController tabController = new TabController();
                    TabInfo tab = TabController.Instance.GetTab(PageID, ActiveModule.PortalID, true);
                    if (tab != null)
                    {
                        bool flag = false;
                        string flaggedUrl = string.Empty;
                        string lang = string.Empty;
                        if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["language"]) && HttpContext.Current.Request.QueryString["language"] != "0")
                        {
                            lang = string.Format("language/{0}/", HttpContext.Current.Request.QueryString["language"]);
                            flag = true;
                        }

                        bool HumanFriendly = true;
                        if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["humanfriendly"]))
                        {
                            HumanFriendly = Convert.ToBoolean(HttpContext.Current.Request.QueryString["humanfriendly"]);
                        }

                        if (!flag)
                        {
                            flaggedUrl = DotNetNuke.Common.Globals.FriendlyUrl(tab, DotNetNuke.Common.Globals.ApplicationURL(tab.TabID), PortalSettings as IPortalSettings);
                        }
                        else
                        {
                            flaggedUrl = DotNetNuke.Common.Globals.FriendlyUrl(tab, string.Format("{0}&language={1}", DotNetNuke.Common.Globals.ApplicationURL(tab.TabID), HttpContext.Current.Request.QueryString["language"]), PortalSettings as IPortalSettings);
                        }

                        switch (urltype)
                        {
                            case 0:
                                {
                                    if (HumanFriendly)
                                    {
                                        result = DotNetNuke.Common.Globals.ResolveUrl(Regex.Replace(flaggedUrl, string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, DotNetNuke.Common.Globals.GetDomainName(HttpContext.Current.Request, true)), "~", RegexOptions.IgnoreCase));
                                    }
                                    else
                                    {
                                        result = DotNetNuke.Common.Globals.ResolveUrl(string.Format("~/tabid/{0}/{1}Default.aspx", tab.TabID, lang));
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (HumanFriendly)
                                    {
                                        result = Regex.Replace(flaggedUrl, string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, DotNetNuke.Common.Globals.GetDomainName(HttpContext.Current.Request, true)), string.Format("{0}", string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, DotNetNuke.Common.Globals.GetDomainName(HttpContext.Current.Request, true))), RegexOptions.IgnoreCase);
                                    }
                                    else
                                    {
                                        result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, DotNetNuke.Common.Globals.GetDomainName(HttpContext.Current.Request, true), string.Format("/tabid/{0}/{1}Default.aspx", tab.TabID, lang));
                                    }
                                }
                                break;
                            case 2:
                                {
                                    string str = DotNetNuke.Common.Globals.LinkClick(tab.TabID.ToString(), ActiveModule.TabID, Null.NullInteger);
                                    if (str.Contains("&language"))
                                    {
                                        str = str.Remove(str.IndexOf("&language"));
                                    }

                                    result = str;
                                }
                                break;
                            case 3:
                                {
                                    string str = DotNetNuke.Common.Globals.LinkClick(tab.TabID.ToString(), ActiveModule.TabID, Null.NullInteger);
                                    if (str.Contains("&language"))
                                    {
                                        str = str.Remove(str.IndexOf("&language"));
                                    }

                                    result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, str);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["anchorlist"]) && HttpContext.Current.Request.QueryString["anchorlist"] != "0")
                    {
                        result = string.Format("{0}#{1}", result, HttpContext.Current.Request.QueryString["anchorlist"]);
                    }
                }
                else
                {
                    int FileID = 0;
                    try { FileID = int.Parse(fileurl); }
                    catch { }
                    switch (urltype)
                    {
                        case 0:
                            {
                                if (FileID > 0)
                                {
                                    file = FileManager.Instance.GetFile(FileID);
                                    if (file != null)
                                    {
                                        result = FileManager.Instance.GetUrl(file).Replace(file.FileName, GetEscapedFileName(file.FileName));
                                    }
                                }
                                else
                                {
                                    result = fileurl;
                                }
                            }
                            break;
                        case 1:
                            {
                                if (FileID > 0)
                                {
                                    file = FileManager.Instance.GetFile(FileID);
                                }

                                if (file != null)
                                {
                                    result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, FileManager.Instance.GetUrl(file).Replace(file.FileName, GetEscapedFileName(file.FileName)));
                                }
                                else
                                {
                                    result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, fileurl);
                                }
                            }
                            break;
                        case 2:
                            {
                                if (FileID > 0)
                                {
                                    file = FileManager.Instance.GetFile(FileID);
                                }
                                else
                                {
                                    file = FileManager.Instance.GetFile(ActiveModule.PortalID, fileurl.Replace(PortalSettings.HomeDirectory, ""));
                                }

                                if (file != null)
                                {
                                    string str = string.Format("fileID={0}", file.FileId);
                                    result = DotNetNuke.Common.Globals.LinkClick(str, ActiveModule.TabID, Null.NullInteger);
                                }
                            }
                            break;
                        case 3:
                            {
                                if (FileID > 0)
                                {
                                    file = FileManager.Instance.GetFile(FileID);
                                }
                                else
                                {
                                    file = FileManager.Instance.GetFile(ActiveModule.PortalID, fileurl.Replace(PortalSettings.HomeDirectory, ""));
                                }

                                if (file != null)
                                {
                                    string str = string.Format("fileID={0}", file.FileId);
                                    result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, DotNetNuke.Common.Globals.LinkClick(str, ActiveModule.TabID, Null.NullInteger));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return result;
        }
        public static dynamic SyncFolderContent(int folderId, bool recursive)
        {
            dynamic result = new ExpandoObject();
            result.IsSuccess = true;
            try
            {
                IFolderInfo folder = FolderManager.Instance.GetFolder(folderId);
                result.FolderName = "";

                if (string.IsNullOrEmpty(folder.FolderPath))
                {
                    if (folder.PortalID == -1)
                    {
                        result.FolderName = "Global Root";
                    }
                    else
                    {
                        result.FolderName = "Site Root";
                    }
                }
                else
                {
                    result.FolderName = folder.DisplayName;
                }

                if (!FolderPermissionController.CanBrowseFolder((FolderInfo)folder))
                {
                    //The user cannot access the content     
                    result.IsSuccess = false;
                    result.Message = "Permission Error!";
                    return result;
                }
                FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, recursive, true);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public static int DeleteFolders(int folderId)
        {
            int count = 0;
            IFolderInfo folder = FolderManager.Instance.GetFolder(folderId);
            if (folder != null)
            {
                if (!HasPermission(folder, "DELETE"))
                {
                    count++;
                }
                else
                {
                    List<IFolderInfo> nonDeletedSubfolders = new List<IFolderInfo>();
                    FolderManager.Instance.DeleteFolder(folder, nonDeletedSubfolders);
                    foreach (IFolderInfo nonDeletedSubfolder in nonDeletedSubfolders)
                    {
                        count++;
                    }
                }
            }
            else
            {
                count++;
            }

            return count;
        }

        public static string CreateNewFolder(int folderParentID, string folderName, int folderType, ref IFolderInfo newFolderInfo, string mappedPath = "")
        {
            string result = "Success";
            try
            {
                if (!string.IsNullOrEmpty(folderName))
                {
                    folderName = CleanDotsAtTheEndOfTheName(folderName);

                    // Chech if the new name has invalid chars
                    if (IsInvalidName(folderName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FolderFileNameHasInvalidcharacters", LocalResourceFile));
                    }

                    // Check if the new name is a reserved name
                    if (IsReservedName(folderName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FolderFileNameIsReserved", LocalResourceFile));
                    }

                    IFolderInfo parentFolder = FolderManager.Instance.GetFolder(folderParentID);

                    if (!HasPermission(parentFolder, "ADD"))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToAdd", LocalResourceFile));
                    }

                    string folderPath = PathUtils.Instance.FormatFolderPath(
                                     PathUtils.Instance.FormatFolderPath(
                                     PathUtils.Instance.StripFolderPath(parentFolder.FolderPath).Replace("\\", "/")) + folderName);

                    mappedPath = PathUtils.Instance.FormatFolderPath(mappedPath);
                    Regex MappedPathRegex = new Regex(@"^(?!\s*[\\/]).*$", RegexOptions.Compiled);
                    if (!MappedPathRegex.IsMatch(mappedPath))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("InvalidMappedPath", LocalResourceFile));
                    }

                    FolderMappingInfo folderMapping = FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, folderType);
                    newFolderInfo = FolderManager.Instance.AddFolder(folderMapping, folderPath, mappedPath.Replace("\\", "/"));
                    return result;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                result = ex.Message;
            }
            return result;
        }

        public static string RenameFile(int fileID, string newFileName)
        {
            string result = "Success";
            try
            {
                if (!string.IsNullOrEmpty(newFileName))
                {
                    newFileName = CleanDotsAtTheEndOfTheName(newFileName);

                    if (string.IsNullOrEmpty(newFileName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FileNameInvalid", LocalResourceFile));
                    }

                    // Chech if the new name has invalid chars
                    if (IsInvalidName(newFileName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FolderFileNameHasInvalidcharacters", LocalResourceFile));
                    }

                    // Check if the new name is a reserved name
                    if (IsReservedName(newFileName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FolderFileNameIsReserved", LocalResourceFile));
                    }

                    IFileInfo file = FileManager.Instance.GetFile(fileID, true);

                    // Check if the name has not changed
                    if (file.FileName == newFileName)
                    {
                        return result;
                    }

                    // Check if user has appropiate permissions
                    IFolderInfo folder = FolderManager.Instance.GetFolder(file.FolderId);
                    if (!HasPermission(folder, "MANAGE"))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToEditFile", LocalResourceFile));
                    }

                    FileManager.Instance.RenameFile(file, newFileName);
                    return result;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                result = ex.Message;
            }
            return result;
        }

        public static string CopyFile(int fileID, int destinationFolderId, bool overwrite)
        {
            IFileInfo file = FileManager.Instance.GetFile(fileID, true);
            IFolderInfo folder = FolderManager.Instance.GetFolder(destinationFolderId);
            IFolderInfo sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!HasPermission(sourceFolder, "COPY"))
            {
                return DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToCopyFolder", LocalResourceFile);
            }
            if (file.FolderId == destinationFolderId)
            {
                string destFileName = Path.GetFileNameWithoutExtension(file.FileName) + "-Copy" + Path.GetExtension(file.FileName);
                int i = 1;
                while (FileManager.Instance.FileExists(folder, destFileName, true))
                {
                    destFileName = Path.GetFileNameWithoutExtension(file.FileName) + "-Copy(" + i + ")" + Path.GetExtension(file.FileName);
                    i++;
                }
                IFileInfo renamedFile = FileManager.Instance.AddFile(folder, destFileName, FileManager.Instance.GetFileContent(file));
                return "Success";
            }
            else if (!overwrite && FileManager.Instance.FileExists(folder, file.FileName, true))
            {
                return "Exist";
            }

            IFileInfo copy = FileManager.Instance.CopyFile(file, folder);
            return "Success";
        }

        public static dynamic MoveFile(int fileID, int destinationFolderId, bool overwrite)
        {
            dynamic Result = new ExpandoObject();
            IFileInfo file = FileManager.Instance.GetFile(fileID, true);
            IFolderInfo folder = FolderManager.Instance.GetFolder(destinationFolderId);
            IFolderInfo sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!HasPermission(sourceFolder, "COPY"))
            {
                return DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToMoveFolder", LocalResourceFile);
            }
            if (file.FolderId == destinationFolderId)
            {
                // User must not move files in the same folder  
                return DotNetNuke.Services.Localization.Localization.GetString("DestinationFolderCannotMatchSourceFolder", LocalResourceFile);
            }
            if (!overwrite && FileManager.Instance.FileExists(folder, file.FileName, true))
            {
                return "Exist";
            }
            IFileInfo copy = FileManager.Instance.MoveFile(file, folder);
            return "Success";
        }

        public static string MoveFolder(int folderId, int destinationFolderId)
        {
            IFolderInfo folder = FolderManager.Instance.GetFolder(folderId);
            if (folder != null)
            {
                if (!HasPermission(folder, "COPY"))
                {
                    return DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToMoveFolder", LocalResourceFile);
                }

                IFolderInfo destinationFolder = FolderManager.Instance.GetFolder(destinationFolderId);

                FolderManager.Instance.MoveFolder(folder, destinationFolder);
                return "Success";
            }
            else
            {
                return DotNetNuke.Services.Localization.Localization.GetString("FolderDoesNotExists", LocalResourceFile);
            }
        }

        public static Stream GetFileContent(int fileId, out string fileName, out string contentType)
        {
            IFileInfo file = FileManager.Instance.GetFile(fileId, true);
            IFolderInfo folder = FolderManager.Instance.GetFolder(file.FolderId);

            if (!HasPermission(folder, "READ"))
            {
                throw new DotNetNukeException(DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToDownload", LocalResourceFile));
            }


            Stream content = FileManager.Instance.GetFileContent(file);
            fileName = file.FileName;
            contentType = file.ContentType;

            EventLogController.Instance.AddLog(file, PortalSettings.Current as IPortalSettings, UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.ADMIN_ALERT);
            return content;
        }

        public static dynamic GetUrl(int fileid)
        {
            dynamic Result = new ExpandoObject();
            IFileInfo file = FileManager.Instance.GetFile(fileid, true);
            if (file != null)
            {
                IFolderInfo folder = FolderManager.Instance.GetFolder(file.FolderId);
                if (!HasPermission(folder, "READ"))
                {
                    Result.Status = DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToDownload", LocalResourceFile);
                    return Result;
                }
            }
            Result.Status = "Success";
            Result.Url = GetEscapedFileName(FileManager.Instance.GetUrl(file));
            Result.Urls = GetUrls(file);
            return Result;
        }

        public static string DeleteFile(int fileID)
        {
            IFileInfo fileInfo = FileManager.Instance.GetFile(fileID, true);
            if (fileInfo != null)
            {
                IFolderInfo folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);
                if (HasPermission(folder, "DELETE"))
                {
                    FileManager.Instance.DeleteFile(fileInfo);
                    DeleteFiles(fileInfo);
                }
            }
            return "Success";
        }

        public static string RenameFolder(int folderId, string newFolderName)
        {
            string result = "Success";
            try
            {
                if (!string.IsNullOrEmpty(newFolderName))
                {
                    newFolderName = CleanDotsAtTheEndOfTheName(newFolderName);

                    // Chech if the new name has invalid chars
                    if (IsInvalidName(newFolderName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FolderFileNameHasInvalidcharacters", LocalResourceFile));
                    }

                    // Check if the new name is a reserved name
                    if (IsReservedName(newFolderName))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("FolderFileNameIsReserved", LocalResourceFile));
                    }

                    IFolderInfo folder = FolderManager.Instance.GetFolder(folderId);

                    if (!HasPermission(folder, "MANAGE"))
                    {
                        throw new Exception(DotNetNuke.Services.Localization.Localization.GetString("UserHasNoPermissionToEditFolder", LocalResourceFile));
                    }

                    // check if the name has not changed
                    if (folder.FolderName == newFolderName)
                    {
                        return result;
                    }

                    FolderManager.Instance.RenameFolder(folder, newFolderName);

                    return result;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                result = ex.Message;
            }
            return result;
        }

        public static dynamic GetFile(PortalSettings PortalSettings, int fileid)
        {
            dynamic result = new ExpandoObject();
            IFileInfo file = FileManager.Instance.GetFile(fileid);
            if (file != null)
            {
                result.Name = file.RelativePath;
                result.FileId = file.FileId;
                result.FileUrl = GetEscapedFileName(FileManager.Instance.GetUrl(file));
                return result;
            }
            return result;
        }

        public static List<ImageUrl> GetUrls(IFileInfo fileInfo)
        {
            List<ImageUrl> Urls = new List<ImageUrl>();
            if (fileInfo != null)
            {
                string FileName = fileInfo.FileName.Remove(fileInfo.FileName.Length - (fileInfo.Extension.Length + 1));
                IFolderInfo folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);
                if (folder != null)
                {
                    IFolderInfo folderinfo = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, folder.FolderPath + ".versions");
                    if (folderinfo != null)
                    {
                        foreach (IFileInfo finfo in FolderManager.Instance.GetFiles(folderinfo))
                        {
                            if (finfo.FileName.StartsWith(FileName + "_" + finfo.Width + "w."))
                            {
                                ImageUrl imgUrl = new ImageUrl
                                {
                                    Url = GetEscapedFileName(FileManager.Instance.GetUrl(finfo)),
                                    Width = finfo.Width,
                                    Type = "image"
                                };
                                Urls.Add(imgUrl);
                                IFileInfo webpfileinfo = FileManager.Instance.GetFile(folderinfo, FileName + "_" + finfo.Width + "w.webp");
                                if (webpfileinfo != null)
                                {
                                    imgUrl = new ImageUrl
                                    {
                                        Url = GetEscapedFileName(FileManager.Instance.GetUrl(webpfileinfo)),
                                        Width = finfo.Width,
                                        Type = "webp"
                                    };
                                    Urls.Add(imgUrl);
                                }
                            }
                        }
                    }
                }
            }
            return Urls.OrderByDescending(w => w.Width).ToList();
        }

        public static void DeleteFiles(IFileInfo fileInfo)
        {
            if (fileInfo != null)
            {
                string FileName = fileInfo.FileName.Remove(fileInfo.FileName.Length - (fileInfo.Extension.Length + 1));
                IFolderInfo folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);
                if (folder != null)
                {
                    IFolderInfo folderinfo = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, folder.FolderPath + ".versions");
                    if (folderinfo != null)
                    {
                        foreach (IFileInfo finfo in FolderManager.Instance.GetFiles(folderinfo))
                        {
                            if (finfo.FileName.StartsWith(FileName + "_" + finfo.Width + "w."))
                            {
                                FileManager.Instance.DeleteFile(finfo);
                                IFileInfo webpfileinfo = FileManager.Instance.GetFile(folderinfo, FileName + "_" + finfo.Width + "w.webp");
                                if (webpfileinfo != null)
                                {
                                    FileManager.Instance.DeleteFile(webpfileinfo);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static string UploadFile(string identifier, HttpContext current, PortalSettings portalSettings, ModuleInfo activeModule, UserInfo userInfo, bool isUploadAllowed, string FileTypes, int MaxSize, string AppName, string uid)
        {
            return UploadFile(identifier, current, portalSettings, activeModule, userInfo, isUploadAllowed, FileTypes, MaxSize, AppName, uid, -1);
        }

        public static string UploadFile(string identifier, HttpContext current, PortalSettings portalSettings, ModuleInfo activeModule, UserInfo userInfo, bool isUploadAllowed, string FileTypes, int MaxSize, string AppName, string uid, int folderid)
        {
            if (!string.IsNullOrEmpty(identifier) && current != null && current.Request != null && current.Request.Files.Count > 0)
            {
                if (!string.IsNullOrEmpty(uid) && identifier == "common_controls_url")
                {
                    bool filebrowserBrowseUrl = true;
                    if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.ToString().Contains("filebrowserImageBrowseUrl"))
                    {
                        filebrowserBrowseUrl = false;
                    }

                    HTMLEditorSetting HTMLEditorSetting = new HTMLEditorSetting();
                    HTMLEditor Editor = HTMLEditor.Query("where UID=@0", uid + activeModule.ModuleID).SingleOrDefault();
                    if (Editor != null)
                    {
                        HTMLEditorSetting = DotNetNuke.Common.Utilities.Json.Deserialize<HTMLEditorSetting>(Editor.Settings);
                    }

                    if (filebrowserBrowseUrl)
                    {
                        FileTypes = HTMLEditorSetting.UploadFilesAllowedAttachmentFileExtensions;
                        MaxSize = HTMLEditorSetting.UploadFilesMaxFileSize;
                    }
                    else
                    {
                        FileTypes = HTMLEditorSetting.UploadImagesAllowedAttachmentFileExtensions;
                        MaxSize = HTMLEditorSetting.UploadImagesMaxFileSize;
                    }
                }
                OnInitialization(current, identifier, activeModule, portalSettings, userInfo, isUploadAllowed, FileTypes, MaxSize, AppName, folderid);
                if (Options != null && Options.AllowUpload)
                {
                    string result = SaveFiles(HttpContext.Current, HttpContext.Current.Request.Files, portalSettings);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(new Exception("Error: You do not have write permission for folder"));
                }
            }
            return "File/s not uploaded successfully!";
        }

        private static string SaveFiles(HttpContext context, HttpFileCollection files, PortalSettings portalSettings)
        {
            string Response = string.Empty;
            int.TryParse(Options.FolderPath, out int folderid);
            if (folderid > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    try
                    {
                        IFolderInfo folderInfo = FolderManager.Instance.GetFolder(folderid);
                        if (folderInfo != null && (HasPermission(folderInfo, "WRITE") || HasPermission(folderInfo, "ADD")))
                        {
                            SaveFile(files, folderInfo, i, ref Response, Options.MaxSize, Options.FileTypes);
                        }
                        else
                        {
                            Response = "Error: You do not have write permission for folder " + (folderInfo != null ? folderInfo.FolderPath.TrimEnd('/') : string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                    if (Response.Contains("Error: You do not have write permission for folder"))
                    {
                        throw new Exception(Response);
                    }
                }
                return Response;
            }
            else
            {
                Options.FolderPath = Options.FolderPath.Replace("/'", "").Replace("'/", "");

                if (!FolderManager.Instance.FolderExists(portalSettings.PortalId, Options.FolderPath.Replace(portalSettings.HomeDirectoryMapPath, "").Replace("\\", "/")))
                {
                    FolderManager.Instance.AddFolder(portalSettings.PortalId, Options.FolderPath.Replace(portalSettings.HomeDirectoryMapPath, "").Replace("\\", "/"));
                }

                if (FolderManager.Instance.FolderExists(portalSettings.PortalId, Options.FolderPath.Replace(portalSettings.HomeDirectoryMapPath, "").Replace("\\", "/")))
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        try
                        {
                            IFolderInfo folderInfo = FolderManager.Instance.GetFolder(portalSettings.PortalId, PathUtils.Instance.GetRelativePath(portalSettings.PortalId, Options.FolderPath));
                            if (folderInfo != null && (HasPermission(folderInfo, "WRITE") || HasPermission(folderInfo, "ADD")))
                            {
                                SaveFile(files, folderInfo, i, ref Response, Options.MaxSize, Options.FileTypes);
                            }
                            else
                            {
                                Response = "Error: You do not have write permission for folder " + (folderInfo != null ? folderInfo.FolderPath.TrimEnd('/') : string.Empty);
                            }
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                        if (Response.Contains("Error: You do not have write permission for folder"))
                        {
                            throw new Exception(Response);
                        }
                    }
                    return Response;
                }
            }
            return "File/s not uploaded successfully!";
        }

        private static void SaveFile(HttpFileCollection files, IFolderInfo folderInfo, int i, ref string Response, int maxsize, string filetypes)
        {
            string[] FileTypes = filetypes.Split(',');
            HttpPostedFile file = files[i];
            string FileName = Security.ReplaceIllegalCharacters(file.FileName);
            string TempFileName = FileName;
            string FileExtension = FileName.Substring(FileName.LastIndexOf('.'));
            int FileSize = (file.ContentLength / 1024) / 1024;

            if (Security.IsAllowedExtension(FileExtension, FileTypes) && FileSize <= maxsize)
            {
                int count = 1;
            Find:
                if (FileManager.Instance.FileExists(folderInfo, TempFileName))
                {
                    TempFileName = FileName.Remove(FileName.Length - FileExtension.Length) + count + FileExtension;
                    count++;
                    goto Find;
                }
                else
                {
                    FileName = TempFileName;
                    IFileInfo fileInfo = FileManager.Instance.AddFile(folderInfo, FileName, file.InputStream);

                    if (Utils.IsImageVersionable(fileInfo))
                    {
                        file.InputStream.Seek(0, SeekOrigin.Begin);
                        CropImage(FileName, FileExtension, folderInfo, file.InputStream);
                    }

                    if (fileInfo != null)
                    {
                        Response = fileInfo.FileName + "fileid" + fileInfo.FileId;
                    }
                }
            }
        }

        private static byte[] ToByteArray(Stream inputStream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                inputStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static void CropImage(string FileName, string FileExtension, IFolderInfo FolderInfo, Stream FileStream)
        {
            IFolderInfo foldersizeinfo;
            if (!FolderManager.Instance.FolderExists(PortalSettings.Current.PortalId, FolderInfo.FolderPath + ".versions"))
            {
                foldersizeinfo = FolderManager.Instance.AddFolder(PortalSettings.Current.PortalId, FolderInfo.FolderPath + ".versions");
            }
            else
            {
                foldersizeinfo = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, FolderInfo.FolderPath + ".versions");
            }

            byte[] photoBytes = ToByteArray(FileStream);

            Image SrcImage = Image.FromStream(FileStream);
            int Width = SrcImage.Width;
            SrcImage.Dispose();

            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    string fileext = "JPEG";
                    ISupportedImageFormat format = new JpegFormat { Quality = 70 };
                    if (FileExtension.ToLower().Contains("png"))
                    {
                        format = new PngFormat { Quality = 70 };
                        fileext = "png";
                    }
                    FileName = FileName.Remove(FileName.Length - FileExtension.Length);
                    string fname = FileName + "_" + Width + "w." + fileext;
                    Size size = new Size(Width, 0);
                    Stream stream = new MemoryStream();
                    // Load, resize, set the format and quality and save an image.
                    imageFactory.Load(inStream)
                                .Resize(size)
                                .Format(format)
                                .Save(stream);
                    FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    fname = FileName + "_" + Width + "w.webp";
                    imageFactory.Load(stream)
                                .Format(new WebPFormat())
                                .Quality(70)
                                .Save(stream);
                    FileManager.Instance.AddFile(foldersizeinfo, fname, stream);

                    //360 image width
                    if (Width > 360)
                    {
                        fname = FileName + "_" + 360 + "w." + fileext;
                        size = new Size(360, 0);
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        fname = FileName + "_" + 360 + "w.webp";
                        imageFactory.Load(stream)
                                    .Format(new WebPFormat())
                                    .Quality(70)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                    }

                    //720 image width
                    if (Width > 720)
                    {
                        fname = FileName + "_" + 720 + "w." + fileext;
                        size = new Size(720, 0);
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        fname = FileName + "_" + 720 + "w.webp";
                        imageFactory.Load(stream)
                                    .Format(new WebPFormat())
                                    .Quality(70)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                    }

                    //1280 image width
                    if (Width > 1280)
                    {
                        fname = FileName + "_" + 1280 + "w." + fileext;
                        size = new Size(1280, 0);
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        fname = FileName + "_" + 1280 + "w.webp";
                        imageFactory.Load(stream)
                                    .Format(new WebPFormat())
                                    .Quality(70)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                    }

                    //1920 image width
                    if (Width > 1920)
                    {
                        fname = FileName + "_" + 1920 + "w." + fileext;
                        size = new Size(1920, 0);
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        fname = FileName + "_" + 1920 + "w.webp";
                        imageFactory.Load(stream)
                                    .Format(new WebPFormat())
                                    .Quality(70)
                                    .Save(stream);
                        FileManager.Instance.AddFile(foldersizeinfo, fname, stream);
                    }

                }
            }
        }

        private static void OnInitialization(HttpContext current, string Identifier, ModuleInfo Module, PortalSettings Portal, UserInfo User, bool isUploadAllowed, string FileTypes, int MaxSize, string AppName, int FolderId)
        {
            if (!string.IsNullOrEmpty(Identifier) && Module != null && Portal != null && User != null)
            {
                Options = new UploadOptions()
                {
                    AllowUpload = isUploadAllowed,
                    FileTypes = FileTypes,
                    MaxSize = MaxSize,
                    FolderPath = FolderId > 0 ? FolderId.ToString() : ParseServerPath(current, Portal, Module.ModuleID, AppName)
                };
            }
        }

        private static string ParseServerPath(HttpContext current, PortalSettings Portal, int ModuleId, string AppName)
        {
            if (current != null && current.Request != null && current.Request.Form != null && !string.IsNullOrEmpty(current.Request.Form["folder"]))
            {
                try
                {
                    int folderid = int.Parse(current.Request.Form["folder"].Replace("folders", ""));
                    return folderid.ToString();
                }
                catch { }
            }
            string MapPath = GetPortalDirectoryPath(Portal, ModuleId, AppName);
            MapPath = MapServerPath(MapPath);
            return MapPath;
        }

        private static string GetPortalDirectoryPath(PortalSettings Portal, int ModuleId, string AppName)
        {
            string MapPath = string.Empty;
            MapPath = Portal.HomeDirectory;
            if (!MapPath.EndsWith("/"))
            {
                MapPath = MapPath + "/";
            }

            MapPath = MapPath + AppName + "/" + ModuleId + "/";
            return MapPath;
        }

        private static string MapServerPath(string path)
        {
            string Path = string.Empty;
            try
            {
                Path = System.Web.Hosting.HostingEnvironment.MapPath(path);
            }
            catch { }
            return Path;
        }

        private static string GetEscapedFileName(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Replace(" ", "%20");
            }

            return fileName;
        }

        public static List<TreeView> GetUrlTypes()
        {
            List<TreeView> result = new List<TreeView>
            {
                new TreeView() { Text = "Relative URL - '/Images/MyImage.jpg'", Value = 0 },
                new TreeView() { Text = "Absolute URL - 'http://www.MyWebsite.com/Images/MyImage.jpg'", Value = 1 },
                new TreeView() { Text = "Relative Secured URL - '/LinkClick.aspx?fileticket=xyz'", Value = 2 },
                new TreeView() { Text = "Absolute Secured URL - 'http://www.MyWebsite.com/LinkClick.aspx?fileticket=xyz'", Value = 3 }
            };
            return result;
        }

        public static List<TreeView> GetFolders(int PortalId)
        {
            List<TreeView> result = new List<TreeView>();
            foreach (IFolderInfo item in FolderManager.Instance.GetFolders(PortalId))
            {
                string foldername = item.FolderPath;

                if (string.IsNullOrEmpty(foldername))
                {
                    foldername = "Site Root";
                }

                result.Add(new TreeView { Value = item.FolderID, Text = foldername });
            }
            return result;
        }

        private static string CleanDotsAtTheEndOfTheName(string name)
        {
            return name.Trim().TrimEnd('.', ' ');
        }

        private static bool IsInvalidName(string itemName)
        {
            Regex invalidFilenameChars = Utilities.RegexUtils.GetCachedRegex("[" + Regex.Escape(GetInvalidChars()) + "]");

            return invalidFilenameChars.IsMatch(itemName);
        }

        private static string GetInvalidChars()
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());

            foreach (char ch in Path.GetInvalidPathChars())
            {
                if (invalidChars.IndexOf(ch) == -1) // The ch does not exists
                {
                    invalidChars += ch;
                }
            }
            return invalidChars;
        }

        private static bool IsReservedName(string name)
        {
            string[] reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "CLOCK$" };
            return reservedNames.Contains(Path.GetFileNameWithoutExtension(name.ToUpperInvariant()));
        }
        public static bool HasBrowseOpenPerm(TreeView folder)
        {
            bool result = false;
            if (folder != null)
            {
                IFolderInfo folderInfo = FolderManager.Instance.GetFolder(folder.Value);
                if (folderInfo != null)
                {
                    string perm = string.Empty;
                    foreach (PermissionInfoBase pinfo in folderInfo.FolderPermissions.ToList())
                    {
                        if (pinfo.RoleID == -1)
                        {
                            if (pinfo.PermissionKey == "READ" && pinfo.AllowAccess)
                            {
                                perm += pinfo.PermissionKey + "|";
                            }
                            else if (pinfo.PermissionKey == "BROWSE" && pinfo.AllowAccess)
                            {
                                perm += pinfo.PermissionKey + "|";
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(perm))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private static bool IsImageFolder(CommonLibraryRepo db, int FolderID)
        {
            bool result = false;
            if (db != null && db.ExecuteScalar<int>(BrowseUploadScript.IsImageFolder(FolderID)) > 0)
            {
                result = true;
            }
            else
            {
                using (db = new CommonLibraryRepo())
                {
                    if (db != null && db.ExecuteScalar<int>(BrowseUploadScript.IsImageFolder(FolderID)) > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private static string GetFileType(IFileInfo file)
        {
            string imagefolderpath = Url.ResolveUrl("~/DesktopModules/Vanjaro/Common/Resources/Images/");
            string result = imagefolderpath + "fa-file.jpg";
            if (file != null)
            {
                if (file.Extension.ToLower().Contains("doc"))
                {
                    result = imagefolderpath + "fa-file-word.jpg";
                }
                else if (file.Extension.ToLower().Contains("avi") || file.Extension.ToLower().Contains("mpg") || file.Extension.ToLower().Contains("mpeg") || file.Extension.ToLower().Contains("wmv") || file.Extension.ToLower().Contains("mov") || file.Extension.ToLower().Contains("mp4") || file.Extension.ToLower().Contains("webm"))
                {
                    result = imagefolderpath + "fa-file-video.jpg";
                }
                else if (file.Extension.ToLower().Contains("ppt"))
                {
                    result = imagefolderpath + "fa-file-powerpoint.jpg";
                }
                else if (file.Extension.ToLower().Contains("pdf"))
                {
                    result = imagefolderpath + "fa-file-pdf.jpg";
                }
                else if (file.Extension.ToLower().Contains("xls"))
                {
                    result = imagefolderpath + "fa-file-excel.jpg";
                }
                else if (file.Extension.ToLower().Contains("mp3") || file.Extension.Contains("wav"))
                {
                    result = imagefolderpath + "fa-file-audio.jpg";
                }
                else if (file.Extension.ToLower().Contains("zip") || file.Extension.ToLower().Contains("rar"))
                {
                    result = imagefolderpath + "fa-file-archive.jpg";
                }
                else if (file.Extension.ToLower().Contains("csv"))
                {
                    result = imagefolderpath + "fa-file-csv.jpg";
                }
                else if (file.Extension.ToLower().Contains("css") || file.Extension.ToLower().Contains("xml") || file.Extension.ToLower().Contains("xsl") || file.Extension.ToLower().Contains("xsd"))
                {
                    result = imagefolderpath + "fa-file-code.jpg";
                }
                else if (file.Extension.ToLower().Contains("jpg") || file.Extension.ToLower().Contains("jpeg") || file.Extension.ToLower().Contains("gif") || file.Extension.ToLower().Contains("bmp") || file.Extension.ToLower().Contains("png") || file.Extension.ToLower().Contains("svg") || file.Extension.ToLower().Contains("ico"))
                {
                    result = FileManager.Instance.GetUrl(file);
                }
            }
            return result;
        }

        public static bool IsImage(string Type, int FolderID, CommonLibraryRepo db)
        {
            if (string.IsNullOrEmpty(Type))
            {
                return IsImageFolder(db, FolderID);
            }
            else if (Type.ToLower() == "image")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
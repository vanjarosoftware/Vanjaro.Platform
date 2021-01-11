using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Url.FriendlyUrl;
using DotNetNuke.Web.Components.Controllers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Vanjaro.Common.Components;
using Vanjaro.Common.Factories;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.UXManager.Library.Factories;

namespace Vanjaro.UXManager.Library
{
    public static partial class Managers
    {
        public class PageManager
        {
            public static int AddModule(PortalSettings PortalSettings, int VisualizerID, int DesktopModuleId)
            {
                int tabModuleId = Null.NullInteger;
                if (TabPermissionController.CanAddContentToPage())
                {
                    foreach (ModuleDefinitionInfo objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(DesktopModuleId).Values)
                    {
                        ModuleInfo objModule = new ModuleInfo();
                        objModule.Initialize(PortalSettings.ActiveTab.PortalID);
                        objModule.PortalID = PortalSettings.ActiveTab.PortalID;
                        objModule.TabID = PortalSettings.ActiveTab.TabID;
                        objModule.ModuleOrder = -1;
                        objModule.ModuleTitle = objModuleDefinition.FriendlyName;
                        objModule.PaneName = "ContentPane";
                        objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                        objModule.ContainerSrc = "[g]containers/vanjaro/base.ascx";
                        objModule.DisplayTitle = true;

                        if (objModuleDefinition.DefaultCacheTime > 0)
                        {
                            objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                            if (PortalSettings.DefaultModuleId > Null.NullInteger && PortalSettings.DefaultTabId > Null.NullInteger)
                            {
                                ModuleInfo defaultModule = ModuleController.Instance.GetModule(PortalSettings.DefaultModuleId, PortalSettings.DefaultTabId, true);
                                if ((defaultModule != null))
                                {
                                    objModule.CacheTime = defaultModule.CacheTime;
                                }
                            }
                        }
                        ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, 0);
                        if (PortalSettings.ContentLocalizationEnabled)
                        {
                            Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.PortalId);
                            //set the culture of the module to that of the tab
                            TabInfo tabInfo = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(objModule.TabID, PortalSettings.PortalId, false);
                            objModule.CultureCode = tabInfo != null ? tabInfo.CultureCode : defaultLocale.Code;
                        }
                        else
                        {
                            objModule.CultureCode = Null.NullString;
                        }

                        objModule.AllTabs = false;
                        ModuleController.Instance.AddModule(objModule);
                        if (tabModuleId == Null.NullInteger)
                        {
                            tabModuleId = objModule.ModuleID;
                        }
                    }
                    if (VisualizerID > 0 && tabModuleId > 0)
                    {
                        dynamic visuInstance = GetInstance("VisualizerFactory");
                        if (visuInstance != null)
                        {
                            visuInstance.GetMethod("ActivateVisualizer", new Type[] { typeof(int), typeof(int) }).Invoke(null, new object[] { VisualizerID, tabModuleId });
                        }
                    }
                }
                return tabModuleId;
            }

            public static List<PageItem> GetPages(int PortalId)
            {
                List<PageItem> result = new List<PageItem>
                {
                    new PageItem() { Text = "Select Page", Value = 0 }
                };
                foreach (TreeView page in BrowseUploadFactory.GetDnnPages(PortalId))
                {
                    result.Add(new PageItem() { Text = page.Text, Value = page.Value });
                    if (page.children != null && page.children.Count > 0)
                    {
                        BindChildPages(result, page.children, "-");
                    }
                }
                return result;
            }

            private static void BindChildPages(List<PageItem> result, List<TreeView> children, string prefix)
            {
                foreach (TreeView page in children)
                {
                    result.Add(new PageItem() { Text = prefix + " " + page.Text, Value = page.Value });
                    if (page.children != null && page.children.Count > 0)
                    {
                        BindChildPages(result, page.children, prefix + ".");
                    }
                }
            }

            public static string GetPageUrl(PortalSettings PortalSettings, int TabID)
            {
                TabInfo tab = TabController.Instance.GetTab(TabID, PortalSettings.PortalId, true);
                if (tab != null)
                {
                    string flaggedUrl = Globals.FriendlyUrl(tab, Globals.ApplicationURL(TabID), PortalSettings as IPortalSettings);
                    return Globals.ResolveUrl(Regex.Replace(flaggedUrl, string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, Globals.GetDomainName(HttpContext.Current.Request, true)), "~", RegexOptions.IgnoreCase));
                }
                return string.Empty;
            }


            private static bool IgnorePackage(string PackageName)
            {
                //Add specific name of ignore package name
                List<string> Packages = new List<string>
                {
                    "Authentication",
                    "Registration",
                    "SearchResults",
                    "ViewProfile",
                    "Extensions",
                    "ModuleCreator",
                    "Console"
                };
                if (Packages.Contains(PackageName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static dynamic GetApps(int PortalID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.DesktopModules, PortalID);
                List<ModuleDefDTO> ModuleDefDTOs = CacheFactory.Get(CacheKey);
                if (ModuleDefDTOs == null)
                {
                    ModuleDefDTOs = new List<ModuleDefDTO>();
                    foreach (KeyValuePair<string, PortalDesktopModuleInfo> DesktopModule in ControlBarController.Instance.GetCategoryDesktopModules(PortalID, "All", ""))
                    {
                        if (!IgnorePackage(DesktopModule.Value.DesktopModule.ModuleName))
                        {
                            ModuleDefDTOs.Add(new ModuleDefDTO
                            {
                                ModuleID = DesktopModule.Value.DesktopModuleID,
                                ModuleName = DesktopModule.Key,
                                ModuleImage = GetDeskTopModuleImage(PortalID, DesktopModule.Value.DesktopModuleID)
                            });
                        }
                    }
                    try
                    {
                        dynamic visuInstance = GetInstance("VisualizerFactory");
                        if (visuInstance != null)
                        {
                            dynamic visualizers = visuInstance.GetMethod("GetAllByPortalID", new Type[] { typeof(int) }).Invoke(null, new object[] { PortalID });
                            foreach (dynamic visualizer in visualizers)
                            {
                                ModuleDefDTOs.Add(new ModuleDefDTO
                                {
                                    UniqueID = visualizer.VisualizerID,
                                    ModuleID = ModuleDefDTOs.Where(c => c.ModuleName == "Live Visualizer").FirstOrDefault().ModuleID,
                                    ModuleName = visualizer.Name,
                                    ModuleImage = visualizer.ImageIcon.Url
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Managers.ExceptionManage.LogException(ex);
                    }
                    CacheFactory.Set(CacheKey, ModuleDefDTOs);
                }
                return ModuleDefDTOs;
            }

            public static List<dynamic> GetParentPages(PortalSettings portalSettings)
            {
                List<dynamic> RootCategory = new List<dynamic>();
                List<TabInfo> AvailablePages = Core.Managers.PageManager.GetPageList(PortalSettings.Current).ToList();
                foreach (TabInfo page in AvailablePages)
                {
                    dynamic tab = new ExpandoObject();
                    tab.DisableLink = (page.TabPermissions.Where(t => t != null && t.RoleID == -1 && t.AllowAccess == true).FirstOrDefault() != null) ? false : true;
                    tab.TabName = page.TabName;
                    tab.TabID = page.TabID;
                    RootCategory.Add(tab);
                    RootCategory.AddRange(GetPagesChildPages(page.TabID, "-", portalSettings));
                }

                return RootCategory;
            }

            private static List<dynamic> GetPagesChildPages(int TabId, string NamePrefix, PortalSettings portalSettings)
            {
                List<dynamic> ChildPages = new List<dynamic>();
                List<TabInfo> PageChildrens = TabController.GetTabsByParent(TabId, portalSettings.PortalId).Where(a => a.IsDeleted == false && a.DisableLink == false).ToList();
                foreach (TabInfo item in PageChildrens)
                {
                    dynamic childTab = new ExpandoObject();
                    childTab.DisableLink = (item.TabPermissions.Where(t => t != null && t.RoleID == -1 && t.AllowAccess == true).FirstOrDefault() != null) ? false : true;
                    childTab.TabName = NamePrefix + " " + item.TabName.TrimStart('-');
                    childTab.TabID = item.TabID;
                    ChildPages.Add(childTab);
                    ChildPages.AddRange(GetPagesChildPages(item.TabID, NamePrefix + "-", portalSettings));
                }

                return ChildPages;
            }

            private static dynamic GetInstance(string Name)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Instance, Name);
                dynamic result = CacheFactory.Get(CacheKey);
                if (result == null)
                {
                    try
                    {
                        string binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith("Mandeeps.DNN.Modules.LiveVisualizer.dll")).SingleOrDefault();
                        IEnumerable<Type> DllFile = from t in Assembly.LoadFrom(binAssemblies).GetTypes()
                                                    where t.Name == Name
                                                    select t;
                        result = DllFile.FirstOrDefault();
                        CacheFactory.Set(CacheKey, result);
                    }
                    catch (Exception) { }
                }
                return result;
            }

            private static string GetDeskTopModuleImage(int PortalID, int DesktopModuleID)
            {
                Dictionary<int, DesktopModuleInfo> portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalID);
                IList<PackageInfo> packages = PackageController.Instance.GetExtensionPackages(PortalID);

                string imageUrl =
                        (from pkgs in packages
                         join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
                         where portMods.Value.DesktopModuleID == DesktopModuleID
                         select pkgs.IconFile).FirstOrDefault();

                imageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + "icon_extensions_32px.png" : imageUrl;
                return System.Web.VirtualPathUtility.ToAbsolute(imageUrl);
            }


            public static string GetCurrentTabUrl(PortalSettings pS, string QueryParameters = null)
            {
                string Language = Thread.CurrentThread.CurrentCulture.ToString();
                if (pS != null && Thread.CurrentThread.CurrentCulture.ToString() == pS.DefaultLanguage)
                    Language = null;

                HttpContext Context = HttpContext.Current;

                string path = "~/Default.aspx";

                if (!string.IsNullOrEmpty(Language))
                    pS.CultureCode = Language;
                else
                    pS.CultureCode = pS.DefaultLanguage;

                if (string.IsNullOrEmpty(QueryParameters))
                    QueryParameters = string.Empty;

                if (Context.Request.QueryString.AllKeys.Count() > 0)
                {
                    string param = string.Empty;
                    foreach (string q in Context.Request.QueryString.AllKeys)
                    {
                        if ((!string.IsNullOrEmpty(q) && q.ToLower() != "language" && q.ToLower() != "tabid") || string.IsNullOrEmpty(q))
                        {
                            if (string.IsNullOrEmpty(q))
                                param += ("&" + Context.Request.QueryString[q]);
                            else
                                param += ("&" + q + "=" + Context.Request.QueryString[q]);
                        }
                    }
                    QueryParameters = param + QueryParameters;
                }

                path = AppendQueryParameters(pS, pS.ActiveTab.TabID, path, QueryParameters, Language);
                return FriendlyUrlProvider.Instance().FriendlyUrl(pS.ActiveTab, path);
            }

            private static string AppendQueryParameters(PortalSettings pS, int TabID, string URL, string QueryParameters, string Language)
            {
                if (true)
                {
                    URL += "&TabId=" + TabID;

                    if (LocaleController.Instance.GetLocales(pS.PortalId).Count() > 1)
                        URL += "&language=" + Language;
                }
                URL += QueryParameters;
                URL = ReplaceFirst(URL, "&", "?");
                return URL;
            }

            private static string ReplaceFirst(string text, string search, string replace)
            {
                int pos = text.IndexOf(search);
                if (pos < 0)
                {
                    return text;
                }
                return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
            }


        }
    }
}
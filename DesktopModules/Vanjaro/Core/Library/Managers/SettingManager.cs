using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Data.Scripts;
using Vanjaro.Core.Entities;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class SettingManager
        {

            public static string GetHostSetting(string Name, bool Secure, string defaultValue = "")
            {
                HostController hostController = new HostController();
                if (Secure)
                    return hostController.GetEncryptedString(Name, Config.GetDecryptionkey());
                return hostController.GetString(Name,defaultValue);
            }
            public static bool GetHostSettingAsBoolean(string Name, bool defaultValue = false)
            {
                HostController hostController = new HostController();
                return hostController.GetBoolean(Name, defaultValue);
            }
            public static void UpdateHostSetting(string Name, string Value, bool Secure)
            {
                HostController hostController = new HostController();
                if (Secure)
                    hostController.UpdateEncryptedString(Name, Value, Config.GetDecryptionkey());
                else
                    hostController.Update(Name, Value, true);
            }
            public static string GetPortalSetting(string Name, bool Secure, string defaultValue = "")
            {
                if (Secure)
                    return PortalController.GetEncryptedString(Name, PortalController.Instance.GetCurrentSettings().PortalId, Config.GetDecryptionkey());
                return PortalController.GetPortalSetting(Name, PortalController.Instance.GetCurrentSettings().PortalId, defaultValue);
            }
            public static bool GetPortalSettingAsBoolean(string Name, bool defaultValue = false)
            {
                return PortalController.GetPortalSettingAsBoolean(Name, PortalController.Instance.GetCurrentSettings().PortalId, defaultValue);
            }
            public static void UpdatePortalSetting(string Name, string Value, bool Secure)
            {
                if (Secure)
                    PortalController.UpdateEncryptedString(PortalController.Instance.GetCurrentSettings().PortalId, Name, Value, Config.GetDecryptionkey());
                else
                    PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentSettings().PortalId, Name, Value, true);
            }           

            public static void UpdateValue(int PortalID, int TabID, string Identifier, string Name, string Value)
            {
                SettingFactory.UpdateValue(PortalID, TabID, Identifier, Name, Value);
            }

            public static string GetValue(int PortalID, int TabID, string Identifier, string Name, List<AngularView> Views)
            {
                return SettingFactory.GetValue(PortalID, TabID, Identifier, Name, Views);
            }

            public static List<Setting> GetSettings(int PortalID, int TabID, string Identifier)
            {
                return SettingFactory.GetSettings(PortalID, TabID, Identifier);
            }

            public static void ApplyingSettings(bool ApplyTemplates, int? PortalID = null)
            {
                int Index = 0;
                foreach (PortalInfo pinfo in PortalController.Instance.GetPortals())
                {
                    try
                    {
                        //Should only happen for distribution
                        if (Index == 0 && IsDistribution(pinfo.PortalID) && !PortalID.HasValue)
                        {
                            DeleteTabs();
                            MoveFavicon();
                            DeleteDefaultMemberProfileProperties();
                        }


                        if ((PortalID.HasValue && PortalID.Value == pinfo.PortalID) || !PortalID.HasValue)
                        {
                            DoIUpgradeable(pinfo, ApplyTemplates);
                        }
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }

                    Index++;

                }
            }

            public static void ApplyingSettings(string Version)
            {

                switch (Version)
                {
                    case "01.00.01":
                        #region Add webp extension
                        string fileExtension = HostController.Instance.GetString("FileExtensions");
                        if (!string.IsNullOrEmpty(fileExtension) && !fileExtension.ToLower().Split(',').Contains("webp"))
                        {
                            var fextn = fileExtension.Split(',').ToList();
                            fextn.Add("webp");
                            HostController.Instance.Update("FileExtensions", string.Join(",", fextn));
                        }

                        //Default End UserExtension Whitelist
                        string defaultEndUserExtensionWhitelist = HostController.Instance.GetString("DefaultEndUserExtensionWhitelist");
                        if (!string.IsNullOrEmpty(defaultEndUserExtensionWhitelist) && !defaultEndUserExtensionWhitelist.ToLower().Split(',').Contains("webp"))
                        {
                            var fextn = defaultEndUserExtensionWhitelist.Split(',').ToList();
                            fextn.Add("webp");
                            HostController.Instance.Update("DefaultEndUserExtensionWhitelist", string.Join(",", fextn));
                        }
                        #endregion
                        break;
                    case "01.00.02":
                        if (!IsDistribution(PortalController.Instance.GetCurrentSettings().PortalId))
                        {
                            HostController.Instance.Update("DisableEditBar", "False");
                        }
                        break;
                }
            }

            private static void MoveFavicon()
            {
                #region Copy favicon
                string CorefaviconPath = HttpContext.Current.Request.PhysicalApplicationPath + "DesktopModules\\Vanjaro\\Core\\Library\\favicon.ico";
                string FaviconPath = HttpContext.Current.Request.PhysicalApplicationPath + "favicon.ico";
                if (File.Exists(FaviconPath))
                {
                    File.Delete(FaviconPath);
                }

                if (File.Exists(CorefaviconPath))
                {
                    File.Copy(CorefaviconPath, FaviconPath);
                }
                #endregion
            }

            private static void DeleteTabs()
            {
                TabInfo FileManagerTab = TabController.Instance.GetTabByName("File Manager", Null.NullInteger);
                if (FileManagerTab != null)
                {
                    TabController.Instance.DeleteTab(FileManagerTab.TabID, Null.NullInteger);
                }

                TabInfo SuperuserTab = TabController.Instance.GetTabByName("Superuser Accounts", Null.NullInteger);
                if (SuperuserTab != null)
                {
                    TabController.Instance.DeleteTab(SuperuserTab.TabID, Null.NullInteger);
                }

                TabInfo HostTab = TabController.Instance.GetTabByName("Host", Null.NullInteger);
                if (HostTab != null)
                {
                    TabController.Instance.DeleteTab(HostTab.TabID, Null.NullInteger);
                }
            }

            public static string GetVersion()
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            public static void Copy(string sourceDirectory, string targetDirectory)
            {
                DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
                DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);
                CopyAll(diSource, diTarget);
            }

            private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
            {
                if (!Directory.Exists(target.FullName))
                {
                    Directory.CreateDirectory(target.FullName);
                }

                foreach (System.IO.FileInfo fi in source.GetFiles())
                {
                    if (!File.Exists(Path.Combine(target.FullName, fi.Name)))
                    {
                        fi.CopyTo(Path.Combine(target.FullName, fi.Name));
                    }
                    else
                    {
                        string FileName = Path.Combine(target.FullName, fi.Name);
                        File.Delete(FileName);
                        fi.CopyTo(Path.Combine(target.FullName, fi.Name));

                    }
                }

                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir =
                        target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
            private static void DoIUpgradeable(PortalInfo pinfo, bool ApplyTemplates)
            {
                UserInfo uInfo = UserController.Instance.GetCurrentUserInfo();

                IFolderInfo fi = FolderManager.Instance.GetFolder(pinfo.PortalID, "Images/");

                #region Copy Vthemes in portal folder

                string BaseEditorFolder = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.GetCurrent(pinfo.PortalID).Name + "/editor");
                Copy(BaseEditorFolder, BaseEditorFolder.Replace("_default", pinfo.PortalID.ToString()));
                try
                {
                    ThemeManager.ProcessScss(pinfo.PortalID);
                }
                catch (Exception ex) { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); }

                string UXManagervDefaultPath = HttpContext.Current.Request.PhysicalApplicationPath + "DesktopModules\\Vanjaro\\Core\\Library\\Images";
                if (Directory.Exists(UXManagervDefaultPath) && fi != null)
                {
                    DirectoryInfo source = new DirectoryInfo(UXManagervDefaultPath);
                    foreach (System.IO.FileInfo _file in source.GetFiles())
                    {
                        if (!FileManager.Instance.FileExists(fi, _file.Name))
                        {
                            FileStream stream = new FileStream(_file.FullName, FileMode.Open, FileAccess.Read);
                            FileManager.Instance.AddFile(fi, _file.Name, stream);
                        }
                    }
                }
                #endregion

                bool IsVanjaroInstalled = PortalController.Instance.GetPortalSettings(pinfo.PortalID).ContainsKey("IsVanjaroInstalled");

                if (IsDistribution(pinfo.PortalID))
                {
                    HostController.Instance.Update("ControlPanel", "DesktopModules/Vanjaro/UXManager/Library/Base.ascx");

                    #region Add Signup
                    TabController controller = new TabController();
                    if (TabController.Instance.GetTabByName("Signup", pinfo.PortalID) == null)
                    {
                        TabInfo NewSignupTab = new TabInfo { PortalID = pinfo.PortalID, TabName = "Signup", IsVisible = false };
                        TabController.Instance.AddTab(NewSignupTab);
                    }
                    #endregion

                    #region Removing Subscribers

                    RoleInfo rInfo = RoleController.Instance.GetRoleByName(pinfo.PortalID, "Subscribers");
                    if (rInfo != null)
                    {
                        RoleController.Instance.DeleteRole(rInfo);
                    }

                    #endregion

                    if (ApplyTemplates)
                    {
                        List<Layout> pageLayouts = GetLayouts(pinfo);
                        ApplyDefaultLayouts(pinfo, uInfo, pageLayouts);
                        pinfo.LogoFile = fi.FolderPath + "vanjaro_logo.png";
                    }

                    #region Update SEO Settings
                    List<StringValue> SeoSettings = new List<StringValue>
                    {
                        new StringValue { Text = FriendlyUrlSettings.ReplaceSpaceWithSetting, Value = "-" },
                        new StringValue { Text = FriendlyUrlSettings.DeletedTabHandlingTypeSetting, Value = "Do404Error" },
                        new StringValue { Text = FriendlyUrlSettings.ForceLowerCaseSetting, Value = "Y" },
                        new StringValue { Text = FriendlyUrlSettings.RedirectUnfriendlySetting, Value = "Y" },
                        new StringValue { Text = FriendlyUrlSettings.RedirectMixedCaseSetting, Value = "True" },
                        new StringValue { Text = FriendlyUrlSettings.UsePortalDefaultLanguageSetting, Value = "True" },
                        new StringValue { Text = FriendlyUrlSettings.AutoAsciiConvertSetting, Value = "False" },
                        new StringValue { Text = "DefaultIconLocation", Value = "icons/Font Awesome" },
                    };
                    UpdatePortalSettings(SeoSettings, pinfo.PortalID, uInfo.UserID);
                    #endregion                    
                }
                if (!IsVanjaroInstalled && ApplyTemplates)
                {
                    #region Signin Tab
                    if (pinfo.LoginTabId == -1)
                    {
                        if (TabController.Instance.GetTabByName("Signin", pinfo.PortalID) == null)
                        {
                            TabController tab = new TabController();

                            TabInfo NewSigninTab = new TabInfo { PortalID = pinfo.PortalID, TabName = "Signin", IsVisible = false };
                            int NewSigninTabID = TabController.Instance.AddTab(NewSigninTab);
                            NewSigninTab = tab.GetTab(NewSigninTabID, pinfo.PortalID);
                            if (NewSigninTab.TabPermissions.Where(t => t != null && t.RoleName == "All Users").FirstOrDefault() != null)
                            {
                                foreach (TabPermissionInfo p in NewSigninTab.TabPermissions.Where(t => t != null && t.RoleName == "All Users"))
                                {
                                    if (p.PermissionKey.ToLower() == "view")
                                    {
                                        p.AllowAccess = true;
                                    }
                                    else
                                    {
                                        p.AllowAccess = false;
                                    }
                                }
                            }
                            else
                            {
                                NewSigninTab.TabPermissions.Add(new TabPermissionInfo
                                {
                                    PermissionID = 3,
                                    TabID = NewSigninTab.TabID,
                                    AllowAccess = true,
                                    RoleID = -1,
                                    RoleName = "All Users",
                                });
                            }
                            TabPermissionController.SaveTabPermissions(NewSigninTab);
                            NewSigninTab.SkinSrc = "[g]skins/vanjaro/base.ascx";
                            NewSigninTab.ContainerSrc = "[g]containers/vanjaro/base.ascx";
                            tab.UpdateTab(NewSigninTab);
                        }

                        List<Layout> pageLayouts = GetLayouts(pinfo);
                        TabInfo SigninTab = TabController.Instance.GetTabByName("Signin", pinfo.PortalID);
                        Layout Signinlayout = pageLayouts.Where(a => a.Name == "Signin").FirstOrDefault();
                        PortalSettings portalSettings = new PortalSettings(pinfo);
                        Layout homelayout = pageLayouts.Where(a => a.Name == "Home").FirstOrDefault();
                        if (SigninTab != null && Signinlayout != null && portalSettings != null)
                        {
                            ProcessBlocks(pinfo.PortalID, homelayout.Blocks);

                            if (portalSettings.ActiveTab == null)
                            {
                                portalSettings.ActiveTab = new TabInfo();
                            }

                            portalSettings.ActiveTab.TabID = SigninTab.TabID;
                            Dictionary<string, object> LayoutData = new Dictionary<string, object>
                            {
                                ["IsPublished"] = false,
                                ["Comment"] = string.Empty,
                                ["gjs-assets"] = string.Empty,
                                ["gjs-css"] = Managers.PageManager.DeTokenizeLinks(Signinlayout.Style.ToString(), pinfo.PortalID),
                                ["gjs-html"] = Managers.PageManager.DeTokenizeLinks(Signinlayout.Content.ToString(), pinfo.PortalID),
                                ["gjs-components"] = Managers.PageManager.DeTokenizeLinks(Signinlayout.ContentJSON.ToString(), pinfo.PortalID),
                                ["gjs-styles"] = Managers.PageManager.DeTokenizeLinks(Signinlayout.StyleJSON.ToString(), pinfo.PortalID)
                            };
                            Core.Managers.PageManager.Update(portalSettings, LayoutData);


                            Pages Page = Managers.PageManager.GetPages(SigninTab.TabID).OrderByDescending(o => o.Version).FirstOrDefault();

                            if (Page != null && uInfo != null)
                            {
                                WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                                Page.Version = 1;
                                Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                                Page.IsPublished = true;
                                Page.PublishedBy = uInfo.UserID;
                                Page.PublishedOn = DateTime.UtcNow;
                                PageFactory.Update(Page, uInfo.UserID);
                            }
                        }
                        if (SigninTab != null)
                        {
                            Core.Managers.LoginManager.AddUpdateLoginModule(SigninTab.TabID, pinfo.PortalID);
                        }

                        pinfo.LoginTabId = SigninTab != null && !SigninTab.IsDeleted ? SigninTab.TabID : Null.NullInteger;
                    }
                    #endregion

                    #region Update Portal Settings
                    IFileInfo file;
                    if (fi != null)
                    {
                        if (FileManager.Instance.FileExists(fi, "vanjaro_social.png") && !PortalController.Instance.GetPortalSettings(pinfo.PortalID).ContainsKey("SocialSharingLogo"))
                        {
                            file = FileManager.Instance.GetFile(fi, "vanjaro_social.png");
                            PortalController.UpdatePortalSetting(pinfo.PortalID, "SocialSharingLogo", "FileID=" + file.FileId, true, pinfo.CultureCode);
                        }

                        if (FileManager.Instance.FileExists(fi, "vanjaro_home.png") && !PortalController.Instance.GetPortalSettings(pinfo.PortalID).ContainsKey("HomeScreenIcon"))
                        {
                            file = FileManager.Instance.GetFile(fi, "vanjaro_home.png");
                            PortalController.UpdatePortalSetting(pinfo.PortalID, "HomeScreenIcon", "FileID=" + file.FileId, true, pinfo.CultureCode);
                        }
                    }
                    PortalController.Instance.UpdatePortalInfo(pinfo);
                    List<StringValue> SettingNameValue = new List<StringValue>
                {
                    new StringValue { Text = "DNN_Enabled", Value = "False" },
                    new StringValue { Text = "Registration_UseEmailAsUserName", Value = "True" },
                    new StringValue { Text = "ClientResourcesManagementMode", Value = "p" },
                    new StringValue { Text = DotNetNuke.Web.Client.ClientResourceSettings.EnableCompositeFilesKey, Value = "True" },
                    new StringValue { Text = DotNetNuke.Web.Client.ClientResourceSettings.MinifyCssKey, Value = "True" },
                    new StringValue { Text = DotNetNuke.Web.Client.ClientResourceSettings.MinifyJsKey, Value = "True" },
                    new StringValue { Text = DotNetNuke.Web.Client.ClientResourceSettings.OverrideDefaultSettingsKey, Value = "True" },
                };
                    int CrmVersion = Host.CrmVersion + 1;
                    SettingNameValue.Add(new StringValue { Text = DotNetNuke.Web.Client.ClientResourceSettings.VersionKey, Value = CrmVersion.ToString() });
                    UpdatePortalSettings(SettingNameValue, pinfo.PortalID, uInfo.UserID);
                    #endregion
                }

                #region Delete Unnecessary Files
                string LayoutPath = HttpContext.Current.Server.MapPath("~/Portals/" + pinfo.PortalID + "/vThemes/" + Core.Managers.ThemeManager.GetCurrent(pinfo.PortalID).Name + "/templates");
                if (Directory.Exists(LayoutPath))
                    Directory.Delete(LayoutPath, true);
                #endregion

                #region Update ResetPassword module control
                ModuleControlInfo passwordResetModuleControlInfo = ModuleControlController.GetModuleControlByControlKey("PasswordReset", Null.NullInteger);
                if (passwordResetModuleControlInfo != null)
                {
                    passwordResetModuleControlInfo.ControlSrc = "DesktopModules/Vanjaro/Core/Providers/Authentication/PasswordReset.ascx";
                }

                ModuleControlController.UpdateModuleControl(passwordResetModuleControlInfo);
                ModuleControlInfo registerModuleControlInfo = ModuleControlController.GetModuleControlByControlKey("Register", Null.NullInteger);
                registerModuleControlInfo.ControlSrc = "DesktopModules/Vanjaro/Core/Providers/Authentication/Register.ascx";
                if (registerModuleControlInfo != null)
                {
                    ModuleControlController.UpdateModuleControl(registerModuleControlInfo);
                }
                #endregion


                if (IsDistribution(pinfo.PortalID))
                    SoftDeleteModule(pinfo.PortalID, Components.Constants.SearchResult);

                if (!IsVanjaroInstalled)
                    PortalController.UpdatePortalSetting(pinfo.PortalID, "IsVanjaroInstalled", "-1");

                if (fi != null)
                    UpdateValue(pinfo.PortalID, 0, "security_settings", "Picture_DefaultFolder", fi.FolderID.ToString());
            }
            public static void ApplyDefaultLayouts(PortalInfo pinfo, UserInfo uInfo, List<Layout> pageLayouts)
            {
                #region Applying Home and Signup layout

                TabInfo HomeTab = TabController.Instance.GetTabByName("Home", pinfo.PortalID);
                Layout homelayout = pageLayouts.Where(a => a.Name == "Home").FirstOrDefault();
                PortalSettings portalSettings = new PortalSettings(pinfo);
                if (HomeTab != null && homelayout != null && portalSettings != null)
                {
                    ProcessBlocks(pinfo.PortalID, homelayout.Blocks);

                    if (portalSettings.ActiveTab == null)
                    {
                        portalSettings.ActiveTab = new TabInfo();
                    }

                    portalSettings.ActiveTab.TabID = HomeTab.TabID;
                    PortalController.UpdatePortalSetting(pinfo.PortalID, "Redirect_AfterLogin", HomeTab.TabID.ToString(), false, portalSettings.CultureCode, false);
                    Dictionary<string, object> LayoutData = new Dictionary<string, object>
                    {
                        ["IsPublished"] = false,
                        ["Comment"] = string.Empty,
                        ["gjs-assets"] = string.Empty,
                        ["gjs-css"] = Managers.PageManager.DeTokenizeLinks(homelayout.Style.ToString(), pinfo.PortalID),
                        ["gjs-html"] = Managers.PageManager.DeTokenizeLinks(homelayout.Content.ToString(), pinfo.PortalID),
                        ["gjs-components"] = Managers.PageManager.DeTokenizeLinks(homelayout.ContentJSON.ToString(), pinfo.PortalID),
                        ["gjs-styles"] = Managers.PageManager.DeTokenizeLinks(homelayout.StyleJSON.ToString(), pinfo.PortalID)
                    };
                    Core.Managers.PageManager.Update(portalSettings, LayoutData);

                    Pages Page = Managers.PageManager.GetPages(HomeTab.TabID).OrderByDescending(o => o.Version).FirstOrDefault();

                    if (Page != null && uInfo != null)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                        Page.Version = 1;
                        Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                        Page.IsPublished = true;
                        Page.PublishedBy = uInfo.UserID;
                        Page.PublishedOn = DateTime.UtcNow;
                        PageFactory.Update(Page, uInfo.UserID);
                    }
                    pinfo.HomeTabId = HomeTab != null && !HomeTab.IsDeleted ? HomeTab.TabID : Null.NullInteger;

                }

                TabInfo SignUpTab = TabController.Instance.GetTabByName("Signup", pinfo.PortalID);
                Layout Signuplayout = pageLayouts.Where(a => a.Name == "Signup").FirstOrDefault();
                if (SignUpTab != null && Signuplayout != null && portalSettings != null)
                {
                    ProcessBlocks(pinfo.PortalID, homelayout.Blocks);

                    if (portalSettings.ActiveTab == null)
                    {
                        portalSettings.ActiveTab = new TabInfo();
                    }

                    portalSettings.ActiveTab.TabID = SignUpTab.TabID;
                    Dictionary<string, object> LayoutData = new Dictionary<string, object>
                    {
                        ["IsPublished"] = false,
                        ["Comment"] = string.Empty,
                        ["gjs-assets"] = string.Empty,
                        ["gjs-css"] = Managers.PageManager.DeTokenizeLinks(Signuplayout.Style.ToString(), pinfo.PortalID),
                        ["gjs-html"] = Managers.PageManager.DeTokenizeLinks(Signuplayout.Content.ToString(), pinfo.PortalID),
                        ["gjs-components"] = Managers.PageManager.DeTokenizeLinks(Signuplayout.ContentJSON.ToString(), pinfo.PortalID),
                        ["gjs-styles"] = Managers.PageManager.DeTokenizeLinks(Signuplayout.StyleJSON.ToString(), pinfo.PortalID)
                    };
                    Core.Managers.PageManager.Update(portalSettings, LayoutData);

                    Pages Page = Managers.PageManager.GetPages(SignUpTab.TabID).OrderByDescending(o => o.Version).FirstOrDefault();


                    if (Page != null && uInfo != null)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                        Page.Version = 1;
                        Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                        Page.IsPublished = true;
                        Page.PublishedBy = uInfo.UserID;
                        Page.PublishedOn = DateTime.UtcNow;
                        PageFactory.Update(Page, uInfo.UserID);
                    }
                }

                TabInfo NotFoundTab = TabController.Instance.GetTabByName("404 Error Page", pinfo.PortalID);
                Layout NotFoundPagelayout = pageLayouts.Where(a => a.Name == "NotFoundPage").FirstOrDefault();
                if (NotFoundTab != null && NotFoundPagelayout != null && portalSettings != null)
                {
                    ProcessBlocks(pinfo.PortalID, homelayout.Blocks);

                    if (portalSettings.ActiveTab == null)
                    {
                        portalSettings.ActiveTab = new TabInfo();
                    }

                    portalSettings.ActiveTab.TabID = NotFoundTab.TabID;
                    Dictionary<string, object> LayoutData = new Dictionary<string, object>
                    {
                        ["IsPublished"] = false,
                        ["Comment"] = string.Empty,
                        ["gjs-assets"] = string.Empty,
                        ["gjs-css"] = Managers.PageManager.DeTokenizeLinks(NotFoundPagelayout.Style.ToString(), pinfo.PortalID),
                        ["gjs-html"] = Managers.PageManager.DeTokenizeLinks(NotFoundPagelayout.Content.ToString(), pinfo.PortalID),
                        ["gjs-components"] = Managers.PageManager.DeTokenizeLinks(NotFoundPagelayout.ContentJSON.ToString(), pinfo.PortalID),
                        ["gjs-styles"] = Managers.PageManager.DeTokenizeLinks(NotFoundPagelayout.StyleJSON.ToString(), pinfo.PortalID)
                    };
                    Core.Managers.PageManager.Update(portalSettings, LayoutData);

                    Pages Page = Managers.PageManager.GetPages(NotFoundTab.TabID).OrderByDescending(o => o.Version).FirstOrDefault();


                    if (Page != null && uInfo != null)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                        Page.Version = 1;
                        Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                        Page.IsPublished = true;
                        Page.PublishedBy = uInfo.UserID;
                        Page.PublishedOn = DateTime.UtcNow;
                        PageFactory.Update(Page, uInfo.UserID);
                    }

                    pinfo.Custom404TabId = NotFoundTab.TabID;
                }

                TabInfo ProfileTab = TabController.Instance.GetTabByName("Profile", pinfo.PortalID);
                Layout Profilelayout = pageLayouts.Where(a => a.Name == "Profile").FirstOrDefault();
                if (ProfileTab != null && Profilelayout != null && portalSettings != null)
                {
                    ProfileTab.ParentId = TabController.Instance.GetTabByName("User", pinfo.PortalID).TabID;
                    TabController.Instance.UpdateTab(ProfileTab);
                    ProcessBlocks(pinfo.PortalID, homelayout.Blocks);
                    pinfo.UserTabId = ProfileTab.TabID;
                    if (portalSettings.ActiveTab == null)
                    {
                        portalSettings.ActiveTab = new TabInfo();
                    }

                    portalSettings.ActiveTab.TabID = ProfileTab.TabID;
                    Dictionary<string, object> LayoutData = new Dictionary<string, object>
                    {
                        ["IsPublished"] = false,
                        ["Comment"] = string.Empty,
                        ["gjs-assets"] = string.Empty,
                        ["gjs-css"] = Managers.PageManager.DeTokenizeLinks(Profilelayout.Style.ToString(), pinfo.PortalID),
                        ["gjs-html"] = Managers.PageManager.DeTokenizeLinks(Profilelayout.Content.ToString(), pinfo.PortalID),
                        ["gjs-components"] = Managers.PageManager.DeTokenizeLinks(Profilelayout.ContentJSON.ToString(), pinfo.PortalID),
                        ["gjs-styles"] = Managers.PageManager.DeTokenizeLinks(Profilelayout.StyleJSON.ToString(), pinfo.PortalID)
                    };
                    Core.Managers.PageManager.Update(portalSettings, LayoutData);

                    Pages Page = Managers.PageManager.GetPages(ProfileTab.TabID).OrderByDescending(o => o.Version).FirstOrDefault();


                    if (Page != null && uInfo != null)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                        Page.Version = 1;
                        Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                        Page.IsPublished = true;
                        Page.PublishedBy = uInfo.UserID;
                        Page.PublishedOn = DateTime.UtcNow;
                        PageFactory.Update(Page, uInfo.UserID);
                    }

                }

                TabInfo SearchResultTab = TabController.Instance.GetTabByName("Search Results", pinfo.PortalID);
                Layout SearchResultlayout = pageLayouts.Where(a => a.Name == "SearchResults").FirstOrDefault();
                if (SearchResultTab != null && SearchResultlayout != null && portalSettings != null)
                {
                    ProcessBlocks(pinfo.PortalID, homelayout.Blocks);
                    if (portalSettings.ActiveTab == null)
                    {
                        portalSettings.ActiveTab = new TabInfo();
                    }

                    portalSettings.ActiveTab.TabID = SearchResultTab.TabID;
                    Dictionary<string, object> LayoutData = new Dictionary<string, object>
                    {
                        ["IsPublished"] = false,
                        ["Comment"] = string.Empty,
                        ["gjs-assets"] = string.Empty,
                        ["gjs-css"] = Managers.PageManager.DeTokenizeLinks(SearchResultlayout.Style.ToString(), pinfo.PortalID),
                        ["gjs-html"] = Managers.PageManager.DeTokenizeLinks(SearchResultlayout.Content.ToString(), pinfo.PortalID),
                        ["gjs-components"] = Managers.PageManager.DeTokenizeLinks(SearchResultlayout.ContentJSON.ToString(), pinfo.PortalID),
                        ["gjs-styles"] = Managers.PageManager.DeTokenizeLinks(SearchResultlayout.StyleJSON.ToString(), pinfo.PortalID)
                    };
                    Core.Managers.PageManager.Update(portalSettings, LayoutData);

                    Pages Page = Managers.PageManager.GetPages(SearchResultTab.TabID).OrderByDescending(o => o.Version).FirstOrDefault();


                    if (Page != null && uInfo != null)
                    {
                        WorkflowState state = WorkflowManager.GetStateByID(Page.StateID.Value);
                        Page.Version = 1;
                        Page.StateID = state != null ? WorkflowManager.GetLastStateID(state.WorkflowID).StateID : 2;
                        Page.IsPublished = true;
                        Page.PublishedBy = uInfo.UserID;
                        Page.PublishedOn = DateTime.UtcNow;
                        PageFactory.Update(Page, uInfo.UserID);
                    }

                }


                #endregion

                #region Update Default SignUp Tab and Search Results page

                pinfo.RegisterTabId = SignUpTab != null && !SignUpTab.IsDeleted ? SignUpTab.TabID : Null.NullInteger;
                pinfo.SearchTabId = SearchResultTab != null && !SearchResultTab.IsDeleted ? SearchResultTab.TabID : Null.NullInteger;

                #endregion
            }

            private static void UpdatePortalSettings(List<StringValue> SettingNameValue, int PortalID, int UserID)
            {
                string query = string.Empty;
                foreach (StringValue setting in SettingNameValue)
                {
                    query += PortalScript.UpdatePortalSettings(setting.Text, setting.Value, PortalID, UserID);
                }

                using (VanjaroRepo db = new VanjaroRepo())
                {
                    db.Execute(query);
                }
            }
            private static void ProcessBlocks(int PortalId, List<CustomBlock> Blocks)
            {
                if (Blocks != null)
                {
                    foreach (CustomBlock item in Blocks)
                    {
                        if (Core.Managers.BlockManager.GetByLocale(PortalId, item.Guid, null) == null)
                        {
                            item.ID = 0;
                            PortalSettings ps = new PortalSettings
                            {
                                PortalId = PortalId
                            };
                            Core.Managers.BlockManager.Add(ps, item, 1);
                        }
                    }
                }
            }

            private static List<Layout> GetLayouts(PortalInfo pinfo)
            {
                List<Layout> layouts = new List<Layout>();
                string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.GetCurrent(pinfo.PortalID).Name + "/templates/website/");
                if (Directory.Exists(FolderPath))
                {
                    foreach (string layout in Directory.GetFiles(FolderPath, "*.json"))
                    {
                        string stringJson = File.ReadAllText(layout);
                        if (!string.IsNullOrEmpty(stringJson))
                        {
                            Layout lay = JsonConvert.DeserializeObject<Layout>(stringJson);
                            if (lay != null)
                            {
                                lay.Name = Path.GetFileNameWithoutExtension(layout);
                                layouts.Add(lay);
                            }
                        }
                    }
                }
                return layouts;
            }
            public static void SoftDeleteModule(int PortalId, string ModuleFriendlyName)
            {
                try
                {
                    using (VanjaroRepo vrepo = new VanjaroRepo())
                    {
                        ModuleDefinitionInfo moduleDefinitionInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(ModuleFriendlyName);
                        System.Collections.Generic.List<ModuleSettings> moduleInfo = vrepo.Fetch<ModuleSettings>("Select T.PortalID,Tabm.TabID,Tabm.ModuleID,Tabm.PaneName,Tabm.IsDeleted from TabModules Tabm inner join Tabs T On Tabm.TabID = t.TabID Where PortalID=@0 AND ModuleTitle= @1", PortalId, ModuleFriendlyName);
                        foreach (ModuleSettings ModuleSettings in moduleInfo)
                        {
                            ModuleController.Instance.DeleteTabModule(ModuleSettings.TabID, ModuleSettings.ModuleID, true);
                        }
                    }
                }
                catch (Exception) { }
            }

            public static void DeleteDefaultMemberProfileProperties()
            {
                try
                {
                    using (VanjaroRepo vrepo = new VanjaroRepo())
                    {
                        vrepo.Execute("Delete from ProfilePropertyDefinition Where PortalID Is NUll AND PropertyName !=@0", "PreferredTimeZone");
                    }
                }
                catch (Exception) { }
            }

            public static bool IsDistribution(int PortalID)
            {
                return "[G]Skins/Vanjaro/Base.ascx" == PortalController.GetPortalSetting("DefaultPortalSkin", PortalID, Null.NullString);
            }

        }
        private class ModuleSettings
        {
            public int PortalID { get; set; }
            public int TabID { get; set; }
            public int ModuleID { get; set; }
            public string PaneName { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
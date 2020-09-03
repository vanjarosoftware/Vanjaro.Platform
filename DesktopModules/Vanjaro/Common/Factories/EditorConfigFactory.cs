using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Vanjaro.Common.Components;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Data.Scripts;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.Common.Factories
{
    public class EditorConfigFactory
    {
        internal static List<IUIData> GetData(PortalSettings portalSettings, ModuleInfo moduleInfo, Dictionary<string, string> parameters)
        {
            int PortalID = portalSettings.PortalId;
            int ModuleID = 0;
            if (moduleInfo != null)
            {
                ModuleID = moduleInfo.ModuleID;
            }

            string profile = string.Empty;
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.ToString().Contains("profile="))
            {
                profile = HttpContext.Current.Request.UrlReferrer.ToString().Split('=').Last();
            }
            else if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.ToString().Contains("profile/"))
            {
                profile = HttpContext.Current.Request.UrlReferrer.ToString().Split('/').Last();
                if (profile.Contains('?'))
                {
                    profile = profile.Split('?').First();
                }
            }
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            List<HTMLEditor_Profile> HTMLEditorProfiles = GetHTMLEditorProfiles(PortalID);
            int ProfileID = 0;
            Settings.Add("UID", new UIData() { Name = "UID", Value = parameters.Where(p => p.Key == "uid").FirstOrDefault().Value });
            HTMLEditor Editor = HTMLEditor.Query("where UID=@0", Settings["UID"].Value + ModuleID).SingleOrDefault();
            if (Editor != null)
            {
                ProfileID = Editor.ProfileID;
                HTMLEditorSetting HTMLEditorSetting = DotNetNuke.Common.Utilities.Json.Deserialize<HTMLEditorSetting>(Editor.Settings);
                if (HTMLEditorSetting.UploadFilesRootFolder == 0)
                {
                    HTMLEditorSetting.UploadFilesRootFolder = BrowseUploadFactory.GetRootFolder(PortalID).FolderID;
                }

                if (HTMLEditorSetting.UploadImagesRootFolder == 0)
                {
                    HTMLEditorSetting.UploadImagesRootFolder = BrowseUploadFactory.GetRootFolder(PortalID).FolderID;
                }

                Settings.Add("Settings", new UIData() { Name = "Settings", Options = HTMLEditorSetting });
            }
            else
            {
                if (!string.IsNullOrEmpty(profile) && HTMLEditorProfiles.Where(p => p.Name.ToLower() == profile.ToLower()).FirstOrDefault() != null)
                {
                    ProfileID = HTMLEditorProfiles.Where(p => p.Name.ToLower() == profile.ToLower()).FirstOrDefault().ProfileID;
                }

                HTMLEditorSetting HTMLEditorSetting = new HTMLEditorSetting();
                HTMLEditorSetting.UploadFilesRootFolder = HTMLEditorSetting.UploadImagesRootFolder = BrowseUploadFactory.GetRootFolder(PortalID).FolderID;
                Settings.Add("Settings", new UIData() { Name = "Settings", Options = HTMLEditorSetting });
            }

            Settings.Add("Profiles", new UIData() { Name = "Profiles", Options = HTMLEditorProfiles, Value = ProfileID.ToString() });
            Settings.Add("EditorOptions", new UIData() { Name = "EditorOptions", Options = GetEditorOptions(HTMLEditorProfiles, int.Parse(Settings["Profiles"].Value)) });
            Settings.Add("LocalizationKeyValue", new UIData() { Name = "LocalizationKeyValue", Options = getLocalizationKeyValue() });
            Settings.Add("Folders", new UIData() { Name = "Folders", Options = BrowseUploadFactory.GetFolders(PortalID) });
            Settings.Add("IsSuperUser", new UIData() { Name = "IsSuperUser", Value = PortalSettings.Current.UserInfo.IsSuperUser.ToString() });
            Settings.Add("ModuleID", new UIData() { Name = "ModuleID", Value = ModuleID.ToString() });
            return Settings.Values.ToList();
        }

        private static Dictionary<string, string> getLocalizationKeyValue()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string resourceFileRoot = VirtualPathUtility.ToAbsolute("~/DesktopModules/Vanjaro/Common/Engines/UIEngine/AngularBootstrap/Views/Common/Controls/App_LocalResources/editorconfig.resx");
            foreach (string field in typeof(EditorOptions).GetFields().Select(field => field.Name).ToList())
            {
                result.Add(field, Localization.GetString(field, resourceFileRoot));
            }
            return result;
        }

        internal static EditorOptions GetEditorOptions(List<HTMLEditor_Profile> hTMLEditorProfiles, int ProfileID)
        {
            EditorOptions result = null;
            HTMLEditor_Profile profile = hTMLEditorProfiles.Where(p => p.ProfileID == ProfileID).SingleOrDefault();
            if (profile != null)
            {
                result = DotNetNuke.Common.Utilities.Json.Deserialize<EditorOptions>(profile.Value);
            }

            return result;
        }

        public static List<HTMLEditor_Profile> GetHTMLEditorProfiles(int portalID)
        {
            List<HTMLEditor_Profile> HTMLEditorProfiles = new List<HTMLEditor_Profile>();
            EditorOptions EditorOptions = new EditorOptions();
            if (EditorOptions != null)
            {
                List<HTMLEditor_Profile> Profiles = HTMLEditor_Profile.Query("where PortalID=@0 or PortalID=@1", portalID, -1).ToList();
                if (Profiles.Where(p => p.Name == "Basic").FirstOrDefault() == null)
                {
                    EditorOptions.Plugins = GetPluginsKeyValue("Basic", EditorOptions);
                    HTMLEditor_Profile profile = new HTMLEditor_Profile
                    {
                        ProfileID = 0,
                        Name = "Basic",
                        PortalID = -1,
                        Value = DotNetNuke.Common.Utilities.Json.Serialize(EditorOptions)
                    };
                    CommonLibraryRepo.GetInstance().Execute("set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile ON;insert into " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile(ProfileID,PortalID,Name,Value)" +
                        " values(" + profile.ProfileID + "," + profile.PortalID + ",'" + profile.Name + "','" + profile.Value + "') ; set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile OFF;");
                    HTMLEditorProfiles.Add(profile);
                }

                if (Profiles.Where(p => p.Name == "Minimal").FirstOrDefault() == null)
                {
                    EditorOptions.Plugins = GetPluginsKeyValue("Minimal", EditorOptions);
                    HTMLEditor_Profile profile = new HTMLEditor_Profile
                    {
                        ProfileID = -3,
                        Name = "Minimal",
                        PortalID = -1,
                        Value = DotNetNuke.Common.Utilities.Json.Serialize(EditorOptions)
                    };
                    CommonLibraryRepo.GetInstance().Execute("set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile ON;insert into " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile(ProfileID,PortalID,Name,Value)" +
                        " values(" + profile.ProfileID + "," + profile.PortalID + ",'" + profile.Name + "','" + profile.Value + "') ; set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile OFF;");
                    HTMLEditorProfiles.Add(profile);
                }

                if (Profiles.Where(p => p.Name == "Standard").FirstOrDefault() == null)
                {
                    EditorOptions.Plugins = GetPluginsKeyValue("Standard", EditorOptions);
                    HTMLEditor_Profile Profile = new HTMLEditor_Profile
                    {
                        ProfileID = -1,
                        Name = "Standard",
                        PortalID = -1,
                        Value = DotNetNuke.Common.Utilities.Json.Serialize(EditorOptions)
                    };
                    CommonLibraryRepo.GetInstance().Execute("set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile ON;insert into " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile(ProfileID,PortalID,Name,Value)" +
                        " values(" + Profile.ProfileID + "," + Profile.PortalID + ",'" + Profile.Name + "','" + Profile.Value + "') ; set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile OFF;");
                    HTMLEditorProfiles.Add(Profile);
                }

                if (Profiles.Where(p => p.Name == "Full").FirstOrDefault() == null)
                {
                    EditorOptions.Plugins = GetPluginsKeyValue("Full", EditorOptions);
                    HTMLEditor_Profile Profile = new HTMLEditor_Profile
                    {
                        ProfileID = -2,
                        Name = "Full",
                        PortalID = -1,
                        Value = DotNetNuke.Common.Utilities.Json.Serialize(EditorOptions)
                    };
                    CommonLibraryRepo.GetInstance().Execute("set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile ON;insert into " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile(ProfileID,PortalID,Name,Value)" +
                        " values(" + Profile.ProfileID + "," + Profile.PortalID + ",'" + Profile.Name + "','" + Profile.Value + "') ; set identity_insert " + CommonScript.TablePrefix + "VJ_Common_HTMLEditor_Profile OFF;");
                    HTMLEditorProfiles.Add(Profile);
                }

                foreach (HTMLEditor_Profile CustomProfile in Profiles)
                {
                    EditorOptions existingEditorOptions = DotNetNuke.Common.Utilities.Json.Deserialize<EditorOptions>(CustomProfile.Value);
                    EditorOptions fullCustomEditorOptions = new EditorOptions();
                    fullCustomEditorOptions.Plugins = EditorConfigFactory.GetPluginsKeyValue("FullCustom", fullCustomEditorOptions);
                    if (existingEditorOptions != null && fullCustomEditorOptions != null)
                    {
                        fullCustomEditorOptions.Plugins = fullCustomEditorOptions.Plugins.ToDictionary(u => u.Key, u => false);
                        foreach (KeyValuePair<string, bool> item in existingEditorOptions.Plugins)
                        {
                            if (fullCustomEditorOptions.Plugins.ContainsKey(item.Key))
                            {
                                fullCustomEditorOptions.Plugins[item.Key] = item.Value;
                            }
                        }
                        existingEditorOptions.Plugins = fullCustomEditorOptions.Plugins;
                        CustomProfile.Value = DotNetNuke.Common.Utilities.Json.Serialize(existingEditorOptions);
                    }
                    HTMLEditorProfiles.Add(CustomProfile);
                }
            }
            return HTMLEditorProfiles;
        }

        internal static string DeleteProfile(int portalID, int profileid)
        {
            if (profileid > 0)
            {
                int ProfileInUse = HTMLEditor.Query("where ProfileID=@0", profileid).Count();
                if (ProfileInUse > 0)
                {
                    return "inuse";
                }
                else
                {
                    HTMLEditor_Profile Profile = HTMLEditor_Profile.FirstOrDefault("where PortalID=@0 and ProfileID=@1", portalID, profileid);
                    if (Profile != null)
                    {
                        Profile.Delete();
                    }

                    return "deleted";
                }
            }
            return null;
        }

        public static Dictionary<string, bool> GetPluginsKeyValue(string Preset, EditorOptions EditorOptions, string SearchPatterns = "XX.mconfig")
        {
            Dictionary<string, bool> _Plugins = GetPluginsBySearchPatterns(SearchPatterns);
            switch (Preset)
            {
                case "Basic":
                    _Plugins = _Plugins.Where(k => EditorOptions.BasicPlugins.Contains(k.Key)).ToDictionary(u => u.Key, u => u.Value);
                    break;
                case "Standard":
                    _Plugins = _Plugins.Where(k => EditorOptions.StandardPlugins.Contains(k.Key)).ToDictionary(u => u.Key, u => u.Value);
                    break;
                case "Full":
                    _Plugins = _Plugins.Where(k => EditorOptions.FullPlugins.Contains(k.Key)).ToDictionary(u => u.Key, u => u.Value);
                    break;
                case "Minimal":
                    _Plugins = _Plugins.Where(k => EditorOptions.MinimalPlugins.Contains(k.Key)).ToDictionary(u => u.Key, u => u.Value);
                    break;
                default:
                    break;
            }
            return _Plugins.OrderBy(o => o.Key).ToDictionary(u => u.Key, u => u.Value);
        }

        public static Dictionary<string, bool> GetPluginsKeyValueSaved(int ProfileID)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            HTMLEditor_Profile HTMLEditor_Profile = HTMLEditor_Profile.Query("where ProfileID=@0", ProfileID).SingleOrDefault();
            if (HTMLEditor_Profile != null)
            {
                EditorOptions editorOptions = DotNetNuke.Common.Utilities.Json.Deserialize<EditorOptions>(HTMLEditor_Profile.Value);
                if (editorOptions != null)
                {
                    result = editorOptions.Plugins.OrderBy(o => o.Key).ToDictionary(u => u.Key, u => u.Value);
                }
            }
            return result;
        }

        private static Dictionary<string, bool> GetPluginsBySearchPatterns(string SearchPatterns)
        {
            Dictionary<string, bool> _Plugins = new Dictionary<string, bool>();
            DirectoryInfo di = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/DesktopModules/Vanjaro/Common/Frameworks/Ckeditor/plugins"));
            if (di != null)
            {
                foreach (DirectoryInfo item in di.GetDirectories())
                {
                    if (item.GetFiles(SearchPatterns).Count() >= 1)
                    {
                        _Plugins.Add(item.Name, true);
                    }
                    else
                    {
                        _Plugins.Remove(item.Name);
                    }
                }
            }

            return _Plugins;
        }

        internal static dynamic SaveProfile(PortalSettings portalSettings, ModuleInfo moduleInfo, int profileid, string profileName, EditorOptions editorOptions, Dictionary<string, string> parameters)
        {
            int portalid = portalSettings.PortalId;
            int moduleid = 0;
            if (moduleInfo != null)
            {
                moduleid = moduleInfo.ModuleID;
            }

            dynamic result = new ExpandoObject();
            result.Data = null;
            result.Message = "";
            result.Profile = "";
            Dictionary<int, string> Profiles = new Dictionary<int, string>
            {
                { 0, "Basic" },
                { -1, "Standard" },
                { -2, "Full" },
                { -3, "Minimal" }
            };

            if (!string.IsNullOrEmpty(profileName) && editorOptions != null)
            {
                if (profileid == 0 && profileName != "Basic" && profileName != "Standard" && profileName != "Full" && profileName != "Minimal")
                {
                    HTMLEditor_Profile Profile = HTMLEditor_Profile.Query("where PortalID=@0 and Name=@1", portalid, profileName).FirstOrDefault();
                    if (Profile == null)
                    {
                        Profile = new HTMLEditor_Profile
                        {
                            Name = profileName,
                            PortalID = portalid,
                            Value = DotNetNuke.Common.Utilities.Json.Serialize(editorOptions)
                        };
                        Profile.Insert();
                        result.Data = GetData(portalSettings, moduleInfo, parameters);
                    }
                    else
                    {
                        result.Message = "Profile Name Exists";
                    }

                    result.Profile = Profile;
                }
                else if (Profiles.ContainsKey(profileid) && Profiles.ContainsValue(profileName) && PortalSettings.Current.UserInfo.IsSuperUser == true)
                {
                    HTMLEditor_Profile Profile = HTMLEditor_Profile.Query("where PortalID=@0 and ProfileID=@1", -1, profileid).FirstOrDefault();
                    if (Profile != null)
                    {
                        if (Profile.Name != profileName)
                        {
                            HTMLEditor_Profile ProfileExists = HTMLEditor_Profile.Query("where PortalID=@0 and Name=@1", -1, profileName).FirstOrDefault();
                            if (ProfileExists != null)
                            {
                                result.Message = "Profile Name Exists";
                                return result;
                            }
                        }
                        Profile.Name = profileName;
                        Profile.Value = DotNetNuke.Common.Utilities.Json.Serialize(editorOptions);
                        Profile.Update();
                        result.Data = GetData(portalSettings, moduleInfo, parameters);
                    }
                    result.Profile = Profile;
                }
                else if (profileid > 0 && profileName != "Basic" && profileName != "Standard" && profileName != "Full" && profileName != "Minimal")
                {
                    HTMLEditor_Profile Profile = HTMLEditor_Profile.Query("where PortalID=@0 and ProfileID=@1", portalid, profileid).FirstOrDefault();
                    if (Profile != null)
                    {
                        if (Profile.Name != profileName)
                        {
                            HTMLEditor_Profile ProfileExists = HTMLEditor_Profile.Query("where PortalID=@0 and Name=@1", portalid, profileName).FirstOrDefault();
                            if (ProfileExists != null)
                            {
                                result.Message = "Profile Name Exists";
                                return result;
                            }
                        }
                        Profile.Name = profileName;
                        Profile.Value = DotNetNuke.Common.Utilities.Json.Serialize(editorOptions);
                        Profile.Update();
                        result.Data = GetData(portalSettings, moduleInfo, parameters);
                    }
                    result.Profile = Profile;
                }
                result.Message = "Profile Name Exists";
            }
            return result;
        }

        public static string GetEditorToolbarMarkup(int moduleid, string uid, string Editor, string Profile)
        {
            if (HTMLEditor_Profile.Query("").Count() < 4)
            {
                GetHTMLEditorProfiles(-1);
            }

            string result = string.Empty;
            HTMLEditor_Profile HTMLEditorProfile = null;
            HTMLEditor HTMLEditor = null;
            if (!string.IsNullOrEmpty(uid))
            {
                uid = uid = uid.ToLower().Trim();
                HTMLEditor = HTMLEditor.Query("where UID=@0", uid).SingleOrDefault();
                if (HTMLEditor != null)
                {
                    HTMLEditorProfile = HTMLEditor_Profile.Query("where ProfileID=@0", HTMLEditor.ProfileID).SingleOrDefault();
                }

                if (HTMLEditorProfile == null)
                {
                    string Preset = "Basic";
                    if (!string.IsNullOrEmpty(Profile))
                    {
                        Preset = Profile;
                    }

                    int ProfileID = 0;
                    if (HTMLEditor != null)
                    {
                        ProfileID = HTMLEditor.ProfileID;
                        if (ProfileID == 0)
                        {
                            Preset = "Basic";
                        }

                        if (ProfileID == -1)
                        {
                            Preset = "Standard";
                        }

                        if (ProfileID == -2)
                        {
                            Preset = "Full";
                        }

                        if (ProfileID == -3)
                        {
                            Preset = "Minimal";
                        }
                    }
                    else
                    {
                        switch (Preset)
                        {
                            case "Standard":
                                ProfileID = -1;
                                break;
                            case "Full":
                                ProfileID = -2;
                                break;
                            case "Minimal":
                                ProfileID = -3;
                                break;
                            default:
                                ProfileID = 0;
                                break;
                        }
                    }
                    EditorOptions EditorOptions = new EditorOptions
                    {
                        Plugins = GetPluginsKeyValueSaved(ProfileID)
                    };
                    HTMLEditorProfile = new HTMLEditor_Profile() { ProfileID = ProfileID, PortalID = -1, Name = Preset, Value = DotNetNuke.Common.Utilities.Json.Serialize(EditorOptions) };
                }
            }
            if (!string.IsNullOrEmpty(Editor) && HTMLEditorProfile != null)
            {
                EditorOptions editorOptions = DotNetNuke.Common.Utilities.Json.Deserialize<EditorOptions>(HTMLEditorProfile.Value);
                if (editorOptions != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("$scope." + Editor + "Options = {");
                    if (!string.IsNullOrEmpty(editorOptions.Height))
                    {
                        sb.Append("height: '" + editorOptions.Height + "',");
                    }

                    if (!string.IsNullOrEmpty(editorOptions.Width))
                    {
                        sb.Append("width: '" + editorOptions.Width + "',");
                    }

                    if (!string.IsNullOrEmpty(editorOptions.UiColor))
                    {
                        sb.Append("uiColor: '" + editorOptions.UiColor + "',");
                    }

                    if (editorOptions.FilebrowserBrowseUrl && PortalSettings.Current.UserInfo.UserID > 0)
                    {
                        sb.Append("filebrowserBrowseUrl: $scope.ui.data.BrowseUrl.Value + '&type=filebrowserBrowseUrl#/common/controls/url/" + uid.Replace(moduleid.ToString(), "") + "',");
                    }

                    if (editorOptions.FilebrowserImageBrowseUrl && PortalSettings.Current.UserInfo.UserID > 0)
                    {
                        sb.Append("filebrowserImageBrowseUrl: $scope.ui.data.BrowseUrl.Value + '&type=filebrowserImageBrowseUrl#/common/controls/url/" + uid.Replace(moduleid.ToString(), "") + "',");
                    }

                    if (HTMLEditorProfile.ProfileID == -3)
                    {
                        sb.Append("removeButtons: 'Cut,Copy,Paste,Undo,Redo,Anchor,Underline,Strike,Subscript,Superscript,Outdent,Indent',");
                    }
                    else
                    {
                        sb.Append("removeButtons: '',");
                    }

                    sb.Append("disableNativeSpellChecker: false,");
                    if (HTMLEditor != null && !string.IsNullOrEmpty(HTMLEditor.Settings))
                    {
                        HTMLEditorSetting HTMLEditorSetting = DotNetNuke.Common.Utilities.Json.Deserialize<HTMLEditorSetting>(HTMLEditor.Settings);
                        if (HTMLEditorSetting != null)
                        {
                            sb.Append("fullPage: " + HTMLEditorSetting.FullPageMode.ToString().ToLower() + ",");
                        }
                    }
                    if (HTMLEditorProfile.ProfileID != -3)
                    {
                        sb.Append("allowedContent: true,");
                    }

                    sb.Append("extraPlugins: '" + GetPlugins(editorOptions) + "'");
                    sb.Append("};");
                    result = sb.ToString();
                }
            }
            return result;
        }

        public static string GetEditorToolbarMarkupForRazor(int moduleid, string uid, string BrowseUrl, string Profile)
        {
            if (HTMLEditor_Profile.Query("").Count() < 4)
            {
                GetHTMLEditorProfiles(-1);
            }

            string result = string.Empty;
            HTMLEditor_Profile HTMLEditorProfile = null;
            HTMLEditor HTMLEditor = null;
            if (!string.IsNullOrEmpty(uid))
            {
                uid = uid = uid.ToLower().Trim();
                HTMLEditor = HTMLEditor.Query("where UID=@0", uid).SingleOrDefault();
                if (HTMLEditor != null)
                {
                    HTMLEditorProfile = HTMLEditor_Profile.Query("where ProfileID=@0", HTMLEditor.ProfileID).SingleOrDefault();
                }

                if (HTMLEditorProfile == null)
                {
                    string Preset = "Basic";
                    if (!string.IsNullOrEmpty(Profile))
                    {
                        Preset = Profile;
                    }

                    int ProfileID = 0;
                    if (HTMLEditor != null)
                    {
                        ProfileID = HTMLEditor.ProfileID;
                        if (ProfileID == 0)
                        {
                            Preset = "Basic";
                        }

                        if (ProfileID == -1)
                        {
                            Preset = "Standard";
                        }

                        if (ProfileID == -2)
                        {
                            Preset = "Full";
                        }

                        if (ProfileID == -3)
                        {
                            Preset = "Minimal";
                        }
                    }
                    else
                    {
                        switch (Preset)
                        {
                            case "Standard":
                                ProfileID = -1;
                                break;
                            case "Full":
                                ProfileID = -2;
                                break;
                            case "Minimal":
                                ProfileID = -3;
                                break;
                            default:
                                ProfileID = 0;
                                break;
                        }
                    }
                    EditorOptions EditorOptions = new EditorOptions
                    {
                        Plugins = GetPluginsKeyValueSaved(ProfileID)
                    };
                    HTMLEditorProfile = new HTMLEditor_Profile() { ProfileID = ProfileID, PortalID = -1, Name = Preset, Value = DotNetNuke.Common.Utilities.Json.Serialize(EditorOptions) };
                }
            }
            if (HTMLEditorProfile != null)
            {
                EditorOptions editorOptions = DotNetNuke.Common.Utilities.Json.Deserialize<EditorOptions>(HTMLEditorProfile.Value);
                if (editorOptions != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    if (!string.IsNullOrEmpty(editorOptions.Height))
                    {
                        sb.Append("\"height\": \"" + editorOptions.Height + "\",");
                    }

                    if (!string.IsNullOrEmpty(editorOptions.Width))
                    {
                        sb.Append("\"width\": \"" + editorOptions.Width + "\",");
                    }

                    if (!string.IsNullOrEmpty(editorOptions.UiColor))
                    {
                        sb.Append("\"uiColor\": \"" + editorOptions.UiColor + "\",");
                    }

                    if (editorOptions.FilebrowserBrowseUrl && PortalSettings.Current.UserInfo.UserID > 0)
                    {
                        sb.Append("\"filebrowserBrowseUrl\": \"" + BrowseUrl + "&type=filebrowserBrowseUrl#/common/controls/url/" + uid.Replace(moduleid.ToString(), "") + "\",");
                    }

                    if (editorOptions.FilebrowserImageBrowseUrl && PortalSettings.Current.UserInfo.UserID > 0)
                    {
                        sb.Append("\"filebrowserImageBrowseUrl\": \"" + BrowseUrl + "&type=filebrowserImageBrowseUrl#/common/controls/url/" + uid.Replace(moduleid.ToString(), "") + "\",");
                    }

                    if (HTMLEditorProfile.ProfileID == -3)
                    {
                        sb.Append("\"removeButtons\": \"Cut,Copy,Paste,Undo,Redo,Anchor,Underline,Strike,Subscript,Superscript,Outdent,Indent\",");
                    }
                    else
                    {
                        sb.Append("\"removeButtons\": \"\",");
                    }

                    sb.Append("\"disableNativeSpellChecker\": false,");
                    if (HTMLEditor != null && !string.IsNullOrEmpty(HTMLEditor.Settings))
                    {
                        HTMLEditorSetting HTMLEditorSetting = DotNetNuke.Common.Utilities.Json.Deserialize<HTMLEditorSetting>(HTMLEditor.Settings);
                        if (HTMLEditorSetting != null)
                        {
                            sb.Append("\"fullPage\": " + HTMLEditorSetting.FullPageMode.ToString().ToLower() + ",");
                        }
                    }
                    if (HTMLEditorProfile.ProfileID != -3)
                    {
                        sb.Append("\"allowedContent\": true,");
                    }

                    sb.Append("\"extraPlugins\": \"" + GetPlugins(editorOptions) + "\"");
                    sb.Append("}");
                    result = sb.ToString();
                }
            }
            return result;
        }

        private static string GetPlugins(EditorOptions editorOptions)
        {
            string result = string.Empty;
            foreach (KeyValuePair<string, bool> item in GetPluginsBySearchPatterns("HA.mconfig"))
            {
                if (item.Value)
                {
                    result += "" + item.Key + ",";
                }
            }
            if (editorOptions != null)
            {
                foreach (KeyValuePair<string, bool> item in editorOptions.Plugins)
                {
                    if (item.Value)
                    {
                        result += "" + item.Key + ",";
                    }
                }
            }
            return result.TrimEnd(',');
        }

        internal static List<ModuleInfo> GetTabModules(ModuleInfo Moduleinfo)
        {
            List<ModuleInfo> result = new List<ModuleInfo>();
            if (Moduleinfo != null)
            {
                ModuleController moduleController = new ModuleController();
                Dictionary<int, ModuleInfo> ModuleInfoDictionary = moduleController.GetTabModules(Moduleinfo.TabID);
                if (ModuleInfoDictionary != null && ModuleInfoDictionary.Values != null && ModuleInfoDictionary.Values.Count > 0)
                {
                    result = ModuleInfoDictionary.Values.Where(t => t.ModuleDefID == Moduleinfo.ModuleDefID).ToList();
                }
            }
            return result;
        }

        internal static List<ModuleInfo> GetPortalModules(ModuleInfo Moduleinfo)
        {
            List<ModuleInfo> result = new List<ModuleInfo>();
            if (Moduleinfo != null)
            {
                ModuleController moduleController = new ModuleController();
                foreach (ModuleInfo modInfo in moduleController.GetModulesByDefinition(Moduleinfo.PortalID, Moduleinfo.DesktopModule.FriendlyName).Cast<ModuleInfo>().ToList())
                {
                    result.Add(modInfo);
                }
            }
            return result;
        }
    }
}
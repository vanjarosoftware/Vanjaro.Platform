using Dnn.PersonaBar.Prompt.Components.Commands.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Vanjaro.Common.ASPNET;
using Vanjaro.Core.Components;
using Vanjaro.Core.Components.Interfaces;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.Core.Entities.Theme;
using Vanjaro.Core.Services.Authentication.OAuth;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class ThemeManager
        {
            public static Theme CurrentTheme
            {
                get
                {
                    int PortalID = PortalSettings.Current != null ? PortalSettings.Current.PortalId : -1;

                    return GetCurrent(PortalID);
                }
            }

            public static Theme GetCurrent(int PortalID)
            {
                Theme theme = new Theme(PortalID);
                return theme;
            }

            public static List<string> GetControlTypes()
            {
                List<string> result = new List<string>
                {
                    "Slider",
                    "Dropdown",
                    "Color Picker",
                    "Fonts"
                };
                return result;
            }
            public static ThemeEditor GetThemeEditor(string categoryguid, string guid)
            {
                int index = 0;
                ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalSettings.Current.PortalId, categoryguid);
                if (ThemeEditorWrapper != null)
                {
                    return GetThemeEditor(ThemeEditorWrapper.ThemeEditors, guid, ref index);
                }
                else
                {
                    return null;
                }
            }

            public static string GetGUID(string name)
            {
                ITheme theme = GetThemes().Where(s => s.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (theme != null)
                    return theme.GUID.ToString();
                else
                    return string.Empty;
            }

            public static List<ITheme> GetThemes()
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Theme, "AllIThemes");
                List<ITheme> Items = CacheFactory.Get(CacheKey);
                if (Items == null || Items.Count == 0)
                {
                    Items = new List<ITheme>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                    foreach (string Path in binAssemblies)
                    {
                        try
                        {
                            Items.AddRange((from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                            where t != (typeof(ITheme)) && (typeof(ITheme).IsAssignableFrom(t))
                                            select Activator.CreateInstance(t) as ITheme).ToList());
                        }
                        catch { continue; }
                    }
                    CacheFactory.Set(CacheKey, Items);
                }
                return Items;
            }

            public static List<IThemeEditor> GetCategories(bool CheckVisibilityPermission)
            {
                if (CheckVisibilityPermission)
                    return GetCategories().Where(x => x.IsVisible).ToList();
                else
                    return GetCategories();
            }

            private static List<IThemeEditor> GetCategories()
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.ThemeCategory, "AllPortals");
                List<IThemeEditor> Items = CacheFactory.Get(CacheKey);
                if (Items == null || Items.Count == 0)
                {
                    Items = new List<IThemeEditor>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                    foreach (string Path in binAssemblies)
                    {
                        try
                        {
                            Items.AddRange((from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                            where t != (typeof(IThemeEditor)) && (typeof(IThemeEditor).IsAssignableFrom(t))
                                            select Activator.CreateInstance(t) as IThemeEditor).ToList());
                        }
                        catch { continue; }
                    }
                    CacheFactory.Set(CacheKey, Items);
                }
                return Items;
            }
            internal static ThemeEditor GetThemeEditor(List<ThemeEditor> themeEditors, string guid, ref int index)
            {
                if (!string.IsNullOrEmpty(guid) && themeEditors != null)
                {
                    foreach (ThemeEditor item in themeEditors)
                    {
                        index++;
                        if (item.Guid.ToLower() == guid.ToLower())
                        {
                            return item;
                        }
                    }
                }
                index = -1;
                return null;
            }

            public static void DeletePortalThemeCss()
            {
                List<int> pids = new List<int>();
                foreach (PortalInfo pi in PortalController.Instance.GetPortals())
                    pids.Add(pi.PortalID);

                string ThemeCssFolder = HttpContext.Current.Server.MapPath("~/Portals");
                Parallel.ForEach(pids,
                new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.50) * 1.0)) },
                pid =>
                {
                    string ThemeCss = ThemeCssFolder + "/" + pid + "/vThemes/" + GetCurrent(pid).Name + "/Theme.css";
                    if (File.Exists(ThemeCss))
                    {
                        File.Copy(ThemeCss, ThemeCss.Replace("Theme.css", "Theme.backup.css"), true);
                        File.Delete(ThemeCss.Replace("Theme.backup.css", "Theme.css"));
                    }
                });
            }

            public static void ProcessScss(int PortalID, bool CheckVisibilityPermission)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder themeEditorJs = new StringBuilder();
                string ThemeName = GetCurrent(PortalID).Name;
                string BootstrapPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeName + "/scss/Bootstrap/bootstrap.scss");
                string BeforePath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeName + "/scss/Before.scss");
                string AfterPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeName + "/scss/After.scss");

                foreach (ThemeFont font in GetFonts(PortalID, "all", CheckVisibilityPermission))
                {
                    if (!string.IsNullOrEmpty(font.Css))
                    {
                        sb.Append(font.Css);
                    }
                }

                if (File.Exists(BeforePath))
                {
                    sb.Append(File.ReadAllText(BeforePath));
                }

                List<string> Css = new List<string>();
                foreach (IThemeEditor category in GetCategories(CheckVisibilityPermission).OrderBy(o => o.ViewOrder).ToList())
                {
                    List<ThemeEditorValue> themeEditorValues = GetThemeEditorValues(PortalID, category.Guid);
                    ThemeEditorWrapper editors = GetThemeEditors(PortalID, category.Guid, CheckVisibilityPermission);
                    if (editors != null && editors.ThemeEditors != null)
                    {
                        foreach (IGrouping<string, ThemeEditor> themeEditorGroup in GetEditorGroups(PortalID, category.Guid, CheckVisibilityPermission))
                        {
                            foreach (ThemeEditor item in themeEditorGroup.OrderBy(a => a.Title).ToList())
                            {
                                string sass = item.Sass;
                                foreach (dynamic ctl in item.Controls)
                                {
                                    string id = ctl.Guid.ToString();
                                    string css = ctl.CustomCSS.ToString();
                                    string variable = ctl.LessVariable.ToString();
                                    string DefaultValue = ctl.DefaultValue.ToString();

                                    ThemeEditorValue editorValue = themeEditorValues?.Where(t => t.Guid.ToLower() == id.ToLower()).FirstOrDefault();
                                    if (editorValue != null)
                                    {
                                        DefaultValue = editorValue.Value;
                                    }

                                    if (!string.IsNullOrEmpty(DefaultValue) && !string.IsNullOrEmpty(variable) && variable.StartsWith("$"))
                                    {
                                        Css.Add(variable + ":" + DefaultValue + " !default;");
                                    }

                                    if (!string.IsNullOrEmpty(DefaultValue) && !string.IsNullOrEmpty(css))
                                    {
                                        string[] strings = new string[] { variable };
                                        strings = css.Split(strings, StringSplitOptions.None);
                                        css = string.Join(DefaultValue, strings);
                                        Css.Add(css + ';');
                                    }

                                    if (!string.IsNullOrEmpty(sass))
                                    {
                                        Css.Add(sass + ';');
                                    }
                                    if (ctl.JavascriptVariable != null && !string.IsNullOrEmpty(ctl.JavascriptVariable.Value) && !string.IsNullOrEmpty(DefaultValue))
                                    {
                                        themeEditorJs.Append(ctl.JavascriptVariable.ToString() + ":'" + DefaultValue + "',");
                                    }
                                }
                            }
                        }
                    }
                }

                if (Css != null && Css.Count > 0)
                {
                    Css = Css.Distinct().ToList();
                    foreach (string str in Css)
                    {
                        sb.Append(str);
                    }
                }

                if (File.Exists(BootstrapPath))
                {
                    sb.Append(File.ReadAllText(BootstrapPath));
                }

                if (File.Exists(AfterPath))
                {
                    sb.Append(File.ReadAllText(AfterPath));
                }

                bool IncrementCrmVersion = false;
                if (sb.Length > 0)
                {
                    string ThemeScss = HttpContext.Current.Server.MapPath("~/Portals/" + PortalID + "/vThemes/" + ThemeName + "/Theme.scss");
                    if (!File.Exists(ThemeScss))
                        File.Create(ThemeScss).Dispose();
                    File.WriteAllText(ThemeScss, sb.ToString());

                    string ThemeCss = HttpContext.Current.Server.MapPath("~/Portals/" + PortalID + "/vThemes/" + ThemeName + "/Theme.css");
                    if (File.Exists(ThemeCss))
                        File.Copy(ThemeCss, ThemeCss.Replace("Theme.css", "Theme.backup.css"), true);

                    string _out = "";
                    Process _process = new Process();
                    _process.StartInfo.UseShellExecute = false;
                    _process.StartInfo.RedirectStandardInput = true;
                    _process.StartInfo.RedirectStandardError = true;
                    _process.StartInfo.RedirectStandardOutput = true;
                    _process.StartInfo.CreateNoWindow = true;
                    _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    _process.StartInfo.FileName = HttpContext.Current.Server.MapPath("~/bin/dart.exe");
                    _process.StartInfo.Arguments = " " + HttpContext.Current.Server.MapPath("~/bin/sass.snapshot") + " " + "" + ThemeScss + " " + ThemeScss.Replace("scss", "css") + " --load-path=" + HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeName + "/scss/Bootstrap/");
                    if (_process.Start())
                    {
                        _out = _process.StandardOutput.ReadToEnd();
                        _process.WaitForExit(30000);
                        if (!_process.HasExited)
                            _process.Kill();
                    }
                    IncrementCrmVersion = true;
                }

                if (themeEditorJs.Length > 0)
                {
                    string ThemeJs = HttpContext.Current.Server.MapPath("~/Portals/" + PortalID + "/vThemes/" + ThemeName + "/theme.editor.js");
                    if (!File.Exists(ThemeJs))
                    {
                        File.Create(ThemeJs).Dispose();
                    }
                    else
                    {
                        File.Copy(ThemeJs, ThemeJs.Replace("theme.editor.js", "theme.editor.backup.js"), true);
                    }
                    string themeEditorJsStr = "var vjthemeeditor={" + themeEditorJs.ToString().TrimEnd(',') + "};";
                    File.WriteAllText(ThemeJs, themeEditorJsStr);
                    IncrementCrmVersion = true;
                }

                if (IncrementCrmVersion)
                    PortalController.IncrementCrmVersion(PortalID);
            }

            public static void Save(string CategoryGuid, List<ThemeEditorValue> ThemeEditorValues)
            {
                File.WriteAllText(GetThemeEditorValueJsonPath(PortalSettings.Current.PortalId, CategoryGuid), JsonConvert.SerializeObject(ThemeEditorValues));
                CacheFactory.Clear(CacheFactory.Keys.ThemeManager);
            }
            public static bool Delete(string CategoryGuid, string Category, string SubCategory)
            {
                try
                {
                    ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalSettings.Current.PortalId, CategoryGuid);
                    if (ThemeEditorWrapper != null && ThemeEditorWrapper.ThemeEditors != null)
                    {
                        if (!string.IsNullOrEmpty(Category) && !string.IsNullOrEmpty(SubCategory))
                        {
                            List<string> FilteredGuids = ThemeEditorWrapper.ThemeEditors.Where(t => t.Category.ToLower() == Category.ToLower() && !string.IsNullOrEmpty(t.Title) && t.Title.ToLower() == SubCategory.ToLower()).Select(s => s.Guid).ToList();
                            if (FilteredGuids != null && FilteredGuids.Count > 0)
                            {
                                ThemeEditorWrapper.ThemeEditors = ThemeEditorWrapper.ThemeEditors.Where(t => !FilteredGuids.Contains(t.Guid)).ToList();
                            }
                        }
                        else
                        {
                            ThemeEditorWrapper.ThemeEditors = ThemeEditorWrapper.ThemeEditors.Where(t => t.Category.ToLower() != Category.ToLower()).ToList();
                        }

                        UpdateThemeEditorJson(PortalSettings.Current.PortalId, CategoryGuid, ThemeEditorWrapper);
                    }
                    return true;
                }
                catch (Exception ex) { ExceptionManager.LogException(ex); return false; }
            }
            private static void UpdateThemeEditorJson(int PortalID, string CategoryGuid, ThemeEditorWrapper ThemeEditorWrapper, bool CheckVisibilityPermission = true)
            {
                string ThemeEditorJsonPath = GetThemeEditorJsonPath(PortalID, CategoryGuid, CheckVisibilityPermission);
                if (ThemeEditorJsonPath.EndsWith("theme.editor.custom.json"))
                {
                    ThemeEditorWrapper.DeveloperMode = true;
                }

                string Content = JsonConvert.SerializeObject(ThemeEditorWrapper);
                File.WriteAllText(ThemeEditorJsonPath, Content);
                CacheFactory.Clear(CacheFactory.Keys.ThemeManager);
            }
            public static bool Update(string categoryGuid, ThemeEditor themeEditor)
            {
                try
                {
                    themeEditor.Sass = themeEditor.Sass.Replace("\"", "'");
                    if (string.IsNullOrEmpty(themeEditor.Guid))
                    {
                        themeEditor.Guid = Guid.NewGuid().ToString();
                    }
                    ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalSettings.Current.PortalId, categoryGuid);
                    if (ThemeEditorWrapper != null && ThemeEditorWrapper.ThemeEditors != null)
                    {
                        int index = -1;
                        ThemeEditor existingThemeEditor = GetThemeEditor(ThemeEditorWrapper.ThemeEditors, themeEditor.Guid, ref index);
                        if (existingThemeEditor != null && index >= 0)
                        {
                            string oldcat = ThemeEditorWrapper.ThemeEditors[index].Category;
                            ThemeEditorWrapper.ThemeEditors[index] = themeEditor;
                            if (string.IsNullOrEmpty(themeEditor.Title))
                            {
                                foreach (ThemeEditor ete in ThemeEditorWrapper.ThemeEditors.Where(t => !string.IsNullOrEmpty(t.Title) && t.Category == oldcat))
                                {
                                    ete.Category = themeEditor.Category;
                                }
                            }
                        }
                        else
                        {
                            ThemeEditorWrapper.ThemeEditors.Add(themeEditor);
                        }
                    }
                    else
                    {
                        if (ThemeEditorWrapper == null)
                        {
                            ThemeEditorWrapper = new ThemeEditorWrapper();
                        }

                        ThemeEditorWrapper.ThemeEditors = new List<ThemeEditor>
                        {
                            themeEditor
                        };
                    }
                    UpdateThemeEditorJson(PortalSettings.Current.PortalId, categoryGuid, ThemeEditorWrapper);
                    return true;
                }
                catch (Exception ex) { ExceptionManager.LogException(ex); return false; }
            }
            public static void BuildThemeEditor(ThemeEditor themeEditor)
            {
                List<ThemeEditorControl> NewControls = new List<ThemeEditorControl>();
                foreach (dynamic control in themeEditor.Controls)
                {
                    if (control.Type == "Slider")
                    {
                        Slider slider = JsonConvert.DeserializeObject<Slider>(control.ToString());
                        if (slider != null)
                        {
                            if (string.IsNullOrEmpty(slider.Guid))
                            {
                                slider.Guid = Guid.NewGuid().ToString();
                            }
                            slider.CustomCSS = slider.CustomCSS.Replace("\"", "'");
                            slider.PreviewCSS = slider.PreviewCSS.Replace("\"", "'");
                            NewControls.Add(slider);
                        }
                    }
                    else if (control.Type == "Dropdown")
                    {
                        Dropdown dropdown = JsonConvert.DeserializeObject<Dropdown>(control.ToString());
                        if (dropdown != null)
                        {
                            if (string.IsNullOrEmpty(dropdown.Guid))
                            {
                                dropdown.Guid = Guid.NewGuid().ToString();
                            }
                            dropdown.CustomCSS = dropdown.CustomCSS.Replace("\"", "'");
                            dropdown.PreviewCSS = dropdown.PreviewCSS.Replace("\"", "'");
                            NewControls.Add(dropdown);
                        }
                    }
                    else if (control.Type == "Color Picker")
                    {
                        ColorPicker colorPicker = JsonConvert.DeserializeObject<ColorPicker>(control.ToString());
                        if (colorPicker != null)
                        {
                            if (string.IsNullOrEmpty(colorPicker.Guid))
                            {
                                colorPicker.Guid = Guid.NewGuid().ToString();
                            }
                            colorPicker.CustomCSS = colorPicker.CustomCSS.Replace("\"", "'");
                            colorPicker.PreviewCSS = colorPicker.PreviewCSS.Replace("\"", "'");
                            NewControls.Add(colorPicker);
                        }
                    }
                    else
                    {
                        Fonts fonts = JsonConvert.DeserializeObject<Fonts>(control.ToString());
                        if (fonts != null)
                        {
                            if (string.IsNullOrEmpty(fonts.Guid))
                            {
                                fonts.Guid = Guid.NewGuid().ToString();
                            }
                            fonts.CustomCSS = fonts.CustomCSS.Replace("\"", "'");
                            fonts.PreviewCSS = fonts.PreviewCSS.Replace("\"", "'");
                            NewControls.Add(fonts);
                        }
                    }
                }
                themeEditor.Controls = new List<dynamic>();
                themeEditor.Controls.AddRange(NewControls);
            }
            public static ThemeEditorWrapper GetThemeEditors(int PortalID, string CategoryGuid, bool CheckVisibilityPermission = true)
            {
                string ThemeEditorJsonPath = GetThemeEditorJsonPath(PortalID, CategoryGuid, CheckVisibilityPermission);
                List<string> RemoveFeatureAccess = new List<string>() { };
                if (!HasAccessIOAuthClient(PortalID))
                    RemoveFeatureAccess.Add("Social Authentication Buttons");

                if (!File.Exists(ThemeEditorJsonPath))
                    File.Create(ThemeEditorJsonPath).Dispose();

                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.ThemeManager, PortalID, CategoryGuid);
                ThemeEditorWrapper result = CacheFactory.Get(CacheKey);
                if (result == null)
                {
                    result = JsonConvert.DeserializeObject<ThemeEditorWrapper>(File.ReadAllText(ThemeEditorJsonPath));
                    CacheFactory.Set(CacheKey, result);
                }

                ThemeEditorWrapper themeEditors = null;
                if (result != null)
                {
                    themeEditors = new ThemeEditorWrapper() { DeveloperMode = result.DeveloperMode, Fonts = result.Fonts, ThemeEditors = new List<ThemeEditor>() };
                    if (result.ThemeEditors != null)
                    {
                        foreach (ThemeEditor te in result.ThemeEditors)
                        {
                            if (!RemoveFeatureAccess.Contains(te.Category))
                                themeEditors.ThemeEditors.Add(te);
                        }
                    }
                }
                return themeEditors;
            }

            public static List<ThemeFont> GetFonts(int PortalID, string CategoryGuid, bool CheckVisibilityPermission = true)
            {
                List<ThemeFont> Fonts = new List<ThemeFont>();
                if (!string.IsNullOrEmpty(CategoryGuid))
                {
                    if (CategoryGuid.ToLower() == "all")
                    {
                        foreach (IThemeEditor te in GetCategories(CheckVisibilityPermission))
                        {
                            ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalID, te.Guid, CheckVisibilityPermission);
                            if (ThemeEditorWrapper != null && ThemeEditorWrapper.Fonts != null)
                            {
                                Fonts.AddRange(ThemeEditorWrapper.Fonts);
                            }
                        }
                    }
                    else
                    {
                        ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalID, CategoryGuid, CheckVisibilityPermission);
                        if (ThemeEditorWrapper != null && ThemeEditorWrapper.Fonts != null)
                        {
                            Fonts.AddRange(ThemeEditorWrapper.Fonts);
                        }
                    }
                }
                return Fonts.GroupBy(t => t.Name)
                        .Select(g => g.First())
                        .ToList();
            }
            public static List<StringTextNV> GetDDLFonts(string CategoryGuid)
            {
                List<StringTextNV> FontList = new List<StringTextNV>();
                FontList.AddRange(GetFonts(PortalSettings.Current.PortalId, CategoryGuid).Select(x => new StringTextNV { Name = x.Name, Value = x.Family }));
                return FontList;
            }
            public static void UpdateFonts(int PortalID, string CategoryGuid, dynamic data, bool CheckVisibilityPermission = true)
            {
                ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalID, CategoryGuid, CheckVisibilityPermission);

                if (ThemeEditorWrapper == null)
                {
                    ThemeEditorWrapper = new ThemeEditorWrapper();
                }

                if (ThemeEditorWrapper.Fonts == null)
                {
                    ThemeEditorWrapper.Fonts = new List<ThemeFont>();
                }

                string GUID = !string.IsNullOrEmpty(data.Guid.ToString()) ? data.Guid.ToString() : Guid.NewGuid().ToString();

                if (ThemeEditorWrapper.Fonts.Where(a => a.Guid.ToLower() == GUID.ToLower()).FirstOrDefault() != null)
                {
                    ThemeFont ThemeFont = ThemeEditorWrapper.Fonts.Where(a => a.Guid.ToLower() == GUID.ToLower()).FirstOrDefault();
                    if (ThemeFont != null)
                    {
                        ThemeFont.Name = data.Name;
                        ThemeFont.Family = data.Family;
                        ThemeFont.Css = data.Css.Replace("\"", "'");
                    }
                }
                else
                {
                    ThemeEditorWrapper.Fonts.Add(new ThemeFont { Guid = GUID, Name = data.Name.ToString(), Family = data.Family.ToString(), Css = data.Css.ToString().Replace("\"", "'") });
                }

                ThemeEditorWrapper.Fonts = ThemeEditorWrapper.Fonts.OrderBy(o => o.Name).ToList();
                UpdateThemeEditorJson(PortalID, CategoryGuid, ThemeEditorWrapper, CheckVisibilityPermission);

            }
            public static void DeleteFonts(string CategoryGuid, ThemeFont data)
            {
                ThemeEditorWrapper ThemeEditorWrapper = GetThemeEditors(PortalSettings.Current.PortalId, CategoryGuid);

                if (ThemeEditorWrapper == null)
                {
                    ThemeEditorWrapper = new ThemeEditorWrapper();
                }

                if (ThemeEditorWrapper.Fonts == null)
                {
                    ThemeEditorWrapper.Fonts = new List<ThemeFont>();
                }

                string GUID = !string.IsNullOrEmpty(data.Guid.ToString()) ? data.Guid.ToString() : Guid.NewGuid().ToString();

                if (ThemeEditorWrapper.Fonts.Where(a => a.Guid.ToLower() == GUID.ToLower()).FirstOrDefault() != null)
                {
                    ThemeFont ThemeFont = ThemeEditorWrapper.Fonts.Where(a => a.Guid.ToLower() == GUID.ToLower()).FirstOrDefault();
                    ThemeEditorWrapper.Fonts.Remove(ThemeFont);
                    UpdateThemeEditorJson(PortalSettings.Current.PortalId, CategoryGuid, ThemeEditorWrapper);
                }
            }
            internal static List<ThemeEditorValue> GetThemeEditorValues(int PortalId, string CategoryGuid)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.ThemeManager, PortalId, "Values", CategoryGuid);
                List<ThemeEditorValue> result = CacheFactory.Get(CacheKey);
                if (result == null)
                {
                    result = JsonConvert.DeserializeObject<List<ThemeEditorValue>>(File.ReadAllText(GetThemeEditorValueJsonPath(PortalId, CategoryGuid)));
                    CacheFactory.Set(CacheKey, result);
                }
                return result;
            }
            public static string GetMarkUp(string identifier, string Guid)
            {
                StringBuilder sb = new StringBuilder();
                ThemeEditorWrapper editors = GetThemeEditors(PortalSettings.Current.PortalId, Guid);
                if (editors != null && editors.ThemeEditors != null)
                {
                    List<ThemeEditorValue> themeEditorValues = GetThemeEditorValues(PortalSettings.Current.PortalId, Guid);
                    foreach (IGrouping<string, ThemeEditor> item in GetEditorGroups(PortalSettings.Current.PortalId, Guid))
                    {
                        sb.Append(GetMarkUp(identifier, item, themeEditorValues, editors.DeveloperMode, Guid));
                    }
                }
                return sb.ToString();
            }
            private static string GetMarkUp(string identifier, IGrouping<string, ThemeEditor> themeEditorGroup, List<ThemeEditorValue> themeEditorValues, bool developerMode, string Guid)
            {
                StringBuilder sb = new StringBuilder();
                if (identifier == "setting_settings")
                {
                    sb.Append("<div class=\"firstblock\"><div class=\"optiontheme mainblocks\"><i id=\"gjs-sm-caret\" class=\"fa fa-caret-right\"></i><label>" + themeEditorGroup.Key + "</label></div><div class=\"child-wrapper\">");
                }
                else
                {
                    sb.Append("<div class=\"firstblock\"><div class=\"optiontheme mainblocks\" style=\"font-weight: bold;font-size: 16px;\"><i id=\"gjs-sm-caret\" class=\"fa fa-caret-right\"></i><label>" + themeEditorGroup.Key + "</label>" + GetCategoryEditClick(themeEditorGroup, developerMode, Guid) + "</div><div class=\"child-wrapper\">");
                }

                foreach (ThemeEditor item in themeEditorGroup.OrderBy(a => a.Title).ToList())
                {
                    if (!string.IsNullOrEmpty(item.Title) && identifier == "setting_settings")
                    {
                        sb.Append("<div class=\"dropdown dropbtn optioncontrol\"><div class=\"togglelabel\" data-bs-toggle=\"dropdown\" data-placement=\"bottom-start\" aria-haspopup=\"true\" data-nodrag=\"\" aria-expanded=\"true\"><label>" + item.Title + "</label><a id=\"dropdownMenuLink\" class=\"dropdownmenu grptitle\" ><em class=\"fas fa-chevron-down\"></em></a></div><div class=\"dropdown-menu subtMenu\" aria-labelledby=\"dropdownMenuLink\">");
                    }

                    if (identifier == "setting_settings")
                    {
                        foreach (dynamic ctl in item.Controls)
                        {
                            if (ctl.Type == "Slider")
                            {
                                Slider slider = JsonConvert.DeserializeObject<Slider>(ctl.ToString());
                                if (slider != null)
                                {
                                    string value = GetGuidValue(themeEditorValues, slider);
                                    sb.Append("<div class=\"field csslider optioncontrol\" id=" + item.Guid + "><label>" + slider.Title + "</label>  <span class=\"input-wrapper\"><input type=\"range\" value=" + value + " guid=" + slider.Guid + " name=" + slider.Title + " value=" + value + " min=" + slider.RangeMin + " max=" + slider.RangeMax + " /><input type=\"number\" guid=" + slider.Guid + " name=" + slider.Title + " value=" + value + " min=" + slider.RangeMin + " max=" + slider.RangeMax + "><span class=\"units\">" + slider.Suffix + "</span></span> " + GetCssMarkup(slider.Guid, slider.CustomCSS, slider.PreviewCSS, slider.LessVariable, item.Sass) + GetPvNotAvailableMarkup(slider.PreviewCSS) + "</div>");
                                }
                            }
                            else if (ctl.Type == "Dropdown")
                            {
                                Dropdown dropdown = JsonConvert.DeserializeObject<Dropdown>(ctl.ToString());
                                if (dropdown != null)
                                {
                                    sb.Append("<div class=\"dropdownselect optioncontrol\" id=" + item.Guid + "><div class=\"dropdownlabel\" ><label >" + dropdown.Title + ":</label></div>");
                                    sb.Append("<div class=\"dropdownOption\"><select  guid=" + dropdown.Guid + ">");
                                    foreach (dynamic opt in dropdown.Options)
                                    {
                                        string Key = string.Empty;
                                        string Value = string.Empty;
                                        foreach (JToken attribute in JToken.Parse(opt.ToString()))
                                        {
                                            JProperty jProperty = attribute.ToObject<JProperty>();
                                            if (jProperty != null)
                                            {
                                                Key = jProperty.Name;
                                                Value = jProperty.Value.ToString();
                                            }
                                        }
                                        string value = GetGuidValue(themeEditorValues, dropdown);
                                        if (Key == value)
                                        {
                                            sb.Append("<option selected=\"selected\" value=" + Key + ">" + Value + "</option>");
                                        }
                                        else
                                        {
                                            sb.Append("<option value=" + Key + ">" + Value + "</option>");
                                        }
                                    }
                                    sb.Append("</select><span class=\"units\">" + dropdown.Suffix + "</span></div>");
                                    //sb.Append("<span class=\"units\">" + dropdown.Suffix + "</span>");
                                    sb.Append(GetCssMarkup(dropdown.Guid, dropdown.CustomCSS, dropdown.PreviewCSS, dropdown.LessVariable, item.Sass) + GetPvNotAvailableMarkup(dropdown.PreviewCSS) + "</div>");
                                }
                            }
                            else if (ctl.Type == "Color Picker")
                            {
                                ColorPicker colorPicker = JsonConvert.DeserializeObject<ColorPicker>(ctl.ToString());
                                if (colorPicker != null)
                                {
                                    sb.Append("<div class=\"field fieldcolor optioncontrol\" id=" + item.Guid + "><label>" + colorPicker.Title + " </label>");
                                    string value = GetGuidValue(themeEditorValues, colorPicker);
                                    sb.Append("<span class=\"input-wrapper\"><input class=\"color\" guid=" + colorPicker.Guid + " type=\"text\" value=\"" + value + "\">");
                                    sb.Append("<span class=\"units\">" + colorPicker.Suffix + "</span>");
                                    sb.Append(GetCssMarkup(colorPicker.Guid, colorPicker.CustomCSS, colorPicker.PreviewCSS, colorPicker.LessVariable, item.Sass) + "</span>" + GetPvNotAvailableMarkup(colorPicker.PreviewCSS) + "</div>");
                                }
                            }
                            else
                            {
                                Fonts fonts = JsonConvert.DeserializeObject<Fonts>(ctl.ToString());
                                if (fonts != null)
                                {
                                    sb.Append("<div class=\"dropdownselect optioncontrol\" id=" + item.Guid + "><div class=\"dropdownlabel\" ><label>" + fonts.Title + ":</label></div>");
                                    sb.Append("<div class=\"dropdownOption\"><select  guid=" + fonts.Guid + ">");
                                    foreach (StringTextNV opt in GetDDLFonts("all"))
                                    {
                                        string value = GetGuidValue(themeEditorValues, fonts);
                                        if (opt.Value == value)
                                        {
                                            sb.Append("<option selected=\"selected\" value=\"" + opt.Value + "\">" + opt.Name + "</option>");
                                        }
                                        else
                                        {
                                            sb.Append("<option value=\"" + opt.Value + "\">" + opt.Name + "</option>");
                                        }
                                    }
                                    sb.Append("</select><span class=\"units\">" + fonts.Suffix + "</span></div>");
                                    // sb.Append("<span class=\"units\">" + fonts.Suffix + "</span>");
                                    sb.Append(GetCssMarkup(fonts.Guid, fonts.CustomCSS, fonts.PreviewCSS, fonts.LessVariable, item.Sass) + GetPvNotAvailableMarkup(fonts.PreviewCSS) + "</div>");
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(item.Title))
                        {
                            sb.Append("</div></div>");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.Title))
                        {
                            sb.Append("<div class=\"dropdown dropbtn optioncontrol\"><label style=\"font-weight: 600;\">" + item.Title + "</label>" + GetSubCategoryEditClick(item, developerMode, Guid) + "</div>");
                        }
                    }
                }
                sb.Append("</div></div>");
                return sb.ToString();
            }
            private static string GetPvNotAvailableMarkup(string previewCSS)
            {
                if (string.IsNullOrEmpty(previewCSS))
                {
                    return "<span class=\"PvNotAvailable\" title=\"Preview not available. Changes will be reflected after theme is saved.\"><em class=\"far fa-eye-slash\" ></em></span>";
                }
                return string.Empty;
            }
            private static string GetSubCategoryEditClick(ThemeEditor item, bool developerMode, string Guid)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"dropdown float-end dropbtn\">");
                sb.Append("<a id=\"dropdownMenuLink\" class=\"dropdownmenu\" data-bs-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"><em class=\"fas fa-ellipsis-v\"></em></a>");
                sb.Append("<div class=\"dropdown-menu\" aria-labelledby=\"dropdownMenuLink\">");
                sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"OpenPopUp('#!/edit/" + Guid + "/" + item.Guid + "')\"><em class=\"fas fa-cog mr-xs\"></em><span>Settings</span></a>");
                if (developerMode)
                {
                    sb.Append("<hr class=\"small\" />");
                    sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"DeleteSubCategory('" + item.Category + "','" + item.Title + "')\"><em class=\"fas fa-trash mr-xs\"></em><span>Delete</span></a>");
                }
                sb.Append("</div>");
                sb.Append("</div>");
                return sb.ToString();
            }
            private static string GetCategoryEditClick(IGrouping<string, ThemeEditor> themeEditorGroup, bool developerMode, string Guid)
            {
                StringBuilder sb = new StringBuilder();

                if (developerMode)
                {
                    sb.Append("<div class=\"dropdown float-end dropbtn\">");
                    sb.Append("<a id=\"dropdownMenuLink\" class=\"dropdownmenu\" data-bs-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"><em class=\"fas fa-ellipsis-v\"></em></a>");
                    sb.Append("<div class=\"dropdown-menu\" aria-labelledby=\"dropdownMenuLink\">");
                    if (themeEditorGroup.Where(g => string.IsNullOrEmpty(g.Title) == true && g.Controls.Count == 0).FirstOrDefault() != null)
                    {
                        string type = themeEditorGroup.Where(g => string.IsNullOrEmpty(g.Title) == true && g.Controls.Count == 0).FirstOrDefault().Guid;
                        if (string.IsNullOrEmpty(type))
                            type = "new";
                        sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"OpenPopUp('#!/edit/" + Guid + "/" + themeEditorGroup.Key + "/" + type + "')\"><em class=\"fas fa-cog mr-xs\"></em><span>Settings</span></a>");
                    }
                    else if (themeEditorGroup.Where(g => string.IsNullOrEmpty(g.Title) == true && g.Controls.Count > 0).FirstOrDefault() != null)
                    {
                        sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"OpenPopUp('#!/edit/" + Guid + "/" + themeEditorGroup.Where(g => string.IsNullOrEmpty(g.Title) == true && g.Controls.Count > 0).FirstOrDefault().Guid + "')\"><em class=\"fas fa-cog mr-xs\"></em><span>Settings</span></a>");
                    }
                    else
                    {
                        string type = "new";
                        if (themeEditorGroup != null && themeEditorGroup.FirstOrDefault() != null)
                            type = themeEditorGroup.FirstOrDefault().Guid;
                        sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"OpenPopUp('#!/edit/" + Guid + "/" + themeEditorGroup.Key + "/" + type + "')\"><em class=\"fas fa-cog mr-xs\"></em><span>Settings</span></a>");
                    }

                    sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"OpenPopUp('#!/edit/" + Guid + "/" + themeEditorGroup.Key + "/newsub')\"><em class=\"fas fa-plus mr-xs\"></em><span>Add Subcategory</span></a>");
                    sb.Append("<hr class=\"small\" />");
                    sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"Delete('" + themeEditorGroup.Key + "')\"><em class=\"fas fa-trash mr-xs\"></em><span>Delete</span></a>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                }
                else if (!developerMode && themeEditorGroup.Where(g => string.IsNullOrEmpty(g.Title) == true && g.Controls.Count > 0).FirstOrDefault() != null)
                {
                    sb.Append("<div class=\"dropdown float-end dropbtn\">");
                    sb.Append("<a id=\"dropdownMenuLink\" class=\"dropdownmenu\" data-bs-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"><em class=\"fas fa-ellipsis-v\"></em></a>");
                    sb.Append("<div class=\"dropdown-menu\" aria-labelledby=\"dropdownMenuLink\">");
                    sb.Append("<a class=\"dropdown-item box-icon\" ng-click=\"OpenPopUp('#!/edit/" + Guid + "/" + themeEditorGroup.Where(g => string.IsNullOrEmpty(g.Title) == true && g.Controls.Count > 0).FirstOrDefault().Guid + "')\"><em class=\"fas fa-cog mr-xs\"></em><span>Settings</span></a>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                }
                return sb.ToString();
            }
            private static string GetGuidValue(List<ThemeEditorValue> themeEditorValues, ThemeEditorControl control)
            {
                string result = control.DefaultValue;
                if (themeEditorValues != null && themeEditorValues.Where(t => t.Guid.ToLower() == control.Guid.ToLower()).FirstOrDefault() != null)
                {
                    result = themeEditorValues.Where(t => t.Guid.ToLower() == control.Guid.ToLower()).FirstOrDefault().Value;
                }
                return result;
            }
            private static string GetCssMarkup(string Guid, string CustomCSS, string PreviewCSS, string LessVariable, string Sass)
            {
                return "<input type=\"hidden\" id=" + Guid + " value=\"" + LessVariable + "\" css=\"" + CustomCSS + "\" prevcss=\"" + PreviewCSS + "\" sass=\"" + Sass + "\">";
            }
            private static string GetThemeEditorJsonPath(int PortalID, string CategoryGuid, bool CheckVisibilityPermission = true)
            {
                IThemeEditor themeEditor = GetCategories(CheckVisibilityPermission).Where(c => c.Guid.ToLower() == CategoryGuid.ToLower()).FirstOrDefault();
                if (themeEditor != null)
                {
                    string path = themeEditor.JsonPath.Replace("{{PortalID}}", PortalID.ToString()).Replace("{{ThemeName}}", GetCurrent(PortalID).Name);
                    string folder = Path.GetDirectoryName(path);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    return path;
                }
                return string.Empty;
            }
            private static string GetThemeEditorValueJsonPath(int PortalId, string CategoryGuid)
            {
                string FolderPath = HttpContext.Current.Server.MapPath("~/Portals/" + PortalId + "/vThemes/" + GetCurrent(PortalId).Name + "/editor/" + CategoryGuid);

                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                if (!File.Exists(FolderPath + "\\theme.json"))
                {
                    File.Create(FolderPath + "\\theme.json").Dispose();
                }

                return FolderPath + "\\theme.json";
            }

            private static bool HasAccessIOAuthClient(int PortalID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.IOAuthClient_Extension, PortalID);
                List<IOAuthClient> OAuthClient = CacheFactory.Get(CacheKey);
                if (OAuthClient == null)
                {
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                    List<IOAuthClient> ServiceInterfaceAssemblies = new List<IOAuthClient>();
                    foreach (string Path in binAssemblies)
                    {
                        try
                        {

                            //get all assemblies 
                            IEnumerable<IOAuthClient> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                        where t != (typeof(IOAuthClient)) && (typeof(IOAuthClient).IsAssignableFrom(t))
                                                                        select Activator.CreateInstance(t) as IOAuthClient;

                            ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IOAuthClient>());
                        }
                        catch { continue; }
                    }
                    OAuthClient = ServiceInterfaceAssemblies;
                    CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                }

                return OAuthClient.Count > 0;

            }
            private static IEnumerable<IGrouping<string, ThemeEditor>> GetEditorGroups(int PortalID, string Guid, bool CheckVisibilityPermission = true)
            {
                List<IGrouping<string, ThemeEditor>> themeEditorGroups = GetThemeEditors(PortalID, Guid, CheckVisibilityPermission).ThemeEditors.GroupBy(g => g.Category).OrderBy(a => a.Key).ToList();
                if (themeEditorGroups != null && themeEditorGroups.Count > 0)
                {
                    IGrouping<string, ThemeEditor> siteItem = themeEditorGroups.Where(a => a.Key == "Site").FirstOrDefault();
                    if (siteItem != null)
                    {
                        themeEditorGroups.Remove(siteItem);
                        themeEditorGroups.Insert(0, siteItem);
                    }
                }
                return themeEditorGroups;
            }


        }
    }
}
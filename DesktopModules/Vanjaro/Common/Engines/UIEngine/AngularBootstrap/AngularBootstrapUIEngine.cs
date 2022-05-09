using DotNetNuke.Abstractions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Globals;
using Vanjaro.Common.Permissions;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Engines
{
    public class AngularBootstrapUIEngine
    {

        public static dynamic GetMarkup(Dictionary<string, string> UIEngineInfo, dynamic UIData)
        {

            StringBuilder Error;
            bool RefreshPage = false;
            Error = new StringBuilder();
            dynamic data = new
            {
                markup = AngularBootstrapUIEngine.BuildUIDynamically(UIEngineInfo, UIData, Error),
                data = AngularBootstrapUIEngine.BuildDataControllerScript(UIData, Error),
                identifier = UIEngineInfo["identifier"],
                script = UIEngineInfo.ContainsKey("InitScript") ? UIEngineInfo["InitScript"] : string.Empty,
                prescript = UIEngineInfo.ContainsKey("PreInitScript") ? UIEngineInfo["PreInitScript"] : string.Empty,
                refresh = RefreshPage
            };

            Utilities.DataCache.SetCache<dynamic>(data, UIEngineInfo["moduleid"] + UIEngineInfo["identifier"]);

            if (Error.Length > 0)
            {
                return Error.ToString();
            }
            else
            {
                return data;
            }
        }
        private static string BuildUIDynamically(Dictionary<string, string> UIEngineInfo, dynamic UIData, StringBuilder Error)
        {

            string uiMarkup = null;

            try
            {
                UIEngineInfo["InitScript"] += Environment.NewLine + "$('[data-bs-toggle=\"tooltip\"]').tooltip();";
                //Get UI Markup From Cache
                Dictionary<string, string> Markup = Vanjaro.Common.Utilities.DataCache.GetItemFromCache<Dictionary<string, string>>(Constants.UIFrameworkCacheKey + UIEngineInfo["appname"]);

                if (Markup == null)
                {
                    try { Markup = ReadMarkup(UIEngineInfo["overloadpath"], Markup, UIEngineInfo); }
                    catch (Exception ex) { Error.Append("<br /><span class=\"MNormalRed\">Failed to read markup while building UI.</span><p>" + ex.Message + ".</p>"); }
                }

                //Valid Layout Name is required to continue
                if (!string.IsNullOrEmpty(UIEngineInfo["layoutmarkup"]))
                {
                    List<dynamic> UIElements = new List<dynamic>();

                    //Build UI Elements by reading the HTML Markup
                    try { BuildUIElements(UIEngineInfo, UIElements, Markup); }
                    catch (Exception ex) { Error.Append("<br /><span class=\"MNormalRed\">Failed to build ui elements.</span><p>" + ex.Message + ".</p><p>" + ex.StackTrace + ".</p>"); }

                    Dictionary<string, StringBuilder> MasterLayouts = new Dictionary<string, StringBuilder>();
                    Dictionary<string, StringBuilder> TempLayouts = new Dictionary<string, StringBuilder>();

                    string LastLayout = string.Empty;
                    string LastLayoutAttr = string.Empty;
                    string LastLayoutClass = string.Empty;
                    string LastViewOrder = string.Empty;
                    string UILayout = Markup["containers.uilayout"];

                    foreach (dynamic _UIElement in UIElements)
                    {
                        uiMarkup = Markup["layouts." + _UIElement.EngineLayout.ToString().ToLower()];

                        if (string.IsNullOrEmpty(LastLayout))
                        {
                            LastLayout = _UIElement.Layout;
                        }

                        if (string.IsNullOrEmpty(LastViewOrder))
                        {
                            LastViewOrder = _UIElement.LayoutViewOrder;
                        }

                        try { MapDataToUIElement(UIData, _UIElement); }
                        catch (Exception ex) { Error.Append("<br /><span class=\"MNormalRed\">Failed to map data to ui element.</span><p>" + ex.Message + ".</p>"); }

                        //Localize it
                        try { LocalizeUIData(UIEngineInfo, _UIElement); }
                        catch (Exception ex) { Error.Append("<br /><span class=\"MNormalRed\">Failed to localize UI.</span><p>" + ex.Message + ".</p>"); }


                        if (!TempLayouts.ContainsKey(_UIElement.Layout.ToString()))
                        {
                            TempLayouts.Add(_UIElement.Layout.ToString(), new StringBuilder());
                            MasterLayouts.Add(_UIElement.Layout.ToString(), new StringBuilder());
                        }

                        if (LastViewOrder != _UIElement.LayoutViewOrder.ToString())
                        {
                            UILayout = UILayout.Replace("[layout-attributes]", LastLayoutAttr);
                            UILayout = UILayout.Replace("[class]", LastLayoutClass);
                            UILayout = UILayout.Replace("[layout]", TempLayouts[LastLayout].ToString());

                            MasterLayouts[LastLayout].Append(UILayout);

                            UILayout = Markup["containers.uilayout"];
                            LastViewOrder = _UIElement.LayoutViewOrder;
                            LastLayout = _UIElement.Layout.ToString();
                            LastLayoutAttr = _UIElement.LayoutAttr;
                            LastLayoutClass = _UIElement.LayoutClass;
                            TempLayouts[LastLayout].Clear();
                        }

                        if (string.IsNullOrEmpty(LastLayoutAttr))
                        {
                            LastLayoutAttr = _UIElement.LayoutAttr;
                        }

                        if (string.IsNullOrEmpty(LastLayoutClass))
                        {
                            LastLayoutClass = _UIElement.LayoutClass;
                        }

                        TempLayouts[_UIElement.Layout.ToString()].Append(GenerateUI(Markup, _UIElement, UIEngineInfo));
                    }

                    if (TempLayouts.ContainsKey(LastLayout) && TempLayouts[LastLayout].Length > 0)
                    {
                        UILayout = UILayout.Replace("[layout-attributes]", LastLayoutAttr);
                        UILayout = UILayout.Replace("[class]", LastLayoutClass);
                        UILayout = UILayout.Replace("[layout]", TempLayouts[LastLayout].ToString());

                        MasterLayouts[LastLayout].Append(UILayout);

                    }

                    foreach (string Layout in MasterLayouts.Keys)
                    {
                        uiMarkup = uiMarkup.Replace("[uilayout:" + Layout + "]", MasterLayouts[Layout].ToString());
                    }
                }
            }
            catch (Exception ex) { Error.Append("<br /><span class=\"MNormalRed\">Failed to build UI dynamically.</span><p>" + ex.Message + ".</p>"); }
            return new DNNLocalizationEngine(Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"]), Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], null), UIEngineInfo["showmissingkeys"] == "true" ? true : false).Parse(uiMarkup);
        }

        private static void LocalizeUIData(Dictionary<string, string> UIEngineInfo, dynamic _UIElement)
        {
            string ResourceFile = Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"]);
            string MissingPrefix = Localization.LocalMissingPrefix;
            string Key = _UIElement.Name.ToString();
            bool ShowMissingKeys = UIEngineInfo["showmissingkeys"] == "true" ? true : false;

            if (_UIElement.Resource != null && _UIElement.ResourceKey != null)
            {
                ResourceFile = Localization.GetSharedResourceFile(UIEngineInfo["uitemplatepath"]);
                MissingPrefix = Localization.SharedMissingPrefix;
                Key = _UIElement.ResourceKey.ToString();
            }

            _UIElement.Label = Localization.Get(Key, "Text", ResourceFile, ShowMissingKeys, MissingPrefix);
            _UIElement.Tooltip = Localization.Get(Key, "Tooltip", ResourceFile, ShowMissingKeys, MissingPrefix);

            if (((IDictionary<string, object>)_UIElement).ContainsKey("Control") == true && (_UIElement.Control == "textbox" || _UIElement.Control == "integer" || _UIElement.Control == "colorpicker"))
            {
                if (_UIElement.DefaultSuffix != null)
                {
                    _UIElement.Suffix = Localization.Get(Key, "Suffix", ResourceFile, ShowMissingKeys, MissingPrefix);
                }

                if (_UIElement.DefaultPrefix != null)
                {
                    _UIElement.Prefix = Localization.Get(Key, "Prefix", ResourceFile, ShowMissingKeys, MissingPrefix);
                }

                if (_UIElement.DefaultHelp != null)
                {
                    _UIElement.Helptext = Localization.Get(Key, "Help", ResourceFile, ShowMissingKeys, MissingPrefix);
                }
            }

            if (((IDictionary<string, object>)_UIElement).ContainsKey("Control") == true && (_UIElement.Control == "toggle" || _UIElement.Control == "checkboxtoggle"))
            {
                _UIElement.ToggleOn = Localization.Get(Key, "ON", ResourceFile, ShowMissingKeys, MissingPrefix);
                _UIElement.ToggleOff = Localization.Get(Key, "OFF", ResourceFile, ShowMissingKeys, MissingPrefix);
            }
        }

        private static Dictionary<string, string> ReadMarkup(string OverLoadPath, Dictionary<string, string> Markup, Dictionary<string, string> UIEngineInfo)
        {
            Markup = new Dictionary<string, string>();

            //Read Markup from Template
            if (Directory.Exists(HttpContext.Current.Server.MapPath("~/" + Constants.UIFrameworkElementsPath)))
            {
                foreach (string file in Directory.EnumerateFiles(HttpContext.Current.Server.MapPath("~/" + Constants.UIFrameworkElementsPath), Constants.UIFrameworkExtensionPattern))
                {
                    Markup.Add(("Elements." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower(), File.ReadAllText(file));
                }
            }

            if (Directory.Exists(HttpContext.Current.Server.MapPath("~/" + Constants.UIFrameworkContainersPath)))
            {
                foreach (string file in Directory.EnumerateFiles(HttpContext.Current.Server.MapPath("~/" + Constants.UIFrameworkContainersPath), Constants.UIFrameworkExtensionPattern))
                {
                    Markup.Add(("Containers." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower(), File.ReadAllText(file));
                }
            }

            if (Directory.Exists(HttpContext.Current.Server.MapPath("~/" + Constants.UIFrameworkLayoutPath)))
            {
                foreach (string file in Directory.EnumerateFiles(HttpContext.Current.Server.MapPath("~/" + Constants.UIFrameworkLayoutPath), Constants.UIFrameworkExtensionPattern))
                {
                    Markup.Add(("Layouts." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower(), new DNNLocalizationEngine(Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"]), Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], null), UIEngineInfo["showmissingkeys"] == "true" ? true : false).Parse(File.ReadAllText(file)));
                }
            }

            if (!string.IsNullOrEmpty(OverLoadPath))
            {
                if (Directory.Exists(OverLoadPath + "\\Elements\\"))
                {
                    foreach (string file in Directory.EnumerateFiles(OverLoadPath + "\\Elements\\", Constants.UIFrameworkExtensionPattern))
                    {
                        if (Markup.ContainsKey(("Elements." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower()))
                        {
                            Markup["" + ("Elements." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower() + ""] = File.ReadAllText(file);
                        }
                        else
                        {
                            Markup.Add(("Elements." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower(), File.ReadAllText(file));
                        }
                    }
                }

                if (Directory.Exists(OverLoadPath + "\\Containers\\"))
                {
                    foreach (string file in Directory.EnumerateFiles(OverLoadPath + "\\Containers\\", Constants.UIFrameworkExtensionPattern))
                    {
                        if (Markup.ContainsKey(("Containers." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower()))
                        {
                            Markup["" + ("Containers." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower() + ""] = File.ReadAllText(file);
                        }
                        else
                        {
                            Markup.Add(("Containers." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower(), File.ReadAllText(file));
                        }
                    }
                }

                if (Directory.Exists(OverLoadPath + "\\Layouts\\"))
                {
                    foreach (string file in Directory.EnumerateFiles(OverLoadPath + "\\Layouts\\", Constants.UIFrameworkExtensionPattern))
                    {
                        if (Markup.ContainsKey(("Layouts." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower()))
                        {
                            Markup["" + ("Layouts." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower() + ""] = File.ReadAllText(file);
                        }
                        else
                        {
                            Markup.Add(("Layouts." + file.Substring(file.LastIndexOf("\\") + 1).Replace(Constants.UIFrameworkMarkupExtension, "")).ToLower(), File.ReadAllText(file));
                        }
                    }
                }
            }

            //Add to cache
            Vanjaro.Common.Utilities.DataCache.SetCache<Dictionary<string, string>>(Markup, Constants.UIFrameworkCacheKey + UIEngineInfo["appname"]);
            return Markup;
        }
        private static void BuildUIElements(Dictionary<string, string> UIEngineInfo, List<dynamic> UIElements, Dictionary<string, string> Markup)
        {
            dynamic layoutData = Json.Deserialize<dynamic>(UIEngineInfo["layoutmarkup"]);

            JArray uilayout = new JArray();

            if (layoutData.child.GetType().Name == "JObject")
            {
                JObject o = layoutData.uiengine.uilayout as JObject;
                uilayout.Add(o);
            }
            else
            {
                uilayout = layoutData.child;
            }

            int UILayoutCounter = 0;

            foreach (dynamic elementLayout in uilayout)
            {
                JArray uielement = new JArray();
                if (elementLayout.child.GetType().Name == "JObject")
                {
                    JObject o = elementLayout.child as JObject;
                    uielement.Add(o);
                }
                else
                {
                    uielement = elementLayout.child;
                }

                int UIElementCounter = 0;


                foreach (dynamic element in uielement)
                {
                    dynamic data = new ExpandoObject();

                    data.EngineLayout = layoutData.attr.layout ?? "default";
                    data.ShowMissingKey = layoutData.attr.showmissingkeys;

                    string LayoutAttributes = string.Empty;
                    string LayoutClass = string.Empty;

                    CreateAttributes(elementLayout, ref LayoutAttributes, ref LayoutClass);

                    data.LayoutAttr = LayoutAttributes;
                    data.LayoutClass = LayoutClass;

                    if (element.tag == "uitemplate")
                    {
                        ProcessCustomTemplate(elementLayout, UILayoutCounter.ToString(), element, UIElements, data);
                    }
                    else if (element.tag == "uielement")
                    {

                        data.Name = element.attr.name;

                        //Check if a Label is explictly provided; otherwise, use Name as label
                        data.Label = element.attr.label;

                        data.Tooltip = element.attr.tooltip;


                        //Suffix
                        if (element.attr.suffix != null)
                        {
                            data.DefaultSuffix = element.attr.suffix;
                        }
                        else
                        {
                            data.DefaultSuffix = null;
                        }

                        //Prefix
                        if (element.attr.prefix != null)
                        {
                            data.DefaultPrefix = element.attr.prefix;
                        }
                        else
                        {
                            data.DefaultPrefix = null;
                        }

                        //Helptext
                        if (element.attr.help != null)
                        {
                            data.DefaultHelp = element.attr.help;
                        }
                        else
                        {
                            data.DefaultHelp = null;
                        }

                        //Shared Resource Key                        
                        if (element.attr.resource != null && (element.attr.resource == "shared"))
                        {
                            data.Resource = element.attr.resource;
                            if (element.attr.resourcekey != null)
                            {
                                data.ResourceKey = element.attr.resourcekey;
                            }
                            else
                            {
                                data.ResourceKey = null;
                            }
                        }
                        else
                        {
                            data.Resource = null;
                            data.ResourceKey = null;

                        }



                        if (data.Label == null)
                        {
                            data.Label = data.Name;
                        }

                        if (data.Tooltip == null)
                        {
                            data.Tooltip = data.Name;
                        }

                        if (element.attr.container != null)
                        {
                            data.Container = element.attr.container.ToString();
                        }

                        data.Control = element.attr.control;

                        if (data.Control == null)
                        {
                            data.Control = "textbox";
                        }
                        else
                        {
                            data.Control = data.Control.ToString().ToLower();
                        }

                        if (element.attr.label != null)
                        {
                            data.LabelEnabled = element.attr.label.ToString();
                        }

                        data.Labelclass = elementLayout.attr.labelclass;

                        if (element.attr.tooltip != null)
                        {
                            data.TooltipEnabled = element.attr.tooltip.ToString();
                        }

                        if (data.Control == "line")
                        {
                            data.LabelEnabled = "false";
                        }

                        if (data.Control == "texteditor")
                        {
                            if (element.attr.profile != null)
                            {
                                data.profile = element.attr.profile.ToString();
                            }
                        }

                        data.ToggleOn = "ON";
                        data.ToggleOff = "OFF";

                        data.ID = UIEngineInfo["identifier"] + UILayoutCounter.ToString() + data.Control + UIElementCounter.ToString();

                        data.Layout = elementLayout.attr.name;
                        data.LayoutViewOrder = UILayoutCounter.ToString();

                        if (data.Layout == null)
                        {
                            data.Layout = "default";
                        }

                        if (element.attr.ngmodel != null)
                        {
                            data.ngmodel = element.attr.ngmodel;
                        }
                        else
                        {
                            data.ngmodel = "ui.data." + element.attr.name + ".Value";
                        }

                        if (element.attr.ngoptions != null)
                        {
                            data.ngoptions = element.attr.ngoptions;
                        }
                        else
                        {
                            data.ngoptions = "ui.data." + element.attr.name + ".Options";
                        }

                        string Attributes = string.Empty;
                        string ContainerAttributes = string.Empty;
                        string Class = string.Empty;
                        string TagsAutocompleteAttributes = string.Empty;

                        CreateAttributes(element, ref Attributes, ref Class, ref ContainerAttributes, ref TagsAutocompleteAttributes);

                        if (element.attr.control != "button")
                        {
                            if (element.attr.width != null)
                            {
                                Attributes += "style" + "= \"width: " + element.attr.width.Value.ToString() + "\" ";
                            }
                            else if (elementLayout.attr.width != null)
                            {
                                Attributes += "style" + "= \"width: " + elementLayout.attr.width.Value.ToString() + "\" ";
                            }
                        }

                        data.Class = Class;
                        data.Attributes = Attributes;
                        data.ContainerAttributes = ContainerAttributes;
                        data.TagsAutocompleteAttributes = TagsAutocompleteAttributes;

                        string Validation = string.Empty;
                        CreateValidation(element, ref Validation);
                        data.Validation = Validation;

                        if (data.Class == null)
                        {
                            data.Class = string.Empty;
                        }

                        if (element.attr.control == "submit")
                        {
                            if (element.attr.cancel != null)
                            {
                                data.Cancel = element.attr.cancel;
                            }
                        }
                        else if (element.attr.control == "button")
                        {
                            if (element.attr.icon != null)
                            {
                                data.icon = element.attr.icon;
                            }
                        }
                        else if (element.attr.control == "tags")
                        {
                            data.ngmodel = element.attr.data;
                            if (element.attr.autocomplete != null && element.attr.autocomplete == "true")
                            {
                                data.autocompletesource = element.attr.autocompletesource;
                            }
                        }
                        else if (element.attr.control == "treeview")
                        {
                            data.removenode = "remove(this)";
                            if (element.attr.removenode != null && element.attr.removenode != "")
                            {
                                data.removenode = element.attr.removenode;
                            }

                            if (element.attr.editnode != null && element.attr.editnode != "")
                            {
                                data.editnode = element.attr.editnode;
                            }

                            if (element.attr.collapsed != null && element.attr.collapsed != "")
                            {
                                data.collapsed = element.attr.collapsed;
                            }

                            data.enableselect = false;
                            if (element.attr.enableselect != null && element.attr.enableselect != "" && element.attr.enableselect == "true")
                            {
                                data.enableselect = true;
                            }
                        }
                        else if (element.attr.control == "upload")
                        {
                            if (element.attr.mode != null && element.attr.mode.Value == "drop")
                            {
                                data.Control = "uploaddrop";
                            }
                            else if (element.attr.mode != null && element.attr.mode.Value == "file")
                            {
                                data.Control = "uploadinput";
                            }
                            else if (element.attr.mode != null && element.attr.mode.Value == "advance")
                            {
                                data.Control = "uploaddefault";
                            }
                            else if (element.attr.mode != null && element.attr.mode.Value == "both")
                            {
                                data.Control = "uploadboth";
                            }
                            else
                            {
                                data.Control = "uploadboth";
                            }

                            data.maxsize = element.attr.maxsize;
                            data.filetypes = element.attr.filetypes;
                            data.folders = element.attr.folders;
                            data.files = element.attr.files;
                            data.autopaging = element.attr.autopaging;
                            if (data.autopaging == null)
                            {
                                data.autopaging = true;
                            }

                            data.autoupload = element.attr.autoupload;
                            data.removedelete = element.attr.removedelete;
                            data.drop = element.attr.drop;

                            if (element.attr.disableselectfile != null)
                            {
                                data.disableselectfile = element.attr.disableselectfile;
                            }
                            else
                            {
                                data.disableselectfile = "false";
                            }
                        }
                        else if (element.attr.control == "grid")
                        {

                            if (element.attr.sortable != null && element.attr.sortable == "true")
                            {
                                //Sortable Key
                                if (element.attr.sortablekey != null)
                                {
                                    data.SortableKey = element.attr.sortablekey;
                                }
                                else
                                {
                                    data.SortableKey = "false";
                                }

                                //Sortable Handle
                                if (element.attr.sortablehandle != null)
                                {
                                    data.SortableHandle = element.attr.sortablehandle;
                                }
                                else
                                {
                                    data.SortableHandle = null;
                                }

                                data.Sortable = element.attr.sortable;
                            }
                            else
                            {
                                data.Sortable = "false";
                            }

                            if (element.attr.sortableupdate != null)
                            {
                                data.SortableUpdate = element.attr.sortableupdate;
                            }
                            else
                            {
                                data.SortableUpdate = "";
                            }

                            if (element.attr.search != null)
                            {
                                data.Search = element.attr.search;
                            }

                            if (element.attr.paged != null && element.attr.paged == "true" && element.attr.pagesize != null)
                            {
                                if (element.attr.xpage != null)
                                {
                                    data.Xpage = element.attr.xpage;
                                }
                                else
                                {
                                    data.Xpage = string.Empty;
                                }

                                if (element.attr.paged != null)
                                {
                                    data.Paged = element.attr.paged;
                                }
                                else
                                {
                                    data.Paged = "false";
                                }

                                if (element.attr.pagesize != null)
                                {
                                    data.PageSize = element.attr.pagesize;
                                }
                                else
                                {
                                    data.PageSize = string.Empty;
                                }
                            }
                            else
                            {
                                data.Paged = "false";
                                data.PageSize = string.Empty;
                                data.Xpage = string.Empty;

                            }

                            JArray uielementcolumn = new JArray();
                            if (element.child.GetType().Name == "JObject")
                            {
                                JObject o = element.child as JObject;
                                uielementcolumn.Add(o);
                            }
                            else
                            {
                                uielementcolumn = element.child;
                            }

                            dynamic columndata = new List<dynamic>();
                            int ColumnCounter = 0;
                            foreach (dynamic column in uielementcolumn)
                            {
                                dynamic _data = new ExpandoObject();
                                _data.displayname = column.attr.name;
                                _data.data = column.attr.data;
                                _data.format = column.attr.format;
                                _data.sort = column.attr.sort;
                                _data.stratio = column.attr.width;
                                _data.filter = column.attr.filter;
                                _data.filtertext = column.attr.filtertext;
                                _data.control = column.attr.control;
                                _data.accessroles = column.attr.accessroles;

                                string _Attributes = string.Empty;
                                string _Class = string.Empty;
                                string _ContainerAttributes = string.Empty;
                                string _TagsAutocompleteAttributes = string.Empty;

                                CreateAttributes(column, ref _Attributes, ref _Class, ref _ContainerAttributes, ref _TagsAutocompleteAttributes);

                                _data.Class = _Class;
                                _data.Attributes = _Attributes;
                                _data.ContainerAttributes = _ContainerAttributes;
                                _data._TagsAutocompleteAttributes = _TagsAutocompleteAttributes;

                                if (_data.control != null)
                                {
                                    dynamic Element = new ExpandoObject();
                                    Element.Name = _data.displayname;
                                    Element.OptionsText = string.Empty;
                                    Element.OptionsValue = string.Empty;
                                    Element.Label = string.Empty;
                                    Element.Tooltip = string.Empty;
                                    Element.ID = data.ID + _data.control.Value + ColumnCounter.ToString();
                                    Element.Control = _data.control.Value;
                                    Element.Value = column.attr.value;
                                    Element.Class = _Class;
                                    Element.Attributes = _Attributes;
                                    Element.TagsAutocompleteAttributes = _TagsAutocompleteAttributes;

                                    if (column.attr.data != null)
                                    {
                                        Element.ngmodel = column.attr.data.Value;
                                    }

                                    if (_data.control.Value == "tags")
                                    {
                                        if (column.attr.autocomplete != null && column.attr.autocomplete == "true")
                                        {
                                            Element.autocompletesource = column.attr.autocompletesource;
                                        }
                                    }
                                    else if (_data.control.Value == "dropdownlist")
                                    {
                                        Element.ngoptions = column.attr.options.Value;
                                        Element.OptionsText = column.attr.optionstext.Value;
                                        Element.OptionsValue = column.attr.optionsvalue.Value;
                                    }
                                    _data.custommarkup = ParseTokens(Element, Markup["elements." + _data.control.Value], Markup, UIEngineInfo);
                                }
                                if (column.child != null)
                                {
                                    JArray uiemplate = new JArray();
                                    if (column.child.GetType().Name == "JObject")
                                    {
                                        JObject o = column.child as JObject;
                                        uiemplate.Add(o);
                                    }
                                    else
                                    {
                                        uiemplate = column.child;
                                    }

                                    StringBuilder sb = new StringBuilder();

                                    foreach (dynamic template in uiemplate)
                                    {
                                        sb.Append(template.text);

                                        if (template.child != null)
                                        {
                                            foreach (dynamic child in template.child)
                                            {
                                                sb.Append(JsonToHtml.json2html(child));
                                            }
                                        }

                                    }
                                    if (sb.Length > 0)
                                    {
                                        _data.custommarkup = sb.ToString().Replace("mn-", "");
                                    }
                                }

                                columndata.Add(_data);
                                ColumnCounter++;
                            }
                            data.Column = columndata;
                        }
                        UIElements.Add(data);
                        UIElementCounter++;
                    }
                }

                UILayoutCounter++;
            }
        }

        private static void CreateValidation(dynamic element, ref string Validation)
        {
            foreach (dynamic item in element.attr)
            {
                if (item.Name.StartsWith("validation"))
                {
                    if (element.attr.control == "upload")
                    {
                        Validation += "file" + item.Name.Replace("validation-", "") + "= \"" + item.Value.ToString() + "\" ";
                    }
                    else if (element.attr.control == "dropdownlist")
                    {
                        Validation += "f" + item.Name.Replace("validation-", "") + "= \"" + item.Value.ToString() + "\" ";
                    }
                    else if (element.attr.control == "textbox")
                    {
                        Validation += "f" + item.Name.Replace("validation-", "") + "= \"" + item.Value.ToString() + "\" ";
                    }
                    else if (element.attr.control == "email")
                    {
                        Validation += "f" + item.Name.Replace("validation-", "") + "= \"" + item.Value.ToString() + "\" ";
                    }
                    else if (element.attr.control == "texteditor")
                    {
                        Validation += "editor" + item.Name.Replace("validation-", "") + "= \"" + item.Value.ToString() + "\" ";
                    }
                }

            }
        }

        private static void CreateAttributes(dynamic element, ref string Attributes, ref string Class)
        {
            string Null = string.Empty;
            CreateAttributes(element, ref Attributes, ref Class, ref Null, ref Null);
        }

        private static void CreateAttributes(dynamic element, ref string Attributes, ref string Class, ref string ContainerAttributes, ref string TagsAutocompleteAttributes)
        {
            foreach (dynamic item in element.attr)
            {
                if (item.Name.StartsWith("attr-class"))
                {
                    if (element.attr.control != null && element.attr.control == "grid")
                    {
                        Class += "table-responsive ";
                    }

                    Class += item.Value.ToString() + " ";
                }
                else if (item.Name.StartsWith("attr"))
                {
                    Attributes += item.Name.Replace("attr-", "") + "= \"" + item.Value.ToString() + "\" ";
                }
                else if (item.Name.StartsWith("container-attr-"))
                {
                    ContainerAttributes += item.Name.Replace("container-attr-", "") + "= \"" + item.Value.ToString() + "\" ";
                }
                else if (item.Name.StartsWith("autocomplete-attr-"))
                {
                    TagsAutocompleteAttributes += item.Name.Replace("autocomplete-attr-", "") + "= \"" + item.Value.ToString() + "\" ";
                }

            }
        }

        private static void ProcessCustomTemplate(dynamic Layout, string LayoutViewOrder, dynamic elementLayout, List<dynamic> UIElements, dynamic data)
        {
            data.Name = string.Empty;
            data.Layout = Layout.attr.name;
            if (data.Layout == null)
            {
                data.Layout = "default";
            }

            data.LayoutViewOrder = LayoutViewOrder;
            data.Content = elementLayout.text;

            if (elementLayout.child != null)
            {
                JArray uitemplate = new JArray();
                if (elementLayout.child.GetType().Name == "JObject")
                {
                    JObject o = elementLayout.child as JObject;
                    uitemplate.Add(o);
                }
                else
                {
                    uitemplate = elementLayout.child;
                }

                StringBuilder sb = new StringBuilder();
                foreach (dynamic template in uitemplate)
                {
                    sb.Append(JsonToHtml.json2html(template));

                }
                data.Content += sb.ToString().Replace("mn-", "");
                UIElements.Add(data);
            }

        }
        private static void MapDataToUIElement(dynamic Settings, dynamic _UIElement)
        {

            if (Settings != null)
            {
                dynamic Name;

                if (_UIElement.Name != "")
                {
                    Name = _UIElement.Name.Value;
                }
                else
                {
                    Name = _UIElement.Name;
                }

                dynamic _UIData = ((IEnumerable)Settings).Cast<dynamic>().Where(d => d.Name == Name).SingleOrDefault();

                if (_UIData != null)
                {
                    string control = _UIElement.Control.ToString();
                    switch (control)
                    {
                        case "integer":
                            {
                                try
                                {
                                    if (int.TryParse(_UIData.Value, out int Value))
                                    {
                                        _UIData.Value = Value;
                                    }

                                    if (decimal.TryParse(_UIData.Value, out decimal decimalValue))
                                    {
                                        _UIData.Value = decimalValue;
                                    }
                                }
                                catch (Exception) { }
                                break;
                            }
                        case "checkboxlist":
                            {
                                if (_UIData.Value == null)
                                {
                                    _UIData.Value = new List<string>();
                                }
                                else
                                {
                                    try
                                    {

                                        //JArray arr = JArray.Parse(_UIData.Value);
                                        //_UIData.Value = arr.ToObject<List<string>>();

                                        _UIData.Value = _UIData.Value.Split(',');

                                    }
                                    catch { _UIData.Value = null; }
                                }
                                break;
                            }
                        case "checkboxtoggle":
                        case "checkbox":
                        case "toggle":
                            {
                                if (_UIData.Value != null)
                                {
                                    if (_UIData.Value.ToString().ToLower() == "true" || _UIData.Value.ToString().ToLower() == "1")
                                    {
                                        _UIData.Value = true;
                                        break;
                                    }
                                }
                                _UIData.Value = false;
                                break;
                            }

                        default:
                            break;
                    }

                    _UIElement.Value = _UIData.Value;
                    _UIElement.Options = _UIData.Options;
                    _UIElement.OptionsText = _UIData.OptionsText;
                    _UIElement.OptionsValue = _UIData.OptionsValue;

                }
                else
                {
                    _UIElement.Value = string.Empty;
                    _UIElement.Options = string.Empty;
                    _UIElement.OptionsText = string.Empty;
                    _UIElement.OptionsValue = string.Empty;
                    _UIElement.Label = string.Empty;
                    _UIElement.Tooltip = string.Empty;
                    _UIElement.Suffix = string.Empty;
                    _UIElement.Prefix = string.Empty;
                    _UIElement.Helptext = string.Empty;
                    _UIElement.Resource = null;
                    _UIElement.ResourceKey = null;

                }
            }

        }

        private static string ShowLabelContainer()
        {


            return string.Empty;
        }

        private static string GenerateUI(Dictionary<string, string> Markup, dynamic _UIElement, Dictionary<string, string> UIEngineInfo)
        {
            string _HTML = string.Empty;
            try
            {
                if (((IDictionary<string, object>)_UIElement).ContainsKey("Control") == true && _UIElement.Control != null)
                {

                    if (_UIElement.Control == "grid")
                    {
                        _HTML = GenerateGrid(ParseTokens(_UIElement, Markup["containers.uielement"].Replace("[field]", Markup["elements." + _UIElement.Control]), Markup, UIEngineInfo), _UIElement, UIEngineInfo, Markup["elements.grid.search"]);
                    }
                    else if (_UIElement.Control == "date")
                    {
                        _HTML = ParseTokens(_UIElement, Markup["containers.uielement"], Markup, UIEngineInfo);

                        UIEngineInfo["InitScript"] += Environment.NewLine + "$(\"#" + _UIElement.ID + "\").datepicker({ beforeShow: function (input, inst) { var $selector = $('#ui-datepicker-div'); if (!$selector.parent().hasClass('container')) $selector.wrap('<div class=\'container\' />');}});";
                    }
                    else if (_UIElement.Control == "bootstrapdatepicker")
                    {
                        _HTML = ParseTokens(_UIElement, Markup["containers.uielement"], Markup, UIEngineInfo);

                        UIEngineInfo["InitScript"] += Environment.NewLine + "$(\"#" + _UIElement.ID + "\").datepicker();";
                    }
                    else
                    {
                        _HTML = ParseTokens(_UIElement, Markup["containers.uielement"], Markup, UIEngineInfo);
                    }

                    if (_UIElement.Control == "colorpicker")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("jQuery('#" + _UIElement.ID + "').ColorPicker({color:'',onBeforeShow:function() { jQuery(this).ColorPickerSetColor(this.value); },onChange:function(hsb, hex, rgb) {");
                        sb.Append("jQuery('#" + _UIElement.ID + "').val(hex);jQuery('#" + _UIElement.ID + "').val(hex);jQuery('#" + _UIElement.ID + "').css('background','#'+jQuery('#" + _UIElement.ID + "').val()); },onSubmit:function(hsb, hex, rgb, el) { jQuery(el).val(hex); jQuery(el).ColorPickerHide();");
                        sb.Append("jQuery('#" + _UIElement.ID + "').val(hex); }})");
                        sb.Append(";");
                        UIEngineInfo["InitScript"] += Environment.NewLine + sb.ToString();
                    }

                    ProcessFileUpload(_UIElement, UIEngineInfo);
                    ProcessTexteditor(_UIElement, UIEngineInfo);
                    ProcessPermissionGrid(_UIElement, UIEngineInfo);

                }
                else if (_UIElement.Content != null)
                {
                    _HTML = _UIElement.Content;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                _HTML = "<span class=\"MNormalRed\">Failed to generate ui, </span>" + Constants.UIFrameworkFieldError;
            }


            return _HTML;
        }

        private static void ProcessTexteditor(dynamic _UIElement, Dictionary<string, string> UIEngineInfo)
        {
            if (_UIElement.Control == "texteditor")
            {
                string Profile = "Basic";
                if (((IDictionary<string, object>)_UIElement).ContainsKey("profile") == true)
                {
                    Profile = _UIElement.profile;
                }

                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_EditorConfig = function() { parent.OpenPopUp(event, 900,'right','', $scope.ui.data.BrowseUrl.Value + \"&v=\" + new Date().getTime() + \"&profile=" + Profile + "#!/common/controls/editorconfig/" + UIEngineInfo["identifier"] + "-" + _UIElement.Name.Value + "\"); };";
                string EditorToolbarMarkup = EditorConfigFactory.GetEditorToolbarMarkup(int.Parse(UIEngineInfo["moduleid"]), UIEngineInfo["identifier"] + "-" + _UIElement.Name.Value + UIEngineInfo["moduleid"], _UIElement.Name.Value, Profile);
                if (!string.IsNullOrEmpty(EditorToolbarMarkup))
                {
                    UIEngineInfo["PreInitScript"] += Environment.NewLine + EditorToolbarMarkup;
                }
            }
        }

        private static void ProcessFileUpload(dynamic _UIElement, Dictionary<string, string> UIEngineInfo)
        {
            if (_UIElement.Control == "uploadinput" || _UIElement.Control == "uploaddrop" || _UIElement.Control == "uploadboth" || _UIElement.Control == "uploaddefault")
            {
                string ResourceFile = Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"]);
                string GlobalResourceFile = Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], null);
                dynamic maxsize = null;
                string filetypes = string.Empty;
                Dictionary<string, string> hostSettings = DotNetNuke.Entities.Controllers.HostController.Instance.GetSettingsDictionary();
                string FileExtensions = hostSettings["FileExtensions"];

                if (_UIElement.maxsize != null)
                {
                    maxsize = _UIElement.maxsize;
                }
                else if (ConfigurationManager.GetSection("system.web/httpRuntime") is HttpRuntimeSection runtimeSection && runtimeSection.MaxRequestLength > 0)
                {
                    maxsize = (runtimeSection.MaxRequestLength / 1024) * 1000000;
                }

                if (_UIElement.filetypes != null)
                {
                    foreach (string s in _UIElement.filetypes.Value.ToString().Split(','))
                    {
                        if (s.StartsWith("$scope."))
                        {
                            filetypes += "'" + s + "',";
                        }
                        else
                        {
                            filetypes += "'" + s.ToLower().Trim() + "',";
                        }
                    }
                }
                else
                {
                    foreach (string s in FileExtensions.Split(','))
                    {
                        filetypes += "'" + s.ToLower().Trim() + "',";
                    }
                }

                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "maxsize = " + maxsize + ";";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + ".headers = { ModuleId:" + UIEngineInfo["moduleid"] + ",TabId:$.ServicesFramework(" + UIEngineInfo["moduleid"] + ").getTabId(),RequestVerificationToken:$.ServicesFramework(" + UIEngineInfo["moduleid"] + ").getAntiForgeryValue()}; ";

                if (filetypes.Contains("$scope.ui"))
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "filetypes = " + filetypes.TrimEnd(',').TrimStart('\'').TrimEnd('\'') + ".split(\",\");";
                }
                else
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "filetypes = [" + filetypes.TrimEnd(',').ToLower() + "]; ";
                }

                if (_UIElement.autoupload != null && _UIElement.autoupload.Value == "true")
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + ".autoUpload = true; ";
                }

                if (_UIElement.disableselectfile != null && _UIElement.disableselectfile == "true")
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "disableselectfile = true; ";
                }
                else
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "disableselectfile = false; ";
                }

                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "IsList = true; ";

                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + ".onSuccessItem = function (fileItem, response, status, headers) { if (response != \"\") fileItem.file.name = response;};";

                if (UIEngineInfo["identifier"] == "common_controls_url")
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + ".url = jQuery.ServicesFramework(" + UIEngineInfo["moduleid"] + ").getServiceRoot('" + UIEngineInfo["appname"] + "')+'upload/files?identifier=" + UIEngineInfo["identifier"] + "&uid='+$scope.ui.data.Uid.Value;$scope." + _UIElement.Name.Value + ".formData = [{ fileDirectory:'',folder: /''/, maxsize: $scope." + _UIElement.Name.Value + "maxsize,filetypes: $scope." + _UIElement.Name.Value + "filetypes }];$scope." + _UIElement.Name.Value + ".filters.push({name: 'maxsize',fn: function (item) {if (item.size > $scope." + _UIElement.Name.Value + "maxsize) {swal('Error!','" + Localization.Get("Filesize", "Text", ResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "' + $scope." + _UIElement.Name.Value + "maxsize/1000000 + ' MB');return false;}else return true;}});$scope." + _UIElement.Name.Value + ".filters.push({name: 'filetype',fn: function (item) {if ($scope." + _UIElement.Name.Value + "filetypes.indexOf(item.name.substr(item.name.lastIndexOf('.') + 1).toLowerCase()) == -1) {swal('Error!','" + Localization.Get("Filetype", "Text", ResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "' + $scope." + _UIElement.Name.Value + "filetypes);return false;}else return true;}});" + Environment.NewLine;
                }
                else
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + ".url = jQuery.ServicesFramework(" + UIEngineInfo["moduleid"] + ").getServiceRoot('" + UIEngineInfo["appname"] + "')+'upload/files?identifier=" + UIEngineInfo["identifier"] + "';$scope." + _UIElement.Name.Value + ".formData = [{ fileDirectory:'',folder: /''/, maxsize: $scope." + _UIElement.Name.Value + "maxsize,filetypes: $scope." + _UIElement.Name.Value + "filetypes }];$scope." + _UIElement.Name.Value + ".filters.push({name: 'maxsize',fn: function (item) {if (item.size > $scope." + _UIElement.Name.Value + "maxsize) {swal('Error!','" + Localization.Get("Filesize", "Text", ResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "' + $scope." + _UIElement.Name.Value + "maxsize/1000000 + ' MB');return false;}else return true;}});$scope." + _UIElement.Name.Value + ".filters.push({name: 'filetype',fn: function (item) {if ($scope." + _UIElement.Name.Value + "filetypes.indexOf(item.name.substr(item.name.lastIndexOf('.') + 1).toLowerCase()) == -1) {swal('Error!','" + Localization.Get("Filetype", "Text", ResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "' + $scope." + _UIElement.Name.Value + "filetypes);return false;}else return true;}});" + Environment.NewLine;
                }

                if (_UIElement.Control == "uploaddefault")
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "TableState = null;";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "ShowFileUpload = true;";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "ShowFileBrowse = false;";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "ShowFileLink = false;";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "FileUploadText = '" + Localization.Get("ClickDropZoneDefault", "Text", GlobalResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "';";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_FileUpoad = function (type) { $scope." + _UIElement.Name.Value + "ShowFileUpload = false; $scope." + _UIElement.Name.Value + "ShowFileBrowse = false; $scope." + _UIElement.Name.Value + "ShowFileLink = false; if (type != undefined) { if (type == \"browse\") { $scope." + _UIElement.Name.Value + "ShowFileBrowse = true; return; } else if (type == \"link\") { $scope." + _UIElement.Name.Value + "ShowFileLink = true;$('#" + _UIElement.Name.Value + "AddLink').focus(); return; } } $scope." + _UIElement.Name.Value + "ShowFileUpload = true; };";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Mouse_FileHover = function (type) { if (type != undefined) { if (type == \"browse\") return $scope." + _UIElement.Name.Value + "FileUploadText = '" + Localization.Get("BrowseFilesystem", "Text", GlobalResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "'; else if (type == \"upload\") return $scope." + _UIElement.Name.Value + "FileUploadText = '" + Localization.Get("UploadAFile", "Text", GlobalResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "'; else if (type == \"link\") return $scope." + _UIElement.Name.Value + "FileUploadText = '" + Localization.Get("EnterURLLink", "Text", GlobalResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "'; } $scope." + _UIElement.Name.Value + "FileUploadText = '" + Localization.Get("ClickDropZoneDefault", "Text", GlobalResourceFile, UIEngineInfo["showmissingkeys"] == "true" ? true : false, Localization.SharedMissingPrefix) + "'; };";
                    if (_UIElement.folders != null)
                    {
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Folders = " + _UIElement.folders + ";";
                    }

                    if (_UIElement.files != null)
                    {
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Files = " + _UIElement.files + ";";
                    }

                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_SelectGetFiles = function (folder) { if (folder != undefined) { if(folder.IsImage || $('.assetIsList').hasClass('btn-primary')) $scope." + _UIElement.Name.Value + "IsList = false; else $scope." + _UIElement.Name.Value + "IsList = true; $('[st-table=\"" + _UIElement.Name.Value + "Files\"]>tbody').hide(); $('[st-table=\"" + _UIElement.Name.Value + "Files\"]>tfoot').hide(); $scope." + _UIElement.Name.Value + "Click_GetFiles(folder.Value);}};";
                    if (UIEngineInfo["identifier"] == "common_controls_url")
                    {
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_GetFiles = function (fid) { if (fid != undefined) {setTimeout(function () {$('#folders' + fid).closest('li').children('span.glyphicon-triangle-right').click();}, 100);$('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('.folders').css('font-weight', ''); $('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('#folders' + fid).css('font-weight', 'bold');if($scope." + _UIElement.Name.Value + "TableState!=undefined){$scope." + _UIElement.Name.Value + "TableState.pagination.start=0;}; $scope.Pipe_" + _UIElement.Name.Value + "Pagging($scope." + _UIElement.Name.Value + "TableState); } };";
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_SelectFile = function (event,fileid) {$('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('td.r_file ').css({\"background-color\": \"initial\", \"font-weight\": \"initial\"}); $(event.target).parent('td').css({\"background-color\": \"#eee\", \"font-weight\": \"bold\"}); common.webApi.get('Upload/GetFile', 'identifier=\"" + UIEngineInfo["identifier"] + "\"&fileid=' + fileid).then(function (data) { if (data.data != undefined && data.data != null) { $scope." + _UIElement.Name.Value + ".queue = []; $scope." + _UIElement.Name.Value + ".selectqueue = []; $scope." + _UIElement.Name.Value + ".selectqueue.push({ filename: data.data.Name, fileid: data.data.FileId, fileurl: data.data.FileUrl });} }) };";
                    }
                    else
                    {
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_GetFiles = function (fid) { if (fid != undefined) {setTimeout(function () {$('#folders' + fid).closest('li').children('span.glyphicon-triangle-right').click();}, 100);$('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('.folders').css('font-weight', ''); $('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('#folders' + fid).css('font-weight', 'bold');if($scope." + _UIElement.Name.Value + "TableState!=undefined){$scope." + _UIElement.Name.Value + "TableState.pagination.start=0;}; $scope.Pipe_" + _UIElement.Name.Value + "Pagging($scope." + _UIElement.Name.Value + "TableState); } };";
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_SelectFile = function (event,fileid) {$('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('td.r_file ').css({\"background-color\": \"initial\", \"font-weight\": \"initial\"}); $(event.target).parent('td').css({\"background-color\": \"#eee\", \"font-weight\": \"bold\"}); common.webApi.get('Upload/GetFile', 'identifier=\"" + UIEngineInfo["identifier"] + "\"&fileid=' + fileid).then(function (data) { if (data.data != undefined && data.data != null) { $scope." + _UIElement.Name.Value + ".queue = []; $scope." + _UIElement.Name.Value + ".selectqueue = []; $scope." + _UIElement.Name.Value + ".selectqueue.push({ filename: data.data.Name, fileid: data.data.FileId, fileurl: data.data.FileUrl }); $scope." + _UIElement.Name.Value + "Click_FileUpoad(); } }) };";
                    }
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_GetSubFolders = function (event, folder) { var $this = $(event.currentTarget); if ($this.hasClass(\"fa-caret-right\") && $this.parent().find('> .rootfolder li').length <= 0) { common.webApi.get('Upload/GetSubFolders', 'identifier=" + UIEngineInfo["identifier"] + "&folderid='+ folder.Value).then(function (data) { if (data.data != undefined && data.data != null) { folder.children = data.data; setTimeout(function () { $this.toggleClass(\'fa-caret-right fa-caret-down\'); if ($this.hasClass(\'fa-caret-down\')) $this.parent().find(\'> .rootfolder\').addClass(\'show\'); else $this.parent().find(\'> .rootfolder\').removeClass(\'show\'); if(typeof $scope.BindFolderEvents != 'undefined'){$scope.BindFolderEvents(folder.Value);}}, 2) } }); } else { if ($this.parent().find(\'> .rootfolder li\').length <= 0) return; $this.toggleClass(\'fa-caret-right fa-caret-down\'); if ($this.hasClass(\'fa-caret-down\')) $this.parent().find(\'> .rootfolder\').addClass(\'show\'); else $this.parent().find(\'> .rootfolder\').removeClass(\'show\'); } };";
                    if (_UIElement.autopaging == true)
                    {
                        UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "GetFilesTimeOutid;";
                        UIEngineInfo["PreInitScript"] += Environment.NewLine + "$scope.Pipe_" + _UIElement.Name.Value + "Pagging = function (tableState) { if($('[type=\"search\"]').val().length==0 && tableState!=undefined) tableState.search=[]; var uid = null; if ($scope.ui.data.Uid != undefined && $scope.ui.data.Uid.Value != undefined) uid = $scope.ui.data.Uid.Value; $scope." + _UIElement.Name.Value + "TableState = tableState; tableState.pagination.numberOfPages = 0; var keyword = \"\"; if (tableState.search.predicateObject != undefined && tableState.search.predicateObject.$ != undefined) keyword = tableState.search.predicateObject.$; var folderid = $scope.ui.data.Folders.Value; if($('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('.folders[style=\"font-weight: bold;\"]').attr('id')!=undefined) folderid = $('[identifier=\"" + UIEngineInfo["identifier"] + "\"]').find('[data-name=\"" + _UIElement.Name.Value + "\"]').find('.folders[style=\"font-weight: bold;\"]').attr('id').replace('folders', ''); if($scope." + _UIElement.Name.Value + "GetFilesTimeOutid){clearTimeout($scope." + _UIElement.Name.Value + "GetFilesTimeOutid);} $scope." + _UIElement.Name.Value + "GetFilesTimeOutid = setTimeout(function () { common.webApi.get('Upload/GetFiles', 'identifier=" + UIEngineInfo["identifier"] + "&folderid=' + folderid + '&uid=' + uid + '&skip=' + tableState.pagination.start + '&pagesize=' + tableState.pagination.number + '&keyword=' + keyword).then(function (data) { $('[st-table=\"" + _UIElement.Name.Value + "Files\"]>tbody').show(); $('[st-table=\"" + _UIElement.Name.Value + "Files\"]>tfoot').show(); if (data.data != null) { tableState.pagination.numberOfPages = data.data.Pages; $('.chkfidall').removeAttr('checked'); $('.chkfid').removeAttr('checked'); $scope." + _UIElement.Name.Value + "Files = []; $.each(data.data.Files, function (key, Value) { $scope." + _UIElement.Name.Value + "Files.push(Value); }) } })}, 500) };";
                    }

                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "Click_AddLink = function (name) { if (name != undefined && name != null) { var link = $('#' + name + 'AddLink').val(); if (mnValidationService.DoValidationAndSubmit(name+'ShowFileLinkStrong')) { $scope." + _UIElement.Name.Value + ".queue = []; $scope." + _UIElement.Name.Value + ".selectqueue = []; $scope." + _UIElement.Name.Value + ".selectqueue.push({ filename: link }); $scope." + _UIElement.Name.Value + "Click_FileUpoad(); } } };";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + _UIElement.Name.Value + "ControlEnable = function (mode) { if (mode != undefined) { var modes = $('[uploadername=\"" + _UIElement.Name.Value + "\"]').attr('controlmodes'); if (modes != undefined && modes != '' && modes.indexOf(mode) < 0) return false; } return true; };";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$(document).keyup(function (e) { if (e.keyCode == 27 && $('#" + _UIElement.Name.Value + "ShowFileBrowseSpan').is(':visible')) { $('#" + _UIElement.Name.Value + "ShowFileBrowseSpan').click(); } else if (e.keyCode == 27 && $('#" + _UIElement.Name.Value + "ShowFileLinkSpan').is(':visible')) { $('#" + _UIElement.Name.Value + "ShowFileLinkSpan').click(); } else if (e.keyCode == 13 && $('#" + _UIElement.Name.Value + "ShowFileLinkStrong').is(':visible')) { $('#" + _UIElement.Name.Value + "ShowFileLinkStrong').click(); } }); $scope.enterLink = function (event) { if (event.keyCode == 13) { event.preventDefault(); } };";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "setTimeout(function () {if($('.folders')[0]!=null){$('.folders')[0].click();}}, 100);";
                }
            }
        }
        private static void ProcessPermissionGrid(dynamic _UIElement, Dictionary<string, string> UIEngineInfo)
        {
            if (_UIElement.Control == "Permissiongrid" || _UIElement.Control == "permissiongrid")
            {
                string ControlName = _UIElement.Name.Value;
                string rolegroup = "$scope." + ControlName + "RoleGroupsValue";
                string ResourceFile = Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"]);
                int portalid = PortalSettings.Current.PortalId;

                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "RoleGroupsOptions=" + Serializer.ToJSON(Factory.RoleFactory.GetAllRoleGroups(portalid, ResourceFile)) + ";";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "RoleGroupsValue=\"-2\";";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "RoleremoteAPI = function(userInputString, timeoutPromise) {";
                UIEngineInfo["InitScript"] += Environment.NewLine + "return common.webApi.get('~permissiosgrid/GetSuggestionRoles?rolegroupId=' + " + rolegroup + " + '&keyword=' + userInputString);};";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "Roles=" + Serializer.ToJSON(_UIElement.Options[ControlName].RolePermissions) + ";";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "Users=" + Serializer.ToJSON(_UIElement.Options[ControlName].UserPermissions) + ";";
                if (_UIElement.Options[ControlName].PermissionDefinitions.Count == 0)
                {
                    List<Permission> PermissionDefinitions = new List<Permission>
                    {
                        _UIElement.Options["PermissionKey"]
                    };
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "Definitions=" + Serializer.ToJSON(PermissionDefinitions) + ";";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "IsInheritedCheckBoxShow=" + _UIElement.Options["IsInheritedCheckBoxShow"].ToString().ToLower() + ";";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "Inherit=" + _UIElement.Options["IsInherited"].ToString().ToLower() + ";";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "InheritID=" + _UIElement.Options["PermissionKey"].PermissionId + ";";
                }
                else
                {
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "Definitions=" + Serializer.ToJSON(_UIElement.Options[ControlName].PermissionDefinitions) + ";";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "IsInheritedCheckBoxShow=" + _UIElement.Options[ControlName].ShowInheritCheckBox.ToString().ToLower() + ";";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "Inherit=" + _UIElement.Options[ControlName].Inherit.ToString().ToLower() + ";";
                    UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "InheritID=" + _UIElement.Options[ControlName].InheritPermissionID + ";";
                }
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "TemplateRolePermisssion=" + Serializer.ToJSON(new RolePermission()) + ";";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope." + ControlName + "TemplateUserPermisssion=" + Serializer.ToJSON(new UserPermission()) + ";";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Show" + ControlName + "Role=false; $scope.Click_" + ControlName + "NewRole = function () { $scope.Show" + ControlName + "Role=true;setTimeout(function () {$('input#Role_value').focus();}, 200); };";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Show" + ControlName + "User=false; $scope.Click_" + ControlName + "NewUser = function () { $scope.Show" + ControlName + "User=true; setTimeout(function () {$('input#User_value').focus();}, 200);  };";
                // javascript method
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Click_" + ControlName + "ReviewContent = function (row, PermissionId) { var IsReview = 'square'; if (row != undefined) { $.each(row.Permissions, function (key, p) { if (p.PermissionId == PermissionId && p.AllowAccess) { IsReview = 'check-square'; } }); } if ($scope." + ControlName + "Inherit == true && $scope." + ControlName + "InheritID == PermissionId) IsReview = 'check-square'; return IsReview; };";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Click_" + ControlName + "CheckIsReview  = function (row, pdefinition) { if (row != undefined) { var isavl = false; if (row.Permissions.length > 0) { var ischeck = false; $.each(row.Permissions, function (key, p) { if (p.PermissionId == pdefinition.PermissionId) { p.AllowAccess = !p.AllowAccess; isavl = true; ischeck = p.AllowAccess;} }); if (ischeck && pdefinition.PermissionId != $scope." + ControlName + "InheritID) { var isviewcheck = true; $.each(row.Permissions, function (key, p) { if (p.PermissionId == $scope." + ControlName + "InheritID) { p.AllowAccess = true; isviewcheck = false } });if (isviewcheck) { var PerDef = []; var isveiwperadd = false; $scope." + ControlName + "Definitions.filter(function (pd) { if (pd.PermissionId === $scope." + ControlName + "InheritID) { PerDef = $.extend(true, {}, pd); isveiwperadd = true; } }); if (isveiwperadd) { var viewper = $.extend(true, {}, PerDef); viewper.AllowAccess = true; row.Permissions.push(viewper); } } } }if (!isavl) { var per = $.extend(true, {}, pdefinition); per.AllowAccess = true; row.Permissions.push(per);var isviewcheck = true; $.each(row.Permissions, function (key, p) { if (p.PermissionId == $scope." + ControlName + "InheritID) { p.AllowAccess = true; isviewcheck = false } }); if (isviewcheck && pdefinition.PermissionId != $scope." + ControlName + "InheritID) { var PerDef = []; var isveiwperadd = false; $scope." + ControlName + "Definitions.filter(function (pd) { if (pd.PermissionId === $scope." + ControlName + "InheritID) { PerDef = $.extend(true, {}, pd); isveiwperadd = true; } }); if (isveiwperadd) { var viewper = $.extend(true, {}, PerDef); viewper.AllowAccess = true; row.Permissions.push(viewper); } } } } };";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Click_" + ControlName + "CheckDisabled = function (PermissionId, row) { if (row != undefined && row.RoleId == 0 || row != undefined && row.RoleName =='Administrators') { return true; } else if ($scope." + ControlName + "Inherit && PermissionId == $scope." + ControlName + "InheritID) { return true; } return false; };";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Click_" + ControlName + "InheritCheck = function () { $scope." + ControlName + "Inherit = !$scope." + ControlName + "Inherit; var PerDef = []; $scope." + ControlName + "Definitions.filter(function (pd) { if (pd.PermissionId === $scope." + ControlName + "InheritID) { PerDef = $.extend(true, {}, pd); } }); if ($scope." + ControlName + "Roles != undefined) for (var i = 0; i < $scope." + ControlName + "Roles.length; i++) { if ($scope." + ControlName + "Roles[i].RoleId != 0) { var isavl = false; $.each($scope." + ControlName + "Roles[i].Permissions, function (key, p) { if (p.PermissionId == $scope." + ControlName + "InheritID) { if ($scope." + ControlName + "Inherit == true) p.AllowAccess = true; else p.AllowAccess = false; isavl = true; } }); if (!isavl) { var per = $.extend(true, {}, PerDef); if ($scope." + ControlName + "Inherit == true) per.AllowAccess = true; else per.AllowAccess = false; $scope." + ControlName + "Roles[i].Permissions.push(per); } } } if ($scope." + ControlName + "Users != undefined) for (var i = 0; i < $scope." + ControlName + "Users.length; i++) { var isavl = false; $.each($scope." + ControlName + "Users[i].Permissions, function (key, p) { if (p.PermissionId == $scope." + ControlName + "InheritID) { if ($scope." + ControlName + "Inherit == true) p.AllowAccess = true; else p.AllowAccess = false; isavl = true; } }); if (!isavl) { var per = $.extend(true, {}, PerDef); if ($scope." + ControlName + "Inherit == true) per.AllowAccess = true; else per.AllowAccess = false; $scope." + ControlName + "Users[i].Permissions.push(per); } } };";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Click_" + ControlName + "AddRole = function (selectedRole) { if (selectedRole != undefined && selectedRole.originalObject != undefined && selectedRole.originalObject.Value != undefined) { var Role = $.extend(true, {}, $scope." + ControlName + "TemplateRolePermisssion); Role.RoleId = selectedRole.originalObject.Value; Role.Locked = $scope." + ControlName + "Inherit; Role.RoleName = selectedRole.originalObject.Label; var IsAvl = false;  $.each($scope." + ControlName + "Roles, function (key, r) { if (r.RoleId == selectedRole.originalObject.Value) IsAvl = true; }); if (!IsAvl) { $.each($scope." + ControlName + "Definitions, function (key, Definitions) { var per = $.extend(true, {}, Definitions); Role.Permissions.push(per); }); $scope." + ControlName + "Roles.push(Role); } selectedRole.originalObject.Value == undefined; $scope.SelectedRole = ''; $scope.$broadcast('angucomplete-alt:clearInput'); } };";
                UIEngineInfo["InitScript"] += Environment.NewLine + "$scope.Click_" + ControlName + "AddUser = function (selectedUser) { if (selectedUser != undefined && selectedUser.originalObject != undefined && selectedUser.originalObject.Value != undefined) { var User = $.extend(true, {}, $scope." + ControlName + "TemplateUserPermisssion); User.UserId = selectedUser.originalObject.Value; User.DisplayName = selectedUser.originalObject.Label; User.UserName = selectedUser.originalObject.UserName; User.Email = selectedUser.originalObject.Email; User.AvatarUrl = selectedUser.originalObject.AvatarUrl; var IsAvl = false; $.each($scope." + ControlName + "Users, function (key, u) { if (u.UserId == selectedUser.originalObject.Value) IsAvl = true; }); if (!IsAvl) { $.each($scope." + ControlName + "Definitions, function (key, Definitions) { var per = $.extend(true, {}, Definitions); User.Permissions.push(per); }); $scope." + ControlName + "Users.push(User); } selectedUser.originalObject.Value == undefined; $scope.SelectedUser = ''; $scope.$broadcast('angucomplete-alt:clearInput'); } };";

            }
        }
        public static string ParseServerPath(int ModuleId, string AppName)
        {
            string MapPath = GetPortalDirectoryPath(ModuleId, AppName);
            MapPath = MapServerPath(MapPath);
            return MapPath;
        }

        private static string GetPortalDirectoryPath(int ModuleId, string AppName)
        {
            PortalInfo pInfo = new PortalController().GetPortal(new ModuleController().GetModule(ModuleId).PortalID);

            string MapPath = pInfo.HomeDirectory;

            if (!MapPath.EndsWith("/"))
            {
                MapPath += "/";
            }

            MapPath = MapPath + AppName + "/" + ModuleId + "/";

            return MapPath;
        }
        public static string MapServerPath(string path)
        {
            string Path = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(HttpRuntime.AppDomainAppPath))
                {
                    Path = HttpRuntime.AppDomainAppPath + path.Replace("~", string.Empty).Replace('/', '\\');
                }
                else
                {
                    Path = System.Web.Hosting.HostingEnvironment.MapPath(path);
                }
            }
            catch { }
            return Path;
        }
        private static string ParseTokens(dynamic _UIElement, string _HTML, Dictionary<string, string> Markup, Dictionary<string, string> UIEngineInfo)
        {
            try
            {
                string OptionsText = _UIElement.OptionsText;
                if (string.IsNullOrEmpty(OptionsText))
                {
                    OptionsText = "Text";
                }

                string OptionsValue = _UIElement.OptionsValue;
                if (string.IsNullOrEmpty(OptionsValue))
                {
                    OptionsValue = "Value";
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("Container") == true && _UIElement.Container == "false")//if container= false but label is true or false
                {
                    _HTML = "[element-label][field]";
                    _HTML = _HTML.Replace("[field]", Markup["elements." + _UIElement.Control]);
                    if (((IDictionary<string, object>)_UIElement).ContainsKey("LabelEnabled") == true && _UIElement.LabelEnabled == "false")
                    {
                        _HTML = _HTML.Replace("[element-label]", string.Empty);
                    }
                    else
                    {
                        _HTML = _HTML.Replace("[element-label]", Markup["elements.label"]);
                    }

                    if (((IDictionary<string, object>)_UIElement).ContainsKey("TooltipEnabled") == true && _UIElement.TooltipEnabled == "false")
                    {
                        _HTML = _HTML.Replace("[field-tooltip]", string.Empty);
                    }
                    else
                    {
                        _HTML = _HTML.Replace("[field-tooltip]", _UIElement.Tooltip);
                    }

                    _HTML = _HTML.Replace("[Upload:Queue]", Markup["elements.uploadqueue"]);
                }
                else
                {
                    _HTML = _HTML.Replace("[field]", Markup["elements." + _UIElement.Control]);
                    if (((IDictionary<string, object>)_UIElement).ContainsKey("LabelEnabled") == true && _UIElement.LabelEnabled == "false")
                    {
                        _HTML = _HTML.Replace("[element-label]", string.Empty);
                    }
                    else
                    {
                        _HTML = _HTML.Replace("[element-label]", Markup["elements.label"]);
                        if (((IDictionary<string, object>)_UIElement).ContainsKey("Labelclass") == true && _UIElement.Labelclass != null)
                        {
                            _HTML = _HTML.Replace("[labelclass]", _UIElement.Labelclass.Value);
                        }
                        else
                        {
                            _HTML = _HTML.Replace("[labelclass]", string.Empty);
                        }
                    }

                    if (((IDictionary<string, object>)_UIElement).ContainsKey("TooltipEnabled") == true && _UIElement.TooltipEnabled == "false")
                    {
                        _HTML = _HTML.Replace("[field-tooltip]", string.Empty);
                    }
                    else
                    {
                        _HTML = _HTML.Replace("[field-tooltip]", _UIElement.Tooltip);
                    }

                    _HTML = _HTML.Replace("[Upload:Queue]", Markup["elements.uploadqueue"]);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("Cancel") == true)
                {
                    if (_UIElement.Cancel == "true")
                    {
                        if (_HTML.Contains("[Cancel]"))
                        {
                            _HTML = _HTML.Replace("[Cancel]", Markup["elements.cancel"]);
                        }
                    }
                }
                /*End*/

                if (((IDictionary<string, object>)_UIElement).ContainsKey("ngoptions") == true && _UIElement.ngoptions != null)
                {
                    try
                    {
                        _HTML = _HTML.Replace("[options]", _UIElement.ngoptions);
                    }
                    catch (Exception) { _HTML = _HTML.Replace("[options]", _UIElement.ngoptions.Value); }
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("ngmodel") == true && _UIElement.ngmodel != null)
                {
                    try
                    {
                        _HTML = _HTML.Replace("[value]", _UIElement.ngmodel);
                    }
                    catch (Exception) { _HTML = _HTML.Replace("[value]", _UIElement.ngmodel.Value); }
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("autocompletesource") == true && _UIElement.autocompletesource != null)
                {
                    _HTML = _HTML.Replace("[auto-complete]", "<auto-complete source=\"" + _UIElement.autocompletesource.Value + "\" select-first-match=\"true\" [autocomplete-attributes]></auto-complete>").Replace("[autocomplete-attributes]", _UIElement.TagsAutocompleteAttributes);
                }

                _HTML = _HTML.Replace("[field-label]", _UIElement.Label);
                _HTML = _HTML.Replace("[field-tooltip]", _UIElement.Tooltip);
                _HTML = _HTML.Replace("[id]", _UIElement.ID);
                _HTML = _HTML.Replace("[name]", _UIElement.Name.Value);
                _HTML = _HTML.Replace("[value]", _UIElement.Name.Value);
                _HTML = _HTML.Replace("[field-control]", _UIElement.Control);
                _HTML = _HTML.Replace("[UI_Field_Value]", _UIElement.Value != null ? _UIElement.Value.ToString() : string.Empty);
                _HTML = _HTML.Replace("[options-text]", OptionsText);
                _HTML = _HTML.Replace("[options-value]", OptionsValue);
                _HTML = _HTML.Replace("[class]", _UIElement.Class);
                _HTML = _HTML.Replace("[attributes]", _UIElement.Attributes);

                if (PortalSettings.Current.EnablePopUps)
                {
                    _HTML = _HTML.Replace("[DNNEditor_Control_Path]", ServiceProvider.NavigationManager.NavigateURL("DNN_Controls_HtmlEditor") + "/mid/" + UIEngineInfo["moduleid"] + "?popUp=true");
                }
                else
                {
                    _HTML = _HTML.Replace("[DNNEditor_Control_Path]", ServiceProvider.NavigationManager.NavigateURL("DNN_Controls_HtmlEditor") + "/mid/" + UIEngineInfo["moduleid"] + "?hidecommandbar=true&SkinSrc=[G]Skins/_default/popUpSkin&popUp=true");
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("Validation") == true && _UIElement.Validation != null)
                {
                    _HTML = _HTML.Replace("[validation]", _UIElement.Validation);
                }
                else
                {
                    _HTML = _HTML.Replace("[validation]", string.Empty);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("ContainerAttributes") == true && _UIElement.ContainerAttributes != null)
                {
                    _HTML = _HTML.Replace("[container-attributes]", _UIElement.ContainerAttributes);
                }
                else
                {
                    _HTML = _HTML.Replace("[container-attributes]", string.Empty);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("Suffix") == true && !string.IsNullOrEmpty(_UIElement.Suffix))
                {
                    _HTML = _HTML.Replace("[suffix]", Markup["elements.suffix"]).Replace("[id]", _UIElement.ID).Replace("[field-suffix]", _UIElement.Suffix != null ? _UIElement.Suffix.ToString() : string.Empty);
                }
                else
                {
                    _HTML = _HTML.Replace("[suffix]", string.Empty);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("Prefix") == true && !string.IsNullOrEmpty(_UIElement.Prefix))
                {
                    _HTML = _HTML.Replace("[prefix]", Markup["elements.prefix"]).Replace("[id]", _UIElement.ID).Replace("[field-prefix]", _UIElement.Prefix != null ? _UIElement.Prefix.ToString() : string.Empty);
                }
                else
                {
                    _HTML = _HTML.Replace("[prefix]", string.Empty);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("Helptext") == true && _UIElement.Helptext != null)
                {
                    _HTML = _HTML.Replace("[helptext]", Markup["elements.helptext"]).Replace("[id]", _UIElement.ID).Replace("[field-help]", _UIElement.Helptext != null ? _UIElement.Helptext.ToString() : string.Empty);
                }
                else
                {
                    _HTML = _HTML.Replace("[helptext]", string.Empty);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("ToggleOn") && !string.IsNullOrEmpty(_UIElement.ToggleOn))
                {
                    _HTML = _HTML.Replace("[toggle-on]", _UIElement.ToggleOn);
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("ToggleOff") && !string.IsNullOrEmpty(_UIElement.ToggleOff))
                {
                    _HTML = _HTML.Replace("[toggle-off]", _UIElement.ToggleOff);
                }

                //Replace token with empty string
                if (_HTML.Contains("[Cancel]"))
                {
                    _HTML = _HTML.Replace("[Cancel]", string.Empty);
                }

                _HTML = _HTML.Replace("[element-label]", string.Empty);
                _HTML = _HTML.Replace("[field-tooltip]", string.Empty);
                _HTML = _HTML.Replace("[field]", string.Empty);

                //for file upload control
                if (((IDictionary<string, object>)_UIElement).ContainsKey("removedelete") == true && _UIElement.removedelete == "true")
                {
                    _HTML = _HTML.Replace("[removedelete]", "ng-hide='true'");
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("enableselect") == true)
                {
                    _HTML = _HTML.Replace("[enableselect]", _UIElement.enableselect.ToString().ToLower());
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("removenode") == true)
                {
                    try { _HTML = _HTML.Replace("[removenode]", _UIElement.removenode.Value); }
                    catch { _HTML = _HTML.Replace("[removenode]", Convert.ToString(_UIElement.removenode)); }
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("editnode") == true)
                {
                    try { _HTML = _HTML.Replace("[editnode]", _UIElement.editnode.Value); }
                    catch { _HTML = _HTML.Replace("[editnode]", Convert.ToString(_UIElement.editnode)); }
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("collapsed") == true)
                {
                    try { _HTML = _HTML.Replace("[collapsed]", _UIElement.collapsed.Value); }
                    catch { _HTML = _HTML.Replace("[collapsed]", Convert.ToString(_UIElement.collapsed)); }
                }

                if (((IDictionary<string, object>)_UIElement).ContainsKey("icon") == true)
                {
                    _HTML = _HTML.Replace("[buttonicon]", _UIElement.icon.Value);
                }

                _HTML = ReplaceEmptyTokens(_HTML);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return _HTML;
        }

        private static string ReplaceEmptyTokens(string _HTML)
        {
            _HTML = _HTML.Replace("[value]", string.Empty);
            _HTML = _HTML.Replace("[auto-complete]", string.Empty);
            _HTML = _HTML.Replace("[autocomplete-attributes]", string.Empty);
            _HTML = _HTML.Replace("[attributes]", string.Empty);
            _HTML = _HTML.Replace("[class]", string.Empty);
            _HTML = _HTML.Replace("[name]", string.Empty);
            _HTML = _HTML.Replace("[id]", string.Empty);
            _HTML = _HTML.Replace("[options]", string.Empty);
            _HTML = _HTML.Replace("[field-label]", string.Empty);
            _HTML = _HTML.Replace("[field-tooltip]", string.Empty);
            _HTML = _HTML.Replace("[Cancel]", string.Empty);
            _HTML = _HTML.Replace("[removedelete]", string.Empty);
            _HTML = _HTML.Replace("[treedepth]", string.Empty);
            _HTML = _HTML.Replace("[removenode]", string.Empty);
            _HTML = _HTML.Replace("[editnode]", string.Empty);
            _HTML = _HTML.Replace("[collapsed]", string.Empty);
            _HTML = _HTML.Replace("[enableselect]", string.Empty);
            _HTML = _HTML.Replace("[buttonicon]", string.Empty);
            return _HTML;
        }

        private static string GenerateGrid(string Markup, dynamic _UIElement, Dictionary<string, string> UIEngineInfo, string SearchMarkup)
        {
            if (_UIElement.Sortable == "true")
            {
                string SortableHandle = _UIElement.SortableHandle;
                string Handle = SortableHandle;
                if (!string.IsNullOrEmpty(SortableHandle))
                {
                    SortableHandle = "handle: '." + SortableHandle + "',";
                }

                if (_UIElement.SortableKey != null && _UIElement.SortableKey != "false")
                {
                    if (string.IsNullOrEmpty(Handle))
                    {
                        Handle = "'#" + _UIElement.ID + " tbody tr'";
                    }
                    else
                    {
                        Handle = "'." + Handle + "'";
                    }

                    UIEngineInfo["InitScript"] += Environment.NewLine + "$(\"#" + _UIElement.ID + " tbody\").sortable({" + SortableHandle + "cancel: false, helper: function(e, tr) {tr.children().each(function() {$(this).width($(this).width()); });return tr;}, axis: 'y',update: function( event, ui ) { console.log(ui.item.closest('tbody').find('tr')); var Seed;$.each(ui.item.closest('tbody').find('tr'),function(index,tr){ var $scope = angular.element($(tr)).scope(); $scope.$apply(function(){ if(!isNaN($scope.row[\"" + _UIElement.SortableKey + "\"])){if(isNaN(Seed)||$scope.row[\"" + _UIElement.SortableKey + "\"]<Seed)Seed=$scope.row[\"" + _UIElement.SortableKey + "\"];}});});if(!isNaN(Seed))Seed=0;$.each(ui.item.closest('tbody').find('tr'),function(index,tr){var $scope = angular.element($(tr)).scope(); $scope.$apply(function(){$scope.row[\"" + _UIElement.SortableKey + "\"]=Seed;Seed++;});});";
                    if (_UIElement.SortableUpdate != "")
                    {
                        UIEngineInfo["InitScript"] += "$scope." + _UIElement.SortableUpdate + "();";
                    }

                    UIEngineInfo["InitScript"] += "} }); window.setTimeout(function(){ $(" + Handle + ").css('cursor','move'); }, 2000);" + Environment.NewLine;
                }
            }

            int colspan = 0;
            if (Markup.Contains("[Grid:Header]"))
            {
                List<string> col = Grid.Header(_UIElement, UIEngineInfo);
                Markup = Markup.Replace("[Grid:Header]", col[0]);
                colspan = Convert.ToInt32(col[1]);
            }
            if (((IDictionary<string, object>)_UIElement).ContainsKey("Search") == true && _UIElement.Search != null && _UIElement.Search == "true")
            {
                if (Markup.Contains("[Grid:Search]"))
                {
                    Markup = Markup.Replace("[Grid:Search]", Grid.Search(ref _UIElement, ref UIEngineInfo, SearchMarkup));
                }
            }

            if (Markup.Contains("[Grid:Filter]"))
            {
                Markup = Markup.Replace("[Grid:Filter]", Grid.Filter(ref _UIElement, ref UIEngineInfo));
            }

            if (Markup.Contains("[Grid:Items]"))
            {
                Markup = Markup.Replace("[Grid:Items]", Grid.Item(ref _UIElement));
            }

            #region Footer for Pagging
            if (((IDictionary<string, object>)_UIElement).ContainsKey("Paged") == true && _UIElement.Paged != null)
            {
                if (Markup.Contains("[Grid:Footer]"))
                {
                    Markup = Markup.Replace("[Grid:Footer]", Grid.Footer(ref _UIElement, colspan));
                }
            }
            #endregion End Footer
            return ReplaceTokenWithEmpty(Markup);
        }

        private static string ReplaceTokenWithEmpty(string Markup)
        {
            //Replace Grid Token with empty string..
            Markup = Markup.Replace("[Grid:Header]", string.Empty);
            Markup = Markup.Replace("[Grid:Search]", string.Empty);
            Markup = Markup.Replace("[Grid:Filter]", string.Empty);
            Markup = Markup.Replace("[Grid:Items]", string.Empty);
            Markup = Markup.Replace("[Grid:Footer]", string.Empty);
            Markup = Markup.Replace("[ToggleOn]", string.Empty);
            Markup = Markup.Replace("[ToggleOff]", string.Empty);
            return Markup;
        }
        private static string BuildDataControllerScript(dynamic UIData, StringBuilder Error)
        {
            StringBuilder script = new StringBuilder();
            script.Append("{ ");

            try
            {
                foreach (dynamic _Setting in UIData)
                {
                    script.Append("\"" + _Setting.Name + "\": ");
                    try
                    {
                        script.Append(DotNetNuke.Common.Utilities.Json.Serialize<dynamic>(_Setting));
                    }
                    catch (Exception) { }
                    script.Append(",");
                }
            }
            catch (Exception ex) { Error.Append("<br /><span class=\"MNormalRed\">Failed to build data script.</span><p>" + ex.Message + ".</p>"); }

            script.Length--; //Remove trailing comma
            script.Append("}");
            return script.ToString();
        }

        public class Grid
        {
            public static List<string> Header(dynamic _UIElement, Dictionary<string, string> UIEngineInfo)
            {

                StringBuilder sb = new StringBuilder();
                List<string> col = new List<string>();
                int colspan = 0;
                foreach (dynamic column in _UIElement.Column)
                {
                    colspan += 1;
                    string attr = string.Empty;
                    if (((IDictionary<string, object>)column).ContainsKey("sort") == true && column.sort != null)
                    {
                        if (column.sort == "true")
                        {
                            attr += " st-sort=" + column.displayname + "";
                            column.Class += "stsort";
                        }
                    }
                    if (((IDictionary<string, object>)column).ContainsKey("stratio") == true && column.stratio != null)
                    {
                        attr += " st-ratio=" + column.stratio.Value + "";
                    }

                    if (((IDictionary<string, object>)column).ContainsKey("accessroles") == true && column.accessroles != null)
                    {
                        attr += " access-roles=" + column.accessroles.Value + "";
                    }

                    if (((IDictionary<string, object>)column).ContainsKey("control") == true && column.control == null && ((IDictionary<string, object>)column).ContainsKey("Attributes") == true && column.Attributes != null && column.Attributes != "")
                    {
                        attr += " " + column.Attributes;
                    }

                    string placeholder = Localization.GetLocal(_UIElement.Name.Value + "_Columns_" + column.displayname, "Text", UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"], UIEngineInfo["showmissingkeys"] == "true" ? true : false);

                    sb.Append("<th" + attr + " class='" + column.Class + " hdtableth'>" + placeholder + "</th>");
                }
                col.Add(sb.ToString());
                col.Add(colspan.ToString());
                return col;
            }


            public static string Search(ref dynamic _UIElement, ref Dictionary<string, string> UIEngineInfo, string SearchMarkup)
            {

                string placeholder = Localization.GetLocal(_UIElement.Name.Value + "_Search", "Text", UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"], UIEngineInfo["showmissingkeys"] == "true" ? true : false);

                if (SearchMarkup.Contains("[placeholder]"))
                {
                    SearchMarkup = SearchMarkup.Replace("[placeholder]", placeholder);
                }

                return SearchMarkup;
            }

            public static string Filter(ref dynamic _UIElement, ref Dictionary<string, string> UIEngineInfo)
            {
                StringBuilder sb = new StringBuilder();
                foreach (dynamic column in _UIElement.Column)
                {
                    if (((IDictionary<string, object>)column).ContainsKey("filter") == true && column.filter != null && column.filter == "true")
                    {
                        string placeholder = Localization.GetLocal(_UIElement.Name.Value + "_" + column.displayname + "_Filter", "Text", UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"], UIEngineInfo["showmissingkeys"] == "true" ? true : false);
                        sb.Append("<tr><th><div class=\"input-group\"><input st-search=" + column.data.Value + " class=\"input-sm form-control\" placeholder='" + placeholder + "' type=\"search\"/></div></th></tr>");
                    }
                }
                return sb.ToString();
            }


            public static string Item(ref dynamic _UIElement)
            {
                StringBuilder sb = new StringBuilder();
                foreach (dynamic column in _UIElement.Column)
                {
                    StringBuilder attr = new StringBuilder();
                    if (((IDictionary<string, object>)column).ContainsKey("stratio") == true && column.stratio != null)
                    {
                        attr.Append(" st-ratio=" + column.stratio.Value + "");
                    }

                    if (column.control == null && column.Attributes != null)
                    {
                        attr.Append(" " + column.Attributes);
                    }

                    if (((IDictionary<string, object>)column).ContainsKey("accessroles") == true && column.accessroles != null)
                    {
                        attr.Append(" access-roles=" + column.accessroles.Value + "");
                    }

                    if (((IDictionary<string, object>)column).ContainsKey("control") == true && column.control == null && ((IDictionary<string, object>)column).ContainsKey("Attributes") == true && column.Attributes != null && column.Attributes != "")
                    {
                        attr.Append(" " + column.Attributes);
                    }

                    if (column.control != null)
                    {
                        if (((IDictionary<string, object>)column).ContainsKey("custommarkup") == true)
                        {
                            sb.Append("<td" + attr + ">" + column.custommarkup + "</td>");
                        }
                    }
                    else if (column.data != null)
                    {
                        if (((IDictionary<string, object>)column).ContainsKey("format") == true && column.format != null)
                        {
                            sb.Append("<td" + attr + ">{{row." + column.data.Value + " | " + column.format.Value + "}}</td>");
                        }
                        else
                        {
                            sb.Append("<td" + attr + ">{{row." + column.data.Value + "}}</td>");
                        }
                    }
                    else
                    {
                        if (((IDictionary<string, object>)column).ContainsKey("custommarkup") == true)
                        {
                            sb.Append("<td" + attr + ">" + column.custommarkup + "</td>");
                        }
                    }
                }
                return sb.ToString();
            }


            public static string Footer(ref dynamic _UIElement, int colspan)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<tfoot><tr>");
                sb.Append("<td colspan=" + colspan + " class=\"text-center stfooter\">");
                sb.Append("<div st-pagination=\"\" st-items-by-page=\"" + _UIElement.PageSize + "\" st-displayed-pages=\"" + _UIElement.Xpage + "\"></div>");
                sb.Append("</td></tr></tfoot>");
                return sb.ToString();
            }

        }



        internal static string GetConfig(string AppName, string FrameworkTemplatePath, string AppTemplatePath, string[] Dependencies, List<AngularView> Templates, string AppConfigJS, string AppJS, string SharedAppTemplateResourceFile, bool ShowMissingKeys)
        {
            string dtkey = ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

            string AppConfigJSFile = string.Empty;
            string AppJs = string.Empty;

            if (File.Exists(HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/" + AppJS))))
            {
                AppJs = new DNNLocalizationEngine(null, SharedAppTemplateResourceFile, ShowMissingKeys).Parse(File.ReadAllText(HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/" + AppJS))) + Environment.NewLine);
            }

            if (File.Exists(HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/" + AppConfigJS))))
            {
                AppConfigJSFile = new DNNLocalizationEngine(null, SharedAppTemplateResourceFile, ShowMissingKeys).Parse(File.ReadAllText(HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/" + AppConfigJS))) + Environment.NewLine);
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder deps = new StringBuilder();
            deps.Append("['mnCommonService'");

            foreach (string d in Dependencies)
            {
                deps.Append(",'" + d + "'");
            }

            deps.Append("]");

            sb.Append("var app = angular.module('" + AppName + "'," + deps.ToString() + ");");

            sb.Append("app.config(function ($routeProvider, $locationProvider) {" + AppConfigJSFile +
                      "$routeProvider.");

            foreach (AngularView t in Templates.Where(t => !t.IsDefaultTemplate))
            {
                foreach (string u in t.UrlPaths)
                {
                    string routeurl = string.Empty;
                    if (t.URLPathType == URLPathTypes.Literal)
                    {
                        routeurl += "'/";
                    }

                    routeurl += u;
                    if (t.URLPathType == URLPathTypes.Literal)
                    {
                        routeurl += "'";
                    }

                    if (t.IsCommon == false)
                    {
                        sb.Append("when(").Append(routeurl).Append(", { " +
                                  "templateUrl: '" + AppTemplatePath + t.TemplatePath + "?v=" + dtkey + "'," +
                                  "accessRoles: '" + t.AccessRoles + "'" +
                                  "}).");
                    }
                    else
                    {
                        sb.Append("when(").Append(routeurl).Append(", { " +
                                   "templateUrl: '" + FrameworkTemplatePath + "Views/" + t.TemplatePath + "?v=" + dtkey + "'," +
                                   "accessRoles: '" + t.AccessRoles + "'" +
                                   "}).");
                    }
                }
            }

            //Add Licensing
            sb.Append("when('/licensing', { " +
                          "templateUrl: '" + FrameworkTemplatePath + "Views/Common/licensing.html?v=" + dtkey + "'," +
                          "accessRoles: 'host'" +
                          "}).");

            //Add activation
            sb.Append("when('/activation', { " +
                          "templateUrl: '" + FrameworkTemplatePath + "Views/Common/activation.html?v=" + dtkey + "'," +
                          "accessRoles: 'host'" +
                          "}).");

            ////Add multiple activation
            //sb.Append("when('/common-licensing', { " +
            //              "templateUrl: '" + FrameworkTemplatePath + "Views/Common/multipleActivation.html'," +
            //              "accessRoles: 'host'" +
            //              "}).");

            //Add Unauthorized
            sb.Append("when('/unauthorized', { " +
                          "templateUrl: '" + FrameworkTemplatePath + "Views/Common/unauthorized.html?v=" + dtkey + "'," +
                          "accessRoles: ''" +
                          "}).");

            //Add EditorConfig
            sb.Append("when('/common/controls/editorconfig/:uid', { " +
                          "templateUrl: '" + FrameworkTemplatePath + "Views/Common/controls/editorconfig.html?v=" + dtkey + "'," +
                          "accessRoles: 'admin'" +
                          "}).");

            AngularView template = Templates.Where(t => t.IsDefaultTemplate).FirstOrDefault();

            if (template != null)
            {
                sb.Append("otherwise({ " +
                             "templateUrl: '" + AppTemplatePath + template.TemplatePath + "?v=" + dtkey + "'," +
                             "accessRoles: '" + template.AccessRoles + "'" +
                             "});");
            }
            else
            {
                sb.Length--;
                sb.Append(";");
            }

            sb.Append("});");

            sb.Append("app.controller('Controller', function ($rootScope, $scope, $attrs, $http, CommonSvc) { " +
                      "CommonSvc.initData($rootScope, $scope, $attrs, $http); " +
                      "});");

            sb.Append("app.filter('unsafe', function($sce) {return function(val) {return $sce.trustAsHtml(val);};});");
            sb.Append(AppJs);

            return sb.ToString();
        }
    }
}

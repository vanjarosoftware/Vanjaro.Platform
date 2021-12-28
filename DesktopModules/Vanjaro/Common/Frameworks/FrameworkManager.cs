using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Vanjaro.Common.ASPNET;

namespace Vanjaro.Common
{
    public static class FrameworkManager
    {
        /// <summary>
        /// Relative Path to the Frameworks Directory within Vanjaro Common Library
        /// </summary>
        private const string FrameworksRoot = "DesktopModules/Vanjaro/Common/Frameworks";

        /// <summary>
        /// Maintains the default version for each framework
        /// </summary>
        private static readonly Dictionary<string, string> FrameworksVersions = new Dictionary<string, string>
        {
            {"AngularJS","1.8.2"},
            {"Bootstrap","5.0.0"},
            {"FontAwesome","5.10.1"},
            {"WebAPI","1.0.0"},
            {"HtmlParser","1.0.0"},
            {"Html2Json","1.0.0"},
            {"Ckeditor",""},
            {"CanvasJS",""},
            {"ReCaptcha",""},
            {"ColorPicker","" },
            {"ValidationJS","1.14.0"},
            {"jQueryAutocomplete","1.0.7"},
            {"jQueryMoment","2.29.0"},
            {"jQuerySweetAlert","1.1.3"},
            {"Mark","8.9.1"},
            {"Numeral","2.0.6"},
            {"JqueryCookie","1.4.1"},
            {"JsonViewer",""},
            {"Barcode",""},
            {"Notify",""},
            {"CodeMirror","5.61.1"},
            {"Toastr","2.1.4"},
            {"ContextMenu","2.8.0"},
            {"BootstrapDatepicker","1.9.0"},
            {"SpectrumColorPicker","1.8.0"},
            {"EnjoyHint",""},
        };

        /// <summary>
        /// Returns an absolute URL to the FilePath for specified Framework with Default Version
        /// </summary>
        /// <param name="Framework"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string Request(string Framework, string FilePath)
        {

            return Request(Framework, GetDefaultVersion(Framework), FilePath);
        }

        /// <summary>
        /// Returns an absolute URL to the FilePath for specified Framework with specified Version
        /// </summary>
        /// <param name="Framework"></param>
        /// <param name="Version"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string Request(string Framework, string Version, string FilePath)
        {
            switch (Framework)
            {
                case "jQueryAutocomplete":
                    {
                        Framework = "jQuery/Plugins/Autocomplete/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "Mark":
                    {
                        Framework = "jQuery/Plugins/Mark/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "jQueryMoment":
                    {
                        Framework = "jQuery/Plugins/Moment/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "jQuerySweetAlert":
                    {
                        Framework = "jQuery/Plugins/SweetAlert/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "Numeral":
                    {
                        Framework = "jQuery/Plugins/Numeral/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "JqueryCookie":
                    {
                        Framework = "jQuery/Plugins/Cookie/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "Notify":
                    {
                        Framework = "jQuery/Plugins/Notify/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "JsonViewer":
                    {
                        Framework = "jQuery/Plugins/JsonViewer/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                case "Barcode":
                    {
                        Framework = "jQuery/Plugins/Barcode/";
                        return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
                    }
                default:
                    return VirtualPathUtility.ToAbsolute("~/" + FrameworksRoot + "/" + Framework + "/" + Version + "/" + FilePath);
            }

        }

        internal static string[] GetDependenciesModuleNames(string[] dependencies)
        {
            List<string> ModuleNames = new List<string>();

            foreach (string d in dependencies)
            {
                if (Enum.IsDefined(typeof(AngularPlugins), d))
                {
                    ModuleNames.Add(GetAngularPluginModuleName((AngularPlugins)Enum.Parse(typeof(AngularPlugins), d)));
                }
            }

            return ModuleNames.ToArray();
        }


        public static void LoadPlugin(Control Control, string Framework, string Plugin, string FilePath)
        {
            LoadPlugin(Control.Page, Framework, Plugin, FilePath, true);
        }
        public static void LoadPlugin(Control Control, string Framework, string Plugin, string FilePath, bool Composite)
        {
            LoadPlugin(Control.Page, Framework, Plugin, FilePath, Composite);
        }
        public static void LoadPlugin(Page Page, string Framework, string Plugin, string FilePath)
        {
            LoadPlugin(Page, Framework, Plugin, FilePath, true);
        }
        public static void LoadPlugin(Page Page, string Framework, string Plugin, string FilePath, bool Composite)
        {
            switch (Framework)
            {
                case "AngularJS":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", FrameworkManager.Request("AngularJS", "Plugins", Plugin + "/mnCommonService.css"));
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + "AngularJS" + "-" + Plugin + "-" + FilePath, FrameworkManager.Request("AngularJS", "Plugins", Plugin + "/" + FilePath), Composite);
                        break;
                    }
            }
        }

        public static void Load(Control Control, string Framework)
        {
            Load(Control.Page, Framework);
        }
        public static void Load(Control Control, string Framework, bool Composite)
        {
            Load(Control.Page, Framework, Composite);
        }
        public static void Load(Page Page, string Framework)
        {
            Load(Page, Framework, true);
        }
        public static void Load(Page Page, string Framework, bool Composite)
        {
            switch (Framework)
            {
                case "AngularJS":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework, Request(Framework, "angular.min.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-route", Request(Framework, "angular-route.min.js"), Composite);
                        break;
                    }
                case "Bootstrap":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/bootstrap.min.css"), Composite, "DnnPageHeaderProvider", 0);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/bootstrap.bundle.min.js"), Composite, "DnnBodyProvider");
                        break;
                    }
                case "FontAwesome":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/fontawesome.css"), Composite);
                        break;
                    }
                case "jQueryAutocomplete":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/jquery.auto-complete.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/jquery.auto-complete.min.js"), Composite);
                        break;
                    }
                case "Mark":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/mark.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/jquery.mark.min.js"), Composite);
                        break;
                    }
                case "jQueryMoment":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "moment.min.js"), Composite);
                        break;
                    }
                case "jQuerySweetAlert":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/sweetalert.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/sweetalert.min.js"), Composite);
                        break;
                    }
                case "WebAPI":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "webAPI.min.js"), Composite);
                        break;
                    }

                case "Ckeditor":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "ckeditor.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-angular-JS", Request(Framework, "ng-ckeditor.min.js"), Composite);
                        break;
                    }
                case "RazorCkeditor":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + "Ckeditor-JS", Request("Ckeditor", "ckeditor.js"), false);
                        break;
                    }
                case "ValidationJS":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "validate.min.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-init-JS", Request(Framework, "mnValidationService.min.js"), Composite);
                        break;
                    }
                case "ReCaptcha":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-recaptcha-JS", Request(Framework, "recaptcha.min.js"), Composite);
                        break;
                    }
                case "Numeral":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/numeral.min.js"), Composite);
                        break;
                    }
                case "Notify":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "notify.min.js"), Composite);
                        break;
                    }
                case "JqueryCookie":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "jquery.cookie.min.js"), Composite);
                        break;
                    }
                case "JsonViewer":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "jquery.json-viewer.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "jquery.json-viewer.js"), Composite);
                        break;
                    }
                case "ColorPicker":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "ColorPicker.min.js"), Composite);
                        break;
                    }
                case "Barcode":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "jquery-barcode.min.js"), Composite);
                        break;
                    }
                case "CodeMirror":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/codemirror.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/codemirror.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/css.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/htmlmixed.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/sql.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/placeholder.js"), Composite);
                        break;
                    }
                case "Toastr":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/toastr.min.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/toastr.min.js"), Composite);
                        break;
                    }
                case "ContextMenu":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/jquery.contextMenu.min.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/jquery.contextMenu.min.js"), Composite);

                        break;
                    }
                case "BootstrapDatepicker":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/bootstrap-datepicker.min.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/bootstrap-datepicker.min.js"), Composite);

                        break;
                    }
                case "SpectrumColorPicker":
                    {
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "css/spectrum.min.css"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "js/spectrum.min.js"), Composite);

                        break;
                    }
                case "EnjoyHint":
                    {
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "kinetic.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "jquery.scrollTo.min.js"), Composite);
                        WebForms.RegisterClientScriptInclude(Page, "VJ-" + Framework + "-JS", Request(Framework, "enjoyhint.min.js"), Composite);
                        WebForms.LinkCSS(Page, "VJ-" + Framework + "-CSS", Request(Framework, "enjoyhint.css"), Composite);
                        break;
                    }
                default:
                    break;
            }
        }

        public static string GetDefaultVersion(string Framework)
        {
            string Version = null;

            try { Version = FrameworksVersions[Framework]; }
            catch { throw new Exception("An error occurred finding default version of " + Framework + " Framework"); }

            return Version;

        }

        public enum Frameworks
        {
            FontAwesome = 1
        }
        public enum JavaScriptPlugins
        {
            ReCaptcha = 0,
            //CanvasJS = 1,
            ValidationJS = 2,
            JsonViewer = 3,
            Notify = 4,
            Numeral = 5,
            ColorPicker = 6,
            Barcode = 7,
            CodeMirror = 8,
            Toastr = 9,
            ContextMenu = 10,
            BootstrapDatepicker = 11,
            SpectrumColorPicker = 12,
            jQueryMoment = 13,
            EnjoyHint = 14
        }
        public enum AngularPlugins
        {
            AutoComplete = 0,
            CKEditor = 1,
            CSVImport = 2,
            Grid = 3,
            FileUpload = 4,
            Dialog = 5,
            InlineEditor = 6,
            Tags = 7,
            TreeView = 8

        }
        public static string GetAngularPluginModuleName(AngularPlugins Plugin)
        {
            switch (Plugin)
            {
                case AngularPlugins.AutoComplete:
                    return "angucomplete-alt";
                case AngularPlugins.CKEditor:
                    return "ngCkeditor";
                case AngularPlugins.CSVImport:
                    return "ngCsvImport";
                case AngularPlugins.Grid:
                    return "smart-table";
                case AngularPlugins.FileUpload:
                    return "angularFileUpload";
                case AngularPlugins.Dialog:
                    return "ngDialog";
                case AngularPlugins.InlineEditor:
                    return "xeditable";
                case AngularPlugins.Tags:
                    return "ngTagsInput";
                case AngularPlugins.TreeView:
                    return "ui.tree";
            }

            return string.Empty;
        }
    }
}
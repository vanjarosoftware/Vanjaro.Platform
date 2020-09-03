using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Scheduling;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Vanjaro.Common.ASPNET;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Globals;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.Common.Foundation
{
    public abstract class AngularModuleBase : PortalModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebForms.RegisterStartupScript(Page, App.Name + "-UIEngineAngularBootstrapPath", "<script>var " + App.Name + "_UIEngineAngularBootstrapPath ='" + UIEngineAngularBootstrapPath + "'; var " + App.Name + "_UITemplatePath = '" + AppTemplatePath + "';</script>", false);

            //Register App JS
            if (!IsPostBack)
            {
                WebForms.RegisterStartupScriptInclude(Page, App.Name + "-AppJS", Page.ResolveUrl(ScriptHandler + "?appname=" + App.Name.ToLower() + "&portalid=" + PortalId.ToString() + "&moduleid=" + ModuleId.ToString()));


                Dictionary<string, object> appProperties = new Dictionary<string, object>
                {
                    { "AppName", App },
                    { "AppTemplatePath", Page.ResolveUrl(AppTemplatePath) },
                    { "FrameworkTemplatePath", Page.ResolveUrl(FrameworkTemplatePath) },
                    { "AngularTemplates", AngularViews },
                    { "ShowMissingKeys", ShowMissingKeys.ToString().ToLower() },
                    { "ResourceFilePath", AppResourceFilePath },
                    { "AppConfigJS", AppConfigJS },
                    { "AppJS", AppJSPath },
                    { "Dependencies", FrameworkManager.GetDependenciesModuleNames(Dependencies) }
                };

                Application["app-" + App.Name.ToLower()] = appProperties;

                InstallScheduler();

            }

            //Register Angular Template Paths
            WebForms.RegisterClientScriptBlock(Page, "AngularTemplates", "var mnAngularTemplatePath = { };", true);
            WebForms.RegisterStartupScript(Page, App.Name + "-TemplatePath", "mnAngularTemplatePath['" + App.Name + "'] = '" + Page.ResolveUrl(AppTemplatePath) + "';", true);

            //Register APP CSS
            if (!IsPostBack && File.Exists(Server.MapPath(AppCSSPath)))
            {
                WebForms.LinkCSS(Page, App.Name + "-CSS", Page.ResolveUrl(AppCSSPath), true);
            }
        }

        private void InstallScheduler()
        {
            if (SchedulingProvider.Instance().GetSchedule("Vanjaro.Common.Components.Scheduler,Vanjaro.Common", string.Empty) == null)
            {
                ScheduleItem Manager = new ScheduleItem
                {
                    TypeFullName = "Vanjaro.Common.Components.Scheduler,Vanjaro.Common",
                    Enabled = true,
                    TimeLapse = 2,
                    TimeLapseMeasurement = "m",
                    RetryTimeLapse = -1,
                    RetryTimeLapseMeasurement = "s",
                    RetainHistoryNum = 5,
                    FriendlyName = "Vanjaro Common"
                };
                SchedulingProvider.Instance().AddSchedule(Manager);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //WebForms.InitURLLibrary(this);

            //Request Services Framework
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            //Request jQuery & UI Support
            JavaScript.RequestRegistration(CommonJs.jQuery);

            if (Dependencies.Contains(Frameworks.jQueryUI.ToString()))
            {
                JavaScript.RequestRegistration(CommonJs.jQueryUI);
                WebForms.LinkCSS(Page, "JQuery" + "-JQuery-css", FrameworkManager.Request("JQuery", "css", "smoothness.css"));
            }

            WebForms.RegisterClientScriptInclude(Page, "DNNModal", Page.ResolveUrl("js/dnn.modalpopup.js"));

            //Load Angular JS
            FrameworkManager.Load(this, "AngularJS");

            //Load Angular Common Service
            FrameworkManager.LoadPlugin(this, "AngularJS", "mnCommonService", "mnCommonService.min.js");

            //Load Ckeditor
            if (Dependencies.Contains(AngularPlugins.CKEditor.ToString()))
            {
                FrameworkManager.Load(this, "Ckeditor", false);
            }

            //Load ColorPicker
            if (Dependencies.Contains(JavaScriptPlugins.ColorPicker.ToString()))
            {
                FrameworkManager.Load(this, "ColorPicker");
            }

            if (Dependencies.Contains(JavaScriptPlugins.ValidationJS.ToString()))
            {
                FrameworkManager.Load(this, "ValidationJS");
            }

            if (Dependencies.Contains(Frameworks.FontAwesome.ToString()))
            {
                FrameworkManager.Load(this, "FontAwesome");
            }

            if (Dependencies.Contains(JavaScriptPlugins.SpectrumColorPicker.ToString()))
            {
                FrameworkManager.Load(this, "SpectrumColorPicker");
            }

            if (Dependencies.Contains(JavaScriptPlugins.ContextMenu.ToString()))
            {
                FrameworkManager.Load(this, "ContextMenu");
            }

            //Load JsonViewer
            if (Dependencies.Contains(JavaScriptPlugins.JsonViewer.ToString()))
            {
                FrameworkManager.Load(this, "JsonViewer");
            }

            //Load JsonViewer
            if (Dependencies.Contains(JavaScriptPlugins.Barcode.ToString()))
            {
                FrameworkManager.Load(this, "Barcode");
            }

            if (Dependencies.Contains(JavaScriptPlugins.Notify.ToString()))
            {
                FrameworkManager.Load(this, "Notify");
            }

            if (Dependencies.Contains(JavaScriptPlugins.Numeral.ToString()))
            {
                FrameworkManager.Load(this, "Numeral");
            }

            if (Dependencies.Contains(JavaScriptPlugins.CodeMirror.ToString()))
            {
                FrameworkManager.Load(this, "CodeMirror");
            }

            if (Dependencies.Contains(JavaScriptPlugins.Toastr.ToString()))
            {
                FrameworkManager.Load(this, "Toastr");
            }

            if (Dependencies.Contains(JavaScriptPlugins.BootstrapDatepicker.ToString()))
            {
                FrameworkManager.Load(this, "BootstrapDatepicker");
            }

            if (Dependencies.Contains(AngularPlugins.Grid.ToString()))
            {
                FrameworkManager.LoadPlugin(this, "AngularJS", "SmartTable", "smart-table.min.js");
            }

            if (Dependencies.Contains(AngularPlugins.FileUpload.ToString()))
            {
                FrameworkManager.LoadPlugin(this, "AngularJS", "FileUpload", "ng-file-upload.min.js");
            }

            if (Dependencies.Contains(AngularPlugins.CSVImport.ToString()))
            {
                FrameworkManager.LoadPlugin(this, "AngularJS", "AngularCsvImport", "angular-csv-import.min.js");
                WebForms.RegisterClientScriptBlock(Page, "ImportHandler", "var mnImportHandler = '" + Page.ResolveUrl("DesktopModules/Vanjaro/Common/Handlers/ImportMapHandler.ashx?portalid=" + PortalId.ToString()) + "';", true);
            }


            WebForms.LinkCSS(Page, "AngularJS" + "-SweetAlert-css", FrameworkManager.Request("AngularJs", "Plugins", "SweetAlert/sweetalert.css"));
            FrameworkManager.LoadPlugin(this, "AngularJS", "SweetAlert", "alert.min.js");
            FrameworkManager.LoadPlugin(this, "AngularJS", "SweetAlert", "SweetAlert.min.js");


            if (Dependencies.Contains(AngularPlugins.TreeView.ToString()))
            {
                WebForms.LinkCSS(Page, "AngularJS" + "-TreeView-css", FrameworkManager.Request("AngularJs", "Plugins", "TreeView/angular-ui-tree.min.css"));
                FrameworkManager.LoadPlugin(this, "AngularJS", "TreeView", "angular-ui-tree.min.js");
            }

            if (Dependencies.Contains(AngularPlugins.Dialog.ToString()))
            {
                WebForms.LinkCSS(Page, "AngularJS" + "-Dialog-css", FrameworkManager.Request("AngularJs", "Plugins", "Dialog/ngDialog.min.css"));
                WebForms.LinkCSS(Page, "AngularJS" + "-Dialog-default-css", FrameworkManager.Request("AngularJs", "Plugins", "Dialog/ngDialog-theme-default.min.css"));
                FrameworkManager.LoadPlugin(this, "AngularJS", "Dialog", "ngDialog.min.js");
            }

            //Load Angular Loading Bar
            FrameworkManager.LoadPlugin(this, "AngularJS", "loading-bar", "loading-bar.min.js");
            WebForms.LinkCSS(Page, "AngularJS" + "-loading-css", FrameworkManager.Request("AngularJs", "Plugins", "loading-bar/loading-bar.min.css"));

            //Load Angular XEditable
            if (Dependencies.Contains(AngularPlugins.InlineEditor.ToString()))
            {
                FrameworkManager.LoadPlugin(this, "AngularJS", "x-editable", "js/xeditable.min.js");
                WebForms.LinkCSS(Page, "AngularJS" + "-xeditable-css", FrameworkManager.Request("AngularJs", "Plugins", "x-editable/css/xeditable.css"));
            }

            //Load Angular Tags Input
            if (Dependencies.Contains(AngularPlugins.Tags.ToString()))
            {
                FrameworkManager.LoadPlugin(this, "AngularJS", "TagsInput", "js/ng-tags-input.min.js");
                WebForms.LinkCSS(Page, "AngularJS" + "-tags-input-css", FrameworkManager.Request("AngularJs", "Plugins", "tagsinput/css/ng-tags-input.min.css"));
                WebForms.LinkCSS(Page, "AngularJS" + "-tags-input-bootstrap-css", FrameworkManager.Request("AngularJs", "Plugins", "tagsinput/css/ng-tags-input.bootstrap.min.css"));
            }

            //Load autocomplete
            if (Dependencies.Contains(AngularPlugins.AutoComplete.ToString()))
            {
                FrameworkManager.LoadPlugin(this, "AngularJS", "autocomplete", "js/autocomplete.min.js");
                WebForms.LinkCSS(Page, "AngularJS" + "-autocomplete-css", FrameworkManager.Request("AngularJs", "Plugins", "autocomplete/css/autocomplete.css"));
            }



            //Load Bootstrap
            FrameworkManager.Load(this, "Bootstrap");

            //Load WebAPI
            FrameworkManager.Load(this, "WebAPI");

            if (Dependencies.Contains(JavaScriptPlugins.ReCaptcha.ToString()))
            {
                FrameworkManager.Load(this, "ReCaptcha");
            }

            //Add Necessary Markup for Angular App
            Literal lit = new Literal
            {
                Text = "<div class=\"container-fluid " + App.Name + " \" id=\"" + App.Name + ModuleId.ToString() + "\" data-app-name=\"" + App.Name + " \" data-show-missing-keys=\"" + ShowMissingKeys.ToString().ToLower() + "\" data-roles=\"" + string.Join(",", AccessRoles) + "\" data-ModuleId=\"" + ModuleId.ToString() + "\" ng-controller=\"" + ControllerName + "\"><div ng-view=\"\" class=\"view-animate\"></div></div>"
            };
            Controls.Add(lit);

            if (!IsPostBack)
            {
                //Add Necessary Script to Initialize App
                string AngularAppScript = "angular.element(document).ready(function () {angular.bootstrap(document.getElementById(\"" + App.Name + ModuleId.ToString() + "\"), [\"" + App.Name + "\"]);});";
                WebForms.RegisterStartupScript(Page, "angular-init" + ModuleId.ToString(), AngularAppScript, true);

                //serialized form data into hidden field
                Dictionary<string, object> dicFormData = new Dictionary<string, object>();
                foreach (string key in HttpContext.Current.Request.Form.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form[key]))
                        {
                            dicFormData.Add(key, HttpContext.Current.Request.Form[key].Replace("'", "&#39;"));
                        }
                        else
                        {
                            dicFormData.Add(key, HttpContext.Current.Request.Form[key]);
                        }
                    }
                }
                Controls.Add(new Literal() { Text = "<input type=\"hidden\" name=\"hfFormData\" value=\'" + JsonConvert.SerializeObject(dicFormData) + "\'>" });

            }

            //serialized form data into hidden field
            //Dictionary<string, object> dicFormData = new Dictionary<string, object>();
            //foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            //{
            //    dicFormData.Add(key, HttpContext.Current.Request.Form[key]);
            //}
            //this.Controls.Add(new Literal() { Text = "<input type=\"hidden\" name=\"hfFormData\" value=\'" + JsonConvert.SerializeObject(dicFormData) + "\'>" });


            //Literal s = new Literal();
            //lit.Text = "<script>" + AngularAppScript + "</script>";
            //this.Controls.Add(s);

        }

        public abstract AppInformation App { get; }
        public abstract List<AngularView> AngularViews { get; }

        public virtual bool ShowMissingKeys => false;
        public virtual string ScriptHandler => "DesktopModules/Vanjaro/Common/Handlers/Script.ashx";
        public virtual string AppResourceFilePath => "DesktopModules/" + App.Name + "/Views/App_LocalResources/View.ascx.resx";
        public virtual string AppCSSPath => "DesktopModules/" + App.Name + "/Resources/Stylesheets/app.css";
        public virtual string AppJSPath => "DesktopModules/" + App.Name + "/Resources/Scripts/app.js";
        public virtual string AppTemplatePath => "DesktopModules/" + App.Name + "/Views/";
        public virtual string AppConfigJS => string.Empty;
        public virtual string FrameworkTemplatePath => Constants.UIFrameworkPath;
        public virtual string ControllerName => "Controller";
        public virtual string AccessRoles => string.Empty;
        public virtual string UIEngineAngularBootstrapPath => "~/DesktopModules/" + App.Name + "/Resources/UIEngine/AngularBootstrap";
        public virtual string[] Dependencies => new string[] { };
    }



}
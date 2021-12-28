using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework.Providers;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Vanjaro.Common.ASPNET
{
    public class WebForms
    {
        public enum Execution
        {
            none,
            async,
            defer
        }

        private static int jsPriority = 101;

        public static void LinkCSS(Page Page, string ID, string URL, Execution execution = Execution.none)
        {
            LinkCSS(Page, ID, URL, true, execution);
        }

        public static void LinkCSS(Page Page, string ID, string URL, bool Composite, Execution execution = Execution.none)
        {
            LinkCSS(Page, ID, URL, Composite, "DnnPageHeaderProvider", execution);
        }

        public static void LinkCSS(Page Page, string ID, string URL, bool Composite, string Provider, Execution execution = Execution.none)
        {
            LinkCSS(Page, ID, URL, Composite, Provider, 101, execution);
        }

        public static void LinkCSS(Page Page, string ID, string URL, bool Composite, string Provider, int Priority, Execution execution = Execution.none)
        {
            if (Composite)
            {
                string relativeURL = string.Empty;
                try
                {
                    relativeURL = VirtualPathUtility.ToAppRelative(URL);
                }
                //Exception is thrown in case of 404; the URL passed is missing the ApplicationPath
                catch
                {
                    relativeURL = VirtualPathUtility.ToAppRelative(DotNetNuke.Common.Globals.ApplicationPath + "/" + URL);
                }

                if (Uri.IsWellFormedUriString(relativeURL, UriKind.Relative) && !relativeURL.Contains('?'))
                {
                    string HtmlAttribute = string.Empty;
                    switch (execution)
                    {
                        case Execution.async:
                            HtmlAttribute = "async:async";
                            break;
                        case Execution.defer:
                            HtmlAttribute = "defer:defer";
                            break;
                    }
                    var include = new DnnCssInclude { ForceProvider = Provider, Priority = Priority, FilePath = relativeURL, Name = string.Empty, Version = string.Empty, HtmlAttributesAsString = HtmlAttribute };
                    var loader = Page.FindControl("ClientResourceIncludes");
                    if (loader != null)
                    {
                        loader.Controls.Add(include);
                    }
                    return;
                }
            }

            if (Page.Header.FindControl(ID) == null)
            {
                LiteralControl lit = new LiteralControl
                {
                    ID = ID
                };

                if (!string.IsNullOrEmpty(URL))
                {
                    lit.Text = "<link rel=\"stylesheet\" href=\"" + URL + "\" type=\"text/css\" media=\"all\" />";
                    switch (execution)
                    {
                        case Execution.async:
                            lit.Text = "<link rel=\"stylesheet\" href=\"" + URL + "\" type=\"text/css\" media=\"all\" async=\"async\" />";
                            break;
                        case Execution.defer:
                            lit.Text = "<link rel=\"stylesheet\" href=\"" + URL + "\" type=\"text/css\" media=\"all\" defer=\"defer\" />";
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(lit.Text))
                {
                    if (Priority == 0)
                        Page.Header.Controls.AddAt(Priority, lit);
                    else
                        Page.Header.Controls.Add(lit);
                }
            }
        }

        public static void RegisterClientStyleBlock(Page Page, string ID, string Style, bool AddStyleTags, Execution execution = Execution.none)
        {
            if (Page.Header.FindControl(ID) == null)
            {
                LiteralControl lit = new LiteralControl
                {
                    ID = ID
                };

                if (AddStyleTags)
                {
                    lit.Text = "<style type=\"text/css\">" + Style + "</style>";
                    switch (execution)
                    {
                        case Execution.async:
                            lit.Text = "<style type=\"text/css\" async=\"async\">" + Style + "</style>";
                            break;
                        case Execution.defer:
                            lit.Text = "<style type=\"text/css\" defer=\"defer\">" + Style + "</style>";
                            break;
                    }
                }
                else
                {
                    lit.Text = Style;
                }

                if (!string.IsNullOrEmpty(lit.Text))
                {
                    Page.Header.Controls.Add(lit);
                }
            }
        }

        public static void RegisterClientScriptInclude(Page Page, string ID, string URL, Execution execution = Execution.none)
        {
            RegisterClientScriptInclude(Page, ID, URL, true, execution);
        }

        public static void RegisterClientScriptInclude(Page Page, string ID, string URL, bool Composite, Execution execution = Execution.none)
        {
            RegisterClientScriptInclude(Page, ID, URL, Composite, "DnnPageHeaderProvider", execution);
        }

        public static void RegisterClientScriptInclude(Page Page, string ID, string URL, bool Composite, string Provider, Execution execution = Execution.none)
        {
            if (Composite)
            {
                string relativeURL = string.Empty;
                try
                {
                    relativeURL = VirtualPathUtility.ToAppRelative(URL);
                }

                //Exception is thrown in case of 404; the URL passed is missing the ApplicationPath
                catch
                {
                    relativeURL = VirtualPathUtility.ToAppRelative(DotNetNuke.Common.Globals.ApplicationPath + "/" + URL);
                }

                if (Uri.IsWellFormedUriString(relativeURL, UriKind.Relative) && !relativeURL.Contains('?'))
                {
                    string HtmlAttribute = string.Empty;
                    switch (execution)
                    {
                        case Execution.async:
                            HtmlAttribute = "async:async";
                            break;
                        case Execution.defer:
                            HtmlAttribute = "defer:defer";
                            break;
                    }
                    var include = new DnnJsInclude { ForceProvider = Provider, Priority = jsPriority, FilePath = relativeURL, Name = string.Empty, Version = string.Empty, HtmlAttributesAsString = HtmlAttribute };
                    var loader = Page.FindControl("ClientResourceIncludes");
                    if (loader != null)
                    {
                        loader.Controls.Add(include);
                    }
                    jsPriority++;
                    return;
                }
            }

            if (Page.Header.FindControl(ID) == null)
            {
                string cdv = "cdv=" + Host.CrmVersion;

                if (URL.Contains("?"))
                    cdv = "&" + cdv;
                else
                    cdv = "?" + cdv;

                string LiteralControlText = "<script src=\"" + URL + cdv + "\" type=\"text/javascript\"></script>";
                switch (execution)
                {
                    case Execution.async:
                        LiteralControlText = "<script src=\"" + URL + cdv + "\" type=\"text/javascript\" async=\"async\"></script>";
                        break;
                    case Execution.defer:
                        LiteralControlText = "<script src=\"" + URL + cdv + "\" type=\"text/javascript\" defer=\"defer\"></script>";
                        break;
                }

                LiteralControl lit = new LiteralControl
                {
                    ID = ID,
                    Text = LiteralControlText
                };
                Page.Header.Controls.Add(lit);
            }
        }
        public static void RegisterStartupScriptInclude(Page Page, string ID, string URL, Execution execution = Execution.none)
        {
            string Script = "<script src=\"" + URL + "\" type=\"text/javascript\"></script>";
            switch (execution)
            {
                case Execution.async:
                    Script = "<script src=\"" + URL + "\" type=\"text/javascript\" async=\"async\"></script>";
                    break;
                case Execution.defer:
                    Script = "<script src=\"" + URL + "\" type=\"text/javascript\" defer=\"defer\"></script>";
                    break;
            }
            RegisterStartupScript(Page, ID, Script, false);
        }
        public static void RegisterClientScriptBlock(Page Page, string ID, string Script, bool AddScriptTags)
        {
            ScriptManager SM = ScriptManager.GetCurrent(Page);

            if (SM != null)
            {
                ScriptManager.RegisterClientScriptBlock(Page, typeof(string), ID, Script, AddScriptTags);
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), ID, Script, AddScriptTags);
            }
        }
        public static void RegisterStartupScript(Page Page, string ID, string Script, bool AddScriptTags)
        {
            ScriptManager SM = ScriptManager.GetCurrent(Page);

            if (SM != null)
            {
                ScriptManager.RegisterStartupScript(Page, typeof(string), ID, Script, AddScriptTags);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(typeof(string), ID, Script, AddScriptTags);
            }
        }


        public static void InitURLLibrary(Control control)
        {
            string Message = "This module requires that you're running \"advanced\" mode of DNN URL Rewriter. Legacy modes such as \"HumanFriendly\" & \"SearchFriendly\" are not supported. Feel free to open a support ticket at Vanjaro.com for further assistance.";
            if (HttpContext.Current.Application.AllKeys.Contains("Common-Rewriter"))
            {
                if (HttpContext.Current.Application["Common-Rewriter"].ToString() != "advanced")
                {
                    Skin.AddModuleMessage(control, Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                ProviderConfiguration ProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("friendlyUrl");
                if (ProviderConfiguration != null && !string.IsNullOrEmpty(ProviderConfiguration.DefaultProvider) && ProviderConfiguration.DefaultProvider.ToLower() == "dnnfriendlyurl")
                {
                    string item = ((Provider)ProviderConfiguration.Providers[ProviderConfiguration.DefaultProvider]).Attributes["urlformat"];
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        item = "searchfriendly";
                    }

                    if (item.ToLower() != "advanced")
                    {
                        Skin.AddModuleMessage(control, Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    }

                    HttpContext.Current.Application.Add("Common-Rewriter", item.ToLower());
                }
                else
                {
                    HttpContext.Current.Application.Add("Common-Rewriter", "advanced");
                }
            }
        }
    }
}



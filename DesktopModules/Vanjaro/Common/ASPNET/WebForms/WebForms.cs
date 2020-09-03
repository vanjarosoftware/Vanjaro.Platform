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
        private static int jsPriority = 101;
        private static int cssPriority = 101;

        public static void LinkCSS(Page Page, string ID, string URL)
        {
            LinkCSS(Page, ID, URL, true);
        }
        public static void LinkCSS(Page Page, string ID, string URL, bool Composite)
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
                    ClientResourceManager.RegisterStyleSheet(Page, relativeURL, cssPriority, "DnnPageHeaderProvider");
                    cssPriority++;

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
                }

                if (!string.IsNullOrEmpty(lit.Text))
                {
                    Page.Header.Controls.Add(lit);
                }
            }
        }

        public static void RegisterClientScriptInclude(Page Page, string ID, string URL)
        {
            RegisterClientScriptInclude(Page, ID, URL, true);
        }
        public static void RegisterClientScriptInclude(Page Page, string ID, string URL, bool Composite)
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
                    ClientResourceManager.RegisterScript(Page, relativeURL, jsPriority, "DnnPageHeaderProvider");
                    jsPriority++;

                    return;
                }
            }

            if (Page.Header.FindControl(ID) == null)
            {
                LiteralControl lit = new LiteralControl
                {
                    ID = ID,
                    Text = "<script src=\"" + URL + "\" type=\"text/javascript\"></script>"
                };
                Page.Header.Controls.Add(lit);
            }
        }
        public static void RegisterStartupScriptInclude(Page Page, string ID, string URL)
        {
            string Script = "<script src=\"" + URL + "\" type=\"text/javascript\"></script>";
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



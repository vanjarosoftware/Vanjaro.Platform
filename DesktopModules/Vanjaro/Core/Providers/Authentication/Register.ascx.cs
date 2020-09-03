using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Web.UI;
using Vanjaro.Common.ASPNET;
using Vanjaro.Core.Entities.Menu;

namespace Vanjaro.Core.Providers.Authentication
{
    public partial class Register : UserUserControlBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Dictionary<string, string> Attributes = Core.Managers.BlockManager.GetGlobalConfigs(PortalController.Instance.GetCurrentSettings() as PortalSettings, "register");
            if (Attributes.ContainsKey("data-block-guid") && !string.IsNullOrEmpty(Attributes["data-block-guid"]))
            {
                ThemeTemplateResponse response = Core.Managers.BlockManager.Render(Attributes);
                if (response != null)
                {
                    VJ_Register.Text = response.Markup;
                    if (response.Scripts != null)
                    {
                        foreach (string script in response.Scripts)
                        {
                            if (!string.IsNullOrEmpty(script))
                            {
                                WebForms.RegisterClientScriptInclude(Page, script, Page.ResolveUrl(script));
                            }
                        }
                    }

                    if (response.Styles != null)
                    {
                        foreach (string style in response.Styles)
                        {
                            if (!string.IsNullOrEmpty(style))
                            {
                                WebForms.LinkCSS(Page, style, Page.ResolveUrl(style));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(response.Script))
                    {
                        WebForms.RegisterClientScriptBlock(Page, "BlocksScript", response.Script, true);
                    }

                    VJ_Register.Text = response.Markup;
                }
            }
        }
    }
}
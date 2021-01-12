using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.UserRequest;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;
using System.Web.UI;
using Vanjaro.Common.ASPNET;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Manager;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.Core.Entities.Menu;
using static Vanjaro.Core.Managers;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Vanjaro.Core.Providers.Authentication
{
    public partial class PasswordReset : UserModuleBase, IThemeTemplate
    {
        private const int RedirectTimeout = 3000;
        private string _ipAddress;

        public Guid Guid => Guid.Parse("7dd44be7-0934-428f-99a9-f22bacbe2c31");

        public string ResouceFilePath
        {
            get
            {
                return Globals.ApplicationMapPath + @"\portals\_default\" + Core.Managers.BlockManager.GetTheme() + "App_LocalResources\\Shared.resx";
            }
        }

        public string BlockPath
        {
            get
            {
                return Core.Managers.BlockManager.GetVirtualPath() + Core.Managers.BlockManager.GetTheme() + "templates\\design\\Password Reset\\";
            }
        }
        public string LocalResourcesFile => "~/DesktopModules/Vanjaro/Core/Providers/Authentication/Resources/";

        public ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
        {
            ThemeTemplateResponse response = new ThemeTemplateResponse();
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            response.Markup = GenerateMarkup(Attributes);
            response.Scripts = new string[3] { "~/DesktopModules/Vanjaro/Common/Frameworks/ValidationJS/1.14.0/validate.min.js", LocalResourcesFile + "Scripts/ResetPassword.js", "~/DesktopModules/Vanjaro/Common/Frameworks/ValidationJS/1.14.0/mnValidationService.min.js" };
            return response;
        }

        private string GenerateMarkup(Dictionary<string, string> Attributes)
        {
            try
            {
                IDictionary<string, object> Objects = new ExpandoObject() as IDictionary<string, object>;
                Entities.ResetPassword resetPassword = new Entities.ResetPassword();
                Objects.Add("ResetPassword", resetPassword);
                string Template = RazorEngineManager.RenderTemplate(Guid.ToString(), BlockPath, "Default", Objects);
                Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                return Template;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnLoad(e);
            _ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(Request));

            if (PortalSettings.LoginTabId != -1 && PortalSettings.ActiveTab.TabID != PortalSettings.LoginTabId)
            {
                Response.Redirect(ServiceProvider.NavigationManager.NavigateURL(PortalSettings.LoginTabId) + Request.Url.Query);
            }

            string ResetToken = string.Empty;
            Dictionary<string, string> Attributes = new Dictionary<string, string>();
            if (HttpContext.Current.Request.QueryString["resetToken"] != null)
            {
                ResetToken = HttpContext.Current.Request.QueryString["resetToken"];
            }
            ThemeTemplateResponse response = Render(Attributes);
            if (response != null)
            {
                VJ_PasswordReset.Text = response.Markup;
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

                UserInfo user = UserController.GetUserByPasswordResetToken(PortalId, ResetToken);
                if (user == null)
                {
                    VJ_PasswordReset.Text = "<div class=\"alert alert-primary\" role=\"alert\">" + Localization.GetString("ResetLinkExpired", "~/Admin/Security/App_LocalResources/PasswordReset.ascx.resx") + "</div>";
                }
                else
                {
                    VJ_PasswordReset.Text = response.Markup.Replace("[Token_Username]", user.Username);
                }
            }
        }
    }
}

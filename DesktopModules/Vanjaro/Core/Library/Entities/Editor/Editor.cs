using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Vanjaro.Core.Entities
{
    public class Editor
    {
        public static void RequestRegistration(EditorOptions options)
        {
            Options = options;
            Options.EditPage = false;

            HttpCookie Cookies = new HttpCookie("UXEditor");
            Cookies.Value = CalculateSHA(PortalSettings.Current.UserId.ToString());
            Cookies.Secure = HttpContext.Current.Request.IsSecureConnection;
            HttpContext.Current.Response.Cookies.Add(Cookies);
            HttpContext.Current.Request.Cookies.Add(Cookies);
        }

        private static string CalculateSHA(string Input, Encoding UseEncoding)
        {
            SHA1CryptoServiceProvider CryptoService;
            CryptoService = new SHA1CryptoServiceProvider();

            byte[] InputBytes = UseEncoding.GetBytes(Input);
            InputBytes = CryptoService.ComputeHash(InputBytes);
            return BitConverter.ToString(InputBytes).Replace("-", "");
        }

        private static string CalculateSHA(string Input)
        {
            // That's just a shortcut to the base method
            return CalculateSHA(Input, System.Text.Encoding.Default);
        }

        private static EditorOptions DefaultSettings()
        {
            EditorOptions options = new EditorOptions()
            {
                UpdateContentUrl = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/save'",
                GetContentUrl = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/get'",
                ContainerID = "#vjEditor",
                EditPage = true,
                ModuleId = -1,
                RevisionGUID = "e2f6ebcb-5d68-4d85-b180-058fb2d26178",
            };

            return options;
        }

        public static EditorOptions Options
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items["vjeditor"] != null)
                    return HttpContext.Current.Items["vjeditor"] as EditorOptions;

                return DefaultSettings();
            }
            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Items["vjeditor"] = value;
            }
        }

        public static bool HasExtensionAccess()
        {
            if (HttpContext.Current.Request.Cookies["UXEditor"] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["UXEditor"].Value))
                return HttpContext.Current.Request.Cookies["UXEditor"].Value == CalculateSHA(PortalSettings.Current.UserId.ToString());
            return false;
        }

    }
}
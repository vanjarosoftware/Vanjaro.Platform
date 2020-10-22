using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace Vanjaro.Core.Services
{
    public class Captcha
    {
        public const string SiteKey = "Vanjaro.Integration.Captcha.SiteKey";
        public const string SecretKey = "Vanjaro.Integration.Captcha.SecretKey";
        public const string Enabled = "Vanjaro.Integration.Captcha.Enabled";
        public static void Request()
        {
            if (IsEnabled())
            {
                if (HttpContext.Current != null)
                {
                    Page page = HttpContext.Current.Handler as Page;
                    if (page != null)
                    {
                        Common.ASPNET.WebForms.RegisterStartupScript(page, "vjrecaptcha", "<script type=\"text/javascript\" id=\"vjrecaptcha\" data-sitekey=" + GetSiteKey() + " src=\"https://www.google.com/recaptcha/api.js?render=" + GetSiteKey() + "\"></script>", false);
                    }
                }
            }
        }
        public static bool Validate()
        {
            bool Valid = true;
            if (IsEnabled())
            {
                string ResponseToken = HttpContext.Current.Request.Headers["vj-recaptcha"];

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=" + GetSecretKey() + "&response=" + ResponseToken);
                try
                {
                    //reading Google recaptcha Response
                    using (WebResponse wResponse = req.GetResponse())
                    {
                        using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                        {
                            string jsonResponse = readStream.ReadToEnd();

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            CaptchaResponse res = js.Deserialize<CaptchaResponse>(jsonResponse);
                            if (HttpContext.Current.Request.Url.Host == res.hostname)
                                Valid = res.Success;

                        }
                    }
                }
                catch (WebException webex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(webex);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }

            }
            return Valid;
        }

        public static bool IsEnabled()
        {
            if (Managers.SettingManager.GetPortalSettingAsBoolean(Enabled))
                return true;
            HostController hostController = new HostController();
            return hostController.GetBoolean(Enabled, false);
        }

        public static string GetSiteKey()
        {
            if (Managers.SettingManager.GetPortalSettingAsBoolean(Enabled))
                return Managers.SettingManager.GetPortalSetting(SiteKey, true);

            HostController hostController = new HostController();
            if (hostController.GetBoolean(Enabled, false))
                return hostController.GetEncryptedString(SiteKey, Config.GetDecryptionkey());
            return string.Empty;
        }

        public static string GetSecretKey()
        {
            if (Managers.SettingManager.GetPortalSettingAsBoolean(Enabled))
                return Managers.SettingManager.GetPortalSetting(SecretKey, true);

            HostController hostController = new HostController();
            if (hostController.GetBoolean(Enabled, false))
                return hostController.GetEncryptedString(SecretKey, Config.GetDecryptionkey());
            return string.Empty;
        }
    }
    public class CaptchaResponse
    {
        public bool Success { get; set; }
        public string hostname { get; set; }
    }
}
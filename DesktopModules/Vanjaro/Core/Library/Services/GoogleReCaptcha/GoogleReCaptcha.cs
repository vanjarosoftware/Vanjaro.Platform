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

namespace Vanjaro.Core.Services
{
    public class GoogleReCaptcha
    {
        public static bool Request(int PortalId, string ResponseToken,string hostname)
        {
            string SecretKey = PortalController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SecretKey", PortalId, Config.GetDecryptionkey());
            HostController hostController = new HostController();
            if (string.IsNullOrEmpty(SecretKey))
                SecretKey = hostController.GetEncryptedString("Vanjaro.Integration.GoogleReCaptcha.SecretKey", Config.GetDecryptionkey());

            bool result = false;
            if (string.IsNullOrEmpty(SecretKey))
                return result;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=" + SecretKey + "&response=" + ResponseToken);
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
                        if (hostname == res.hostname)
                            result = res.Success;

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

            return result;
        }
    }
    public class CaptchaResponse
    {
        public bool Success { get; set; }
        public string hostname { get; set; }
    }
}
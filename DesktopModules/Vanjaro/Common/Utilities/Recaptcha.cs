using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Common.Utilities
{
    public class Recaptcha
    {
        public static Dictionary<string, bool> GetValidatedCaptchas(string SecretKey)
        {
            Dictionary<string, bool> CatchaStatus = new Dictionary<string, bool>();
            if (!string.IsNullOrEmpty(SecretKey) && HttpContext.Current != null)
            {
                foreach (string responsekey in HttpContext.Current.Request.Form.AllKeys.Where(k => k.StartsWith("mCommonCaptchaResponse")))
                {
                    string captchaFieldGuid = responsekey.Replace("mCaptchaResponse", "").Replace("_", "-");

                    //if (string.IsNullOrEmpty(HttpContext.Current.Request.Form[responsekey]))
                    //    return true;

                    //HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=" + SecretKey + "&response=" + HttpContext.Current.Request.Form[responsekey]);
                    //try
                    //{
                    //    //reading Google recaptcha Response
                    //    using (WebResponse wResponse = req.GetResponse())
                    //    {
                    //        using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    //        {
                    //            string jsonResponse = readStream.ReadToEnd();

                    //            JavaScriptSerializer js = new JavaScriptSerializer();
                    //            CaptchaResponse res = js.Deserialize<CaptchaResponse>(jsonResponse);

                    //            return res.Success;
                    //        }
                    //    }
                    //}
                    //catch (WebException webex)
                    //{
                    //    DotNetNuke.Services.Exceptions.Exceptions.LogException(webex);
                    //}
                    //catch (Exception ex)
                    //{
                    //    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    //}
                }
            }
            return CatchaStatus;
        }
    }
    public class CaptchaResponse
    {
        public bool Success { get; set; }
    }
}
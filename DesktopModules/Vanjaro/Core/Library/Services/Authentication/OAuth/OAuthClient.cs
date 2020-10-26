using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

namespace Vanjaro.Core.Services.Authentication.OAuth
{
    public class OAuthClient
    {
        public OAuthClient(IOAuthClient provider, string providerName, string clientID, string clientSecret, string authRedirectEndpoint, string authTokenEndpoint, string resourceEndpoint)
        {
            this.ProviderName = providerName;

            this.ClientID = clientID;
            this.ClientSecret = clientSecret;

            this.RedirectUri = new Uri(Globals.LoginURL(string.Empty, false)).ToString(); //.Replace("http","https").Replace("local","com").Replace("dev","www");

            this.AuthRedirectEndPoint = authRedirectEndpoint;
            this.AuthTokenEndPoint = authTokenEndpoint;
            
            this.ResourceEndPoints = new List<string>() { resourceEndpoint };
            
            this.Provider = provider;

            this.AuthMethod = "GET";

            this.User = new OAuthUser();

            this.AuthTokenQuery = true;
        }

        public string GetAuthorizationUrl()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("state", this.ProviderName);
            queryString.Add("scope", this.Scope);
            queryString.Add("client_id", this.ClientID);
            queryString.Add("redirect_uri", this.RedirectUri);
            queryString.Add("response_type", "code");

            return this.AuthRedirectEndPoint + "?" + queryString.ToString();
        }

        private string GetAuthToken(string code)
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("client_id", this.ClientID);
            queryString.Add("client_secret", this.ClientSecret);
            queryString.Add("redirect_uri", this.RedirectUri);
            queryString.Add("grant_type", "authorization_code");
            queryString.Add("code", code);

            string response;

            if (AuthMethod == "GET")
                response = GetWebResponse(this.AuthTokenEndPoint + "?" + queryString.ToString());
            else
                response = PostWebResponse(this.AuthTokenEndPoint, queryString.ToString());

            var dictionary = Json.Deserialize<IDictionary<string, object>>(response);
            this.AuthToken = Convert.ToString(dictionary["access_token"]);
            
            return this.AuthToken;
        }

        public void ProcessResources(string code = null)
        {
            if (string.IsNullOrEmpty(AuthToken) && !string.IsNullOrEmpty(code))
                GetAuthToken(code);

            if (!string.IsNullOrEmpty(AuthToken))
            {
                foreach (var Endpoint in ResourceEndPoints)
                {
                    string accessToken = string.Empty;

                    if (AuthTokenQuery)
                    {
                        if (Endpoint.Contains("?"))
                            accessToken = "&access_token=" + this.AuthToken;
                        else
                            accessToken = "?access_token=" + this.AuthToken;
                    }
                    string response = GetWebResponse(Endpoint + accessToken);

                    Provider.OnResourceResponse(response);
                }
            }

        }



        public string PostWebResponse(string Url, string Parameters)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    if (!string.IsNullOrEmpty(Provider.AuthHeader))
                        wc.Headers.Add(Provider.AuthHeader);

                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    return wc.UploadString(Url, Parameters);
                }
                catch (Exception ex)
                {
                    string test = ex.Message;
                }

                return null;
            }
        }
        public string GetWebResponse(string Url)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    if (!string.IsNullOrEmpty(Provider.AuthHeader))
                        wc.Headers.Add(Provider.AuthHeader);

                    return wc.DownloadString(Url);
                }
                catch (Exception ex)
                {
                    string test = ex.Message;
                }

                return null;
            }
        }
        public string Scope { get; set; }
        public string ProviderName { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }

        public string AuthRedirectEndPoint { get; set; }
        public string AuthTokenEndPoint { get; set; }

        public List<string> ResourceEndPoints { get; set; }
        public string AuthToken { get; private set; }
        public string AuthMethod { get; set; }

        public OAuthUser User { get; set; }
        public IOAuthClient Provider { get; set; }
        public bool AuthTokenQuery { get;  set; }
    }
}
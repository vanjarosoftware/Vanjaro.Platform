using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Services.Authentication.OAuth
{
    public interface IOAuthClient
    {
        bool Enabled { get; }
        string ProviderName { get; }
        string AuthHeader { get; }
        void OnResourceResponse(string response);
        OAuthClient Client { get; set; }
    }
}
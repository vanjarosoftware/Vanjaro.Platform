using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanjaro.Core.Services.Authentication.OAuth
{
    public interface IOAuthClient
    {
        bool Enabled { get; }
        string State { get; }

        OAuthUser GetUser(string response);
        OAuthClient Client { get; set; }
    }
}
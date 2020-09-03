using DotNetNuke.Services.Authentication;
using System;

namespace Vanjaro.Core.UserControls
{
    public partial class Login : AuthenticationLoginBase
    {
        public override bool Enabled => true;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}
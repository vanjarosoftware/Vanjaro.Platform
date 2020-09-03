using DotNetNuke.Services.Authentication;
using System;

namespace Vanjaro.Core.Providers.Authentication
{
    public partial class Settings : AuthenticationSettingsBase
    {
        public override void UpdateSettings()
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}
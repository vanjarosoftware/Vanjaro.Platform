using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vanjaro.Core.Packager.Vanjaro
{
    public partial class Install : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string mode = string.Empty;

            if (this.Request.QueryString["mode"] != null)
            {
                mode = this.Request.QueryString["mode"].ToLowerInvariant();
            }

            if (mode == "upgrade")
            {
                Upgrade();
            }
        }

        private void Upgrade()
        {
            // Start Timer
            DotNetNuke.Services.Upgrade.Upgrade.StartTimer();

            // Write out Header
            HtmlUtils.WriteHeader(this.Response, "installResources");

            this.Response.Write("<h2>Install Resources Status Report</h2>");
            this.Response.Flush();

            // install new resources(s)
            var packages = DotNetNuke.Services.Upgrade.Upgrade.GetInstallPackages();
            foreach (var package in packages)
            {
                DotNetNuke.Services.Upgrade.Upgrade.InstallPackage(package.Key, package.Value.PackageType, true);
            }

            this.Response.Write("<h2>Upgrade Complete</h2>");
            this.Response.Write("<br><br><h2><a href='../Default.aspx'>Click Here To Access Your Site</a></h2><br><br>");
            this.Response.Flush();

            // Write out Footer
            HtmlUtils.WriteFooter(this.Response);

            DotNetNuke.Services.Upgrade.Upgrade.DeleteInstallerFiles();
        }
    }
}
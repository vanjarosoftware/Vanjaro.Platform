using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vanjaro.Core.Packager.Vanjaro
{
    public partial class Upgrade : System.Web.UI.Page
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Upgrade));

        protected void Page_Load(object sender, EventArgs e)
        {
            switch (Globals.Status)
            {
                case Globals.UpgradeStatus.Upgrade:
                    {
                        DeleteUpgradeFile();
                        Response.Redirect(Page.ResolveUrl("Install/Install.aspx?mode=upgrade"));
                        break;
                    }
                default:
                    UpgradePlatform();
                    break;
            }
        }

        private void UpgradePlatform()
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
            
            DeleteUpgradeFile();
        }

        private static void DeleteUpgradeFile()
        {
            // Delete Upgrade.aspx
            try
            {
                FileSystemUtils.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "Upgrade.aspx"));
            }
            catch (Exception ex)
            {
                Logger.Error("File deletion failed for [Install\\" + "Upgrade.aspx" + "]. PLEASE REMOVE THIS MANUALLY." + ex);
            }
        }
    }
}
using Ionic.Zip;
using MetroFramework.Forms;
using Microsoft.Web.Administration;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Vanjaro.Installer
{
    public partial class Main : MetroForm
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.FormClosing += Main_FormClosing; 

            LoadForm();

            UpdateForm();

            LoadReleases();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.SiteURL = string.Empty;
            Properties.Settings.Default.SiteTLD = tbSiteTLD.Text;
            Properties.Settings.Default.PhysicalPath = tbPhysicalPath.Text;
            Properties.Settings.Default.DatabaseServer = tbDatabaseServer.Text;

            Properties.Settings.Default.Save();
        }
        private void LoadForm()
        {
            tabs.SelectedIndex = 0;

            tbSiteURL.Text = Properties.Settings.Default.SiteURL;
            tbSiteTLD.Text = Properties.Settings.Default.SiteTLD;
            tbPhysicalPath.Text = Properties.Settings.Default.PhysicalPath;
            tbDatabaseServer.Text = Properties.Settings.Default.DatabaseServer;

            BindUpgradeList();
        }

        public static IEnumerable<Release> GetReleases()
        {
            var client = new GitHubClient(new ProductHeaderValue("Vanjaro.Platform"));
            return client.Repository.Release.GetAll("vanjarosoftware", "Vanjaro.Platform").Result;
        }
        private void LoadReleases()
        {
            try
            {
                var releases = GetReleases();

                cbVersion.DataSource = releases;
                cbVersion.DisplayMember = "Name";
                cbVersion.ValueMember = "Id";

                cbUpgradeVersion.DataSource = releases;
                cbUpgradeVersion.DisplayMember = "Name";
                cbUpgradeVersion.ValueMember = "Id";
            }
            catch 
            {
                MessageBox.Show("Unable to fetch latest Vanjaro Releases. Make sure you're connected to internet and restart application.", "Network Offline",MessageBoxButtons.OK, MessageBoxIcon.Error);

                cbVersion.Visible = false;
                cbUpgradeVersion.Visible = false;

                lVersion.Visible = false;
                lUpgradeVersion.Visible = false;

            }
        }

        private void bCreateSite_Click(object sender, EventArgs e)
        {
            bCreateSite.Enabled = false;
            bCreateSite.Text = "Please Wait";
            bCreateSite.Refresh();

            if (ValidateSite())
            {
                if (UpdateDNS())
                {
                    if (CreateSite())
                    {
                        if (CreateDatabase())
                        {
                            if (ExtractRelease("install", PlatformArchitecture, SiteDirectory + "\\Website\\"))
                            {
                                if (UpdateConfig())
                                {
                                    LaunchSite();

                                    //MessageBox.Show("Continue installation using the site wizard...", "Site Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    BindUpgradeList();
                                }
                            }
                        }
                    }
                }
            }

            bCreateSite.Enabled = true;
            bCreateSite.Text = "Create Site";
            bCreateSite.Refresh();
        }

        private void LaunchSite()
        {
            Process.Start("http://" + SiteName);
        }

        private bool UpdateConfig()
        {
            try 
            {
                var doc = XDocument.Load(SiteDirectory + "\\Website\\web.config");
                doc.Root.Element("connectionStrings").Element("add").Attribute("connectionString").Value = GetConnectionString(DatabaseName);
                doc.Save(SiteDirectory + "\\Website\\web.config");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Web.Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private bool ExtractRelease(string ReleaseType, string Architecture, string SiteDirectory)
        {
            try
            {
                ZipFile package = null;

                if (Properties.Settings.Default.UseLocalPackages)
                {
                    if (ReleaseType == "install")
                        package = ZipFile.Read(Properties.Settings.Default.LocalInstallPackage);
                    else
                        package = ZipFile.Read(Properties.Settings.Default.LocalUpgradePackage);
                }
                else
                    package = ZipFile.Read(GetReleasePackagePath(ReleaseType, Architecture));

                if (ReleaseType == "install")
                {
                    pbCreateSite.Visible = true;
                    lProgressBar.Visible = true;
                    pbCreateSite.Maximum = package.Count();

                    package.ExtractProgress += PackageInstall_ExtractProgress;
                    package.ExtractAll(SiteDirectory, ExtractExistingFileAction.OverwriteSilently);

                    pbCreateSite.Visible = false;
                    lProgressBar.Visible = false;
                }
                else
                {
                    pbUpgradeSite.Visible = true;
                    lUpgradeProgressBar.Visible = true;
                    pbUpgradeSite.Maximum = package.Count();

                    package.ExtractProgress += PackageUpgrade_ExtractProgress;
                    package.ExtractAll(SiteDirectory, ExtractExistingFileAction.OverwriteSilently);

                    pbUpgradeSite.Visible = false;
                    lUpgradeProgressBar.Visible = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Extract Package", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void PackageInstall_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                pbCreateSite.Value = e.EntriesExtracted;
                lProgressBar.Text = "Copying: " + e.CurrentEntry.FileName;
                lProgressBar.Refresh();
            }
        }

        private void PackageUpgrade_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                pbUpgradeSite.Value = e.EntriesExtracted;
                lUpgradeProgressBar.Text = "Copying: " + e.CurrentEntry.FileName;
                lUpgradeProgressBar.Refresh();
            }
        }
        private bool CreateDatabase()
        {
            string dbQuery = "USE master" + Environment.NewLine + "CREATE DATABASE [" + DatabaseName + "]";

            string dbPermission = "USE master" + Environment.NewLine + 
                "exec sp_grantlogin '" + SiteAppIdentity + "'" + Environment.NewLine + 
                "USE [" + DatabaseName + "]" + Environment.NewLine + 
                "exec sp_grantdbaccess '" + SiteAppIdentity + "','" + SiteName + "'" + Environment.NewLine + 
                "exec sp_addrolemember 'db_owner','" + SiteName + "'";

            SqlConnection con = new SqlConnection(GetConnectionString("master"));
            SqlCommand dbCmd = new SqlCommand(dbQuery, con);
            SqlCommand dbPermCmd = new SqlCommand(dbPermission, con);

            try
            {
                con.Open();
                dbCmd.ExecuteNonQuery();
                dbPermCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Create Database", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                con.Close();
            }

            return false;
        }
        private bool CreateSite()
        {
            try
            {
                using (ServerManager server = new ServerManager())
                {
                    string siteName = tbSiteURL.Text + tbSiteTLD.Text;
                    var bindingInfo = "*:80:" + siteName;

                    Site site = server.Sites.Add(siteName, "http", bindingInfo, SiteDirectory + "\\Website");
                    site.TraceFailedRequestsLogging.Enabled = true;
                    site.TraceFailedRequestsLogging.Directory = SiteDirectory + "\\Logs";
                    site.LogFile.Directory = SiteDirectory + "\\Logs" + "\\W3svc" + site.Id.ToString();

                    ApplicationPool appPool = server.ApplicationPools.Add(siteName);
                    appPool.ManagedRuntimeVersion = "v4.0";
                    appPool.Enable32BitAppOnWin64 = Properties.Settings.Default.Use32Bit;
                    site.ApplicationDefaults.ApplicationPoolName = siteName;

                    server.CommitChanges();

                    Directory.CreateDirectory(SiteDirectory + "\\Website");
                    SetFolderPermission(@"IIS APPPOOL\" + siteName, SiteDirectory + "\\Website");
                    SetFolderPermission(GetAuthAccounts(), SiteDirectory + "\\Website");

                    Directory.CreateDirectory(SiteDirectory + "\\Logs");
                    SetFolderPermission(GetDBAccount(), SiteDirectory + "\\Logs");
                    SetFolderPermission(GetAuthAccounts(), SiteDirectory + "\\Logs");

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create Site", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private static string GetAuthAccounts()
        {
            return new System.Security.Principal.SecurityIdentifier("S-1-5-11").Translate(typeof(System.Security.Principal.NTAccount)).Value;
        }
        private string GetDBAccount()
        {
            string dbServer = tbDatabaseServer.Text.Trim();

            if (dbServer.IndexOf(@"\") > -1)
                return  @"NT Service\MSSQL$" + dbServer.Substring(dbServer.LastIndexOf(@"\") + 1).ToUpper();
            
            return @"NT Service\MSSQLSERVER";
        }

        private static void SetFolderPermission(String accountName, String folderPath)
        {
            try
            {
                var accessRule1 = new FileSystemAccessRule(accountName, FileSystemRights.Modify, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
                var accessRule2 = new FileSystemAccessRule(accountName, FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow);

                var dir = new DirectoryInfo(folderPath);
                var dirAccessControl = dir.GetAccessControl();
                
                dirAccessControl.ModifyAccessRule(AccessControlModification.Set, accessRule1, out bool modified);                               
                dirAccessControl.ModifyAccessRule(AccessControlModification.Add, accessRule2, out modified);

                dir.SetAccessControl(dirAccessControl);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error: " + ex.Message, "Set Folder Permissions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool SiteExists(string siteName)
        {

            using (ServerManager server = new ServerManager())
            {

                foreach (var site in server.Sites)
                {
                    if (site.Name.ToLower() == siteName.ToLower())
                        return true;
                }
            }

            return false;
        }

        private bool IISInstalled()
        {
            try
            {
                using (ServerManager server = new ServerManager())
                {
                    var sites = server.Sites;
                    return true;
                }
            }
            catch { }

            return false;
        }
        private bool UpdateDNS()
        {
            try
            {
                string dnsEntry = "\t127.0.0.1 \t" + tbSiteURL.Text + tbSiteTLD.Text;
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts";
                string[] dnsEntries = File.ReadAllLines(filePath);

                if (!dnsEntries.Contains(dnsEntry))
                {
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        sw.WriteLine(dnsEntry);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Adding DNS Entry to Hosts File");
            }

            return false;

        }

        private bool ValidateSite()
        {
            if (Properties.Settings.Default.UseLocalPackages)
            {
                DialogResult dR = MessageBox.Show("Are you sure you want to install: " + Properties.Settings.Default.LocalInstallPackage, "Install Local Package?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dR == DialogResult.No)
                    return false;
            }

            //Dropped support for 32 Bit Packages. Force 64 Bit
            Properties.Settings.Default.Use32Bit = false;

            //if (Properties.Settings.Default.Use32Bit)
            //{
            //    DialogResult dR = MessageBox.Show("Are you sure you want to install the 32 Bit (x86) Package? Click No to install 64 bit (x64) package", "Install 32 bit", MessageBoxButtons.YesNo);

            //    if (dR == DialogResult.No)
            //        Properties.Settings.Default.Use32Bit = false;
            //}

            if (string.IsNullOrEmpty(tbSiteURL.Text) || string.IsNullOrEmpty(tbSiteTLD.Text) || string.IsNullOrEmpty(tbDatabaseServer.Text) || string.IsNullOrEmpty(tbPhysicalPath.Text))
            {
                MessageBox.Show("Please provide Site URL, Physical Path, & Database Server Name", "New Installation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!Directory.Exists(tbPhysicalPath.Text.TrimEnd('\\')))
            {
                MessageBox.Show("Physical Path does not exist", "New Installation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (Directory.Exists(SiteDirectory))
            {
                MessageBox.Show("Site directory already exists. Try another Site URL", "New Installation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!IISInstalled())
            {
                MessageBox.Show("Unable to connect to IIS. Verify IIS is installed and running", "IIS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (SiteExists(tbSiteURL.Text + tbSiteTLD.Text))
            {
                MessageBox.Show("Site name is in use. Try another Site URL", "IIS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!DatabaseOnline())
            {
                MessageBox.Show("Unable to connect to database. Verify Database Server Name and try again.", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (DatabaseExists(tbDatabaseName.Text))
            {
                MessageBox.Show("Database name is in use. Try another Site URL", "Database Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!ReleaseExists("install"))
            {
                return false;
            }

            return true;
        }

        private string GetReleasePackagePath(string ReleaseType, string Architecture = null)
        {
            if (Architecture == null)
            {
                if (ReleaseType.ToLower() == "install")
                    Architecture = PlatformArchitecture;
                else
                    Architecture = "upgrade";
            }

            var ReleaseDir = Directory.GetCurrentDirectory() + @"\Releases\";
            Release rel = cbVersion.SelectedItem as Release;
            var asset = rel.Assets.Where(r => r.Name.ToLower().Contains("platform") && r.Name.ToLower().Contains(ReleaseType.ToLower()) && r.Name.ToLower().Contains(Architecture)).FirstOrDefault();
            
            return ReleaseDir + asset.Name;
        }
        private bool ReleaseExists(string ReleaseType, string Architecture = null)
        {
            if (Properties.Settings.Default.UseLocalPackages)
            {
                if (ReleaseType == "install" && !File.Exists(Properties.Settings.Default.LocalInstallPackage))
                {
                    MessageBox.Show("Local Install Package does not exist", "Local Package", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (!File.Exists(Properties.Settings.Default.LocalUpgradePackage))
                {
                    MessageBox.Show("Local Upgrade Package does not exist", "Local Package", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            else if (!cbVersion.Visible && !cbUpgradeVersion.Visible)
            {
                MessageBox.Show("Unable to fetch latest Vanjaro Releases. Make sure you're connected to internet and restart application.", "Network Offline", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (Architecture == null)
            {
                if (ReleaseType.ToLower() == "install")
                    Architecture = PlatformArchitecture;
                else
                    Architecture = "upgrade";
            }

            var ReleaseDir = Directory.GetCurrentDirectory() + @"\Releases\";
            Release rel = cbVersion.SelectedItem as Release;
            var asset = rel.Assets.Where(r => r.Name.ToLower().Contains("platform") && r.Name.ToLower().Contains(ReleaseType.ToLower()) && r.Name.ToLower().Contains(Architecture)).FirstOrDefault();

            if (asset != null)
            {
                if (!File.Exists(GetReleasePackagePath(ReleaseType, Architecture)))
                {
                    if (!File.Exists(ReleaseDir))
                        Directory.CreateDirectory(ReleaseDir);

                    if (ReleaseType == "install")
                    {
                        WebClient client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_InstallDownloadProgressChanged);
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_InstallDownloadFileCompleted);
                        client.DownloadFileAsync(new Uri(asset.BrowserDownloadUrl), ReleaseDir + asset.Name);
                        pbCreateSite.Visible = true;
                        lProgressBar.Text = "Downloading " + asset.Name;
                        lProgressBar.Visible = true;
                        lProgressBar.Refresh();
                    }
                    else
                    {
                        WebClient client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_UpgradeDownloadProgressChanged);
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_UpgradeDownloadFileCompleted);
                        client.DownloadFileAsync(new Uri(asset.BrowserDownloadUrl), ReleaseDir + asset.Name);
                        pbUpgradeSite.Visible = true;
                        lUpgradeProgressBar.Text = "Downloading " + asset.Name;
                        lUpgradeProgressBar.Visible = true;
                        lUpgradeProgressBar.Refresh();
                    }
                }
                else
                    return true;
            }

            return false;
        }

        private void client_InstallDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            pbCreateSite.Visible = false;
            lProgressBar.Visible = false;
            pbCreateSite.Refresh();
            lProgressBar.Refresh();

            bCreateSite_Click(this, null);
        }

        private void client_InstallDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            pbCreateSite.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        private void client_UpgradeDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            pbUpgradeSite.Visible = false;
            lUpgradeProgressBar.Visible = false;
            pbUpgradeSite.Refresh();
            lUpgradeProgressBar.Refresh();

            bUpgradeSite_Click(this, null);
        }

        private void client_UpgradeDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            pbUpgradeSite.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        private bool DatabaseOnline()
        {
            SqlConnection con = new SqlConnection(GetConnectionString("master"));
            SqlCommand cmd = new SqlCommand("SELECT 1",con);

            try
            {
                con.Open();
                return (int)cmd.ExecuteScalar() == 1;
            }
            catch (Exception ex) { }
            finally
            {
                con.Close();
            }

            return false;
        }

        private string GetConnectionString(string dbName)
        {
            return "Server=" + tbDatabaseServer.Text + ";Initial Catalog=" + dbName + ";" + GetDBAuthString();
        }

        private string GetDBAuthString()
        {
            if (Properties.Settings.Default.UseWindowsAuth)
                return "Integrated Security=True;";
            else
                return "User ID=" + Properties.Settings.Default.SQLUsername + ";Password=" + Properties.Settings.Default.SQLPassword + ";";
        }

        private bool DatabaseExists(string dbName)
        {
            SqlConnection con = new SqlConnection(GetConnectionString("master"));
            SqlCommand cmd = new SqlCommand("SELECT DB_ID('" + dbName + "')", con);

            try
            {
                con.Open();
                int result = -1;
                return int.TryParse(cmd.ExecuteScalar().ToString(), out result);
            }
            catch (Exception ex) { }
            finally
            {
                con.Close();
            }

            return false;
        }

        private void tbSiteURL_TextChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void tbSiteTLD_TextChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void UpdateForm()
        {
            tbDatabaseName.Text = (tbSiteURL.Text + tbSiteTLD.Text).Replace(".","_");
        }

        public string SiteDirectory { get { return tbPhysicalPath.Text.TrimEnd('\\') + "\\" + tbSiteURL.Text + tbSiteTLD.Text; } }
        public string SiteName { get { return tbSiteURL.Text + tbSiteTLD.Text; } }
        public string SiteAppIdentity { get { return "IIS APPPOOL\\" + SiteName; } }
        public string DatabaseName { get { return SiteName.Replace(".", "_"); } }

        public string PlatformArchitecture
        {
            get
            {
                if (Properties.Settings.Default.Use32Bit)
                    return "x86";
                else
                    return "x64";
            }
        }

        private void bOtherSettings_Click(object sender, EventArgs e)
        {
            Settings form = new Settings();
            form.ShowDialog();
        }

        private void BindUpgradeList()
        {
            lbVanjaroSites.DataSource = GetVanjaroSites();
            lbVanjaroSites.ClearSelected();
        }

        private List<VanjaroSite> GetVanjaroSites()
        {
            List<VanjaroSite> sites = new List<VanjaroSite>();

            using (ServerManager server = new ServerManager())
            {

                foreach (var site in server.Sites.OrderBy(s => s.Name))
                {
                    var PhysicalPath = site.Applications.FirstOrDefault().VirtualDirectories.FirstOrDefault().PhysicalPath;

                    string version = null;
                    try
                    {
                        FileVersionInfo vanjaroCore = FileVersionInfo.GetVersionInfo(PhysicalPath + "\\bin\\Vanjaro.Core.dll");

                        if (vanjaroCore != null)
                            version = vanjaroCore.ProductMajorPart + "." + vanjaroCore.ProductMinorPart + "." + vanjaroCore.ProductBuildPart;
                    }
                    catch { }
                    
                    if (version != null)
                    {
                        var app = server.ApplicationPools.Where(a => a.Name == site.Name).SingleOrDefault();

                        if (app != null)
                            sites.Add(new VanjaroSite(site.Name, version, PhysicalPath, app.Enable32BitAppOnWin64));
                    }
                }
            }

            return sites;
        }

        private void bDeleteSite_Click(object sender, EventArgs e)
        {
            if (lbVanjaroSites.SelectedItems.Count > 0)
            {
                DialogResult dR = MessageBox.Show("This action cannot be done. It will remove site from IIS, delete all files, and delete database. Proceed with caution!","Delete Selected Sites?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);

                if (dR == DialogResult.Yes)
                {
                    bDeleteSite.Enabled = false;
                    bDeleteSite.Text = "Deleting...";
                    bDeleteSite.Refresh();

                    foreach (var site in lbVanjaroSites.SelectedItems)
                        DeleteSite(site as VanjaroSite);

                    lUpgradeProgressBar.Visible = false;

                    bDeleteSite.Enabled = true;
                    bDeleteSite.Text = "Delete";

                    MessageBox.Show("Selected sites deleted", "Sites Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    BindUpgradeList();
                }
            }
        }

        private void DeleteSite(VanjaroSite site)
        {
            lUpgradeProgressBar.Visible = true;
            lUpgradeProgressBar.Text = "Deleting: " + site.Name;
            lUpgradeProgressBar.Refresh();

            var removeDir = RemoveSite(site.Name);
            RemoveSiteDirectories(removeDir);
            RemoveDNS(site.Name);
            RemoveDatabase(site.Name);
        }

        private void RemoveDatabase(string siteName)
        {
            string dbQuery = "USE master" + Environment.NewLine + "DROP DATABASE [" + siteName.Replace(".","_") + "]";

            string dbPermission = "USE master" + Environment.NewLine +
                "exec sp_revokelogin '" + "IIS APPPOOL\\" + siteName + "'";

            SqlConnection con = new SqlConnection(GetConnectionString("master"));
            SqlCommand dbCmd = new SqlCommand(dbQuery, con);
            SqlCommand dbPermCmd = new SqlCommand(dbPermission, con);

            try
            {
                con.Open();
                dbCmd.ExecuteNonQuery();
                dbPermCmd.ExecuteNonQuery();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Create Database", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                con.Close();
            }
        }

        private void RemoveSiteDirectories(string removeDir)
        {
            try
            {
                if (removeDir != null)
                {
                    var dir = new DirectoryInfo(removeDir);

                    if (dir != null && dir.Parent.Exists)
                        Directory.Delete(dir.Parent.FullName, true);
                }
            }
            catch { }
        }

        private string RemoveSite(string siteName)
        {
            
            string removeDir = null;
            try
            {
                using (ServerManager server = new ServerManager())
                {
                    var site = server.Sites.Where(s => s.Name == siteName).SingleOrDefault();
                    var app = server.ApplicationPools.Where(a => a.Name == siteName).SingleOrDefault();

                    //Changing the log directory releases the log files and allows directories to be removed
                    site.LogFile.Directory = System.IO.Path.GetTempPath();

                    if (app.State == ObjectState.Started || app.State == ObjectState.Starting)
                        try { app.Stop(); } catch { }

                    server.CommitChanges();
                }

                using (ServerManager server = new ServerManager())
                {
                    var app = server.ApplicationPools.Where(a => a.Name == siteName).SingleOrDefault();
                    var site = server.Sites.Where(s => s.Name == siteName).SingleOrDefault();
                    
                    if (app != null)
                        server.ApplicationPools.Remove(app);

                    if (site != null)
                    {
                        removeDir = site.Applications.FirstOrDefault().VirtualDirectories.FirstOrDefault().PhysicalPath;
                        server.Sites.Remove(site);
                    }

                    server.CommitChanges();
                }
            }
            catch (Exception ex) { }
            return removeDir;
        }

        private void RemoveDNS(string siteName)
        {
            try
            {
                string dnsEntry = "\t127.0.0.1 \t" + siteName;
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts";
                string dnsEntries = File.ReadAllText(filePath);

                if (dnsEntries.Contains(dnsEntry))
                {
                    dnsEntries = dnsEntries.Replace(dnsEntry, string.Empty);
                    File.WriteAllText(filePath,dnsEntries);                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Adding DNS Entry to Hosts File");
            }
        }

        private void bUpgradeSite_Click(object sender, EventArgs e)
        {
            if (lbVanjaroSites.SelectedItems.Count > 0)
            {
                if (Properties.Settings.Default.UseLocalPackages)
                {
                    DialogResult dRR = MessageBox.Show("Are you sure you want to upgrade: " + Properties.Settings.Default.LocalUpgradePackage, "Upgrade Local Package?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dRR == DialogResult.No)
                        return;
                }
                else if (!cbVersion.Visible && !cbUpgradeVersion.Visible)
                {
                    MessageBox.Show("Unable to fetch latest Vanjaro Releases. Make sure you're connected to internet and restart application.", "Network Offline", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!ReleaseExists("upgrade"))
                {
                    return;
                }

                DialogResult dR = MessageBox.Show("This action cannot be done. Proceed with caution!", "Upgrade Selected Sites?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);

                if (dR == DialogResult.Yes)
                {
                    bUpgradeSite.Enabled = false;
                    bUpgradeSite.Refresh();

                    foreach (var site in lbVanjaroSites.SelectedItems)
                        UpgradeSite(site as VanjaroSite);

                    lUpgradeProgressBar.Visible = false;

                    bUpgradeSite.Enabled = true;
                    bUpgradeSite.Text = "Upgrade";


                    BindUpgradeList();
                }
            }
        }

        private void UpgradeSite(VanjaroSite vanjaroSite)
        {

            var Architecture = vanjaroSite.Enable32Bit ? "x86" : "x64";

            //Dropped support for 32 Bit Packages. Force 64 Bit
            if (Architecture == "x86")
            {
                MessageBox.Show("32 Bit App Pools are no longer supported", "Please update App Pool settings and try again.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bUpgradeSite.Text = "Upgrading " + vanjaroSite.Name;
            bUpgradeSite.Refresh();

            //Copy app_offline
            File.Copy(Directory.GetCurrentDirectory() + "\\app_offline.htm", vanjaroSite.PhysicalPath + "\\app_offline.htm");

            //Extract Files
            ExtractRelease("upgrade", null, vanjaroSite.PhysicalPath);

            //Remove app_offline
            File.Delete(vanjaroSite.PhysicalPath + "\\app_offline.htm");

            //Initialize Upgrade
            Process.Start("http://" + vanjaroSite.Name.TrimEnd('/') + "/install/upgrade.aspx");

        }

        private void bBrowsePhysicalPath_Click(object sender, EventArgs e)
        {
            fbdPhysicalPath.ShowDialog();
            tbPhysicalPath.Text = fbdPhysicalPath.SelectedPath;
        }
    }
    public class VanjaroSite
    {
        public VanjaroSite(string Name, string Version, string PhysicalPath, bool Enable32Bit)
        {
            this.Name = Name;
            this.Version = Version;
            this.PhysicalPath = PhysicalPath;
            this.Enable32Bit = Enable32Bit;
        }

        public string Name { get; set; }
        public string Version { get; set; }

        public string PhysicalPath { get; set; }
        public bool Enable32Bit { get; set; }
        public override string ToString()
        {
            return Name + " (v" + Version + ")";
        }
    }
}

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
            Properties.Settings.Default.SiteURL = tbSiteURL.Text;
            Properties.Settings.Default.SiteTLD = tbSiteTLD.Text;
            Properties.Settings.Default.PhysicalPath = tbPhysicalPath.Text;
            Properties.Settings.Default.DatabaseServer = tbDatabaseServer.Text;

            Properties.Settings.Default.Save();
        }
        private void LoadForm()
        {
            tbSiteURL.Text = Properties.Settings.Default.SiteURL;
            tbSiteTLD.Text = Properties.Settings.Default.SiteTLD;
            tbPhysicalPath.Text = Properties.Settings.Default.PhysicalPath;
            tbDatabaseServer.Text = Properties.Settings.Default.DatabaseServer;
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
                cbVersion.DataSource = GetReleases();
                cbVersion.DisplayMember = "Name";
                cbVersion.ValueMember = "Id";
            }
            catch 
            {
                MessageBox.Show("Unable to fetch latest Vanjaro Releases. Make sure you're connected to internet and restart application.", "Network Offline",MessageBoxButtons.OK, MessageBoxIcon.Error);

                System.Windows.Forms.Application.Exit();
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
                            if (ExtractRelease("install", SiteDirectory))
                            {
                                if (UpdateConfig())
                                {
                                    LaunchSite();
                                    System.Windows.Forms.Application.Exit();
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

        private bool ExtractRelease(string ReleaseType, string SiteDirectory)
        {
            try
            {
                var package = ZipFile.Read(GetReleasePackagePath(ReleaseType));

                pbCreateSite.Maximum = package.Entries.Count();
                pbCreateSite.Visible = true;
                lProgressBar.Visible = true;

                package.ExtractProgress += Package_ExtractProgress;
                package.ExtractAll(SiteDirectory + "\\Website\\", ExtractExistingFileAction.OverwriteSilently);

                pbCreateSite.Visible = false;
                lProgressBar.Visible = false;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Extract Package", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void Package_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            pbCreateSite.Value = e.EntriesExtracted;

            if (e.CurrentEntry != null)
            {
                lProgressBar.Text = "Copying: " + e.CurrentEntry.FileName;
                lProgressBar.Refresh();
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
                ServerManager server = new ServerManager();
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
                MessageBox.Show("Error: " + ex.Message, "Set Folder Permissions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool SiteExists(string siteName)
        {

            ServerManager server = new ServerManager();

            foreach (var site in server.Sites)
            {
                if (site.Name.ToLower() == siteName.ToLower())
                    return true;
            }

            return false;
        }

        private bool IISInstalled()
        {
            try
            {
                var sites = new ServerManager().Sites;
                return true;
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
            if (Properties.Settings.Default.Use32Bit)
            {
                DialogResult dR = MessageBox.Show("Are you sure you want to install the 32 Bit (x86) Package? Click No to install 64 bit (x64) package", "Install 32 bit", MessageBoxButtons.YesNo);

                if (dR == DialogResult.No)
                    Properties.Settings.Default.Use32Bit = false;
            }

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

        private string GetReleasePackagePath(string ReleaseType)
        {
            var ReleaseDir = Directory.GetCurrentDirectory() + @"\Releases\";
            Release rel = cbVersion.SelectedItem as Release;
            var asset = rel.Assets.Where(r => r.Name.ToLower().Contains("platform") && r.Name.ToLower().Contains(ReleaseType.ToLower()) && r.Name.ToLower().Contains(PlatformArchitecture)).FirstOrDefault();
            
            return ReleaseDir + asset.Name;
        }
        private bool ReleaseExists(string ReleaseType)
        {
            var ReleaseDir = Directory.GetCurrentDirectory() + @"\Releases\";
            Release rel = cbVersion.SelectedItem as Release;
            var asset = rel.Assets.Where(r => r.Name.ToLower().Contains("platform") && r.Name.ToLower().Contains(ReleaseType.ToLower()) && r.Name.ToLower().Contains(PlatformArchitecture)).FirstOrDefault();

            if (asset != null)
            {
                if (!File.Exists(GetReleasePackagePath(ReleaseType)))
                {
                    if (!File.Exists(ReleaseDir))
                        Directory.CreateDirectory(ReleaseDir);

                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(asset.BrowserDownloadUrl), ReleaseDir + asset.Name);
                    pbCreateSite.Visible = true;
                    lProgressBar.Visible = true;
                }
                else
                    return true;
            }

            return false;
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            pbCreateSite.Visible = false;
            lProgressBar.Visible = false;
            pbCreateSite.Refresh();
            lProgressBar.Refresh();

            bCreateSite_Click(this, null);
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            pbCreateSite.Value = int.Parse(Math.Truncate(percentage).ToString());
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
    }
}

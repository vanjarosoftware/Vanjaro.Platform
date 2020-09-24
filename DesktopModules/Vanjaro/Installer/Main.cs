using MetroFramework.Forms;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanjaro.Installer.Managers;

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
            LoadReleases();
            UpdateForm();
        }

        private void LoadReleases()
        {
            try
            {
                cbVersion.DataSource = ReleaseManager.GetReleases();
                cbVersion.DisplayMember = "Name";
                cbVersion.ValueMember = "Id";
            }
            catch 
            {
                MessageBox.Show("Unable to fetch latest Vanjaro Releases. Make sure you're connected to internet and restart application.");

                System.Windows.Forms.Application.Exit();
            }
        }

        private void bCreateSite_Click(object sender, EventArgs e)
        {
            bCreateSite.Enabled = false;
            bCreateSite.Text = "Please Wait";

            if (SiteValidates())
            {
                if (HostsFileUpdated())
                {

                }
            }
            
            bCreateSite.Enabled = true;
            bCreateSite.Text = "Create Site";
            
        }

        private bool HostsFileUpdated()
        {
            try
            {
                string dnsEntry = "\t127.0.0.1 \t" + tbSiteURL.Text + tbSiteTLD.Text;
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts";
                string[] hostsEntries = File.ReadAllLines(filePath);

                if (!hostsEntries.Contains(dnsEntry))
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

        private bool SiteValidates()
        {
            if (string.IsNullOrEmpty(tbSiteURL.Text) || string.IsNullOrEmpty(tbSiteTLD.Text) || string.IsNullOrEmpty(tbDatabaseServer.Text) || string.IsNullOrEmpty(tbPhysicalPath.Text))
            {
                MessageBox.Show("Please provide Site URL, Physical Path, & Database Server Name", "New Installation");
                return false;
            }

            if (!Directory.Exists(tbPhysicalPath.Text.TrimEnd('\\')))
            {
                MessageBox.Show("Physical Path does not exist", "New Installation");
                return false;
            }

            if (Directory.Exists(tbPhysicalPath.Text.TrimEnd('\\') + "\\" + tbSiteURL.Text + tbSiteTLD.Text))
            {
                MessageBox.Show("Site directory already exists. Try another Site URL", "New Installation");
                return false;
            }

            if (!DatabaseOnline())
            {
                MessageBox.Show("Unable to connect to database. Verify Database Server Name and try again.", "Database Connection");
                return false;
            }

            if (DatabaseExists(tbDatabaseName.Text))
            {
                MessageBox.Show("Database name is in use. Try another Site URL", "Database Name");
                return false;
            }

            if (!ReleaseExists("install"))
            {
                return false;
            }

            return true;
        }

        private bool ReleaseExists(string ReleaseType)
        {
            var ReleaseDir = Directory.GetCurrentDirectory() + @"\Releases\";
            Release rel = cbVersion.SelectedItem as Release;
            var asset = rel.Assets.Where(r => r.Name.ToLower().Contains("platform") && r.Name.ToLower().Contains(ReleaseType.ToLower()) && r.Name.ToLower().Contains("x64")).FirstOrDefault();

            if (asset != null)
            {
                if (!File.Exists(ReleaseDir + asset.Name))
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
            SqlConnection con = new SqlConnection("Server=" + tbDatabaseServer.Text + "; Initial Catalog=master; Integrated Security = True;");
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
        private bool DatabaseExists(string dbName)
        {
            SqlConnection con = new SqlConnection("Server=" + tbDatabaseServer.Text + "; Initial Catalog=master; Integrated Security = True;");
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
    }
}

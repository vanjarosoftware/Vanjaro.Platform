namespace Vanjaro.Installer
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.tabs = new MetroFramework.Controls.MetroTabControl();
            this.metroTabPage1 = new MetroFramework.Controls.MetroTabPage();
            this.bBrowsePhysicalPath = new MetroFramework.Controls.MetroButton();
            this.bOtherSettings = new MetroFramework.Controls.MetroLink();
            this.lProgressBar = new MetroFramework.Controls.MetroLabel();
            this.tbPhysicalPath = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel5 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
            this.tbDatabaseName = new MetroFramework.Controls.MetroTextBox();
            this.tbDatabaseServer = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.pbCreateSite = new MetroFramework.Controls.MetroProgressBar();
            this.bCreateSite = new MetroFramework.Controls.MetroButton();
            this.tbSiteTLD = new MetroFramework.Controls.MetroTextBox();
            this.lVersion = new MetroFramework.Controls.MetroLabel();
            this.tbSiteURL = new MetroFramework.Controls.MetroTextBox();
            this.cbVersion = new MetroFramework.Controls.MetroComboBox();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.metroTabPage2 = new MetroFramework.Controls.MetroTabPage();
            this.bDeleteSite = new MetroFramework.Controls.MetroButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbVanjaroSites = new System.Windows.Forms.ListBox();
            this.lUpgradeProgressBar = new MetroFramework.Controls.MetroLabel();
            this.pbUpgradeSite = new MetroFramework.Controls.MetroProgressBar();
            this.bUpgradeSite = new MetroFramework.Controls.MetroButton();
            this.lUpgradeVersion = new MetroFramework.Controls.MetroLabel();
            this.cbUpgradeVersion = new MetroFramework.Controls.MetroComboBox();
            this.fbdPhysicalPath = new System.Windows.Forms.FolderBrowserDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabs.SuspendLayout();
            this.metroTabPage1.SuspendLayout();
            this.metroTabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.metroTabPage1);
            this.tabs.Controls.Add(this.metroTabPage2);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.ItemSize = new System.Drawing.Size(185, 31);
            this.tabs.Location = new System.Drawing.Point(20, 60);
            this.tabs.Multiline = true;
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(383, 390);
            this.tabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabs.TabIndex = 0;
            this.tabs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // metroTabPage1
            // 
            this.metroTabPage1.Controls.Add(this.bBrowsePhysicalPath);
            this.metroTabPage1.Controls.Add(this.bOtherSettings);
            this.metroTabPage1.Controls.Add(this.lProgressBar);
            this.metroTabPage1.Controls.Add(this.tbPhysicalPath);
            this.metroTabPage1.Controls.Add(this.metroLabel5);
            this.metroTabPage1.Controls.Add(this.metroLabel4);
            this.metroTabPage1.Controls.Add(this.tbDatabaseName);
            this.metroTabPage1.Controls.Add(this.tbDatabaseServer);
            this.metroTabPage1.Controls.Add(this.metroLabel2);
            this.metroTabPage1.Controls.Add(this.pbCreateSite);
            this.metroTabPage1.Controls.Add(this.bCreateSite);
            this.metroTabPage1.Controls.Add(this.tbSiteTLD);
            this.metroTabPage1.Controls.Add(this.lVersion);
            this.metroTabPage1.Controls.Add(this.tbSiteURL);
            this.metroTabPage1.Controls.Add(this.cbVersion);
            this.metroTabPage1.Controls.Add(this.metroLabel3);
            this.metroTabPage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.metroTabPage1.HorizontalScrollbarBarColor = true;
            this.metroTabPage1.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage1.Name = "metroTabPage1";
            this.metroTabPage1.Size = new System.Drawing.Size(375, 351);
            this.metroTabPage1.TabIndex = 0;
            this.metroTabPage1.Text = "New Installation";
            this.metroTabPage1.VerticalScrollbarBarColor = true;
            // 
            // bBrowsePhysicalPath
            // 
            this.bBrowsePhysicalPath.Location = new System.Drawing.Point(342, 156);
            this.bBrowsePhysicalPath.Name = "bBrowsePhysicalPath";
            this.bBrowsePhysicalPath.Size = new System.Drawing.Size(28, 23);
            this.bBrowsePhysicalPath.TabIndex = 17;
            this.bBrowsePhysicalPath.Text = "...";
            this.bBrowsePhysicalPath.Click += new System.EventHandler(this.bBrowsePhysicalPath_Click);
            // 
            // bOtherSettings
            // 
            this.bOtherSettings.FontWeight = MetroFramework.MetroLinkWeight.Light;
            this.bOtherSettings.Location = new System.Drawing.Point(281, 10);
            this.bOtherSettings.Name = "bOtherSettings";
            this.bOtherSettings.Size = new System.Drawing.Size(89, 23);
            this.bOtherSettings.TabIndex = 16;
            this.bOtherSettings.Text = "Other Settings";
            this.bOtherSettings.Click += new System.EventHandler(this.bOtherSettings_Click);
            // 
            // lProgressBar
            // 
            this.lProgressBar.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lProgressBar.Location = new System.Drawing.Point(0, 326);
            this.lProgressBar.Name = "lProgressBar";
            this.lProgressBar.Size = new System.Drawing.Size(370, 23);
            this.lProgressBar.TabIndex = 15;
            this.lProgressBar.Text = "Downloading Release...";
            this.lProgressBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lProgressBar.Visible = false;
            // 
            // tbPhysicalPath
            // 
            this.tbPhysicalPath.Location = new System.Drawing.Point(0, 156);
            this.tbPhysicalPath.Name = "tbPhysicalPath";
            this.tbPhysicalPath.Size = new System.Drawing.Size(336, 23);
            this.tbPhysicalPath.TabIndex = 14;
            this.tbPhysicalPath.Text = "C:\\Sites";
            // 
            // metroLabel5
            // 
            this.metroLabel5.AutoSize = true;
            this.metroLabel5.Location = new System.Drawing.Point(0, 134);
            this.metroLabel5.Name = "metroLabel5";
            this.metroLabel5.Size = new System.Drawing.Size(83, 19);
            this.metroLabel5.TabIndex = 13;
            this.metroLabel5.Text = "Physical Path";
            // 
            // metroLabel4
            // 
            this.metroLabel4.AutoSize = true;
            this.metroLabel4.Location = new System.Drawing.Point(184, 188);
            this.metroLabel4.Name = "metroLabel4";
            this.metroLabel4.Size = new System.Drawing.Size(103, 19);
            this.metroLabel4.TabIndex = 12;
            this.metroLabel4.Text = "Database Name";
            // 
            // tbDatabaseName
            // 
            this.tbDatabaseName.Location = new System.Drawing.Point(188, 210);
            this.tbDatabaseName.Name = "tbDatabaseName";
            this.tbDatabaseName.ReadOnly = true;
            this.tbDatabaseName.Size = new System.Drawing.Size(182, 23);
            this.tbDatabaseName.TabIndex = 11;
            // 
            // tbDatabaseServer
            // 
            this.tbDatabaseServer.Location = new System.Drawing.Point(0, 210);
            this.tbDatabaseServer.Name = "tbDatabaseServer";
            this.tbDatabaseServer.Size = new System.Drawing.Size(182, 23);
            this.tbDatabaseServer.TabIndex = 10;
            this.tbDatabaseServer.Text = "(local)";
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(-4, 188);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(105, 19);
            this.metroLabel2.TabIndex = 9;
            this.metroLabel2.Text = "Database Server";
            // 
            // pbCreateSite
            // 
            this.pbCreateSite.Location = new System.Drawing.Point(-4, 300);
            this.pbCreateSite.Name = "pbCreateSite";
            this.pbCreateSite.Size = new System.Drawing.Size(374, 23);
            this.pbCreateSite.TabIndex = 8;
            this.pbCreateSite.Visible = false;
            // 
            // bCreateSite
            // 
            this.bCreateSite.Location = new System.Drawing.Point(-4, 251);
            this.bCreateSite.Name = "bCreateSite";
            this.bCreateSite.Size = new System.Drawing.Size(374, 43);
            this.bCreateSite.Style = MetroFramework.MetroColorStyle.Red;
            this.bCreateSite.TabIndex = 7;
            this.bCreateSite.Text = "Create New Site";
            this.bCreateSite.Click += new System.EventHandler(this.bCreateSite_Click);
            // 
            // tbSiteTLD
            // 
            this.tbSiteTLD.Location = new System.Drawing.Point(188, 104);
            this.tbSiteTLD.Name = "tbSiteTLD";
            this.tbSiteTLD.Size = new System.Drawing.Size(182, 23);
            this.tbSiteTLD.TabIndex = 6;
            this.tbSiteTLD.Text = ".vanjaro.local";
            this.tbSiteTLD.TextChanged += new System.EventHandler(this.tbSiteTLD_TextChanged);
            // 
            // lVersion
            // 
            this.lVersion.AutoSize = true;
            this.lVersion.Location = new System.Drawing.Point(0, 14);
            this.lVersion.Name = "lVersion";
            this.lVersion.Size = new System.Drawing.Size(51, 19);
            this.lVersion.TabIndex = 2;
            this.lVersion.Text = "Version";
            this.lVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbSiteURL
            // 
            this.tbSiteURL.Location = new System.Drawing.Point(0, 104);
            this.tbSiteURL.Name = "tbSiteURL";
            this.tbSiteURL.Size = new System.Drawing.Size(182, 23);
            this.tbSiteURL.TabIndex = 5;
            this.tbSiteURL.Text = "mysite";
            this.tbSiteURL.TextChanged += new System.EventHandler(this.tbSiteURL_TextChanged);
            // 
            // cbVersion
            // 
            this.cbVersion.FormattingEnabled = true;
            this.cbVersion.ItemHeight = 23;
            this.cbVersion.Location = new System.Drawing.Point(0, 35);
            this.cbVersion.Name = "cbVersion";
            this.cbVersion.Size = new System.Drawing.Size(370, 29);
            this.cbVersion.TabIndex = 3;
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(0, 82);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(57, 19);
            this.metroLabel3.TabIndex = 4;
            this.metroLabel3.Text = "Site URL";
            // 
            // metroTabPage2
            // 
            this.metroTabPage2.Controls.Add(this.bDeleteSite);
            this.metroTabPage2.Controls.Add(this.groupBox1);
            this.metroTabPage2.Controls.Add(this.lUpgradeProgressBar);
            this.metroTabPage2.Controls.Add(this.pbUpgradeSite);
            this.metroTabPage2.Controls.Add(this.bUpgradeSite);
            this.metroTabPage2.Controls.Add(this.lUpgradeVersion);
            this.metroTabPage2.Controls.Add(this.cbUpgradeVersion);
            this.metroTabPage2.HorizontalScrollbarBarColor = true;
            this.metroTabPage2.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage2.Name = "metroTabPage2";
            this.metroTabPage2.Size = new System.Drawing.Size(375, 351);
            this.metroTabPage2.TabIndex = 1;
            this.metroTabPage2.Text = "Upgrade";
            this.metroTabPage2.VerticalScrollbarBarColor = true;
            // 
            // bDeleteSite
            // 
            this.bDeleteSite.Highlight = true;
            this.bDeleteSite.Location = new System.Drawing.Point(0, 251);
            this.bDeleteSite.Name = "bDeleteSite";
            this.bDeleteSite.Size = new System.Drawing.Size(109, 43);
            this.bDeleteSite.Style = MetroFramework.MetroColorStyle.Red;
            this.bDeleteSite.TabIndex = 20;
            this.bDeleteSite.Text = "Delete";
            this.bDeleteSite.Click += new System.EventHandler(this.bDeleteSite_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.lbVanjaroSites);
            this.groupBox1.Location = new System.Drawing.Point(0, 70);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(370, 160);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sites";
            // 
            // lbVanjaroSites
            // 
            this.lbVanjaroSites.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbVanjaroSites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbVanjaroSites.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbVanjaroSites.ForeColor = System.Drawing.Color.Gray;
            this.lbVanjaroSites.FormattingEnabled = true;
            this.lbVanjaroSites.ItemHeight = 20;
            this.lbVanjaroSites.Items.AddRange(new object[] {
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven"});
            this.lbVanjaroSites.Location = new System.Drawing.Point(3, 16);
            this.lbVanjaroSites.Name = "lbVanjaroSites";
            this.lbVanjaroSites.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lbVanjaroSites.Size = new System.Drawing.Size(364, 141);
            this.lbVanjaroSites.TabIndex = 2;
            // 
            // lUpgradeProgressBar
            // 
            this.lUpgradeProgressBar.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lUpgradeProgressBar.Location = new System.Drawing.Point(0, 326);
            this.lUpgradeProgressBar.Name = "lUpgradeProgressBar";
            this.lUpgradeProgressBar.Size = new System.Drawing.Size(370, 23);
            this.lUpgradeProgressBar.TabIndex = 18;
            this.lUpgradeProgressBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lUpgradeProgressBar.Visible = false;
            // 
            // pbUpgradeSite
            // 
            this.pbUpgradeSite.Location = new System.Drawing.Point(-4, 300);
            this.pbUpgradeSite.Name = "pbUpgradeSite";
            this.pbUpgradeSite.Size = new System.Drawing.Size(374, 23);
            this.pbUpgradeSite.TabIndex = 17;
            this.pbUpgradeSite.Visible = false;
            // 
            // bUpgradeSite
            // 
            this.bUpgradeSite.Highlight = true;
            this.bUpgradeSite.Location = new System.Drawing.Point(116, 251);
            this.bUpgradeSite.Name = "bUpgradeSite";
            this.bUpgradeSite.Size = new System.Drawing.Size(255, 43);
            this.bUpgradeSite.Style = MetroFramework.MetroColorStyle.Green;
            this.bUpgradeSite.TabIndex = 16;
            this.bUpgradeSite.Text = "Upgrade";
            this.bUpgradeSite.Click += new System.EventHandler(this.bUpgradeSite_Click);
            // 
            // lUpgradeVersion
            // 
            this.lUpgradeVersion.AutoSize = true;
            this.lUpgradeVersion.Location = new System.Drawing.Point(0, 14);
            this.lUpgradeVersion.Name = "lUpgradeVersion";
            this.lUpgradeVersion.Size = new System.Drawing.Size(51, 19);
            this.lUpgradeVersion.TabIndex = 4;
            this.lUpgradeVersion.Text = "Version";
            this.lUpgradeVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbUpgradeVersion
            // 
            this.cbUpgradeVersion.FormattingEnabled = true;
            this.cbUpgradeVersion.ItemHeight = 23;
            this.cbUpgradeVersion.Location = new System.Drawing.Point(0, 35);
            this.cbUpgradeVersion.Name = "cbUpgradeVersion";
            this.cbUpgradeVersion.Size = new System.Drawing.Size(370, 29);
            this.cbUpgradeVersion.TabIndex = 5;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(158, 14);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 470);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.tabs);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Resizable = false;
            this.TextAlign = System.Windows.Forms.VisualStyles.HorizontalAlign.Center;
            this.Load += new System.EventHandler(this.Main_Load);
            this.tabs.ResumeLayout(false);
            this.metroTabPage1.ResumeLayout(false);
            this.metroTabPage1.PerformLayout();
            this.metroTabPage2.ResumeLayout(false);
            this.metroTabPage2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl tabs;
        private MetroFramework.Controls.MetroTabPage metroTabPage1;
        private MetroFramework.Controls.MetroComboBox cbVersion;
        private MetroFramework.Controls.MetroLabel lVersion;
        private MetroFramework.Controls.MetroTabPage metroTabPage2;
        private MetroFramework.Controls.MetroProgressBar pbCreateSite;
        private MetroFramework.Controls.MetroButton bCreateSite;
        private MetroFramework.Controls.MetroTextBox tbSiteTLD;
        private MetroFramework.Controls.MetroTextBox tbSiteURL;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroTextBox tbDatabaseServer;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private MetroFramework.Controls.MetroTextBox tbDatabaseName;
        private MetroFramework.Controls.MetroTextBox tbPhysicalPath;
        private MetroFramework.Controls.MetroLabel metroLabel5;
        private MetroFramework.Controls.MetroLabel lProgressBar;
        private MetroFramework.Controls.MetroLink bOtherSettings;
        private System.Windows.Forms.ListBox lbVanjaroSites;
        private System.Windows.Forms.GroupBox groupBox1;
        private MetroFramework.Controls.MetroLabel lUpgradeProgressBar;
        private MetroFramework.Controls.MetroProgressBar pbUpgradeSite;
        private MetroFramework.Controls.MetroButton bUpgradeSite;
        private MetroFramework.Controls.MetroLabel lUpgradeVersion;
        private MetroFramework.Controls.MetroComboBox cbUpgradeVersion;
        private MetroFramework.Controls.MetroButton bDeleteSite;
        private MetroFramework.Controls.MetroButton bBrowsePhysicalPath;
        private System.Windows.Forms.FolderBrowserDialog fbdPhysicalPath;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
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
            this.metroTabControl1 = new MetroFramework.Controls.MetroTabControl();
            this.metroTabPage1 = new MetroFramework.Controls.MetroTabPage();
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
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.tbSiteURL = new MetroFramework.Controls.MetroTextBox();
            this.cbVersion = new MetroFramework.Controls.MetroComboBox();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.metroTabPage2 = new MetroFramework.Controls.MetroTabPage();
            this.bOtherSettings = new MetroFramework.Controls.MetroLink();
            this.metroTabControl1.SuspendLayout();
            this.metroTabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Controls.Add(this.metroTabPage1);
            this.metroTabControl1.Controls.Add(this.metroTabPage2);
            this.metroTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTabControl1.ItemSize = new System.Drawing.Size(185, 31);
            this.metroTabControl1.Location = new System.Drawing.Point(20, 60);
            this.metroTabControl1.Multiline = true;
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.SelectedIndex = 0;
            this.metroTabControl1.Size = new System.Drawing.Size(383, 390);
            this.metroTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.metroTabControl1.TabIndex = 0;
            this.metroTabControl1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // metroTabPage1
            // 
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
            this.metroTabPage1.Controls.Add(this.metroLabel1);
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
            this.tbPhysicalPath.Size = new System.Drawing.Size(371, 23);
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
            this.bCreateSite.Text = "Create Site";
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
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(0, 14);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(51, 19);
            this.metroLabel1.TabIndex = 2;
            this.metroLabel1.Text = "Version";
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.metroTabPage2.HorizontalScrollbarBarColor = true;
            this.metroTabPage2.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage2.Name = "metroTabPage2";
            this.metroTabPage2.Size = new System.Drawing.Size(375, 343);
            this.metroTabPage2.TabIndex = 1;
            this.metroTabPage2.Text = "Upgrade";
            this.metroTabPage2.VerticalScrollbarBarColor = true;
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
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 470);
            this.Controls.Add(this.metroTabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Resizable = false;
            this.Text = "Vanjaro";
            this.TextAlign = System.Windows.Forms.VisualStyles.HorizontalAlign.Center;
            this.Load += new System.EventHandler(this.Main_Load);
            this.metroTabControl1.ResumeLayout(false);
            this.metroTabPage1.ResumeLayout(false);
            this.metroTabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl metroTabControl1;
        private MetroFramework.Controls.MetroTabPage metroTabPage1;
        private MetroFramework.Controls.MetroComboBox cbVersion;
        private MetroFramework.Controls.MetroLabel metroLabel1;
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
    }
}
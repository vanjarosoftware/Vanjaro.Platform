namespace Vanjaro.Installer
{
    partial class Settings
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
            this.rb64Bit = new MetroFramework.Controls.MetroRadioButton();
            this.rb32Bit = new MetroFramework.Controls.MetroRadioButton();
            this.rbWindowsAuth = new MetroFramework.Controls.MetroRadioButton();
            this.rbSQLAuth = new MetroFramework.Controls.MetroRadioButton();
            this.tbSQLUsername = new MetroFramework.Controls.MetroTextBox();
            this.tbSQLPassword = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.toLocalPackages = new MetroFramework.Controls.MetroToggle();
            this.bBrowseInstallPackage = new MetroFramework.Controls.MetroButton();
            this.bBrowseUpgradePackage = new MetroFramework.Controls.MetroButton();
            this.tbInstallPackage = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.tbUpgradePackage = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
            this.ofdInstallPackage = new System.Windows.Forms.OpenFileDialog();
            this.ofdUpgradePackage = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // rb64Bit
            // 
            this.rb64Bit.AutoSize = true;
            this.rb64Bit.Checked = true;
            this.rb64Bit.Location = new System.Drawing.Point(17, 31);
            this.rb64Bit.Name = "rb64Bit";
            this.rb64Bit.Size = new System.Drawing.Size(150, 15);
            this.rb64Bit.TabIndex = 0;
            this.rb64Bit.TabStop = true;
            this.rb64Bit.Text = "Use 64 Bit (x64) Package";
            this.rb64Bit.UseVisualStyleBackColor = true;
            // 
            // rb32Bit
            // 
            this.rb32Bit.AutoSize = true;
            this.rb32Bit.Location = new System.Drawing.Point(17, 52);
            this.rb32Bit.Name = "rb32Bit";
            this.rb32Bit.Size = new System.Drawing.Size(150, 15);
            this.rb32Bit.TabIndex = 1;
            this.rb32Bit.Text = "Use 32 Bit (x86) Package";
            this.rb32Bit.UseVisualStyleBackColor = true;
            // 
            // rbWindowsAuth
            // 
            this.rbWindowsAuth.AutoSize = true;
            this.rbWindowsAuth.Checked = true;
            this.rbWindowsAuth.Location = new System.Drawing.Point(17, 28);
            this.rbWindowsAuth.Name = "rbWindowsAuth";
            this.rbWindowsAuth.Size = new System.Drawing.Size(154, 15);
            this.rbWindowsAuth.TabIndex = 2;
            this.rbWindowsAuth.TabStop = true;
            this.rbWindowsAuth.Text = "Windows Authentication";
            this.rbWindowsAuth.UseVisualStyleBackColor = true;
            this.rbWindowsAuth.CheckedChanged += new System.EventHandler(this.rbWindowsAuth_CheckedChanged);
            // 
            // rbSQLAuth
            // 
            this.rbSQLAuth.AutoSize = true;
            this.rbSQLAuth.Location = new System.Drawing.Point(17, 49);
            this.rbSQLAuth.Name = "rbSQLAuth";
            this.rbSQLAuth.Size = new System.Drawing.Size(161, 15);
            this.rbSQLAuth.TabIndex = 3;
            this.rbSQLAuth.Text = "SQL Server Authentication";
            this.rbSQLAuth.UseVisualStyleBackColor = true;
            // 
            // tbSQLUsername
            // 
            this.tbSQLUsername.Location = new System.Drawing.Point(17, 97);
            this.tbSQLUsername.Name = "tbSQLUsername";
            this.tbSQLUsername.Size = new System.Drawing.Size(211, 23);
            this.tbSQLUsername.TabIndex = 4;
            // 
            // tbSQLPassword
            // 
            this.tbSQLPassword.Location = new System.Drawing.Point(17, 148);
            this.tbSQLPassword.Name = "tbSQLPassword";
            this.tbSQLPassword.PasswordChar = '*';
            this.tbSQLPassword.Size = new System.Drawing.Size(211, 23);
            this.tbSQLPassword.TabIndex = 5;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(17, 75);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(68, 19);
            this.metroLabel1.TabIndex = 6;
            this.metroLabel1.Text = "Username";
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(17, 126);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(63, 19);
            this.metroLabel2.TabIndex = 7;
            this.metroLabel2.Text = "Password";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbSQLUsername);
            this.groupBox1.Controls.Add(this.metroLabel2);
            this.groupBox1.Controls.Add(this.rbSQLAuth);
            this.groupBox1.Controls.Add(this.rbWindowsAuth);
            this.groupBox1.Controls.Add(this.metroLabel1);
            this.groupBox1.Controls.Add(this.tbSQLPassword);
            this.groupBox1.Location = new System.Drawing.Point(23, 181);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(245, 191);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database Authentication";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rb32Bit);
            this.groupBox2.Controls.Add(this.rb64Bit);
            this.groupBox2.Location = new System.Drawing.Point(23, 75);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(245, 100);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Application Pool";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.toLocalPackages);
            this.groupBox3.Controls.Add(this.bBrowseInstallPackage);
            this.groupBox3.Controls.Add(this.bBrowseUpgradePackage);
            this.groupBox3.Controls.Add(this.tbInstallPackage);
            this.groupBox3.Controls.Add(this.metroLabel3);
            this.groupBox3.Controls.Add(this.tbUpgradePackage);
            this.groupBox3.Controls.Add(this.metroLabel4);
            this.groupBox3.Location = new System.Drawing.Point(23, 378);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(245, 163);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Local Packages";
            // 
            // toLocalPackages
            // 
            this.toLocalPackages.AutoSize = true;
            this.toLocalPackages.Location = new System.Drawing.Point(148, 30);
            this.toLocalPackages.Name = "toLocalPackages";
            this.toLocalPackages.Size = new System.Drawing.Size(80, 17);
            this.toLocalPackages.TabIndex = 14;
            this.toLocalPackages.Text = "Off";
            this.toLocalPackages.UseVisualStyleBackColor = true;
            // 
            // bBrowseInstallPackage
            // 
            this.bBrowseInstallPackage.Location = new System.Drawing.Point(200, 66);
            this.bBrowseInstallPackage.Name = "bBrowseInstallPackage";
            this.bBrowseInstallPackage.Size = new System.Drawing.Size(28, 23);
            this.bBrowseInstallPackage.TabIndex = 12;
            this.bBrowseInstallPackage.Text = "...";
            this.bBrowseInstallPackage.Click += new System.EventHandler(this.bBrowseInstallPackage_Click);
            // 
            // bBrowseUpgradePackage
            // 
            this.bBrowseUpgradePackage.Location = new System.Drawing.Point(200, 117);
            this.bBrowseUpgradePackage.Name = "bBrowseUpgradePackage";
            this.bBrowseUpgradePackage.Size = new System.Drawing.Size(28, 23);
            this.bBrowseUpgradePackage.TabIndex = 13;
            this.bBrowseUpgradePackage.Text = "...";
            this.bBrowseUpgradePackage.Click += new System.EventHandler(this.bBrowseUpgradePackage_Click);
            // 
            // tbInstallPackage
            // 
            this.tbInstallPackage.Location = new System.Drawing.Point(17, 66);
            this.tbInstallPackage.Name = "tbInstallPackage";
            this.tbInstallPackage.Size = new System.Drawing.Size(177, 23);
            this.tbInstallPackage.TabIndex = 8;
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(17, 95);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(61, 19);
            this.metroLabel3.TabIndex = 11;
            this.metroLabel3.Text = "Upgrade";
            // 
            // tbUpgradePackage
            // 
            this.tbUpgradePackage.Location = new System.Drawing.Point(17, 117);
            this.tbUpgradePackage.Name = "tbUpgradePackage";
            this.tbUpgradePackage.Size = new System.Drawing.Size(177, 23);
            this.tbUpgradePackage.TabIndex = 9;
            // 
            // metroLabel4
            // 
            this.metroLabel4.AutoSize = true;
            this.metroLabel4.Location = new System.Drawing.Point(17, 44);
            this.metroLabel4.Name = "metroLabel4";
            this.metroLabel4.Size = new System.Drawing.Size(41, 19);
            this.metroLabel4.TabIndex = 10;
            this.metroLabel4.Text = "Install";
            // 
            // ofdInstallPackage
            // 
            this.ofdInstallPackage.Filter = "Packages|*.zip";
            // 
            // ofdUpgradePackage
            // 
            this.ofdUpgradePackage.Filter = "Packages|*.zip";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 564);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.Resizable = false;
            this.Text = "Settings";
            this.TextAlign = System.Windows.Forms.VisualStyles.HorizontalAlign.Center;
            this.Load += new System.EventHandler(this.Settings_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroRadioButton rb64Bit;
        private MetroFramework.Controls.MetroRadioButton rb32Bit;
        private MetroFramework.Controls.MetroRadioButton rbWindowsAuth;
        private MetroFramework.Controls.MetroRadioButton rbSQLAuth;
        private MetroFramework.Controls.MetroTextBox tbSQLUsername;
        private MetroFramework.Controls.MetroTextBox tbSQLPassword;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private MetroFramework.Controls.MetroToggle toLocalPackages;
        private MetroFramework.Controls.MetroButton bBrowseInstallPackage;
        private MetroFramework.Controls.MetroButton bBrowseUpgradePackage;
        private MetroFramework.Controls.MetroTextBox tbInstallPackage;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroTextBox tbUpgradePackage;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private System.Windows.Forms.OpenFileDialog ofdInstallPackage;
        private System.Windows.Forms.OpenFileDialog ofdUpgradePackage;
    }
}
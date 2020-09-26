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
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 395);
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
    }
}
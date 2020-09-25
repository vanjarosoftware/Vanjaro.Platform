using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vanjaro.Installer
{
    public partial class Settings : MetroForm
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.FormClosing += Settings_FormClosing;
            LoadForm();
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Use32Bit = rb32Bit.Checked;
            Properties.Settings.Default.UseWindowsAuth = rbWindowsAuth.Checked;

            Properties.Settings.Default.SQLUsername = tbSQLUsername.Text;
            Properties.Settings.Default.SQLPassword = tbSQLPassword.Text;

            Properties.Settings.Default.Save();
        }

        private void LoadForm()
        {
            rb32Bit.Checked = Properties.Settings.Default.Use32Bit;
            rbWindowsAuth.Checked = Properties.Settings.Default.UseWindowsAuth;
            rbSQLAuth.Checked = !Properties.Settings.Default.UseWindowsAuth;
            tbSQLUsername.Text = Properties.Settings.Default.SQLUsername;
            tbSQLPassword.Text = Properties.Settings.Default.SQLPassword;
        }

        private void rbWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuth();
        }

        private void UpdateAuth()
        {
            if (rbWindowsAuth.Checked)
            {
                tbSQLUsername.Enabled = false;
                tbSQLPassword.Enabled = false;
            }
            else
            {
                tbSQLUsername.Enabled = true;
                tbSQLPassword.Enabled = true;

                tbSQLUsername.Select();
            }
        }
    }
}

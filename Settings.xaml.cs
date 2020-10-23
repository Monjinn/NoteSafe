using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NoteSafe
{
    /// <summary>
    /// Interaction logic for Settings window
    /// </summary>
    public partial class Settings : Window
    {
        /// <summary>
        /// Constructor for settings
        /// </summary>
        public Settings()
        {
            InitializeComponent();

            CB_ShowPassword.IsChecked = Properties.Settings.Default.Ui_show_password;
            CB_ShowUsername.IsChecked = Properties.Settings.Default.Ui_show_username;
            TB_DB_Path.Text = Properties.Settings.Default.Database_location;
        }

        /// <summary>
        /// Event handler for accept
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Ui_show_password = (bool)CB_ShowPassword.IsChecked;
            Properties.Settings.Default.Ui_show_username = (bool)CB_ShowUsername.IsChecked;
            Properties.Settings.Default.Database_location = TB_DB_Path.Text;
            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Event handler for decline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Event handler for browsing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Database files (*.mdf)|*.mdf";
            dialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            if (dialog.ShowDialog() == true)
            {
                TB_DB_Path.Text = dialog.FileName;
            }
        }
    }
}

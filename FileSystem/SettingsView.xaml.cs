using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaxiLyrics.PluginsInterface;

namespace FileSystemPlugin
{
    /// <summary>
    /// Settings UI for FileSystem.
    /// </summary>
    public partial class SettingsView : SettingsUserControl
    {
        FileSystem fs;
        #region SettingsUserControl
        
        /// <summary>
        /// Applies changes made in UI to plugin.
        /// </summary>
        public override void AcceptChanges()
        {
            try
            {
                var di = new System.IO.DirectoryInfo(pathTextBox.Text);
                if(!di.Exists)
                    throw new WrongSettingException("The path you've specified is not a valid folder path");
                fs.Roots.Clear();
                fs.Roots.Add(di);
            }
            catch (Exception e)
            {
                throw new WrongSettingException("The path you've specified is not a valid folder path");
            }
        }
        
        /// <summary>
        /// Discards every change made.
        /// </summary>
        public override void DeclineChanges()
        {
        }
        #endregion

        /// <summary>
        /// Settings view constructor. Initializes UI, sets contexts.
        /// </summary>
        /// <param name="fs"></param>
        public SettingsView(FileSystem fs)
        {
            InitializeComponent();
            this.fs = fs;
        }

        #region Event Handlers
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            //This is windows forms dialog - unfortunately, there is no analog in WPF
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathTextBox.Text = dlg.SelectedPath;
            }
        }

        private void SettingsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            pathTextBox.Text = fs.Roots[0].FullName;
        }
        #endregion
    }
}

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
using System.Windows.Shapes;
using MaxiLyrics.PluginsInterface;

namespace MaxiLyrics
{
    /// <summary>
    /// PluginsSettingsWindow - class, that contains interaction logic for window,
    /// that is a wrapper for SettingsUserControl instances. Has OK and CANCEL buttons, as well as
    /// text box that shows exception causes if any.
    /// </summary>
    public partial class PluginSettingsWindow : Window
    {
        SettingsUserControl suc;
        /// <summary>
        /// Constructor for PluginSettingsWindow - sets SettingsUserControl to work with.
        /// </summary>
        /// <param name="suc">SettingsUserControl instance to associate this window with.</param>
        public PluginSettingsWindow(SettingsUserControl suc)
        {
            InitializeComponent();
            this.suc = suc;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Add(suc);
            Grid.SetRow(suc, 0);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                suc.AcceptChanges();
                DialogResult = true;
                Close();
            }
            catch (WrongSettingException ex)
            {
                errorText.Text = ex.Message;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                suc.DeclineChanges();
                DialogResult = false;
                Close();
            }
            catch (WrongSettingException ex)
            {
                errorText.Text = ex.Message;
            }
        }
    }
}

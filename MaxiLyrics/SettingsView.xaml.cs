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

namespace MaxiLyrics
{
    /// <summary>
    /// SettingsUserControl instance for global settings window
    /// </summary>
    public partial class SettingsView : SettingsUserControl
    {
        #region SettingsUserControl
        /// <summary>
        /// Applies settings for every plugin step-by-step
        /// </summary>
        public override void AcceptChanges()
        {
            foreach (PluginNameAndSettings pnas in plugins)
                pnas.View.AcceptChanges();
        }
        /// <summary>
        /// Discards settings for every plugin step-by-step
        /// </summary>
        public override void DeclineChanges()
        {
            foreach (PluginNameAndSettings pnas in plugins)
                pnas.View.DeclineChanges();
        }
        #endregion

        /// <summary>
        /// Just helper structure for LINQ queries
        /// </summary>
        struct PluginNameAndSettings
        {
            public SettingsUserControl View { get; set; }
            public String Name { get; set; }
        }

        MainWindow mw;
        IEnumerable<PluginNameAndSettings> plugins;

        /// <summary>
        /// SettingsView constructor - initializes and sets datacontext.
        /// </summary>
        /// <param name="mw">MainWindow instance - contains lists of all plugins</param>
        public SettingsView(MainWindow mw)
        {
            InitializeComponent();
            this.mw = mw;
        }

        private void SettingsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            plugins = (from p in mw.allPlugins
                       where (p is IHaveSettings)&&((p as IHaveSettings).HaveSettingsView)
                      select new PluginNameAndSettings()
                        {View = (p as IHaveSettings).GetSettingsView(), Name = (p as IPlugin).Name}
                      ).ToList();
            pluginsTabControl.ItemsSource = plugins;
            if (pluginsTabControl.Items.Count > 0)
                pluginsTabControl.SelectedIndex = 0;
        }
    }
}

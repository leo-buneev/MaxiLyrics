using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MaxiLyrics.PluginsInterface;
using System.Runtime.Serialization.Formatters.Binary;

namespace MaxiLyrics
{
    /// <summary>
    /// MainWindow class - contains basic interaction logic for MainWindow.xaml
    /// and availaible plugins lists. Is root in MEF tree.
    /// </summary>

    [Export(typeof(MainWindow))]
    public partial class MainWindow : Window
    {
        #region Plugin connections
        [Import(typeof(IPlayer))]
        IPlayer player;
        [Import(typeof(IPlaylist))]
        IPlaylist playlist;
        [ImportMany(typeof(ITabPlugin))]
        IEnumerable<ITabPlugin> tabPlugins;
        [Import(typeof(FileFormats.IGlobalFileFactory))]
        FileFormats.IGlobalFileFactory fileFactory;

        /// <summary>
        /// List of all availaible plugins
        /// </summary>
        public List<IPlugin> allPlugins;

        /// <summary>
        /// Dictionary of all available plugins that implement IHaveSettings,
        /// indexed by their name.
        /// </summary>
        public Dictionary<String, IHaveSettings> pluginsWithSettings;
        #endregion

        #region Settings related functionality
        /// <summary>
        /// General method, that shows wrapper wrapper window 
        /// for UserControl, passed as argument.
        /// </summary>
        /// <param name="suc">SettingsUserControl that should be showed inside window.</param>
        private void ShowSettings(SettingsUserControl suc)
        {
            PluginSettingsWindow psw = new PluginSettingsWindow(suc);
            psw.ShowDialog();
        }
        /// <summary>
        /// Serializes settings for every plugin into a single file.
        /// </summary>
        private void SaveSettings()
        {
            List<List<object>> allSettings = new List<List<object>>();
            allSettings.Add(SaveGeneralSettings());

            List<String> pluginNames = new List<string>();
            foreach (var p in allPlugins)
            {
                IHaveSettings ihs = p as IHaveSettings;
                if (ihs != null)
                {
                    pluginNames.Add(p.Name);
                    allSettings.Add(ihs.SaveSettings().ToList());
                }
            }
            BinaryFormatter bf = new BinaryFormatter();

            System.IO.FileStream stream;
            try
            {
                stream = System.IO.File.Open("settings.ini", System.IO.FileMode.Create);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return;
            }
            try
            {
                bf.Serialize(stream, pluginNames);
                bf.Serialize(stream, allSettings);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            finally
            {
                stream.Close();
            }
        }
        /// <summary>
        /// Returns MainWindow-related settings (Such as position and size) to serialize
        /// </summary>
        /// <returns>List of objects that represent settings to save.</returns>
        private List<object> SaveGeneralSettings()
        {
            List<object> generalSettings = new List<object>();
            generalSettings.Add(this.Left);
            generalSettings.Add(this.Top);
            generalSettings.Add(this.Width);
            generalSettings.Add(this.Height);
            generalSettings.Add(this.WindowState);
            return generalSettings;
        }
        /// <summary>
        /// Sets MainWindow-related settings to provided values.
        /// </summary>
        /// <param name="generalSettings">Values, provided to set MainWindow settings.</param>
        private void LoadGeneralSettings(List<object> generalSettings)
        {
            this.Left = (double)generalSettings[0];
            this.Top = (double)generalSettings[1];
            this.Width = (double)generalSettings[2];
            this.Height = (double)generalSettings[3];
            this.WindowState = (System.Windows.WindowState)generalSettings[4];
        }
        /// <summary>
        /// Deserializes settings for every plugin from settings file.
        /// </summary>
        private void LoadSettings()
        {
            System.IO.FileStream stream;
            try
            {
                stream = System.IO.File.Open("settings.ini", System.IO.FileMode.Open);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return;
            }
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                List<String> pluginNames = bf.Deserialize(stream) as List<String>;
                List<List<object>> allSettings = bf.Deserialize(stream) as List<List<object>>;
                LoadGeneralSettings(allSettings[0]);
                for (int i = 0; i < pluginNames.Count; i++)
                {
                    IHaveSettings ihs;
                    if (pluginsWithSettings.TryGetValue(pluginNames[i], out ihs))
                        ihs.LoadSettings(allSettings[i + 1]);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return;
            }
            finally
            {
                stream.Close();
            }
        }
        #endregion

        /// <summary>
        /// MainWindow constructor - initializes UI and collections, 
        /// may be implicitly called by MEF.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            allPlugins = new List<IPlugin>();
            pluginsWithSettings = new Dictionary<String, IHaveSettings>();
        }

        #region Event Handlers
        private void theMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            allPlugins.Add(player);
            topGrid.Children.Add(player.GetView());

            allPlugins.Add(playlist);
            bottomGrid.Children.Add(playlist.GetView());

            allPlugins.AddRange(tabPlugins);

            pluginsTabControl.ItemsSource = tabPlugins;
            if (pluginsTabControl.Items.Count > 0)
                pluginsTabControl.SelectedIndex = 0;

            foreach (IPlugin p in allPlugins)
            {
                var ihs = p as IHaveSettings;
                if (ihs != null)
                {
                    pluginsWithSettings.Add(p.Name, ihs);
                }
            }

            LoadSettings();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings(new SettingsView(this));
        }

        private void showPlaylistButton_Unchecked(object sender, RoutedEventArgs e)
        {
            showPluginsButton.IsChecked = true;
        }

        private void showPluginsButton_Unchecked(object sender, RoutedEventArgs e)
        {
            showPlaylistButton.IsChecked = true;
        }

        private void gridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            showingPlaylistStoryboard.To = new GridLength(playlistColumn.ActualWidth, GridUnitType.Star);
            showingPluginsStoryboard.To = new GridLength(pluginsColumn.ActualWidth, GridUnitType.Star);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            var ihs = tb.DataContext as IHaveSettings;
            if (ihs.HaveSettingsView)
                ShowSettings(ihs.GetSettingsView());
        }

        private void theMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        #endregion
    }



    /// <summary>
    /// Converter that helps MainWindow.xaml bind directly to plugins
    /// </summary>
    public class PluginToViewConverter : IValueConverter
    {

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var t = value as IVisible;
            if (t == null) return null;
            else return t.GetView();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("This should never be called");
        }
    }

    /// <summary>
    /// Converter that helps MainWindow.xaml bind directly to plugins
    /// </summary>
    public class PluginToSettingsVisibilityConverter : IValueConverter
    {

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var t = value as IHaveSettings;
            if ((t != null)&&(t.HaveSettingsView))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("This should never be called");
        }
    }
}

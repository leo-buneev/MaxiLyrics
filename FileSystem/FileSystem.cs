using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MaxiLyrics.PluginsInterface;
using System.IO;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FileSystemPlugin
{
    /// <summary>
    /// Interface for FileSystemPlugin
    /// </summary>
    public interface IFileSystemPlugin : ITabPlugin, IHaveSettings
    {
    }
    
    /// <summary>
    /// FileSystemPlugin implementation - tab plugin, some sort of simplified explorer.
    /// </summary>
    [Export(typeof(ITabPlugin))]
    public class FileSystem : DependencyObject, IFileSystemPlugin
    {
        #region IPlugin
        String IPlugin.Name { get { return "File explorer"; } }
        String IPlugin.Description { get { return "Allows exploring your local files right inside your favourite player!"; } }
        String IPlugin.Publisher { get { return "Leonid Buneev"; } }
        int IPlugin.MajorVersion { get { return 0; } }
        int IPlugin.MinorVersion { get { return 8; } }
        #endregion
        #region IVisible
        UserControl IVisible.GetView()
        {
            return new FileSystemView(this);
        }
        #endregion
        #region IHaveSettings
        SettingsUserControl IHaveSettings.GetSettingsView()
        {
            return new SettingsView(this);
        }
        void IHaveSettings.LoadSettings(List<object> values)
        {
            var x = values[0] as ObservableCollection<DirectoryInfo>;
            if (x != null)
                SetValue(RootsProperty, x);
        }
        List<object> IHaveSettings.SaveSettings()
        {
            List<object> settings = new List<object>();
            settings.Add(Roots);
            return settings;
        }
        bool IHaveSettings.HaveSettingsView
        {
            get { return true; }
        }
        #endregion
        #region Plugin connections
        [Import(typeof(IPlaylist))]
        IPlaylist playlist;
        #endregion


        /// <summary>
        /// FileSystem constructor - initializes default values
        /// </summary>
        public FileSystem()
        {
            Roots = new ObservableCollection<DirectoryInfo>();
            String myMusicPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            Roots.Add(new DirectoryInfo(myMusicPath));
        }

        /// <summary>
        /// Dependency property for Roots property
        /// </summary>
        public static readonly DependencyProperty RootsProperty =
            DependencyProperty.Register("Roots", typeof(ObservableCollection<DirectoryInfo>), typeof(FileSystem));

        /// <summary>
        /// Roots property
        /// </summary>
        public ObservableCollection<DirectoryInfo> Roots
        {
            get { return (ObservableCollection<DirectoryInfo>)GetValue(RootsProperty); }
            set { SetValue(RootsProperty, value); }
        }

        /// <summary>
        /// Plays specified file or folder. Actually just delegates this tastk to Playlist.
        /// </summary>
        /// <param name="di">File or folder to play.</param>
        public void Play(FileSystemInfo di)
        {
            playlist.Clear();
            playlist.AddPathRecursive(di.FullName);
            playlist.Play(0);
        }
        
        /// <summary>
        /// Adds specified File or Folder to playlist. Actually delegates this task to Playlist. 
        /// </summary>
        /// <param name="di"></param>
        public void Add(FileSystemInfo di)
        {
            playlist.AddPathRecursive(di.FullName);
        }
    }
}

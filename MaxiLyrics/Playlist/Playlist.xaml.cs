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
using System.ComponentModel.Composition;
using MaxiLyrics.PluginsInterface;

namespace MaxiLyrics
{
    /// <summary>
    /// Playlist class. Contains functionality and view of Playlist plugin.
    /// </summary>
    [Export(typeof(IPlaylist))]
    public partial class Playlist : UserControl, IPlaylist
    {
        #region IPlugin
        String IPlugin.Name { get { return "Playlist core"; } }
        String IPlugin.Description { get { return "Provides basic functionality for managing music queue"; } }
        String IPlugin.Publisher { get { return "Leonid Buneev"; } }
        int IPlugin.MajorVersion { get { return 0; } }
        int IPlugin.MinorVersion { get { return 31; } }
        #endregion
        #region IPlaylist
        ObservableCollection<PlaylistElement> filesInPlaylist;
        public void Clear()
        {
            filesInPlaylist.Clear();
        }
        public void AddFiles(IEnumerable<IFile> files)
        {
            foreach (IFile f in files)
                AddFile(f);
        }
        public void AddFile(IFile file)
        {
            filesInPlaylist.Add(new PlaylistElement(file));
        }
        public void AddPathsRecursive(String[] filepaths)
        {
            foreach (String filepath in filepaths)
                AddPathRecursive(filepath);
        }
        public void Next()
        {
            Play(NowPlayingIndex + 1);
        }
        public void Prev()
        {
            Play(NowPlayingIndex - 1);
        }
        public void AddPathRecursive(String filepath)
        {
            if (System.IO.Directory.Exists(filepath))
                addDirectoryRecursive(filepath);
            else
                addFilePath(filepath);
            if ((!player.CanPlay) && (mainListView.Items.Count != 0))
            {
                Play(0);
            }
        }
        public void Play(int idx)
        {
            if (mainListView.Items.Count > idx)
            {
                if (NowPlaying != null)
                    NowPlaying.IsPlaying = false;
                NowPlayingIndex = idx;
                NowPlaying = (PlaylistElement)mainListView.Items[NowPlayingIndex];
                NowPlaying.IsPlaying = true;
                player.SetSource(NowPlaying.File);
                player.Play();
            }
            else
                player.SetSource(null);
        }
        #endregion
        #region IVisible
        UserControl IVisible.GetView()
        {
            return this;
        }
        #endregion
        #region Plugin connections
        
        [Import(typeof(IPlayer))]
        private IPlayer player;
        
        [Import(typeof(FileFormats.IGlobalFileFactory))]
        FileFormats.IGlobalFileFactory fileFactory;
        #endregion

        private int nowPlayingIndex;

        /// <summary>
        /// Playlist item that was set as current audio source for player.
        /// </summary>
        protected PlaylistElement NowPlaying
        {
            get;
            set;
        }
        /// <summary>
        /// Index of item in Playlist that was set as current audio source for player.
        /// </summary>
        protected int NowPlayingIndex
        {
            get { return nowPlayingIndex; }
            set
            {
                if (value < 0)
                    value = mainListView.Items.Count - 1;
                else if (value >= mainListView.Items.Count)
                    value = 0;
                nowPlayingIndex = value;
                mainListView.ScrollIntoView(mainListView.Items[value]);
            }
        }

        /// <summary>
        /// Playlist constructor. Initializes UI and lists, sets contexts.
        /// </summary>
        public Playlist()
        {
            InitializeComponent();
            filesInPlaylist = new ObservableCollection<PlaylistElement>();
            mainListView.ItemsSource = filesInPlaylist;
        }

        #region Private Functions
        private void addDirectoryRecursive(String path)
        {
            var files = System.IO.Directory.EnumerateFiles(path);
            foreach (var f in files)
                addFilePath(f);
            var dirs = System.IO.Directory.EnumerateDirectories(path);
            foreach (var d in dirs)
                addDirectoryRecursive(d);
        }
        private void addFilePath(String filepath)
        {
            var file = fileFactory.CreateFile(filepath);
            if (file != null)
            {
                PlaylistElement pe = new PlaylistElement(file);
                filesInPlaylist.Add(pe);
            }
        }
        #endregion
        #region Event Handlers
        private void mainListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        private void mainListView_Drop(object sender, DragEventArgs e)
        {
            String[] filePaths = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            AddPathsRecursive(filePaths);
        }
        private void mainListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Play(mainListView.SelectedIndex);
        }
        private void mainListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Delete:
                    int i;
                    while ((i = mainListView.SelectedIndex) != -1)
                    {
                        mainListView.Items.RemoveAt(i);
                        if (nowPlayingIndex >= i)
                            nowPlayingIndex--;
                    }
                    break;
                case Key.Enter:
                    if(mainListView.SelectedIndex != -1)
                        Play(mainListView.SelectedIndex);
                    break;
            }

        }
        #endregion
    }

    /// <summary>
    /// Helper class that represents IFile with propery IsPlaying (for XAML binding purposes).
    /// </summary>
    public class PlaylistElement : DependencyObject
    {
        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(PlaylistElement));
        public PlaylistElement(IFile file)
        {
            this.File = file;
            IsPlaying = false;
        }
        public IFile File { get; set; }
        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }
        public Uri Path { get { return File.Path; } }
        public override string ToString()
        {
            return File.ToString();
        }
    }
}

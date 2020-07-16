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
using System.ComponentModel.Composition;
using System.Windows.Threading;
using MaxiLyrics.PluginsInterface;

namespace MaxiLyrics
{
    /// <summary>
    /// Player class - contains functionality and view of basic player.
    /// </summary>

    [Export(typeof(IPlayer))]
    public partial class Player : UserControl, IPlayer
    {
        #region IPlugin
        /// <inheritdoc />
        String IPlugin.Name { get { return "File explorer"; } }
        /// <inheritdoc />
        String IPlugin.Description { get { return "Allows exploring your local files right inside your favourite player!"; } }
        /// <inheritdoc />
        String IPlugin.Publisher { get { return "Leonid Buneev"; } }
        /// <inheritdoc />
        int IPlugin.MajorVersion { get { return 0; } }
        /// <inheritdoc />
        int IPlugin.MinorVersion { get { return 24; } }
        #endregion
        #region IPlayer

        /// <summary>
        /// Dependency property for IsPlaying property.
        /// </summary>
        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(Player));
        
        /// <inheritdoc />
        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }
        
        /// <summary>
        /// Dependency property for CanPlay property.
        /// </summary>
        public static readonly DependencyProperty CanPlayProperty =
            DependencyProperty.Register("CanPlay", typeof(bool), typeof(Player));
        
        /// <inheritdoc />
        public bool CanPlay
        {
            get { return (bool)GetValue(CanPlayProperty); }
            set { SetValue(CanPlayProperty, value); }
        }
        
        /// <inheritdoc />
        public void SetSource(IFile file)
        {
            if (file == null)
                mediaPlayerElement.Source = null;
            else
                mediaPlayerElement.Source = file.Path;
        }
        
        /// <inheritdoc />
        public void Play(IFile file)
        {
            IsPlaying = false;
            mediaPlayerElement.Stop();
            SetSource(file);
            Play();
        }
        
        /// <inheritdoc />
        public void Play()
        {
            mediaPlayerElement.Play();
            IsPlaying = true;
            positionSliderUpdater.Start();
        }
        
        /// <inheritdoc />
        public void Stop()
        {
            mediaPlayerElement.Stop();
            IsPlaying = false;
            positionSliderUpdater.Stop();
        }
        
        /// <inheritdoc />
        public void Pause()
        {
            mediaPlayerElement.Pause();
            IsPlaying = false;
            positionSliderUpdater.Stop();
        }
        
        /// <inheritdoc />
        public void PlayOrPause()
        {
            if (IsPlaying)
                Pause();
            else
                Play();
        }
        #endregion
        #region IVisible
        /// <inheritdoc />
        UserControl IVisible.GetView()
        {
            return this;
        }
        #endregion

        [Import(typeof(IPlaylist))]
        private IPlaylist playlist;

        private bool IsSliderDragging;
        private DispatcherTimer positionSliderUpdater;
        
        /// <summary>
        /// Player constructor - initializes UI, timers, contexts, etc.
        /// </summary>
        public Player()
        {
            InitializeComponent();
            mediaPlayerElement.LoadedBehavior = MediaState.Manual;
            IsPlaying = false;
            positionSliderUpdater = new DispatcherTimer();
            positionSliderUpdater.Interval = TimeSpan.FromMilliseconds(100);
            positionSliderUpdater.Tick += new EventHandler(positionSliderUpdater_Tick);
        }

        #region Event Handlers
        void positionSliderUpdater_Tick(object sender, EventArgs e)
        {
            if(!IsSliderDragging)
                positionSlider.Value = mediaPlayerElement.Position.TotalMilliseconds;
        }
        private void playerPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        private void playerPanel_Drop(object sender, DragEventArgs e)
        {
            String[] files = (String[])e.Data.GetData(DataFormats.FileDrop);
            playlist.Clear();
            playlist.AddPathsRecursive(files);
            playlist.Play(0);
        }
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPlay)
                PlayOrPause();
            else
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "Music files (*.mp3)|*.mp3";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == true)
                    playlist.AddPathsRecursive(ofd.FileNames);
            }
        }
        private void mediaPlayerElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerElement.NaturalDuration.HasTimeSpan)
                positionSlider.Maximum = mediaPlayerElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            CanPlay = true;
        }
        private void mediaPlayerElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaPlayerElement.Stop();
            positionSliderUpdater.Stop();
            CanPlay = false;
            playlist.Next();
        }
        private void positionSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSliderDragging = true;
            positionSlider.CaptureMouse();
        }
        private void positionSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSliderDragging)
            {
                IsSliderDragging = false;
                double pos = e.GetPosition(positionSlider).X * positionSlider.Maximum / positionSlider.ActualWidth;
                positionSlider.Value = pos;
                mediaPlayerElement.Position = TimeSpan.FromMilliseconds(pos);
                positionSlider.ReleaseMouseCapture();
            }
        }
        private void positionSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSliderDragging)
            {
                double pos = e.GetPosition(positionSlider).X * positionSlider.Maximum / positionSlider.ActualWidth;
                positionSlider.Value = pos;
            }
        }
        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            playlist.Prev();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            playlist.Next();
        }
        #endregion
    }

    /// <summary>
    /// Helper converter for XAML binding purposes - actually just sets text of PlayButton based on current player state.
    /// </summary>
    class PlayerButtonValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bval = (bool)value;
            return bval ? "l l" : "►";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

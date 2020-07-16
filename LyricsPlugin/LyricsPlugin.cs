using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxiLyrics.PluginsInterface;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.IO;

namespace LyricsPlugin
{
    public interface ILyricsPlugin : ITabPlugin
    {
    }

    [Export(typeof(ITabPlugin))]
    public class LyricsPlugin : ILyricsPlugin
    {
        #region IPlugin
        string IPlugin.Name
        {
            get { return "Lyrics"; }
        }
        string IPlugin.Publisher
        {
            get { return "Displays lyrics for now playing song, if availaible"; }
        }
        string IPlugin.Description
        {
            get { return "Displays lyrics for now playing song, if availaible"; }
        }
        int IPlugin.MajorVersion
        {
            get { return 0; }
        }
        int IPlugin.MinorVersion
        {
            get { return 6; }
        }
        #endregion
        #region IVisible
        UserControl IVisible.GetView()
        {
            return new LyricsView(this);
        }
        #endregion

        [Import(typeof(IPlayer))]
        IPlayer Player;

        public DirectoryInfo LyricsFolder { get; set; }

        public LyricsPlugin()
        {
            LyricsFolder = new DirectoryInfo(@"C:\\Lyrics");
        }


    }
}

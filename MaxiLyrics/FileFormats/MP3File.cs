using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MaxiLyrics.PluginsInterface;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace MaxiLyrics.FileFormats
{
    /// <summary>
    /// Factory for mp3 files
    /// </summary>
    [Export(typeof(IFileFactory))]
    class MP3FileFactory : IFileFactory
    {
        /// <summary>
        /// Pattern that match .mp3 files
        /// </summary>
        public System.Text.RegularExpressions.Regex pattern
        {
            get { return new Regex(@".*\.mp3", RegexOptions.IgnoreCase); }
        }

        /// <summary>
        /// Factory method. Creates MP3File instance.
        /// </summary>
        /// <param name="path">Path to local MP3 file.</param>
        /// <returns>IFile instance.</returns>
        public IFile CreateFile(string path)
        {
            return new MP3File(path);
        }
    }

    /// <summary>
    /// MP3 IFile implementation. Can extract basic MP3 tags.
    /// </summary>
    class MP3File : IFile
    {
        #region Constructors
        /// <summary>
        /// MP3 file constructor that accepts path to file. Metadata reading is part
        /// of this constructor as well.
        /// </summary>
        /// <param name="path"></param>
        public MP3File(String path)
        {
            this.path = new FileInfo(path);
            info = new IdSharp.AudioInfo.Mpeg(path, false);
            id3v2 = new IdSharp.Tagging.ID3v2.ID3v2Tag(path);
            id3v1 = new IdSharp.Tagging.ID3v1.ID3v1Tag(path);
        }
        #endregion

        #region Variables
        private FileInfo path;
        private IdSharp.AudioInfo.Mpeg info;
        private IdSharp.Tagging.ID3v2.ID3v2Tag id3v2;
        private IdSharp.Tagging.ID3v1.ID3v1Tag id3v1;
        #endregion

        #region Properties
        /// <summary>
        /// Path to MP3 file
        /// </summary>
        public Uri Path
        {
            get { return new Uri(path.FullName); }
        }

        /// <summary>
        /// Artist, retreived from ID3 tags
        /// </summary>
        public String Artist
        {
            get
            {
                return (String.IsNullOrEmpty(id3v2.Artist)) ? id3v1.Artist : id3v2.Artist;
            }
            set
            {
                id3v2.Artist = value;
                id3v2.Save(Path.AbsolutePath);
                id3v1.Artist = value;
                id3v1.Save(Path.AbsolutePath);
            }
        }
        /// <summary>
        /// Title, retreived from ID3 tags
        /// </summary>
        public String Title
        {
            get
            {
                return (String.IsNullOrEmpty(id3v2.Title)) ? id3v1.Title : id3v2.Title;
            }
            set
            {
                id3v2.Title = value;
                id3v2.Save(Path.AbsolutePath);
                id3v1.Artist = value;
                id3v1.Save(Path.AbsolutePath);
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// ToString override - uses ID3 tags data if possible, otherwise returns filename.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Title))
            {
                if (!String.IsNullOrEmpty(Artist))
                    return Artist + " - " + Title;
                else return Title;
            }
            else return path.Name;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxiLyrics.PluginsInterface;
using System.ComponentModel.Composition;

namespace MaxiLyrics.FileFormats
{
    /// <summary>
    /// Factory for StreamFile. Here "File" means just "Audio Source" - it can be
    /// even radio URL.
    /// </summary>
    [Export(typeof(IFileFactory))]
    class StreamFileFactory : IFileFactory
    {
        /// <summary>
        /// Pattern that matches all paths, that StreamFactory can handle.
        /// </summary>
        public System.Text.RegularExpressions.Regex pattern
        {
            get { return new System.Text.RegularExpressions.Regex(@"http:\\\\.*"); }
        }

        /// <summary>
        /// Factory method. Creates StreamFile instance.
        /// </summary>
        /// <param name="path">Path to some audio source.</param>
        /// <returns>IFile instance.</returns>
        public IFile CreateFile(string path)
        {
            return new StreamFile(new Uri(path));
        }
    }

    /// <summary>
    /// IFile implementation for strem audio sources (for example, from internet.)
    /// </summary>
    class StreamFile : IFile
    {
        private Uri uri;
        /// <summary>
        /// StreamFile constructor - accepts uri to audio source as parameter.
        /// </summary>
        /// <param name="uri">Path to audio source.</param>
        public StreamFile(Uri uri)
        {
            this.uri = uri;
        }
        /// <summary>
        /// Path to source of this StreamFile.
        /// </summary>
        public Uri Path
        {
            get { return uri; }
        }
    }
}

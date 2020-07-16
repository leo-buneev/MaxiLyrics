using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MaxiLyrics.PluginsInterface;

namespace MaxiLyrics.FileFormats
{
    /// <summary>
    /// Interface for File Factory, that is responsible for opening all files.
    /// </summary>
    public interface IGlobalFileFactory : IPlugin
    {
        IFile CreateFile(String path);
    }

    /// <summary>
    /// GlobalFileFactory plugin. Provides functionality for opening files.
    /// </summary>
    [Export(typeof(IGlobalFileFactory))]
    public class GlobalFileFactory : IGlobalFileFactory
    {
        [ImportMany(typeof(IFileFactory))]
        IEnumerable<IFileFactory> factories;
        public GlobalFileFactory() { }

        /// <summary>
        /// This method tries to retrieve audio source from given path and read its metadata.
        /// It enumerates through all availaible IFileFactory implementations, and if there is
        /// no suitable factory, returns null.
        /// </summary>
        /// <param name="path">String with full path to audio source.</param>
        /// <returns>IFile instance or null if path cannot be opened.</returns>
        public IFile CreateFile(String path)
        {
            foreach (var f in factories)
            {
                if (f.pattern.IsMatch(path))
                {
                    var file = f.CreateFile(path);
                    return file;
                }
            }
            //TODO: Some logic to try open unknown files
            return null; 
        }

        #region IPlugin
        string IPlugin.Name
        {
            get { return "File extensions manager"; }
        }

        string IPlugin.Publisher
        {
            get { return "Leonid Buneev"; }
        }

        string IPlugin.Description
        {
            get { return "Provides functionality for opening files of different extensions."; }
        }

        int IPlugin.MajorVersion
        {
            get { return 0; }
        }

        int IPlugin.MinorVersion
        {
            get { return 7; }
        }
        #endregion
    }
}

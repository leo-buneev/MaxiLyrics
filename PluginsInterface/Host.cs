using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

/// <summary>
/// Namespace that contains basic interfaces, that every plugin for MaxiLyrics must 
/// implement or at least know about their existence. Contains in PluginsInterface.dll
/// </summary>
namespace MaxiLyrics.PluginsInterface
{
    /// <summary>
    /// Interface for playlist component - one of two core components. Contains basic playlist functionality.
    /// If you need to access playlist from your plugins code, retreive the IPlayer
    /// interface via MEF.
    /// <code>
    /// class myPlugin : IPlugin
    /// {
    /// //...
    /// [Import(typeof(IPlayer))]
    /// IPlayer playerInstance;
    /// //...
    /// }
    /// </code>
    /// By using of this pattern, reference to IPlayer will be retrieved automatically after constructor.
    /// </summary>
    public interface IPlayer : IVisible, IPlugin
    {
        /// <summary>
        /// Changes current audio source.
        /// </summary>
        /// <param name="file">Audio source instance. Can be null.</param>
        void SetSource(IFile file);
        /// <summary>
        /// Starts audio playing, if correct audio source was provided.
        /// If player is already in playing state, nothing happens.
        /// </summary>
        void Play();
        /// <summary>
        /// Pauses audio playing. If player was already paused or stopped, nothing happens.
        /// </summary>
        void Pause();
        /// <summary>
        /// Starts or pauses audio playing, depending on current player state.
        /// </summary>
        void PlayOrPause();
        /// <summary>
        /// Stops audio playing and sets audio source position to the beginning, if possible.
        /// </summary>
        void Stop();
        /// <summary>
        /// Checks, that all conditions for playing (for example, that correct audio source was provided) are satisfied.
        /// </summary>
        bool CanPlay { get; }

        //event EventHandler PositionChanged;
    }

    /// <summary>
    /// Interface for playlist component - one of two core components. Contains basic playlist functionality.
    /// If you need to access playlist from your plugins code, retreive the IPlaylist
    /// interface via MEF.
    /// <code>
    /// class myPlugin : IPlugin
    /// {
    /// //...
    /// [Import(typeof(IPlaylist))]
    /// IPlaylist playlistInstance;
    /// //...
    /// }
    /// </code>
    /// By using of this pattern, reference to IPlaylist will be retrieved automatically after constructor.
    /// </summary>
    public interface IPlaylist : IVisible, IPlugin
    {
        /// <summary>
        /// Sets players audio source to the next file in playlist
        /// </summary>
        void Next();
        /// <summary>
        /// Sets players audio source to the previous file in playlist
        /// </summary>
        void Prev();
        /// <summary>
        /// Sets players audio source to the file, that is on a certain pozition in playlist.
        /// </summary>
        /// <param name="idx">Position of the file in playlist. Can be out of range - in that 
        /// case closest possible number will be used.</param>
        void Play(int idx);
        /// <summary>
        /// Adds every file inside specified folders (or their subfolders) to the playlist.
        /// </summary>
        /// <param name="filepaths">Paths to files and folders to add. Cannot be null - in that case throws exception.</param>
        void AddPathsRecursive(String[] filepaths);
        /// <summary>
        /// If argument is folderpath, adds every file inside to the playlist.
        /// If argument is filepath, adds this file to playlist.
        /// </summary>
        /// <param name="filepath">Path to folder or file. Incorrect path or path to location without enough files throws exception.</param>
        void AddPathRecursive(String filepath);
        /// <summary>
        /// Adds list of files to current playlist.
        /// </summary>
        /// <param name="files">List of files to add. Cannot be null</param>
        void AddFiles(IEnumerable<IFile> files);
        /// <summary>
        /// Adds the file specified in argument to the playlist
        /// </summary>
        /// <param name="file">File to add.</param>
        void AddFile(IFile file);
        /// <summary>
        /// Clears current playlist.
        /// <remarks>Doesn't affect players current audiosource - it means, 
        /// that this method will not interrupt current playback.</remarks>
        /// </summary>
        void Clear();
    }
}

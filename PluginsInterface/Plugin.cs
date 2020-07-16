using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Windows;

namespace MaxiLyrics.PluginsInterface
{
    /// <summary>
    /// Interface, that every plugin must implement.
    /// Contains basic metadata.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Plugins title
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Name of company or person who has published this plugin
        /// </summary>
        String Publisher { get; }
        /// <summary>
        /// Plugins descripton -- cca 5 lines of text
        /// </summary>
        String Description { get; }
        /// <summary>
        /// Major version of plugin (>= 1 only for stable releases)
        /// </summary>
        int MajorVersion { get; }
        /// <summary>
        /// Minor version of plugin - used for correct settings loading, updates etc
        /// </summary>
        int MinorVersion { get; }
    }

    /// <summary>
    /// Interface for plugins, that want their view to be represented as tab in tabControl from mainWindow.
    /// </summary>
    public interface ITabPlugin : IVisible, IPlugin
    {
    }

    /// <summary>
    /// Interface for visible plugins (right now only tab plugins are visible).
    /// Provides functionality to retreive UserControl, that represents plugin view.
    /// </summary>
    public interface IVisible
    {
        /// <summary>
        /// Returns UserControl, that represents plugin view.
        /// </summary>
        /// <returns>Instance of UserControl.</returns>
        UserControl GetView();
    }

    /// <summary>
    /// Basic interface for every audio data source.
    /// Must provide valid Uri, that will be used for playback.
    /// <remarks>If file contains additional metadata, such as tags, better
    /// use IFileWithMetadata interface.</remarks>
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Gets audio source uri
        /// </summary>
        Uri Path { get; }
    }

    /// <summary>
    /// Interface for audio data source, that contains additional metadata.
    /// Provides method for reading metadata - used by host application to
    /// delay metadata loading.
    /// </summary>
    public interface IFileWithMetadata : IFile
    {
        void ReadMetadata();
    }

    public interface IFileFactory
    {
        System.Text.RegularExpressions.Regex pattern { get; }
        IFile CreateFile(String path);
    }

    /// <summary>
    /// Interface, that contains functionality for serialization and modifying plugin settings.
    /// Serialization is done on higher level, plugin just must correctly implement LoadSettings and SaveSettings methods.
    /// Modifying is possible via SettingsUserControl - if you want to provide plugin users functionality for modifying some settings,
    /// you should:
    ///  - implement custom SettingsUserControl
    ///  - return its instance in GetSettingsView
    ///  - Ensure that HaveSettingsView returns true.
    /// <remarks>
    /// Usually GetSettingsView will just return new instance of GetSettingsView. Using singletone pattern can cause problems,
    /// if user will call settings window twice - SettingsUserControl will be logical child of first window, therefore cannot be logical
    /// child of second window.
    /// </remarks> 
    /// </summary>
    public interface IHaveSettings
    {
        /// <summary>
        /// Returns correct instance of SettingsUserControl, that provides user interface for modifying plugin settings.
        /// </summary>
        /// <returns>Instance of SettingsUserControl. Null if plugin doesn't implement SettingsUserControl.</returns>
        SettingsUserControl GetSettingsView();

        /// <summary>
        /// Provides information, if SettingsUserControl is implemented by plugin and availaible.
        /// </summary>
        bool HaveSettingsView { get; }

        /// <summary>
        /// Returns settings, that must be saved and loaded at next program launch.
        /// </summary>
        /// <returns>List of objects, that should be saved. Ensure, that every object is serializable.</returns>
        List<object> SaveSettings();

        /// <summary>
        /// Sets plugin settings to values provided.
        /// </summary>
        /// <param name="values">List of objects, that represent plugin settings.</param>
        void LoadSettings(List<object> values);
    }

    /// <summary>
    /// Standard user control with functionality to apply or decline settings. 
    /// <remarks>
    /// Could be abstract, but that causes problems for integrated in VS2010 WPF Designer.
    /// </remarks>
    /// </summary>
    public class SettingsUserControl : UserControl
    {
        /// <summary>
        /// Accepts every changes made in this control to plugin settings.
        /// Throws WrongSettingException, if some setting is invalid.
        /// </summary>
        public virtual void AcceptChanges() { }
        /// <summary>
        /// Discards any changes made in this control.
        /// </summary>
        public virtual void DeclineChanges() { }
    }

    /// <summary>
    /// Exception that is thrown if user tries to submit invalid settings.
    /// </summary>
    public class WrongSettingException : Exception
    {
        public WrongSettingException(String message) : base(message) { }
        public WrongSettingException(String message, Exception inner) : base(message, inner) { }
    }
}

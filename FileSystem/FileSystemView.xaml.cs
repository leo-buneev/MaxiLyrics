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
using MaxiLyrics;
using System.IO;
using MaxiLyrics.PluginsInterface;

namespace FileSystemPlugin
{

    /// <summary>
    /// UI for FileSystem plugin.
    /// </summary>
    partial class FileSystemView : UserControl
    {
        FileSystem vm;
        /// <summary>
        /// Constructor for FileSystemView - accepts reference to plugin itself as parameter.
        /// </summary>
        /// <param name="vm">FileSystem instance</param>
        public FileSystemView(FileSystem vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            this.vm = vm;
        }

        #region Event handlers
        private void fsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sItem = fsTree.SelectedItem as DirectoryInfo;
            if (sItem != null)
                fsList.ItemsSource = sItem.EnumerateFiles("*.mp3");
            else
                fsList.ItemsSource = null;
        }

        private void fsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = fsList.SelectedItem as FileInfo;
            if (fsList.SelectedItem != null)
                vm.Play(fi);
        }

        private void fsList_DragBegin(object sender, RoutedEventArgs e)
        {
            DragDrop.DoDragDrop(fsList,
                    new DataObject(DataFormats.FileDrop, (from FileSystemInfo i in fsList.SelectedItems select i.FullName).ToArray()),
                    DragDropEffects.All);
        }

        private void playButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            DirectoryInfo di = tb.DataContext as DirectoryInfo;
            vm.Play(di);
        }

        private void addButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            DirectoryInfo di = tb.DataContext as DirectoryInfo;
            vm.Add(di);
        }


        bool dragging = false;
        private void fsTreeItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
        }


        private void fsTreeItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
        }

        private void fsTreeItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                dragging = false;
                DirectoryInfo di = fsTree.SelectedItem as DirectoryInfo;
                DataObject data = null;

                if (di != null)
                {
                    data = new DataObject(DataFormats.FileDrop, new String[] { di.FullName });
                    DragDropEffects dde = DragDropEffects.Copy;
                    DragDropEffects de = DragDrop.DoDragDrop(fsTree, data, dde);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Helper converter for tree hierarchy data template, defined in XAML.
    /// </summary>
    public class DirectoryToTreeNodeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                DirectoryInfo dir;
                String dirStr = value as String;
                if (dirStr != null)
                    dir = new DirectoryInfo(dirStr);
                else
                    dir = value as DirectoryInfo;
                if (dir == null) return null;
                return dir.EnumerateDirectories();
            }
            catch { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("This should never be called");
        }
    }
}

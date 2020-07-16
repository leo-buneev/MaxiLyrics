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

namespace LyricsPlugin
{
    /// <summary>
    /// Interaction logic for LyricsView.xaml
    /// </summary>
    public partial class LyricsView : UserControl
    {
        LyricsPlugin lp;
        public LyricsView(LyricsPlugin lp)
        {
            this.lp = lp;
            InitializeComponent();
        }
    }
}

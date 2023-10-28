using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.View
{
    public class BrowserListFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public FileNode FileNode { get; }
        public string Text { get; }
        public ImageSource? ImageSource { get; }
        public FontStyle FontStyle { get; }

        public BrowserListFile(FileNode fileNode, string text, ImageSource? imageSource, FontStyle fontStyle)
        {
            FileNode = fileNode;
            Text = text;
            ImageSource = imageSource;
            FontStyle = fontStyle;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class BrowserListFileControl : UserControl
    {
        public BrowserListFileControl()
        {
            InitializeComponent();
        }

    }
}

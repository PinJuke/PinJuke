using PinJuke.Playlist;
using SVGImage.SVG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using DotNetProjects.SVGImage.SVG.FileLoaders;
using FFmpeg.AutoGen;

namespace PinJuke.View
{
    public class BrowserListFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text { get; }
        public ImageSource? ImageSource { get; }
        public bool Selected { get; }

        public BrowserListFile(string text, ImageSource? imageSource = null, bool selected = false)
        {
            Text = text;
            ImageSource = imageSource;
            Selected = selected;
        }
    }


    /// <summary>
    /// Interaction logic for BrowserList.xaml
    /// </summary>
    public partial class BrowserList : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? RemovalRequestedEvent;

        private FileNode? fileNode = null;
        public FileNode? FileNode
        {
            get => fileNode;
            set
            {
                if (value != fileNode)
                {
                    fileNode = value;
                    UpdateView();
                }
            }
        }

        private IReadOnlyList<BrowserListFile> files = new List<BrowserListFile>();
        public IReadOnlyList<BrowserListFile> Files
        {
            get => files;
            private set
            {
                if (value != files)
                {
                    files = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BrowserList()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void AnimateUpIn()
        {
            BeginStoryboard((Storyboard)Resources["UpInStoryboard"]);
        }

        public void AnimateUpOut()
        {
            BeginStoryboard((Storyboard)Resources["UpOutStoryboard"]);
        }

        public void AnimateDownIn()
        {
            BeginStoryboard((Storyboard)Resources["DownInStoryboard"]);
        }

        public void AnimateDownOut()
        {
            BeginStoryboard((Storyboard)Resources["DownOutStoryboard"]);
        }

        private void OutStoryboard_Completed(object sender, EventArgs e)
        {
            RemovalRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateView()
        {
            List<BrowserListFile> files = new();
            var fileNode = FileNode;

            if (fileNode == null)
            {
                Files = files;
                return;
            }

            var morePreviousFileNodes = false;
            var moreNextFileNodes = false;
            var count = 1;
            var startFileNode = fileNode;
            var endFileNode = fileNode;

            for (var i = 0; ; ++i)
            {
                morePreviousFileNodes = startFileNode.PreviousSibling != null;
                if (i >= 3 || !morePreviousFileNodes)
                {
                    break;
                }
                startFileNode = startFileNode.PreviousSibling!;
                count++;
            }

            for (var i = 0; ; ++i)
            {
                moreNextFileNodes = endFileNode.NextSibling != null;
                if (i >= 3 || !moreNextFileNodes)
                {
                    break;
                }
                endFileNode = endFileNode.NextSibling!;
                count++;
            }

            if (morePreviousFileNodes)
            {
                files.Add(new("..."));
            }
            for (var itemFileNode = startFileNode; itemFileNode != endFileNode.NextSibling; itemFileNode = itemFileNode.NextSibling!)
            {
                string imageSource = itemFileNode.Type switch
                {
                    FileType.Directory => @"icons/folder-outline.svg",
                    FileType.M3u => @"icons/folder-outline.svg",
                    FileType.Music => @"icons/musical-notes-outline.svg",
                    FileType.Video => @"icons/videocam-outline.svg",
                    _ => throw new NotImplementedException(),
                };
                var drawingImage = SvgImageLoader.Instance.GetFromResource(imageSource);
                files.Add(new(itemFileNode.DisplayName, drawingImage, itemFileNode == fileNode));
            }
            if (moreNextFileNodes)
            {
                files.Add(new("..."));
            }
            Files = files;
        }

    }

}


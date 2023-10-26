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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.View
{
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

        private IReadOnlyList<string> files = new List<string>();
        public IReadOnlyList<string> Files
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
            List<string> files = new();
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
                files.Add("...");
            }
            for (var itemFileNode = startFileNode; itemFileNode != endFileNode.NextSibling; itemFileNode = itemFileNode.NextSibling!)
            {
                files.Add((itemFileNode == fileNode ? ">>" : "") + itemFileNode.DisplayName);
            }
            if (moreNextFileNodes)
            {
                files.Add("...");
            }
            Files = files;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {

        }
    }

}


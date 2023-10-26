using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
    public partial class Browser : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private BrowserList browserList = new();

        private bool browserVisible = false;
        public bool BrowserVisible
        {
            get => browserVisible;
            set
            {
                if (value != browserVisible)
                {
                    browserVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

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
                    NotifyPropertyChanged();
                }
            }
        }

        private string navigationDisplayPath = "";
        public string NavigationDisplayPath
        {
            get => navigationDisplayPath;
            private set
            {
                if (value != navigationDisplayPath)
                {
                    navigationDisplayPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Browser()
        {
            InitializeComponent();
            DataContext = this;
            AddBrowserList(browserList);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddBrowserList(BrowserList browserList)
        {
            BrowserListContainer.Children.Add(browserList);
            browserList.RemovalRequestedEvent += BrowserList_RemovalRequestedEvent;
        }

        private void BrowserList_RemovalRequestedEvent(object? sender, EventArgs e)
        {
            Debug.WriteLine("Removing browser list.");
            BrowserListContainer.Children.Remove((BrowserList)sender!);
        }

        private void UpdateView()
        {
            var oldFileNode = browserList.FileNode;
            var newFileNode = FileNode;
            if (oldFileNode == null || newFileNode == null)
            {
                browserList.FileNode = newFileNode;
                NavigationDisplayPath = newFileNode?.DisplayBasePath ?? "";
                return;
            }

            if (newFileNode.IsAncestorOf(oldFileNode))
            {
                browserList.AnimateUpOut();
                browserList = new();
                AddBrowserList(browserList);
                browserList.AnimateUpIn();
            }
            else if (oldFileNode.IsAncestorOf(newFileNode))
            {
                browserList.AnimateDownOut();
                browserList = new();
                AddBrowserList(browserList);
                browserList.AnimateDownIn();
            }

            browserList.FileNode = newFileNode;
            NavigationDisplayPath = newFileNode.DisplayBasePath;
        }


    }
}

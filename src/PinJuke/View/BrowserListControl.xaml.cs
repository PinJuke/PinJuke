﻿using PinJuke.Playlist;
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

namespace PinJuke.View
{
    public partial class BrowserListControl : BaseControl
    {
        private static readonly Dictionary<FileType, string> iconPaths = new()
        {
            {FileType.Directory, @"icons\folder-outline.svg"},
            {FileType.DirectoryUp, @"icons\arrow-back-outline.svg"},
            {FileType.M3u, @"icons\folder-outline.svg"},
            {FileType.Music, @"icons\musical-notes-outline.svg"},
            {FileType.Video, @"icons\videocam-outline.svg"},
            {FileType.Stream, @"icons\download-outline.svg"},
        };

        public event EventHandler? RemovalRequestedEvent;

        private FileNode? fileNode = null;
        public FileNode? FileNode
        {
            get => fileNode;
            set
            {
                var oldFileNode = fileNode;
                if (value != oldFileNode)
                {
                    fileNode = value;
                    UpdateView(oldFileNode, fileNode);
                    NotifyPropertyChanged();
                }
            }
        }

        private List<BrowserListFile> files = new List<BrowserListFile>();
        public List<BrowserListFile> Files
        {
            get => files;
            private set => this.SetField(ref files, value);
        }

        private int selectedFileIndex = -1;
        public int SelectedFileIndex
        {
            get => selectedFileIndex;
            private set => this.SetField(ref selectedFileIndex, value);
        }

        public BrowserListControl()
        {
            InitializeComponent();
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

        private void UpdateView(FileNode? oldFileNode, FileNode? newFileNode)
        {
            if (newFileNode == null || oldFileNode == null || newFileNode.Parent != oldFileNode.Parent)
            {
                Files = CreateBrowserListFiles(newFileNode);
            }
            SelectedFileIndex = Files.FindIndex(it => it.FileNode == newFileNode);
            if (SelectedFileIndex != -1)
            {
                FilesListBox.ScrollIntoView(Files[SelectedFileIndex]);
            }
        }

        private List<BrowserListFile> CreateBrowserListFiles(FileNode? newFileNode)
        {
            List<BrowserListFile> files = new();
            if (newFileNode == null)
            {
                return files;
            }
            var startFileNode = newFileNode?.Parent?.FirstChild ?? newFileNode;
            for (var fileNode = startFileNode; fileNode != null; fileNode = fileNode.NextSibling)
            {
                DrawingImage? drawingImage = null;
                if (iconPaths.TryGetValue(fileNode.Type, out var iconPath))
                {
                    drawingImage = SvgImageLoader.Instance.GetFromResource(iconPath);
                }
                files.Add(new(fileNode, fileNode.DisplayName, drawingImage, fileNode.Type == FileType.DirectoryUp ? FontStyles.Italic : FontStyles.Normal));
            }
            return files;
        }

    }

}


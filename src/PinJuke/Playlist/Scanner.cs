using NaturalSort.Extension;
using SoftCircuits.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace PinJuke.Playlist
{
    public class ScanResult
    {
        public FileNode RootFileNode { get; }
        public OrderedDictionary<string, FileNode> PlayableFileNodesByFullName { get; } = new();
        public List<FileNode> M3uFileNodes { get; } = new();

        public ScanResult(FileNode rootFileNode)
        {
            RootFileNode = rootFileNode;

            for (var eachFileNode = rootFileNode.FirstChild; eachFileNode != null; eachFileNode = eachFileNode.GetNextInList())
            {
                if (eachFileNode.Type == FileType.M3u)
                {
                    M3uFileNodes.Add(eachFileNode);
                }
                else if (eachFileNode.Playable)
                {
                    PlayableFileNodesByFullName.Add(eachFileNode.FullName.ToLowerInvariant(), eachFileNode);
                }
            }
        }

        public bool TryGetPlayableFileNode(string fullPath, [MaybeNullWhen(false)] out FileNode fileNode)
        {
            return PlayableFileNodesByFullName.TryGetValue(fullPath.ToLowerInvariant(), out fileNode);
        }

        public FileNode? TryGetPlayableFileNodeOrDefault(string fullPath)
        {
            return TryGetPlayableFileNode(fullPath, out var fileNode) ? fileNode : null;
        }
    }

    class ScanCanceledException : Exception
    {
    }

    public class Scanner : BackgroundWorker
    {
        private static readonly Dictionary<string, FileType> fileTypes = new()
        {
            {"mp3", FileType.Music},
            {"mp4", FileType.Video},
            {"m3u", FileType.M3u},
        };

        private readonly string path;

        public Scanner(string path)
        {
            WorkerSupportsCancellation = true;
            this.path = path;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            var directoryInfo = new DirectoryInfo(path);
            var rootFileNode = new FileNode(directoryInfo.FullName, GetDisplayName(directoryInfo.FullName), FileType.Directory);
            ScanResult scanResult;
            try
            {
                ScanDirectory(rootFileNode, directoryInfo);
                ResolveM3uFiles(new ScanResult(rootFileNode));
                scanResult = new ScanResult(rootFileNode);
            }
            catch (ScanCanceledException)
            {
                e.Cancel = true;
                return;
            }
            e.Result = scanResult;
        }

        protected void CheckCancellation()
        {
            if (CancellationPending)
            {
                // Accept the cancellation request.
                throw new ScanCanceledException();
            }
        }

        protected string GetDisplayName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf('\\') + 1);
        }

        protected void AppendFileIfSupportedType(FileNode parent, FileInfo fileInfo, bool excludeM3u = false)
        {
            var extension = fileInfo.Extension;
            if (extension.IsNullOrEmpty())
            {
                return;
            }
            extension = extension[1..].ToLowerInvariant();
            if (fileTypes.TryGetValue(extension, out var fileType))
            {
                if (excludeM3u && fileType == FileType.M3u)
                {
                    return;
                }
                parent.AppendChild(new FileNode(fileInfo.FullName, GetDisplayName(fileInfo.FullName), fileType));
            }
        }

        protected bool ScanDirectory(FileNode fileNode, DirectoryInfo directoryInfo)
        {
            CheckCancellation();

            DirectoryInfo[] directories;
            FileInfo[] files;
            try
            {
                directories = directoryInfo.GetDirectories();
                files = directoryInfo.GetFiles();
            }
            catch (IOException)
            {
                return false;
            }

            var orderedDirectories = directories.OrderBy(x => x.Name, new NaturalSortComparer(StringComparison.CurrentCultureIgnoreCase));
            foreach (var childDirectoryInfo in orderedDirectories)
            {
                var childFileNode = new FileNode(childDirectoryInfo.FullName, GetDisplayName(childDirectoryInfo.FullName), FileType.Directory);
                var hasChildren = ScanDirectory(childFileNode, childDirectoryInfo);
                // Discard directories with no files of interest.
                if (hasChildren)
                {
                    fileNode.AppendChild(childFileNode);
                }
            }
            var orderedFiles = files.OrderBy(x => x.Name, new NaturalSortComparer(StringComparison.CurrentCultureIgnoreCase));
            foreach (var fileInfo in orderedFiles)
            {
                AppendFileIfSupportedType(fileNode, fileInfo);
            }

            if (fileNode.ChildCount == 0)
            {
                return false;
            }

            fileNode.InsertBefore(new FileNode("", Strings.BrowserUp, FileType.DirectoryUp), fileNode.FirstChild);
            return true;
        }

        protected void ResolveM3uFiles(ScanResult scanResult)
        {
            foreach (var m3uFileNode in scanResult.M3uFileNodes)
            {
                CheckCancellation();

                var basePath = Path.GetDirectoryName(m3uFileNode.FullName);
                if (basePath.IsNullOrEmpty())
                {
                    continue;
                }

                using var streamReader = new StreamReader(m3uFileNode.FullName, true);
                for (; ; )
                {
                    var line = streamReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }

                    var fullPath = Path.GetFullPath(line, basePath);
                    AppendFileIfSupportedType(m3uFileNode, new FileInfo(fullPath), true);

                    // avoid duplicates
                    if (scanResult.TryGetPlayableFileNode(fullPath, out var replacedFileNode))
                    {
                        replacedFileNode.Remove();
                    }
                }
            }

            foreach (var m3uFileNode in scanResult.M3uFileNodes)
            {
                if (m3uFileNode.ChildCount == 0)
                {
                    m3uFileNode.Remove();
                }
                else
                {
                    m3uFileNode.InsertBefore(new FileNode("", Strings.BrowserUp, FileType.DirectoryUp), m3uFileNode.FirstChild);
                }
            }
        }

    }
}

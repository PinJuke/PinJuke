using NaturalSort.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            try
            {
                ScanDirectory(rootFileNode, directoryInfo);
                ResolveM3uFiles(rootFileNode);
            }
            catch (ScanCanceledException)
            {
                e.Cancel = true;
                return;
            }
            e.Result = rootFileNode;
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
            if (string.IsNullOrEmpty(extension))
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

            var directories = directoryInfo.GetDirectories().OrderBy(x => x.Name, new NaturalSortComparer(StringComparison.CurrentCultureIgnoreCase));
            foreach (var childDirectoryInfo in directories)
            {
                var childFileNode = new FileNode(childDirectoryInfo.FullName, GetDisplayName(childDirectoryInfo.FullName), FileType.Directory);
                var hasChildren = ScanDirectory(childFileNode, childDirectoryInfo);
                // Discard directories with no files of interest.
                if (hasChildren)
                {
                    fileNode.AppendChild(childFileNode);
                }
            }
            var files = directoryInfo.GetFiles().OrderBy(x => x.Name, new NaturalSortComparer(StringComparison.CurrentCultureIgnoreCase));
            foreach (var fileInfo in files)
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

        protected void ResolveM3uFiles(FileNode rootFileNode)
        {
            // lower case full path => FileNode
            var playableFileNodesByFullName = new Dictionary<string, FileNode>();
            var m3uFileNodes = new List<FileNode>();

            for (var eachFileNode = rootFileNode.FirstChild; eachFileNode != null; eachFileNode = eachFileNode.GetNextInList())
            {
                if (eachFileNode.Type == FileType.M3u)
                {
                    m3uFileNodes.Add(eachFileNode);
                }
                else if (eachFileNode.Playable)
                {
                    playableFileNodesByFullName.Add(eachFileNode.FullName.ToLowerInvariant(), eachFileNode);
                }
            }

            foreach (var m3uFileNode in m3uFileNodes)
            {
                CheckCancellation();

                var basePath = Path.GetDirectoryName(m3uFileNode.FullName);
                if (string.IsNullOrEmpty(basePath))
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
                    if (playableFileNodesByFullName.TryGetValue(fullPath.ToLowerInvariant(), out var replacedFileNode))
                    {
                        replacedFileNode.Remove();
                    }
                }
            }

            foreach (var m3uFileNode in m3uFileNodes)
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

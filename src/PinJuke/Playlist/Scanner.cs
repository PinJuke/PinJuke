using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            var fileNode = new FileNode(directoryInfo.FullName, GetDisplayName(directoryInfo.FullName), FileType.Directory);
            try
            {
                ScanDirectory(fileNode, directoryInfo);
                ResolveM3uFiles(fileNode);
            }
            catch (ScanCanceledException)
            {
                e.Cancel = true;
                return;
            }
            e.Result = fileNode;
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

        protected void ScanDirectory(FileNode fileNode, DirectoryInfo directoryInfo)
        {
            CheckCancellation();

            foreach (var childDirectoryInfo in directoryInfo.GetDirectories())
            {
                var childFileNode = new FileNode(childDirectoryInfo.FullName, GetDisplayName(childDirectoryInfo.FullName), FileType.Directory);
                ScanDirectory(childFileNode, childDirectoryInfo);
                // Discard directories with no files of interest.
                if (childFileNode.ChildCount > 0)
                {
                    fileNode.AppendChild(childFileNode);
                }
            }
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                AppendFileIfSupportedType(fileNode, fileInfo);
            }
        }

        protected void ResolveM3uFiles(FileNode fileNode)
        {
            // lower case full path => FileNode
            var fileNodesByFullName = new Dictionary<string, FileNode>();
            var m3uFileNodes = new List<FileNode>();

            for (var eachFileNode = fileNode.FirstChild; eachFileNode != null; eachFileNode = eachFileNode.GetNextInList())
            {
                fileNodesByFullName.Add(eachFileNode.FullName.ToLowerInvariant(), eachFileNode);
                if (eachFileNode.Type == FileType.M3u)
                {
                    m3uFileNodes.Add(eachFileNode);
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
                    if (fileNodesByFullName.TryGetValue(fullPath.ToLowerInvariant(), out var replacedFileNode))
                    {
                        replacedFileNode.Remove();
                    }
                }
            }

        }

    }
}

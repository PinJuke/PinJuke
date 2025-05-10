using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace PinJuke.Playlist
{
    public enum FileType
    {
        Directory,
        DirectoryUp,
        Music,
        Video,
        Stream,
        M3u,
    }

    public class FileNode
    {
        public FileNode? Parent { get; private set; } = null;
        public FileNode? NextSibling { get; private set; } = null;
        public FileNode? PreviousSibling { get; private set; } = null;
        public FileNode? FirstChild { get; private set; } = null;
        public FileNode? LastChild { get; private set; } = null;

        public string FullName { get; }
        public string DisplayName { get; }
        public FileType Type { get; }
        public int ChildCount { get; private set; } = 0;

        public bool Playable => Type == FileType.Music || Type == FileType.Video || Type == FileType.Stream;

        public string DisplayBasePath
        {
            get
            {
                var names = new List<string>();
                for (var node = this.Parent; node != null; node = node.Parent)
                {
                    names.Add(node.DisplayName);
                }
                names.Reverse();
                return string.Join(@"\", names);
            }
        }

        public FileNode(string fullName, string displayName, FileType type)
        {
            FullName = fullName;
            DisplayName = displayName;
            Type = type;
        }

        public void AppendChild(FileNode child)
        {
            InsertBefore(child, null);
        }

        public void InsertBefore(FileNode child, FileNode? referenceChild)
        {
            if (child.Parent != null)
            {
                throw new ArgumentException("Child is already appended.");
            }

            if (referenceChild != null)
            {
                if (referenceChild.Parent != this)
                {
                    throw new ArgumentException("Reference child is invalid.");
                }

                child.NextSibling = referenceChild;
                child.PreviousSibling = referenceChild.PreviousSibling;
            }
            else
            {
                child.NextSibling = null;
                child.PreviousSibling = LastChild;
            }

            if (child.NextSibling != null)
            {
                child.NextSibling.PreviousSibling = child;
            }
            else
            {
                LastChild = child;
            }

            if (child.PreviousSibling != null)
            {
                child.PreviousSibling.NextSibling = child;
            }
            else
            {
                FirstChild = child;
            }

            child.Parent = this;
            ChildCount++;
        }

        public void RemoveChild(FileNode child)
        {
            if (child.Parent != this)
            {
                throw new ArgumentException("Child is invalid.");
            }

            if (child.NextSibling != null)
            {
                child.NextSibling.PreviousSibling = child.PreviousSibling;
            }
            else
            {
                LastChild = child.PreviousSibling;
            }
            if (child.PreviousSibling != null)
            {
                child.PreviousSibling.NextSibling = child.NextSibling;
            }
            else
            {
                FirstChild = child.NextSibling;
            }

            child.Parent = null;
            child.NextSibling = null;
            child.PreviousSibling = null;
            ChildCount--;
        }

        public void Remove()
        {
            Parent?.RemoveChild(this);
        }

        public bool IsAncestorOf(FileNode other)
        {
            for (var node = other.Parent; node != null; node = node.Parent)
            {
                if (node == this)
                {
                    return true;
                }
            }
            return false;
        }

        public FileNode? GetNextInList()
        {
            if (FirstChild != null)
            {
                return FirstChild;
            }
            for (var node = this; node != null; node = node.Parent)
            {
                if (node.NextSibling != null)
                {
                    return node.NextSibling;
                }
            }
            return null;
        }

        public FileNode? GetPreviousInList()
        {
            if (PreviousSibling != null)
            {
                return PreviousSibling.GetLastInList();
            }
            return Parent;
        }

        public FileNode GetLastInList()
        {
            var node = this;
            while (node.LastChild != null)
            {
                node = node.LastChild;
            }
            return node;
        }

        public FileNode? FindThisOrNextPlayable()
        {
            for (var node = this; ; node = node.GetNextInList())
            {
                if (node == null || node.Playable)
                {
                    return node;
                }
            }
        }

        public FileNode? FindThisOrPreviousPlayable()
        {
            for (var node = this; ; node = node.GetPreviousInList())
            {
                if (node == null || node.Playable)
                {
                    return node;
                }
            }
        }

        private FileNode? GetFirstRegularChild()
        {
            var child = FirstChild;
            for (; child != null && child.Type == FileType.DirectoryUp; child = child.NextSibling) ;
            return child;
        }

        public FileNode? FindChild()
        {
            var child = GetFirstRegularChild();
            if (child == null)
            {
                return null;
            }
            for (; ; )
            {
                if (child.Playable || child.NextSibling != null)
                {
                    break;
                }
                var childChild = child.GetFirstRegularChild();
                if (childChild == null)
                {
                    break;
                }
                child = childChild;
            }
            return child;
        }

        public FileNode? FindParent()
        {
            var parent = Parent;
            if (parent == null)
            {
                return null;
            }
            for (; ; )
            {
                if (parent.Parent == null || parent.NextSibling != null || parent != parent.Parent.GetFirstRegularChild())
                {
                    break;
                }
                parent = parent.Parent;
            }
            return parent;
        }
    }
}

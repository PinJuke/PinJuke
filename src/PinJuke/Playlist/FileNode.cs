﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Playlist
{
    public enum FileType
    {
        Directory = 0,
        Music = 1,
        Video = 2,
        M3u = 3,
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

        public bool Playable => Type == FileType.Music || Type == FileType.Video;

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
        }

        public void Remove()
        {
            Parent?.RemoveChild(this);
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

        public FileNode? GetNextPlayableInList()
        {
            for (var node = GetNextInList(); ; node = node.GetNextInList())
            {
                if (node == null || node.Playable)
                {
                    return node;
                }
            }
        }

        public FileNode? GetPreviousPlayableInList()
        {
            for (var node = GetPreviousInList(); ; node = node.GetPreviousInList())
            {
                if (node == null || node.Playable)
                {
                    return node;
                }
            }
        }
    }
}

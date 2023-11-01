﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class ProjectMPlaylistException : Exception
    {
        public ProjectMPlaylistException(string message) : base(message)
        {
        }
    }

    public class ProjectMPlaylist : IDisposable
    {
        private nint handle = nint.Zero;
        internal nint Handle
        {
            get => handle != nint.Zero ? handle : throw new ProjectMPlaylistException("ProjectM playlist handle is null.");
        }

        private ProjectMRenderer? projectMRenderer = null;

        public ProjectMPlaylist()
        {
            handle = LibProjectMPlaylist.Create(nint.Zero);
            if (handle == nint.Zero)
            {
                throw new ProjectMPlaylistException("Failed to create a projectM playlist instance.");
            }
        }

        ~ProjectMPlaylist() => Release();

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        internal void Release()
        {
            if (handle != nint.Zero)
            {
                Disconnect();

                LibProjectMPlaylist.Destroy(handle);
                handle = nint.Zero;
            }
        }

        public void Connect(ProjectMRenderer projectMRenderer)
        {
            if (projectMRenderer.Playlist != null)
            {
                projectMRenderer.Playlist.Disconnect();
            }
            Debug.Assert(projectMRenderer.Playlist == null);
            LibProjectMPlaylist.Connect(Handle, projectMRenderer.Handle);
            projectMRenderer.Playlist = this;
            this.projectMRenderer = projectMRenderer;
        }

        public void Disconnect()
        {
            if (projectMRenderer == null)
            {
                return;
            }
            Debug.Assert(projectMRenderer.Playlist == this);
            LibProjectMPlaylist.Connect(Handle, nint.Zero);
            projectMRenderer.Playlist = null;
            projectMRenderer = null;
        }

        public void AddPath(string path, bool recurseSubdirs, bool allowDuplicates)
        {
            LibProjectMPlaylist.AddPath(Handle, path, recurseSubdirs, allowDuplicates);
        }

        public void SetShuffle(bool shuffle)
        {
            LibProjectMPlaylist.SetShuffle(Handle, shuffle);
        }

        public void PlayNext(bool hardCut)
        {
            LibProjectMPlaylist.PlayNext(Handle, hardCut);
        }
    }
}

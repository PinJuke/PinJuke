﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class LibProjectMPlaylist
    {
        private const string DLL = "projectM-4-playlist.dll";

        [DllImport(DLL, EntryPoint = "projectm_playlist_create")]
        public static extern nuint Create(nuint projectMHandle);

        [DllImport(DLL, EntryPoint = "projectm_playlist_destroy")]
        public static extern void Destroy(nuint projectMPlaylistHandle);

        [DllImport(DLL, EntryPoint = "projectm_playlist_connect")]
        public static extern void Connect(nuint projectMPlaylistHandle, nuint projectMHandle);

        [DllImport(DLL, EntryPoint = "projectm_playlist_add_path")]
        public static extern uint AddPath(nuint projectMPlaylistHandle, string path, bool recurseSubdirs, bool allowDuplicates);

        [DllImport(DLL, EntryPoint = "projectm_playlist_set_shuffle")]
        public static extern void SetShuffle(nuint projectMPlaylistHandle, bool shuffle);

        [DllImport(DLL, EntryPoint = "projectm_playlist_play_next")]
        public static extern uint PlayNext(nuint projectMPlaylistHandle, bool hardCut);

        [DllImport(DLL, EntryPoint = "projectm_playlist_play_previous")]
        public static extern uint PlayPrevious(nuint projectMPlaylistHandle, bool hardCut);

        [DllImport(DLL, EntryPoint = "projectm_playlist_play_last")]
        public static extern uint PlayLast(nuint projectMPlaylistHandle, bool hardCut);

        [DllImport(DLL, EntryPoint = "projectm_playlist_item")]
        public static extern nuint Item(nuint projectMPlaylistHandle, uint index);

        [DllImport(DLL, EntryPoint = "projectm_playlist_free_string")]
        public static extern void FreeString(nuint pString);

        [DllImport(DLL, EntryPoint = "projectm_playlist_get_position")]
        public static extern uint GetPosition(nuint projectMPlaylistHandle);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class LibProjectMPlaylist
    {
        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_create")]
        public static extern nuint Create(nuint projectMHandle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_destroy")]
        public static extern void Destroy(nuint projectMPlaylistHandle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_connect")]
        public static extern void Connect(nuint projectMPlaylistHandle, nuint projectMHandle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_add_path")]
        public static extern uint AddPath(nuint projectMPlaylistHandle, string path, bool recurseSubdirs, bool allowDuplicates);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_set_shuffle")]
        public static extern void SetShuffle(nuint projectMPlaylistHandle, bool shuffle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_play_next")]
        public static extern uint PlayNext(nuint projectMPlaylistHandle, bool hardCut);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_item")]
        unsafe public static extern byte* Item(nuint projectMPlaylistHandle, uint index);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_free_string")]
        unsafe public static extern void FreeString(byte* str);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_get_position")]
        public static extern uint GetPosition(nuint projectMPlaylistHandle);
    }
}

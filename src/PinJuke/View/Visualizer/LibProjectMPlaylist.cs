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
        public static extern nint Create(nint projectMHandle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_destroy")]
        public static extern nint Destroy(nint projectMPlaylistHandle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_connect")]
        public static extern void Connect(nint projectMPlaylistHandle, nint projectMHandle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_add_path")]
        public static extern uint AddPath(nint projectMPlaylistHandle, string path, bool recurseSubdirs, bool allowDuplicates);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_set_shuffle")]
        public static extern uint SetShuffle(nint projectMPlaylistHandle, bool shuffle);

        [DllImport("projectM-4-playlist.dll", EntryPoint = "projectm_playlist_play_next")]
        public static extern uint PlayNext(nint projectMPlaylistHandle, bool hardCut);
    }
}

using PinJuke.Audio;
using PinJuke.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class Visualizer : IDisposable
    {
        private ProjectMRenderer projectMRenderer;
        private ProjectMPlaylist projectMPlaylist;
        private AudioManager? audioManager = null;
        private bool initialized = false;

        public Visualizer()
        {
            try
            {
                projectMRenderer = new();
                projectMPlaylist = new();
                projectMPlaylist.Connect(projectMRenderer);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (projectMRenderer != null)
            {
                if (projectMPlaylist != null)
                {
                    if (audioManager != null)
                    {
                        audioManager.RemovePcmDataListener(projectMRenderer);
                    }
                    projectMPlaylist.Dispose();
                }
                projectMRenderer.Dispose();
            }
        }

        public void Initialize(AudioManager audioManager, Milkdrop milkdrop)
        {
            if (initialized)
            {
                throw new InvalidOperationException("Visualizer is already initialized.");
            }
            
            projectMRenderer.SetTextureSearchPaths([milkdrop.TexturesPath]);
            projectMRenderer.SetPresetDuration(60);
            projectMPlaylist.AddPath(milkdrop.PresetsPath, true, false);
            projectMPlaylist.SetShuffle(true);
            projectMPlaylist.PlayNext(true);

            audioManager.AddPcmDataListener(projectMRenderer);
            this.audioManager = audioManager;
            initialized = true;
        }

        public void Render(int framebuffer)
        {
            projectMRenderer.Render(framebuffer);
        }

        public void SetSize(int width, int height)
        {
            projectMRenderer.SetSize(width, height);
        }

        public string? GetCurrentItem()
        {
            return projectMPlaylist.GetCurrentItem();
        }

        public void PlayNext(bool hardCut)
        {
            projectMPlaylist.PlayNext(hardCut);
        }

        public void PlayLast(bool hardCut)
        {
            projectMPlaylist.PlayLast(hardCut);
        }
    }
}

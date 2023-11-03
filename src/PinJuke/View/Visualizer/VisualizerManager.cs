using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using PinJuke.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PinJuke.View.Visualizer
{
    public class NotificationClient : IMMNotificationClient
    {
        private readonly VisualizerManager visualizerManager;

        public NotificationClient(VisualizerManager visualizerManager)
        {
            this.visualizerManager = visualizerManager;
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
        }

        public void OnDeviceRemoved(string deviceId)
        {
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow == DataFlow.Render)
            {
                visualizerManager.QueueRestart();
            }
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }
    }

    public class VisualizerManager : IDisposable
    {
        public Milkdrop Milkdrop { get; }

        private readonly MMDeviceEnumerator deviceEnumerator;
        private readonly NotificationClient notificationClient;

        private WasapiLoopbackCapture? wasapiLoopbackCapture = null;
        private readonly HashSet<ProjectMRenderer> renderers = new();

        public VisualizerManager(Milkdrop milkdrop)
        {
            Milkdrop = milkdrop;

            deviceEnumerator = new MMDeviceEnumerator();
            notificationClient = new NotificationClient(this);
            deviceEnumerator.RegisterEndpointNotificationCallback(notificationClient);

            CheckRestart();
        }

        public void Dispose()
        {
            wasapiLoopbackCapture?.Dispose();
        }

        public void Add(ProjectMRenderer projectMRenderer)
        {
            renderers.Add(projectMRenderer);
        }

        public void Remove(ProjectMRenderer projectMRenderer)
        {
            renderers.Remove(projectMRenderer);
        }

        public void QueueRestart()
        {
            Debug.WriteLine("VisualizerManager: Queue restart...");
            Application.Current.Dispatcher.Invoke(new Action(CheckRestart));
        }

        private void CheckRestart()
        {
            if (wasapiLoopbackCapture != null && wasapiLoopbackCapture.CaptureState != CaptureState.Stopped)
            {
                Debug.WriteLine("VisualizerManager: No need to restart.");
                return;
            }

            Debug.WriteLine("VisualizerManager: Restarting...");

            wasapiLoopbackCapture?.Dispose();
            wasapiLoopbackCapture = null;

            wasapiLoopbackCapture = new WasapiLoopbackCapture(deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));

            wasapiLoopbackCapture.DataAvailable += WasapiLoopbackCapture_DataAvailable;
            wasapiLoopbackCapture.RecordingStopped += WasapiLoopbackCapture_RecordingStopped;
            wasapiLoopbackCapture.StartRecording();
        }

        private void WasapiLoopbackCapture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            var numBytes = e.BytesRecorded;

            var channels = (uint)wasapiLoopbackCapture!.WaveFormat.Channels;
            var count = (uint)numBytes / sizeof(float) / channels;

            byte[] buffer = new byte[numBytes];
            Array.Copy(e.Buffer, buffer, numBytes);

            //Debug.WriteLine("VisualizerManager: data available: " + count + " samples " + wasapiLoopbackCapture!.WaveFormat);

            Application.Current.Dispatcher.Invoke(new Action(() => {
                PcmAddFloat(buffer, count, channels);
            }));
        }

        private void WasapiLoopbackCapture_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            Debug.WriteLine("VisualizerManager: Recording stopped.");
            QueueRestart();
        }

        private void PcmAddFloat(byte[] buffer, uint count, uint channels)
        {
            foreach (var renderer in renderers)
            {
                renderer.PcmAddFloat(buffer, count, channels);
            }
        }

    }
}

using NAudio.CoreAudioApi.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.Audio.OpenAL;

namespace PinJuke.Audio
{
    public interface PcmDataListener
    {
        public void OnPcmData(byte[] samples, uint count, uint channels);
    }

    public class NotificationClient : IMMNotificationClient
    {
        private readonly AudioManager audioManager;

        public NotificationClient(AudioManager audioManager)
        {
            this.audioManager = audioManager;
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
                audioManager.QueueRestart();
            }
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }
    }

    public class AudioManager : IDisposable
    {
        private readonly HashSet<PcmDataListener> pcmDataListeners = new();

        private readonly MMDeviceEnumerator deviceEnumerator;
        private readonly NotificationClient notificationClient;

        private MMDevice? device = null;
        private WasapiLoopbackCapture? wasapiLoopbackCapture = null;
        private bool restarting = false;

        public AudioManager()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            notificationClient = new NotificationClient(this);
            deviceEnumerator.RegisterEndpointNotificationCallback(notificationClient);

            CheckRestart();
        }

        public void Dispose()
        {
            Close();
        }

        public void AddPcmDataListener(PcmDataListener pcmDataListener)
        {
            pcmDataListeners.Add(pcmDataListener);
        }

        public void RemovePcmDataListener(PcmDataListener pcmDataListener)
        {
            pcmDataListeners.Remove(pcmDataListener);
        }

        public float? AddVolumeLevel(float delta)
        {
            if (device == null)
            {
                return null;
            }
            float volumeLevel = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            volumeLevel = Math.Max(0f, Math.Min(1f, volumeLevel + delta));
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volumeLevel;
            return volumeLevel;
        }

        private void Close()
        {
            wasapiLoopbackCapture?.Dispose();
            wasapiLoopbackCapture = null;
            device?.Dispose();
            device = null;
        }

        private void Open()
        {
            try
            {
                device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
            catch (COMException exception)
            {
                Debug.WriteLine("AudioManager: No device.");
                return;
            }

            Debug.WriteLine("AudioManager: Capturing...");
            wasapiLoopbackCapture = new WasapiLoopbackCapture(device);
            wasapiLoopbackCapture.DataAvailable += WasapiLoopbackCapture_DataAvailable;
            wasapiLoopbackCapture.RecordingStopped += WasapiLoopbackCapture_RecordingStopped;
            wasapiLoopbackCapture.StartRecording();
        }

        public void QueueRestart()
        {
            Debug.WriteLine("AudioManager: Queue restart...");
            Application.Current.Dispatcher.InvokeAsync(new Action(CheckRestart));
        }

        private async void CheckRestart()
        {
            if (restarting)
            {
                return;
            }

            restarting = true;

            try
            {
                Close();
                await Task.Delay(100);
                Open();
            }
            finally
            {
                restarting = false;
            }
        }

        private void WasapiLoopbackCapture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            var numBytes = e.BytesRecorded;

            var channels = (uint)wasapiLoopbackCapture!.WaveFormat.Channels;
            var count = (uint)numBytes / sizeof(float) / channels;

            byte[] buffer = new byte[numBytes];
            Array.Copy(e.Buffer, buffer, numBytes);

            //Debug.WriteLine("AudioManager: data available: " + count + " samples " + wasapiLoopbackCapture!.WaveFormat);

            Application.Current.Dispatcher.InvokeAsync(new Action(() => {
                OnPcmData(buffer, count, channels);
            }));
        }

        private void WasapiLoopbackCapture_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            Debug.WriteLine("AudioManager: Recording stopped.");
            QueueRestart();
        }

        private void OnPcmData(byte[] samples, uint count, uint channels)
        {
            foreach (var pcmListener in pcmDataListeners)
            {
                pcmListener.OnPcmData(samples, count, channels);
            }
        }

    }
}

using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unosquare.FFME.Common;

namespace PinJuke.View.Media
{
    /// <summary>
    /// Based on https://github.com/unosquare/ffmediaelement/blob/v3.50.0/Unosquare.FFME.Windows.Sample/Foundation/FileInputStream.cs
    /// </summary>
    public unsafe class MediaInputStream : IMediaInputStream
    {
        private readonly Stream stream;
        private readonly bool dontDispose;
        private readonly object readLock = new object();
        private readonly byte[] readBuffer;

        public Uri StreamUri { get; }

        public bool CanSeek => true;

        public int ReadBufferLength => 1024 * 16;

        public InputStreamInitializing? OnInitializing { get; }

        public InputStreamInitialized? OnInitialized { get; }

        public MediaInputStream(Stream stream, Uri streamUri, bool dontDispose = false)
        {
            this.stream = stream;
            this.StreamUri = streamUri;
            this.dontDispose = dontDispose;
            readBuffer = new byte[ReadBufferLength];
        }

        public void Dispose()
        {
            if (dontDispose)
            {
                return;
            }
            stream.Dispose();
        }

        public void Reset()
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        public unsafe int Read(void* opaque, byte* targetBuffer, int targetBufferLength)
        {
            lock (readLock)
            {
                try
                {
                    var readCount = stream.Read(readBuffer, 0, readBuffer.Length);
                    if (readCount > 0)
                        Marshal.Copy(readBuffer, 0, (IntPtr)targetBuffer, readCount);

                    return readCount;
                }
                catch (Exception)
                {
                    return ffmpeg.AVERROR_EOF;
                }
            }
        }

        public unsafe long Seek(void* opaque, long offset, int whence)
        {
            lock (readLock)
            {
                try
                {
                    return whence == ffmpeg.AVSEEK_SIZE ?
                        stream.Length : stream.Seek(offset, SeekOrigin.Begin);
                }
                catch
                {
                    return ffmpeg.AVERROR_EOF;
                }
            }
        }
    }
}

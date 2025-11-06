using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Model
{
    public class MediaEventToken<T>
    {
        private readonly MediaEventArgs<T> mediaEvent;

        public MediaEventToken(MediaEventArgs<T> mediaEvent)
        {
            this.mediaEvent = mediaEvent;
        }

        public void Continue()
        {
            mediaEvent.ContinueFromToken(this);
        }
    }

    public enum MediaEventType
    {
        Play,
        End,
    }

    public class MediaEventArgs<T>
    {
        public MediaEventType Type { get; }
        private readonly Action<T> continueWith;
        private readonly HashSet<MediaEventToken<T>> tokens = new();

        public bool Intercepted
        {
            get => tokens.Count != 0;
        }

        public T Data { get; }

        public MediaEventArgs(MediaEventType type, Action<T> continueWith, T data)
        {
            this.Type = type;
            this.continueWith = continueWith;
            this.Data = data;
        }

        public MediaEventToken<T> Intercept()
        {
            var token = new MediaEventToken<T>(this);
            tokens.Add(token);
            return token;
        }

        public void ContinueFromToken(MediaEventToken<T> token)
        {
            tokens.Remove(token);
            ContinueIfNotIntercepted();
        }

        public void ContinueIfNotIntercepted()
        {
            if (!Intercepted)
            {
                continueWith(Data);
            }
        }
    }
}

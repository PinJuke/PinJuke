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

    // Legacy event classes for backwards compatibility
    public class MediaEventToken
    {
        private readonly MediaEventArgs mediaEvent;

        public MediaEventToken(MediaEventArgs mediaEvent)
        {
            this.mediaEvent = mediaEvent;
        }

        public void Continue()
        {
            mediaEvent.ContinueFromToken(this);
        }
    }

    public abstract class MediaEventArgs
    {
        private readonly Action<PlayFileType> continueWith;
        private readonly HashSet<MediaEventToken> tokens = new();

        public bool Intercepted
        {
            get => tokens.Count != 0;
        }

        public PlayFileType Type { get; }

        public MediaEventArgs(Action<PlayFileType> continueWith, PlayFileType type)
        {
            this.continueWith = continueWith;
            this.Type = type;
        }

        public MediaEventToken Intercept()
        {
            var token = new MediaEventToken(this);
            tokens.Add(token);
            return token;
        }

        public void ContinueFromToken(MediaEventToken token)
        {
            tokens.Remove(token);
            ContinueIfNotIntercepted();
        }

        public void ContinueIfNotIntercepted()
        {
            if (!Intercepted)
            {
                continueWith(Type);
            }
        }
    }

    public class PlayMediaEventArgs : MediaEventArgs
    {
        public PlayMediaEventArgs(Action<PlayFileType> continueWith, PlayFileType type) : base(continueWith, type)
        {
        }
    }

    public class EndMediaEventArgs : MediaEventArgs
    {
        public EndMediaEventArgs(Action<PlayFileType> continueWith, PlayFileType type) : base(continueWith, type)
        {
        }
    }
}

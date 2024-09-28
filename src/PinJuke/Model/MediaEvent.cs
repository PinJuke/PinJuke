using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Model
{
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
        private readonly Action<MediaEventArgs> continueWith;
        private readonly HashSet<MediaEventToken> tokens = new();

        public bool Intercepted
        {
            get => tokens.Count != 0;
        }

        public MediaEventArgs(Action<MediaEventArgs> continueWith)
        {
            this.continueWith = continueWith;
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
                continueWith(this);
            }
        }
    }

    public class PlayMediaEventArgs : MediaEventArgs
    {
        public PlayMediaEventArgs(Action<MediaEventArgs> continueWith) : base(continueWith)
        {
        }
    }

    public class EndMediaEventArgs : MediaEventArgs
    {
        public EndMediaEventArgs(Action<MediaEventArgs> continueWith) : base(continueWith)
        {
        }
    }
}

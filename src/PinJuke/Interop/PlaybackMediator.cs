using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PinJuke.Interop
{

    [Flags]
    public enum EXECUTION_STATE : uint
    {
        ES_SYSTEM_REQUIRED = 0x00000001,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
    }

    public class PlaybackMediator : IDisposable
    {
        private readonly MainModel mainModel;
        private bool disposed = false;

        public PlaybackMediator(MainModel mainModel)
        {
            this.mainModel = mainModel;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
        }

        [DllImport("kernel32.dll")]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);


        public void Startup()
        {
            mainModel.PropertyChanged += MainModel_PropertyChanged;
            Update();
        }

        private void MainModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.Playing):
                    Update();
                    break;
            }
        }

        private void Update()
        {
            var playing = mainModel.Playing;
            var flags = playing
                ? EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED
                : EXECUTION_STATE.ES_CONTINUOUS;
            SetThreadExecutionState(flags);
        }
    }
}

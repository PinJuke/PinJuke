using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Onboarding
{
    public class Model : IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool licenseAccepted = false;
        public bool LicenseAccepted
        {
            get => licenseAccepted;
            set => this.SetField(ref licenseAccepted, value);
        }
        public Display PlayFieldDisplay { get; } = new();
        public Display BackGlassDisplay { get; } = new();
        public Display DmdDisplay { get; } = new();

        private bool dmdEnabled = true;
        public bool DmdEnabled
        {
            get => dmdEnabled;
            set => this.SetField(ref dmdEnabled, value);
        }

        private bool dofEnabled = true;
        public bool DofEnabled
        {
            get => dofEnabled;
            set => this.SetField(ref dofEnabled, value);
        }

        private string dofPath = "";
        public string DofPath
        {
            get => dofPath;
            set => this.SetField(ref dofPath, value);
        }

        private bool dofPathInvalid = false;
        public bool DofPathInvalid
        {
            get => dofPathInvalid;
            set => this.SetField(ref dofPathInvalid, value);
        }

        private bool? consent = null;
        public bool? Consent
        {
            get => consent;
            set => this.SetField(ref consent, value);
        }

        private bool updateCheckEnabled = true;
        public bool UpdateCheckEnabled
        {
            get => updateCheckEnabled;
            set => this.SetField(ref updateCheckEnabled, value);
        }

        private bool createPlaylist = true;
        public bool CreatePlaylist
        {
            get => createPlaylist;
            set => this.SetField(ref createPlaylist, value);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }
    }

    public class Display : IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int left = 0;
        public int Left
        {
            get => left;
            set => this.SetField(ref left, value);
        }

        private int top = 0;
        public int Top
        {
            get => top;
            set => this.SetField(ref top, value);
        }

        private int width = 0;
        public int Width
        {
            get => width;
            set => this.SetField(ref width, value);
        }

        private int height = 0;
        public int Height
        {
            get => height;
            set => this.SetField(ref height, value);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }
    }
}

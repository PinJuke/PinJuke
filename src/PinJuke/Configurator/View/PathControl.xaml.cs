using PinJuke.View;
using System.IO;
using System.Windows;
using System.Windows.Media;


namespace PinJuke.Configurator.View
{
    public partial class PathControl : ConfiguratorControl
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (this.SetField(ref enabled, value))
                {
                    NotifyPropertyChanged(nameof(RelativeCheckBoxEnabled));
                }
            }
        }

        private bool emptyEnabled = false;
        public bool EmptyEnabled
        {
            get => emptyEnabled;
            set => this.SetField(ref emptyEnabled, value);
        }

        private bool relativeEnabled = true;
        public bool RelativeEnabled
        {
            get => relativeEnabled;
            set
            {
                var changed = this.SetField(ref relativeEnabled, value);
                SetPath(Path);
                if (changed)
                {
                    NotifyPropertyChanged(nameof(RelativeCheckBoxEnabled));
                }
            }
        }

        public bool RelativeCheckBoxEnabled
        {
            get => Enabled && RelativeEnabled;
        }

        private int inputWidth = 200;
        public int InputWidth
        {
            get => inputWidth;
            set => this.SetField(ref inputWidth, value);
        }

        private bool relativeDefault = true;
        public bool RelativeDefault
        {
            get => relativeDefault;
            set
            {
                this.SetField(ref relativeDefault, value);
                SetPath(Path);
            }
        }

        private bool fileMode = false;
        public bool FileMode
        {
            get => fileMode;
            set
            {
                if (this.SetField(ref fileMode, value))
                {
                    NotifyPropertyChanged(nameof(ChooseText));
                }
            }
        }

        public string FileExtension { get; set; } = "";
        public string FileFilter { get; set; } = "";

        public MediaPathProvider? MediaPathProvider { get; set; } = null;

        private string RelativeRootDir
        {
            get
            {
                return MediaPathProvider?.GetMediaPath() ?? Directory.GetCurrentDirectory();
            }
        }

        private string path = "";
        public string Path
        {
            get => path;
            set
            {
                if (this.SetField(ref path, value))
                {
                    Relative = !System.IO.Path.IsPathRooted(path);
                    NotifyPropertyChanged(nameof(PathInvalid));
                }
            }
        }

        public string FullPath
        {
            get
            {
                return System.IO.Path.GetFullPath(Path, RelativeRootDir);
            }
        }

        private bool relative = false;
        public bool Relative
        {
            get => relative;
            set
            {
                if (this.SetField(ref relative, value))
                {
                    SetPath(Path);
                }
            }
        }

        public bool PathInvalid
        {
            get
            {
                return !Path.IsNullOrEmpty()
                    && (FileMode ? !File.Exists(FullPath) : !Directory.Exists(FullPath));
            }
        }

        public string ChooseText
        {
            get => FileMode ? Strings.ChooseFile : Strings.ChooseDirectory;
        }

        public string RelativePathText
        {
            get => Strings.RelativePath;
        }

        public ImageSource WarningImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\warning-outline.svg");
        }


        public PathControl()
        {
            InitializeComponent();

            // Update relative state.
            SetPath("");
        }

        private void ConfiguratorControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(PathInvalid));
        }

        private void SetPath(string path)
        {
            var relative = Relative;
            var relativeRootDir = RelativeRootDir;
            if (path.IsNullOrEmpty())
            {
                relative = relativeEnabled ? relativeDefault : false;
            }
            else if (System.IO.Path.IsPathRooted(path))
            {
                if (relative)
                {
                    var relativePath = System.IO.Path.GetRelativePath(relativeRootDir, path);
                    relative = relativePath != path;
                    path = relativePath;
                }
            }
            else
            {
                if (!relative)
                {
                    if (!path.IsNullOrEmpty())
                    {
                        path = System.IO.Path.GetFullPath(path, relativeRootDir);
                    }
                }
            }
            Path = path;
            Relative = relative;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            if (FileMode)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.DefaultExt = FileExtension; // Default file extension
                dialog.Filter = FileFilter; // Filter files by extension
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(FullPath) ?? "";
                bool? result = dialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                SetPath(dialog.FileName);
            }
            else
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog();
                dialog.Multiselect = false;
                dialog.InitialDirectory = FullPath;
                bool? result = dialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                SetPath(dialog.FolderName);
            }
        }
    }
}

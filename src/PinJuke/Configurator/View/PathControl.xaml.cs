using System.IO;
using System.Windows;


namespace PinJuke.Configurator.View
{
    public partial class PathControl : ConfiguratorControl
    {
        private bool emptyEnabled = false;
        public bool EmptyEnabled
        {
            get => emptyEnabled;
            set => SetField(ref emptyEnabled, value);
        }

        private bool relativeEnabled = false;
        public bool RelativeEnabled
        {
            get => relativeEnabled;
            set => SetField(ref relativeEnabled, value);
        }

        private bool fileMode = false;
        public bool FileMode
        {
            get => fileMode;
            set
            {
                if (SetField(ref fileMode, value))
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
                if (SetField(ref path, value))
                {
                    Relative = !System.IO.Path.IsPathRooted(path);
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
                if (SetField(ref relative, value))
                {
                    SetPath(Path);
                }
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


        public PathControl()
        {
            InitializeComponent();
        }

        private void SetPath(string path)
        {
            var relative = Relative;
            var relativeRootDir = RelativeRootDir;
            if (System.IO.Path.IsPathRooted(path))
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

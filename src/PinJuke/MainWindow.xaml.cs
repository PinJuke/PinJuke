using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainModel mainModel;
        private Configuration.Display displayConfig;

        public MainWindow(MainModel mainModel, Configuration.Display displayConfig)
        {
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;

            InitializeComponent();

            Title = displayConfig.Role.ToString();
            Left = displayConfig.Window.Left;
            Top = displayConfig.Window.Top;
            Width = displayConfig.Window.Width;
            Height = displayConfig.Window.Height;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Title + " loaded.");
            Play();
            mainModel.ShutdownEvent += MainModel_ShutdownEvent;
            mainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " closed.");
            mainModel.ShutdownEvent -= MainModel_ShutdownEvent;
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
        }

        private void MainModel_ShutdownEvent(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " received shotdown event.");
            Close();
        }

        private void MainModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.NavigationNode):
                    Browser1.SetFileNode(mainModel.NavigationNode);
                    break;
            }
        }

        private async void Play()
        {
            await Media.Open(new Uri(@"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
        }

    }
}

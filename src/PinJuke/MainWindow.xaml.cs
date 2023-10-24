using PinJuke.Model;
using System;
using System.Collections.Generic;
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
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Debug.WriteLine(e.Key);

            base.OnKeyDown(e);
        }
    }
}

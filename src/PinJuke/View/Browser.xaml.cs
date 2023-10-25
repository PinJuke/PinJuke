using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

namespace PinJuke.View
{
    /// <summary>
    /// Interaction logic for Browser.xaml
    /// </summary>
    public partial class Browser : UserControl
    {
        // TODO: Use Data Binding...
        public List<string> Files {
            set
            {
                ItemsControl1.ItemsSource = value;
            }
        }

        public Browser()
        {
            InitializeComponent();
        }

        public void SetFileNode(FileNode? fileNode)
        {
            List<string> files = new();
            if (fileNode == null)
            {
                Files = files;
                return;
            }

            var morePreviousFileNodes = false;
            var moreNextFileNodes = false;
            var count = 1;
            var startFileNode = fileNode;
            var endFileNode = fileNode;

            for (var i = 0; ; ++i)
            {
                morePreviousFileNodes = startFileNode.PreviousSibling != null;
                if (i >= 3 || !morePreviousFileNodes)
                {
                    break;
                }
                startFileNode = startFileNode.PreviousSibling!;
                count++;
            }
            
            for (var i = 0; ; ++i)
            {
                moreNextFileNodes = endFileNode.NextSibling != null;
                if (i >= 3 || !moreNextFileNodes)
                {
                    break;
                }
                endFileNode = endFileNode.NextSibling!;
                count++;
            }

            if (morePreviousFileNodes)
            {
                files.Add("...");
            }
            for (var itemFileNode = startFileNode; itemFileNode != endFileNode.NextSibling; itemFileNode = itemFileNode.NextSibling!)
            {
                files.Add((itemFileNode == fileNode ? ">>" : "") + itemFileNode.DisplayName);
            }
            if (moreNextFileNodes)
            {
                files.Add("...");
            }
            Files = files;
        }
    }
}

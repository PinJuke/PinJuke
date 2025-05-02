using PinJuke.Configurator.Factory;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PinJuke.Configurator.View
{
    public class IniDocumentTabItem : TabItem
    {
        public GroupControlFactory GroupControlFactory { get; }
        public string FilePath { get; }
        public string TemplateFilePath { get; }

        protected IniDocument? IniDocument { get; set; } = null;
        public GroupControl GroupControl { get; private set; }

        public IniDocumentTabItem(GroupControlFactory groupControlFactory, string filePath, string templateFilePath)
        {
            GroupControlFactory = groupControlFactory;
            FilePath = filePath;
            TemplateFilePath = templateFilePath;

            var header = new TextBlock();
            header.Text = Path.GetFileName(filePath);
            header.ToolTip = filePath;
            Header = header;

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = scrollViewer;

            GroupControl = groupControlFactory.CreateControl();
            GroupControl.Margin = new Thickness(10, 0, 10, 10);
            scrollViewer.Content = GroupControl;
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            if (IniDocument == null)
            {
                LoadIniFile();
            }
            
            base.OnSelected(e);
        }

        protected void LoadIniFile()
        {
            IniDocument = IniReader.TryRead(FilePath)
                ?? IniReader.TryRead(TemplateFilePath)
                ?? new IniDocument();
            GroupControlFactory.WriteToControl(GroupControl, IniDocument);

        }

        public void SaveToFile()
        {
            if (IniDocument == null)
            {
                return;
            }
            GroupControlFactory.ReadFromControl(GroupControl, IniDocument);
            using var textWriter = new StreamWriter(FilePath);
            IniDocument.WriteTo(textWriter);
        }
    }
}

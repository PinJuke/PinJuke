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
        protected GroupControl GroupControl { get; }

        public IniDocumentTabItem(GroupControlFactory groupControlFactory, string filePath, string templateFilePath)
        {
            GroupControlFactory = groupControlFactory;
            FilePath = filePath;
            TemplateFilePath = templateFilePath;

            Header = Path.GetFileName(filePath);
            ToolTip = filePath;

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = scrollViewer;

            GroupControl = groupControlFactory.CreateControl();
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
            IniDocument = ReadIni(FilePath)
                ?? ReadIni(TemplateFilePath)
                ?? new IniDocument();
            GroupControlFactory.WriteToControl(GroupControl, IniDocument);

        }

        protected IniDocument? ReadIni(string filePath)
        {
            var iniReader = new IniReader();
            try
            {
                return iniReader.Read(filePath);
            }
            catch (IniIoException)
            {
                return null;
            }
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

using PinJuke.Configuration;
using PinJuke.Configurator;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;

namespace PinJuke.Configurator.Factory
{
    public class ControllerSelectConverter : BaseConverter<SelectControl>
    {
        public ControllerSelectConverter(Parser parser, string sectionName, string entryName) 
            : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(SelectControl control, IniDocument iniDocument)
        {
            var buttonNumber = control.SelectedIndex;
            iniDocument[SectionName][EntryName] = Parser.FormatInt(buttonNumber);
        }

        public override void WriteToControl(SelectControl control, IniDocument iniDocument)
        {
            var buttonNumber = Parser.ParseInt(iniDocument[SectionName][EntryName]) ?? 0;
            control.SelectedIndex = buttonNumber;
        }
    }
}

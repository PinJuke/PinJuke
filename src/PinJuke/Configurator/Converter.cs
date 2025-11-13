using PinJuke.Configuration;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configurator
{
    public interface Converter<in T>
    {
        void ReadFromControl(T control, IniDocument iniDocument);
        void WriteToControl(T control, IniDocument iniDocument);
    }

    public abstract class BaseConverter<T> : Converter<T>
    {
        public Parser Parser { get; }
        public string SectionName { get; }
        public string EntryName { get; }

        public BaseConverter(Parser parser, string sectionName, string entryName)
        {
            Parser = parser;
            SectionName = sectionName;
            EntryName = entryName;
        }

        public abstract void ReadFromControl(T control, IniDocument iniDocument);
        public abstract void WriteToControl(T control, IniDocument iniDocument);
    }

    public class PathConverter : BaseConverter<PathControl>
    {
        public PathConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(PathControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = Parser.FormatString(control.Path);
        }

        public override void WriteToControl(PathControl control, IniDocument iniDocument)
        {
            control.Path = Parser.ParseString(iniDocument[SectionName][EntryName]) ?? "";
        }
    }

    public class BoolConverter : BaseConverter<BoolControl>
    {
        public BoolConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(BoolControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = Parser.FormatBool(control.Value);
        }

        public override void WriteToControl(BoolControl control, IniDocument iniDocument)
        {
            control.Value = Parser.ParseBool(iniDocument[SectionName][EntryName]) ?? false;
        }
    }

    public class StringConverter : BaseConverter<TextControl>
    {
        public StringConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(TextControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = Parser.FormatString(control.Value);
        }

        public override void WriteToControl(TextControl control, IniDocument iniDocument)
        {
            control.Value = Parser.ParseString(iniDocument[SectionName][EntryName]) ?? "";
        }
    }

    public class IntNumberConverter : BaseConverter<NumberControl>
    {
        public IntNumberConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(NumberControl control, IniDocument iniDocument)
        {
            int? i = control.Value == null ? null : (int)Math.Round(control.Value.Value);
            iniDocument[SectionName][EntryName] = Parser.FormatInt(i);
        }

        public override void WriteToControl(NumberControl control, IniDocument iniDocument)
        {
            control.Value = Parser.ParseInt(iniDocument[SectionName][EntryName]);
        }
    }

    public class FloatNumberConverter : BaseConverter<NumberControl>
    {
        public FloatNumberConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(NumberControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = Parser.FormatFloat(control.Value);
        }

        public override void WriteToControl(NumberControl control, IniDocument iniDocument)
        {
            control.Value = Parser.ParseFloat(iniDocument[SectionName][EntryName]);
        }
    }

    public class IntSelectConverter : BaseConverter<SelectControl>
    {
        public IntSelectConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(SelectControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = Parser.FormatInt((int?)control.SelectedValue);
        }

        public override void WriteToControl(SelectControl control, IniDocument iniDocument)
        {
            control.SelectedValue = Parser.ParseInt(iniDocument[SectionName][EntryName]);
        }
    }

    public class EnumSelectConverter<TEnum> : BaseConverter<SelectControl> where TEnum : struct
    {
        public EnumSelectConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(SelectControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = Parser.FormatEnum<TEnum>((TEnum?)control.SelectedValue);
        }

        public override void WriteToControl(SelectControl control, IniDocument iniDocument)
        {
            control.SelectedValue = Parser.ParseEnum<TEnum>(iniDocument[SectionName][EntryName]);
        }
    }

    public class StringSelectConverter : BaseConverter<SelectControl>
    {
        public StringSelectConverter(Parser parser, string sectionName, string entryName) : base(parser, sectionName, entryName)
        {
        }

        public override void ReadFromControl(SelectControl control, IniDocument iniDocument)
        {
            iniDocument[SectionName][EntryName] = control.SelectedValue?.ToString() ?? "";
        }

        public override void WriteToControl(SelectControl control, IniDocument iniDocument)
        {
            var value = iniDocument[SectionName][EntryName];
            control.SelectedValue = string.IsNullOrEmpty(value) ? null : value;
        }
    }

}

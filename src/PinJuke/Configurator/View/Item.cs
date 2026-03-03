using System;
using System.Collections.Generic;
using System.Text;

namespace PinJuke.Configurator.View
{
    public class Item
    {
        public string Label { get; }
        public object? Value { get; }

        public Item(string label, object? value)
        {
            Label = label;
            Value = value;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}

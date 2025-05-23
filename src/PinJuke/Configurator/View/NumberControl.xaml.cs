﻿using System;
using System.IO;
using System.Windows;


namespace PinJuke.Configurator.View
{
    public partial class NumberControl : ConfiguratorControl
    {
        private float? value = null;
        public float? Value
        {
            get => value;
            set
            {
                if (this.SetField(ref this.value, value))
                {
                    ValueString = value == null ? "" : (Convert.ToString(value) ?? "");
                }
            }
        }

        private string valueString = "";
        public string ValueString
        {
            get => valueString;
            set
            {
                if (this.SetField(ref valueString, value))
                {
                    Value = value == "" ? null : Convert.ToSingle(value);
                }
            }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set => this.SetField(ref enabled, value);
        }


        public NumberControl()
        {
            InitializeComponent();
        }

    }
}

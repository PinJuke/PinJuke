﻿using PinJuke.View;
using System;
using System.Collections.Generic;
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

namespace PinJuke.Configurator.View
{
    public partial class GroupControl : BaseControl
    {
        private string labelText = "";
        public string LabelText
        {
            get => labelText;
            set => SetField(ref labelText, value);
        }

        public GroupControl()
        {
            InitializeComponent();
        }
    }
}
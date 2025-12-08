using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke.Utility
{
    public class UiUtil
    {
        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, AppDomain.CurrentDomain.FriendlyName);
        }
    }
}

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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.Onboarding
{
    public partial class Page : ContentControl
    {
        public event EventHandler? RemovalRequestedEvent;

        public Page()
        {
            InitializeComponent();
        }

        public void AnimateInitial()
        {
            BeginStoryboard((Storyboard)Resources["InitialStoryboard"]);
        }

        public void AnimateBackIn()
        {
            BeginStoryboard((Storyboard)Resources["BackInStoryboard"]);
        }

        public void AnimateBackOut()
        {
            BeginStoryboard((Storyboard)Resources["BackOutStoryboard"]);
        }

        public void AnimateNextIn()
        {
            BeginStoryboard((Storyboard)Resources["NextInStoryboard"]);
        }

        public void AnimateNextOut()
        {
            BeginStoryboard((Storyboard)Resources["NextOutStoryboard"]);
        }

        private void OutStoryboard_Completed(object sender, EventArgs e)
        {
            RemovalRequestedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}

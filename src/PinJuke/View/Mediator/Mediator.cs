using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PinJuke.View.Mediator
{
    public class Mediator
    {
        private readonly Control control;
        private bool initialized = false;

        public Mediator(Control control)
        {
            this.control = control;
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            control.Loaded += Control_Loaded;
            control.Unloaded += Control_Unloaded;
            if (control.IsLoaded)
            {
                OnLoaded();
            }
        }

        private void Control_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            OnLoaded();
        }

        private void Control_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            OnUnloaded();
        }

        protected virtual void OnLoaded() 
        {
        }

        protected virtual void OnUnloaded()
        {
        }
    }
}

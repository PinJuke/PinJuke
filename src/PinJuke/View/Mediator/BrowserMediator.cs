using DirectOutput.Cab.Out.DMX;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PinJuke.View.Mediator
{
    public class BrowserMediator : Mediator
    {
        private readonly BrowserControl browserControl;
        private readonly MainModel mainModel;

        public BrowserMediator(BrowserControl browserControl, MainModel mainModel) : base(browserControl)
        {
            this.browserControl = browserControl;
            this.mainModel = mainModel;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateView();
            mainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        protected override void OnUnloaded()
        {
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
            base.OnUnloaded();
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.NavigationNode):
                    UpdateView();
                    break;
                case nameof(MainModel.BrowserVisible):
                    UpdateView();
                    break;
            }
        }

        private void UpdateView()
        {
            browserControl.FileNode = mainModel.NavigationNode;
            browserControl.ViewVisible = mainModel.BrowserVisible;
        }
    }
}

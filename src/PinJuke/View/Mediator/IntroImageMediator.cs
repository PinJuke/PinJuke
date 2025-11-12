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
    public class IntroImageMediator : Mediator
    {
        private readonly IntroImageControl introImageControl;
        private readonly MainModel mainModel;

        public IntroImageMediator(IntroImageControl introImageControl, MainModel mainModel) : base(introImageControl)
        {
            this.introImageControl = introImageControl;
            this.mainModel = mainModel;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateVisibility();
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
                case nameof(MainModel.SceneType):
                    UpdateVisibility();
                    break;
            }
        }

        private void UpdateVisibility()
        {
            introImageControl.ViewVisible = mainModel.SceneType == SceneType.Intro;
        }
    }
}

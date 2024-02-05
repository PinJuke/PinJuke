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
using Unosquare.FFME;

namespace PinJuke.View
{
    public partial class MediaControl : BaseControl
    {
        public event EventHandler? MediaEndedEvent;

        public MediaControl()
        {
            InitializeComponent();

            // https://github.com/unosquare/ffmediaelement/issues/388#issuecomment-491851750
            MediaElement.RendererOptions.UseLegacyAudioOut = true;
            MediaElement.MediaEnded += MediaElement_MediaEnded;
        }

        private void MediaElement_MediaEnded(object? sender, EventArgs e)
        {
            MediaEndedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}

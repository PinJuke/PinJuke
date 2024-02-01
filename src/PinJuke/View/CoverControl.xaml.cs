using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace PinJuke.View
{
    /// <summary>
    /// Interaction logic for CoverControl.xaml
    /// </summary>
    public partial class CoverControl : PinJukeControl
    {
        private ImageSource? coverImageSource = null;
        public ImageSource? CoverImageSource
        {
            get => coverImageSource;
            set => SetField(ref coverImageSource, value);
        }

        public ImageSource DefaultCoverImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\disc-outline.svg");
        }

        public string TitleLabelText
        {
            get => Strings.CoverTitle;
        }

        public string ArtistLabelText
        {
            get => Strings.CoverArtist;
        }
 
        public string YearLabelText
        {
            get => Strings.CoverYear;
        }

        public string AlbumLabelText
        {
            get => Strings.CoverAlbum;
        }

        private string? titleText = null;
        public string? TitleText
        {
            get => titleText;
            set => SetField(ref titleText, value);
        }

        private string? artistText = null;
        public string? ArtistText
        {
            get => artistText;
            set => SetField(ref artistText, value);
        }

        private string? yearText = null;
        public string? YearText
        {
            get => yearText;
            set => SetField(ref yearText, value);
        }

        private string? albumText = null;
        public string? AlbumText
        {
            get => albumText;
            set => SetField(ref albumText, value);
        }

        public CoverControl() : base()
        {
            InitializeComponent();
        }

    }
}

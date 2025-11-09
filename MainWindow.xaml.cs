using System.Windows;
using System.Windows.Media.Imaging;

namespace ConvolutionalMasks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadOriginalImage();
        }
        private void LoadOriginalImage()
        {
            Uri uri = new("/images/homer.jpg", UriKind.Relative);
            BitmapImage bitmap = new(uri);
            ImgOriginal.Source = bitmap;
        }

        private void BtnApplyConvolution_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
using ConvolutionalMasks.Filters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ConvolutionalMasks
{
    public partial class MainWindow : Window
    {
        private BitmapImage bitmap;
        private byte[] pixelData;
        private int width, height, stride, bytesPerPixel;
        private System.Windows.Media.PixelFormat format;
        private double dpiX, dpiY;

        public MainWindow()
        {
            InitializeComponent();
            DisplayImage("/images/homer.jpg");
        }

        private void DisplayImage(string relativePath)
        {
            bitmap = new BitmapImage(new Uri(relativePath, UriKind.Relative));
            ImgOriginal.Source = bitmap;
            LoadImageData();
        }

        private void LoadImageData()
        {
            WriteableBitmap source = new(bitmap);
            width = source.PixelWidth;
            height = source.PixelHeight;
            bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            stride = width * bytesPerPixel;
            pixelData = new byte[height * stride];
            source.CopyPixels(pixelData, stride, 0);
            format = source.Format;
            dpiX = source.DpiX;
            dpiY = source.DpiY;
        }

        private void BtnDisplayImage_Click(object sender, RoutedEventArgs e)
        {
            if (CmbImages.SelectedItem is not ComboBoxItem selectedItem)
                return;

            string value = selectedItem.Content.ToString() ?? string.Empty;

            string path = value switch
            {
                "Homer" => "/images/homer.jpg",
                "Cat" => "/images/cat.jpg",
                "Flowers" => "/images/flowers.jpg",
                _ => "/images/flower.jpg"
            };

            DisplayImage(path);
        }

        private void BtnApplyConvolution_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMasks.SelectedItem is not ComboBoxItem selectedItem)
                return;

            string value = selectedItem.Content.ToString() ?? string.Empty;

            double[,]? kernel = value switch
            {
                "Blur" => Kernels.Blur,
                "Sharpen" => Kernels.Sharpen,
                "Emboss" => Kernels.Emboss,
                "Custom1" => Kernels.Custom1,
                "Custom2" => Kernels.Custom2,
                "Custom3" => Kernels.Custom3,
                _ => null
            };

            if (kernel is not null)
                ApplyConvolutionToImage(kernel);
        }

        private void ApplyConvolutionToImage(double[,] kernel)
        {
            double factor = Kernels.GetFactor(kernel);
            byte[] resultPixels = ApplyConvolution(pixelData, width, height, stride, bytesPerPixel, kernel, factor);
            WriteableBitmap result = CreateImageFromPixels(resultPixels, width, height, stride, format, dpiX, dpiY);
            ImgResult.Source = result;
        }

        private static byte[] ApplyConvolution(byte[] pixelData, int width, int height, int stride, int bytesPerPixel,
                                               double[,] kernel, double factor)
        {
            byte[] resultPixels = new byte[pixelData.Length];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double r = 0, g = 0, b = 0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int pixelX = x + kx;
                            int pixelY = y + ky;
                            int pixelIndex = (pixelY * stride) + (pixelX * bytesPerPixel);

                            b += pixelData[pixelIndex + 0] * kernel[ky + 1, kx + 1];
                            g += pixelData[pixelIndex + 1] * kernel[ky + 1, kx + 1];
                            r += pixelData[pixelIndex + 2] * kernel[ky + 1, kx + 1];
                        }
                    }

                    int resultIndex = (y * stride) + (x * bytesPerPixel);
                    resultPixels[resultIndex + 0] = (byte)Math.Clamp(b * factor, 0, 255);
                    resultPixels[resultIndex + 1] = (byte)Math.Clamp(g * factor, 0, 255);
                    resultPixels[resultIndex + 2] = (byte)Math.Clamp(r * factor, 0, 255);
                    resultPixels[resultIndex + 3] = 255;
                }
            }

            return resultPixels;
        }

        private static WriteableBitmap CreateImageFromPixels(byte[] pixelData, int width, int height, int stride,
                                              System.Windows.Media.PixelFormat format, double dpiX, double dpiY)
        {
            WriteableBitmap bmp = new(width, height, dpiX, dpiY, format, null);
            bmp.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            return bmp;
        }
    }
}

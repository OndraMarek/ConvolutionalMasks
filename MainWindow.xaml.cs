using ConvolutionalMasks.Filters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ConvolutionalMasks
{
    public partial class MainWindow : Window
    {
        private BitmapImage? bitmap;
        private byte[] pixelData = [];
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
            Array.Copy(pixelData, resultPixels, pixelData.Length);

            int kernelWidth = kernel.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int radiusX = kernelWidth / 2;
            int radiusY = kernelHeight / 2;

            int channelsToProcess = (bytesPerPixel == 4) ? 3 : bytesPerPixel;

            for(int y = radiusY; y < height - radiusY; y++)
            {
                for (int x = radiusX; x < width - radiusX; x++)
                {
                    for (int c = 0; c < channelsToProcess; c++)
                    {
                        double sum = 0;

                        for (int ky = 0; ky < kernelHeight; ky++)
                        {
                            for (int kx = 0; kx < kernelWidth; kx++)
                            {
                                int pixelX = x + (kx - radiusX);
                                int pixelY = y + (ky - radiusY);

                                int pixelIndex = (pixelY * stride) + (pixelX * bytesPerPixel) + c;

                                sum += pixelData[pixelIndex] * kernel[ky, kx];
                            }
                        }

                        int resultIndex = (y * stride) + (x * bytesPerPixel) + c;

                        resultPixels[resultIndex] = (byte)Math.Clamp(sum * factor, 0, 255);
                    }
                }
            });

            return resultPixels;
        }

        private static WriteableBitmap CreateImageFromPixels(byte[] pixelData, int width, int height, int stride,
                                              System.Windows.Media.PixelFormat format, double dpiX, double dpiY)
        {
            WriteableBitmap bmp = new(width, height, dpiX, dpiY, format, null);
            bmp.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            return bmp;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace TestAppFox
{
    public static class BytesToImageConverter
    {
        public static BitmapSource Decode(byte[] encodedImage)
        {
            try
            {
                using (var ms = new MemoryStream(encodedImage))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();

                    if (image.DpiX == 96 && image.DpiY == 96)
                    {
                        return image;
                    }

                    return ConvertBitmapTo96DPI(image);
                }
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        private static BitmapSource ConvertBitmapTo96DPI(BitmapImage bitmapImage)
        {
            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;

            int stride = width * bitmapImage.Format.BitsPerPixel;
            byte[] pixelData = new byte[stride * height];
            bitmapImage.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, 96, 96, bitmapImage.Format, bitmapImage.Palette, pixelData, stride);
        }
    }
}

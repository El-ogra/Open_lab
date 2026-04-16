using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;

namespace Open_lab.Services
{
    public class BarcodeService : IBarcodeService
    {
        public ImageSource GenerateCode128(string content, int width = 360, int height = 96)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Barcode content is required.", nameof(content));
            }

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 2,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(content.Trim());
            var image = BitmapSource.Create(
                pixelData.Width,
                pixelData.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                pixelData.Pixels,
                pixelData.Width * 4);
            image.Freeze();
            return image;
        }
    }
}

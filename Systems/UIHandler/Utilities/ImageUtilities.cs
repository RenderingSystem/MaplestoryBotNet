using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractImageSharpConverter
    {
        public abstract Image<Bgra32> Crop(Image<Bgra32> imageSharpImage, Rect crop);

        public abstract BitmapSource ConvertToBitmap(Image<Bgra32> imageSharpImage);
    }


    public class ImageSharpConverter : AbstractImageSharpConverter
    {
        public override Image<Bgra32> Crop(Image<Bgra32> imageSharpImage, Rect crop)
        {
            var cropX = Convert.ToInt32(Math.Max(0, Math.Min(crop.X, imageSharpImage.Width - 1)));
            var cropY = Convert.ToInt32(Math.Max(0, Math.Min(crop.Y, imageSharpImage.Height - 1)));
            var cropWidth = Convert.ToInt32(Math.Min(crop.Width, imageSharpImage.Width - crop.X));
            var cropHeight = Convert.ToInt32(Math.Min(crop.Height, imageSharpImage.Height - crop.Y));
            var cropRect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            return imageSharpImage.Clone(x => x.Crop(cropRect));
        }

        public override BitmapSource ConvertToBitmap(Image<Bgra32> imageSharpImage)
        {
            var sourceMemoryGroup = imageSharpImage.GetPixelMemoryGroup();
            var sourceSpan = sourceMemoryGroup[0].Span;
            var sourceBytes = MemoryMarshal.AsBytes(sourceSpan);
            int width = imageSharpImage.Width;
            int height = imageSharpImage.Height;
            int stride = width * 4;
            int requiredSize = stride * height;
            var pixelBuffer = new byte[requiredSize];
            sourceBytes.CopyTo(pixelBuffer);
            return BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                pixelBuffer,
                stride
            );
        }
    }
}

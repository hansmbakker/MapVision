using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MapVision.Helpers
{
    public static class UIElementExtensions
    {
        public static async Task<VideoFrame> RenderToVideoFrameAsync(this UIElement uiElement)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(uiElement);

            var buffer = await renderTargetBitmap.GetPixelsAsync();
            var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                buffer,
                BitmapPixelFormat.Bgra8,
                renderTargetBitmap.PixelWidth,
                renderTargetBitmap.PixelHeight,
                BitmapAlphaMode.Ignore);

            buffer = null;
            renderTargetBitmap = null;

            VideoFrame vf = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            return vf;
        }
    }
}

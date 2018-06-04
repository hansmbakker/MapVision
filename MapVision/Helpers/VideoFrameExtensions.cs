using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MapVision.Helpers
{

    public static class VideoFrameExtensions
    {
        public static async Task<VideoFrame> CropVideoFrameAsync(this VideoFrame inputVideoFrame, uint targetWidth, uint targetHeight)
        {
            bool useDX = inputVideoFrame.SoftwareBitmap == null;

            BitmapBounds cropBounds = new BitmapBounds();
            uint h = targetHeight;
            uint w = targetWidth;
            var frameHeight = useDX ? inputVideoFrame.Direct3DSurface.Description.Height : inputVideoFrame.SoftwareBitmap.PixelHeight;
            var frameWidth = useDX ? inputVideoFrame.Direct3DSurface.Description.Width : inputVideoFrame.SoftwareBitmap.PixelWidth;

            //var requiredAR = ((float)targetWidth / targetHeight);
            //w = Math.Min((uint)(requiredAR * frameHeight), (uint)frameWidth);
            //h = Math.Min((uint)(frameWidth / requiredAR), (uint)frameHeight);
            w = Math.Min((targetWidth), (uint)frameWidth);
            h = Math.Min((targetHeight), (uint)frameHeight);
            cropBounds.X = (uint)((frameWidth - w) / 2);
            cropBounds.Y = (uint)((frameHeight - h) / 2);
            cropBounds.Width = w;
            cropBounds.Height = h;

            VideoFrame croppedVideoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)targetWidth, (int)targetHeight, BitmapAlphaMode.Ignore);

            await inputVideoFrame.CopyToAsync(croppedVideoFrame, cropBounds, null);

            return croppedVideoFrame;
        }

        public static async Task SaveToFileAsync(this VideoFrame videoFrame)
        {
            bool useDX = videoFrame.SoftwareBitmap == null;

            SoftwareBitmap softwareBitmap = null;
            if (useDX)
            {
                softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(videoFrame.Direct3DSurface);
            }
            else { softwareBitmap = videoFrame.SoftwareBitmap; }

            var savePicker = new FileSavePicker();
            savePicker.DefaultFileExtension = ".png";
            savePicker.FileTypeChoices.Add(".png", new List<string> { ".png" });
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.SuggestedFileName = "snapshot.png";

            // Prompt the user to select a file 
            var saveFile = await savePicker.PickSaveFileAsync();

            // Verify the user selected a file 
            if (saveFile == null)
                return;

            // Encode the image to the selected file on disk 
            using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
            }
        }
    }
}

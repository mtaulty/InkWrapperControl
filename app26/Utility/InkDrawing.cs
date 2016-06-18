namespace App26.Utility
{
  using Microsoft.Graphics.Canvas;
  using System;
  using System.Runtime.InteropServices.WindowsRuntime;
  using System.Threading.Tasks;
  using Windows.Graphics.Imaging;
  using Windows.UI;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml.Media;
  using Windows.UI.Xaml.Media.Imaging;

  static class InkDrawing
  {
    public static async Task<ImageSource> InkToBitmapSourceAtSizeAsync(
      int width, 
      int 
      height,
      InkStrokeContainer inkStrokes)
    {
      SoftwareBitmapSource source = null;
      var win2dDevice = CanvasDevice.GetSharedDevice();

      // render the ink to the image.
      using (var target = new CanvasRenderTarget(
        win2dDevice,
        width,
        height,
        96.0f)) // TBD on the 96.0f here, is this what causes me blur?
      {
        using (var drawingSession = target.CreateDrawingSession())
        {
          drawingSession.Clear(Colors.Transparent);

          var strokes = inkStrokes?.GetStrokes();

          if ((strokes != null) && (strokes.Count > 0))
          {
            drawingSession.DrawInk(strokes);
          }
        }

        var outputBitmap = new SoftwareBitmap(
          BitmapPixelFormat.Bgra8,
          (int)target.SizeInPixels.Width,
          (int)target.SizeInPixels.Height,
          BitmapAlphaMode.Premultiplied);

        // TODO: is this buffer cleaned up?
        outputBitmap.CopyFromBuffer(target.GetPixelBytes().AsBuffer());

        source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(outputBitmap);
      }
      return (source);
    }
  }
}

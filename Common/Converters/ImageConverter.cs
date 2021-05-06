using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Common.Utilities;

namespace Common.Converters
{
    public static class ImageConverter
    {
        private static readonly int defaultImageWidth = 1920, defaultImageHegiht = 1080;

        public static BitmapImage ToImageSource(string path)
        {
            if (File.Exists(path))
            {
                Bitmap image = new Bitmap(path);
                return image.ToBitmapImage();
            }
            return null;
        }

        /// <summary>
        /// Thumbnail 이미지 저장
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapImage SaveThumbnail(string path)
        {
            if (File.Exists(path))
            {
                Bitmap thumbnail = new Bitmap(path);
                return ImageConverter.BitMapToBitmapImage(thumbnail, ImageFormat.Png);
            }
            return null;
        }


        /// <summary>
        /// Bitmap을 BitmapImage로 변환 한다.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="imgFormat"></param>
        /// <param name="imgSize"></param>
        /// <returns></returns>
        public static BitmapImage BitMapToBitmapImage(Bitmap bitmap, ImageFormat imgFormat, Size imgSize = new Size())
        {
            Bitmap objBitmap;
            BitmapImage bitmapImage = new BitmapImage();
            using (var ms = new System.IO.MemoryStream())
            {
                if (imgSize.Width > 0 && imgSize.Height > 0)
                    objBitmap = new Bitmap(bitmap, new Size(imgSize.Width, imgSize.Height));
                else
                    objBitmap = new Bitmap(bitmap, new Size(bitmap.Width, bitmap.Height));
                objBitmap.Save(ms, imgFormat);

                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = null;
                bitmapImage.DecodePixelWidth = imgSize.Width;
                bitmapImage.DecodePixelHeight = imgSize.Height;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        /// <summary>
        /// 바이너리 데이터를 BitmapImage로 변환한다.
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public static BitmapImage ByteArrToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        /// <summary>
        /// screenshot of a WPF control?
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="control"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void ScreenShot(string filePath, Visual control, double dpi = 96)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(control);
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Pbgra32);
            renderBitmap.Render(control);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderBitmap));
            //pngImage.Frames.Add(CreateResizedImage(renderBitmap, 128, 128, 0, dpi));
            using (Stream fileStream = File.Create(filePath))
            {
                pngImage.Save(fileStream);
            }
        }

        public static string ResourceUriToStream(Bitmap bitmap)
        {
            string base64String = null;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);

                    // Convert Image to byte[]
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    base64String = Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }

            return base64String;
        }

        public static string FilePathToString(string filePath)
        {
            string base64String = null;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var image = System.Drawing.Image.FromFile(@filePath, true);

                    int sourceWidth = image.Width;
                    int sourceHeight = image.Height;

                    if (defaultImageHegiht < image.Height || defaultImageWidth < image.Width)
                    {
                        if (image.Height > image.Width)
                        {
                            sourceHeight = image.Height;
                            sourceWidth = (int)(((double)sourceWidth / (double)sourceHeight) * (double)sourceWidth);
                        }
                        else
                        {
                            sourceHeight = (int)(((double)sourceHeight / (double)sourceWidth) * (double)sourceHeight);
                            sourceWidth = image.Width;
                        }
                    }

                    var bitmap = (sourceWidth == image.Width && sourceHeight == image.Height) ?
                                    new Bitmap(image) : new Bitmap(image, sourceWidth, sourceHeight);
                    bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    bitmap.Save(ms, ImageFormat.Png);

                    // Convert Image to byte[]
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    base64String = Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }

            return base64String;
        }

        public static BitmapImage FileSteamToImage(string fileStream)
        {
            var bitmapImage = new BitmapImage();
            MemoryStream ms = null;

            try
            {
                // Convert Base64 String to byte[]
                var imageBytes = Convert.FromBase64String(fileStream);
                using (ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    ms.Write(imageBytes, 0, imageBytes.Length);

                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = null;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }

            return bitmapImage;
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            return new Bitmap(bitmapImage.StreamSource);
        }

        public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new Bitmap(tempBitmap);
                }
            }
        }

        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin, double dpi)
        {
            var rect = new System.Windows.Rect(margin, margin, width - margin * 2, height - margin * 2);
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                dpi, dpi,              // Default(96) DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }
    }

    public static class RenderVisualService
    {
        private const double defaultDpi = 96.0;

        public static ImageSource RenderToPNGImageSource(Visual targetControl)
        {
            var renderTargetBitmap = GetRenderTargetBitmapFromControl(targetControl);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            var result = new BitmapImage();

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = memoryStream;
                result.EndInit();
            }

            return result;
        }

        public static void RenderToPNGFile(string filename, Visual targetControl)
        {
            var renderTargetBitmap = GetRenderTargetBitmapFromControl(targetControl);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            try
            {
                using (var fileStream = new FileStream(filename, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"There was an error saving the file: {ex.Message}");
            }
        }

        private static BitmapSource GetRenderTargetBitmapFromControl(Visual targetControl, double dpi = defaultDpi)
        {
            if (targetControl == null) return null;

            var bounds = VisualTreeHelper.GetDescendantBounds(targetControl);
            var renderTargetBitmap = new RenderTargetBitmap((int)(bounds.Width * dpi / 96.0),
                                                            (int)(bounds.Height * dpi / 96.0),
                                                            dpi,
                                                            dpi,
                                                            PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(targetControl);
                drawingContext.DrawRectangle(visualBrush, null, new System.Windows.Rect(new System.Windows.Point(), bounds.Size));
            }

            renderTargetBitmap.Render(drawingVisual);
            return renderTargetBitmap;
        }
    }
}

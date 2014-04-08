using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Drawing;

namespace MangaConverter
{
    public static class ImgUtil
    {

        public static void Compress(Image b, Stream output, ImageFormat fmt, int? jpegQ = null)
        {
            if (fmt == ImageFormat.Jpeg)
                b.Save(output, GetJpgCodec(), GetQualityEncodeParam(jpegQ));
            else
                b.Save(output, fmt);
        }

        public static void Compress(Image b, String outputFile, ImageFormat fmt, int? jpegQ = null)
        {
            if (fmt == ImageFormat.Jpeg)
                b.Save(outputFile, GetJpgCodec(), GetQualityEncodeParam(jpegQ));
            else
                b.Save(outputFile, fmt);
        }

        private static ImageCodecInfo GetJpgCodec()
        {
            return ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(e => e.MimeType == "image/jpeg");
        }

        private static EncoderParameters GetQualityEncodeParam(int? quality)
        {
            var ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(Encoder.Quality, quality ?? 85);
            return ep;
        }

        /// <summary>
        /// Make a color matrix that fill image with targetColor but keep alpha transparency.
        /// </summary>
        public static ColorMatrix MakeReplaceColorMatrix(Color targetColor)
        {
            var c = targetColor;
            return new ColorMatrix(new[]{
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {c.R/255f, c.G/255f, c.B/255f, 0, 1}});
        }

        /// <summary>
        /// Resize a bitmap. Aspec ratio is always preserved
        /// </summary>
        /// <param name="src">input bitmap</param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="crop">If true and maxWidth and maxHeight are not null, crop image to have output image fit both max dimentions</param>
        /// <param name="stretch">If true allow src image to grow to fit max dimentions</param>
        /// <param name="colorMatrix">If not null, apply this color matrix to image</param>
        /// <returns></returns>
        public static Bitmap Scale(Image src, int? maxWidth, int? maxHeight, bool crop = false, bool stretch = false, ColorMatrix colorMatrix = null)
        {
            if (maxWidth == null || maxHeight == null)
                crop = false;
            int maxW = maxWidth ?? int.MaxValue;
            int maxH = maxHeight ?? int.MaxValue;

            if (maxW < 1 || maxH < 1)
                throw new ArgumentException("maxW < 1 || maxH < 1");

            double wratio = maxW / (double)src.Width;
            double hratio = maxH / (double)src.Height;

            double ratio = crop
                ? Math.Max(wratio, hratio)
                : Math.Min(wratio, hratio);

            if (!stretch)
                ratio = Math.Min(1, ratio);

            int destW = Math.Min((int)Math.Ceiling(src.Width * ratio), maxW);
            int destH = Math.Min((int)Math.Ceiling(src.Height * ratio), maxH);

            int viewportW = Math.Min(src.Width, (int)Math.Round(destW / ratio));
            int viewportH = Math.Min(src.Height, (int)Math.Round(destH / ratio));

            var result = new Bitmap(destW, destH, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(result))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Avoid artefacts at image edges
                // (see: http://www.codeproject.com/KB/GDI-plus/imgresizoutperfgdiplus.aspx)
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    if(colorMatrix != null)
                        wrapMode.SetColorMatrix(colorMatrix);
                    g.DrawImage(src, new Rectangle(0, 0, result.Width, result.Height),
                        (src.Width - viewportW) / 2,
                        (src.Height - viewportH) / 2,
                        viewportW, viewportH, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return result;
        }

        /// <summary>
        /// Split src horizontally into images of equal width
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IEnumerable<Bitmap> HSplit(Bitmap src, int parts = 2)
        {
            if (parts == 1)
            {
                yield return src;
                yield break;
            }

            if(parts < 1 || parts > src.Width)
                throw new ArgumentOutOfRangeException("parts");
            int wStart = 0;
            for(int i = 0; i < parts; i++){
                var w = src.Width / parts + (src.Width % parts > i ? 1 : 0);
                yield return CopyRect(src, wStart, 0, w, src.Height);
                wStart += w;
            }
        }

        public static Bitmap CopyRect(Bitmap src, int x, int y, int w, int h)
        {
            Bitmap result = new Bitmap(w, h);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(src, new Rectangle(0, 0, w, h),
                    x,
                    y,
                    result.Width, result.Height, GraphicsUnit.Pixel, null);
            }
            return result;
        }

        public static Bitmap RemoveWhiteBg(Bitmap src)
        {
            var result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            byte minLevel = byte.MaxValue;
            for (int x = 0; x < src.Width; x++)
            {
                for (int y = 0; y < src.Width; y++)
                {
                    var c = src.GetPixel(x, y);
                    minLevel = (byte)Math.Min(minLevel, (c.R + c.G + c.B) / 3);
                }
            }

            for (int x = 0; x < src.Width; x++)
            {
                for (int y = 0; y < src.Width; y++)
                {
                    var c = src.GetPixel(x, y);
                    byte level = (byte)((c.R + c.G + c.B) / 3);
                    byte alpha = (byte)(((255 - level) / ((float)(255 - minLevel))) * 255);
                    result.SetPixel(x, y, Color.FromArgb(alpha, level, level, level));
                }
            }

            return result;
        }

        /// <summary>
        /// Remove white borders
        /// </summary>
        /// <param name="src"></param>
        /// <param name="minBrightness">Define minimum brightness for keeped pixels. where 0.0 represents black and 1.0 represents white</param>
        /// <returns></returns>
        public static Image CropWhiteBorders(Bitmap src, double minBrightness)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;
            for (int x = 0; x < src.Width; x++)
            {
                for (int y = 0; y < src.Height; y++)
                {
                    bool keep = src.GetPixel(x, y).GetBrightness() >= minBrightness;
                    if (!keep)
                        continue;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
            if (minX >= maxX)
                throw new EmptyImageExeption();

            Bitmap result = new Bitmap(maxX - minX, maxY - minY);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(src, new Rectangle(0, 0, result.Width, result.Height),
                    minX,
                    minY,
                    result.Width, result.Height, GraphicsUnit.Pixel, null);
            }
            src.Dispose();
            return result;
        }

        public class EmptyImageExeption : Exception { }

    }
}

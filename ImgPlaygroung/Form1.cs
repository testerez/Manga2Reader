using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using AForge.Imaging;
using System.Diagnostics;
using MangaConverter;
using AForge;
using AForge.Math.Geometry;
using AForge.Math;

namespace ImgPlaygroung
{
    public partial class Form1 : Form
    {
        Bitmap _original;
        Bitmap Original
        {
            get
            {
                return _original;
            }
            set
            {
                _original = value;
                pb1.Image = value;
            }
        }

        Bitmap _result;

        Bitmap W
        {
            get
            {
                return _result ?? Original;
            }
            set
            {
                _result = value;
                pb2.Image = value;
                if (value != null)
                {
                    var histo = new ImageStatistics(GrayscaleIfNeeded(value)).Gray;
                    var b = new Bitmap(pbHisto.Width, pbHisto.Height);
                    DrawHistogram(b, histo, Color.Black);
                    pbHisto.Image = b;
                }
                else
                {
                    pbHisto.Image = null;
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            Original = (Bitmap)System.Drawing.Image.FromFile(@"C:\Users\Tom\Desktop\test pages\0015.jpg");
            WindowState = FormWindowState.Maximized;
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            W = null;
        }

        private void btBinarize_Click(object sender, EventArgs e)
        {
            Apply(new Threshold(int.Parse(tbBinarize.Text)));
        }

        private void btGrayscale_Click(object sender, EventArgs e)
        {
            Apply(new GrayscaleBT709());
        }

        private void Apply(IFilter f)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                var r = f.Apply(W);
                watch.Stop();
                Log("{0} applyed in {1}ms", f.GetType().ToString().Split('.').Last(), watch.ElapsedMilliseconds);
                W = r;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DrawPoints(Bitmap src, IEnumerable<IntPoint> points, int w = 2, Color? color = null)
        {
            using (var g = Graphics.FromImage(src))
            using (var brush = new SolidBrush(color ?? GetRandomColor()))
            using (var pen = new Pen(brush, w))
            {
                foreach (IntPoint p in points)
                {
                    g.DrawRectangle(pen, p.X - w / 2, p.Y - w / 2, w, w);
                }
            }
        }

        void DrawLines(Bitmap src, IEnumerable<System.Drawing.Point> points, int w = 2, Color? color = null)
        {
            using (var g = Graphics.FromImage(src))
            using (var brush = new SolidBrush(color ?? GetRandomColor()))
            using (var pen = new Pen(brush, w))
            {
                g.DrawLines(pen, points.ToArray());
            }
        }

        void DrawRectangle(Bitmap src, Rectangle r, int w = 2, Color? color = null)
        {
            using (var g = Graphics.FromImage(src))
            using (var brush = new SolidBrush(color ?? GetRandomColor()))
            using (var pen = new Pen(brush, w))
            {
                g.DrawRectangle(pen, r);
            }
        }

        public void DrawHistogram(Bitmap src, Histogram h, Color? color = null)
        {
            if (h.Max == 0 || h.Values.Length == 0)
                return;
            var xRatio = (src.Width - 2) / (double)h.Values.Length;
            var yRatio = (src.Height - 2) / (double)h.Values.Max();
            DrawLines(src, h.Values.Select((v, i) => new System.Drawing.Point(
                (int)(i * xRatio) + 1,
                src.Height - (int)(v * yRatio) -1
            )), 2, color);
        }

        int colorIndex = 0;
        Color GetRandomColor()
        {
            var colors = new[]{
                Color.Red,
                Color.Blue,
                Color.Green,
                Color.Yellow,
                Color.Salmon,
                Color.Pink,
                Color.Cyan
            };
            
            return colors[colorIndex++ % colors.Length];
        }

        public static Bitmap GrayscaleIfNeeded(Bitmap src)
        {
            return src.PixelFormat == PixelFormat.Format8bppIndexed
                ? src
                : Grayscale.CommonAlgorithms.BT709.Apply(src);
        }

        void ForcecGrayscale()
        {
            W = GrayscaleIfNeeded(W);
        }

        private void btCurrentTest_Click(object sender, EventArgs e)
        {
            ForcecGrayscale();
            Apply(new OtsuThreshold());
            Apply(new BlobsFiltering()
            {
                CoupledSizeFiltering = true,
                MinWidth = 6,
                MinHeight = 6
            });
        }

        void DrawHoughLines()
        {
            HoughLineTransformation lineTransform = new HoughLineTransformation()
            {
                MinLineIntensity = 100,
                StepsPerDegree = 5,
            };
            // apply Hough line transofrm
            lineTransform.ProcessImage(W);

            var lines = lineTransform.GetMostIntensiveLines(10);

            W = AForge.Imaging.Image.Clone(W, PixelFormat.Format32bppRgb);
            foreach (HoughLine line in lines)
            {
                var l = line;
                Log("%90: {2}, l.Theta:{1}, l.Radius: {0}, Intensity: {3}", l.Radius, l.Theta, l.Theta % 90, l.Intensity);
                // get line's radius and theta values
                int r = line.Radius;
                double t = line.Theta;

                // check if line is in lower part of the image
                if (r < 0)
                {
                    t += 180;
                    r = -r;
                }

                // convert degrees to radians
                t = (t / 180) * Math.PI;

                // get image centers (all coordinate are measured relative
                // to center)
                int w2 = W.Width / 2;
                int h2 = W.Height / 2;

                double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

                if (line.Theta != 0)
                {
                    // none-vertical line
                    x0 = -w2; // most left point
                    x1 = w2;  // most right point

                    // calculate corresponding y values
                    y0 = (-Math.Cos(t) * x0 + r) / Math.Sin(t);
                    y1 = (-Math.Cos(t) * x1 + r) / Math.Sin(t);
                }
                else
                {
                    // vertical line
                    x0 = line.Radius;
                    x1 = line.Radius;

                    y0 = h2;
                    y1 = -h2;
                }

                var sourceData = W.LockBits(new Rectangle(0, 0, W.Width, W.Height), ImageLockMode.ReadOnly, W.PixelFormat);
                // draw line on the image
                Drawing.Line(sourceData,
                    new IntPoint((int)x0 + w2, h2 - (int)y0),
                    new IntPoint((int)x1 + w2, h2 - (int)y1),
                    Color.Red);
                W.UnlockBits(sourceData);
            }
        }

        private void CropBorders(object sender, EventArgs e)
        {
            W = MangaConverter.MangaConverter.CropBorders(W);
        }

        void Log(String message, params Object[] args)
        {
            tbLog.Text += String.Format(message, args) + "\r\n";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DrawHoughLines();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            W = MangaConverter.MangaConverter.Straighten(W);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            W = MangaConverter.MangaConverter.SplitPage(W).Last();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            W = MangaConverter.MangaConverter.SplitPage(W).First();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            W = new MangaConverter.MangaConverter(MangaOutputFormat.PC).Clean(W);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            W = MangaConverter.MangaConverter.OptimizeContrast(W);
        }
    }
}

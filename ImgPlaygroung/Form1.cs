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
        Bitmap Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                pb2.Image = value;
            }
        }

        Bitmap Working
        {
            get
            {
                return Result ?? Original;
            }
        }

        public Form1()
        {
            InitializeComponent();
            Original = (Bitmap)System.Drawing.Image.FromFile(@"C:\torrent\Monster - Urasawa\Monster Tome 01\Monster Tome 01 - 005.jpg");
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            Result = null;
        }

        private void btBinarize_Click(object sender, EventArgs e)
        {
            ApplyFilter(new Threshold(int.Parse(tbBinarize.Text)));
        }

        private void btGrayscale_Click(object sender, EventArgs e)
        {
            ApplyFilter(new GrayscaleBT709());
        }

        private void ApplyFilter(IFilter f)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                var r = f.Apply(Working);
                watch.Stop();
                Log("{0} applyed in {1}ms", f.GetType().ToString().Split('.').Last(), watch.ElapsedMilliseconds);
                Result = r;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DrawPoints(IEnumerable<IntPoint> points, int w = 2)
        {
            var result = AForge.Imaging.Image.Clone(Working, PixelFormat.Format32bppArgb);
            Working.Dispose();
            using (var g = Graphics.FromImage(result))
            using (var brush = new SolidBrush(GetRandomColor()))
            using (var pen = new Pen(brush, w)){
                foreach (IntPoint p in points)
                {
                    g.DrawRectangle(pen, p.X - w/2, p.Y - w/2, w, w);
                }
            }
            Result = result;
        }

        int colorIndex = 0;
        Color GetRandomColor()
        {
            return new[]{
                Color.Red,
                Color.Blue,
                Color.Green,
                Color.Yellow,
                Color.Salmon,
                Color.Pink,
                Color.Cyan
            }[colorIndex++];
        }

        private void btCurrentTest_Click(object sender, EventArgs e)
        {
            var splited = Result = MangaConverter.MangaConverter.SplitPage(Working).Last();

        }
        void DrawHoughLines()
        {
            HoughLineTransformation lineTransform = new HoughLineTransformation()
            {
                MinLineIntensity = 100,
                StepsPerDegree = 5,
            };
            // apply Hough line transofrm
            lineTransform.ProcessImage(Working);

            var lines = lineTransform.GetMostIntensiveLines(10);

            Result = AForge.Imaging.Image.Clone(Working, PixelFormat.Format32bppRgb);
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
                int w2 = Working.Width / 2;
                int h2 = Working.Height / 2;

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

                var sourceData = Working.LockBits(new Rectangle(0, 0, Working.Width, Working.Height), ImageLockMode.ReadOnly, Working.PixelFormat);
                // draw line on the image
                Drawing.Line(sourceData,
                    new IntPoint((int)x0 + w2, h2 - (int)y0),
                    new IntPoint((int)x1 + w2, h2 - (int)y1),
                    Color.Red);
                Working.UnlockBits(sourceData);
            }
        }

        void Grayscale()
        {
            if(Working.PixelFormat != PixelFormat.Format8bppIndexed)
                ApplyFilter(new GrayscaleBT709());
        }

        private void CropBorders(object sender, EventArgs e)
        {
            Result = MangaConverter.MangaConverter.CropBorders(Working);
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
            Result = MangaConverter.MangaConverter.Straighten(Working);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Result = MangaConverter.MangaConverter.SplitPage(Working).Last();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Result = MangaConverter.MangaConverter.SplitPage(Working).First();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Result = MangaConverter.MangaConverter.Straighten(Working);
            Result = MangaConverter.MangaConverter.CropBorders(Working);
            Result = MangaConverter.MangaConverter.OptimizeContrast(Working);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Result = MangaConverter.MangaConverter.OptimizeContrast(Working);
        }
    }
}

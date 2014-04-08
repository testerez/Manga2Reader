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
                pbOriginal.Image = value;
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
                pfResult.Image = value;
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
            Original = (Bitmap)System.Drawing.Image.FromFile(@"C:\torrent\Monster - Urasawa\Monster Tome 01\Monster Tome 01 - 095.jpg");
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

        private void btCurrentTest_Click(object sender, EventArgs e)
        {
            Result = MangaConverter.MangaConverter.SplitPage(Working).First();

            

            ApplyFilter(new GrayscaleBT709());
            var splited = Result;

            var stats = new ImageStatistics(Working);
            var hist = stats.Gray;
            Log("min={0}, max={1}, mean={2}, median={3}, SdtDev={4}", hist.Min, hist.Max, hist.Mean, hist.Median, hist.StdDev);
            ApplyFilter(new Threshold((int)(hist.Mean - hist.StdDev / 2)));


            //BlobsFiltering blobFilter = new BlobsFiltering();
            //// configure filter
            //blobFilter.CoupledSizeFiltering = true;
            //var minSize = Working.Height / 20;
            //blobFilter.MinWidth = minSize;
            //blobFilter.MinHeight = minSize;
            //blobFilter.CoupledSizeFiltering = true;

            //ApplyFilter(blobFilter);
            //ApplyFilter(new Invert());

            //ApplyFilter(blobFilter);
            //ApplyFilter(new Invert());

            int maxBorderSize = (int)(Working.Height * 0.15);

            var crops = new int[4];

            for (int j = 0; j < 4; j++)
            {
                Working.RotateFlip(RotateFlipType.Rotate270FlipNone);

                var h = Working.Height - maxBorderSize;
                var imgData = Working.LockBits(
                    new Rectangle(0, maxBorderSize / 2, maxBorderSize, h),
                    ImageLockMode.ReadOnly,
                    Working.PixelFormat);
                try
                {
                    var hHisto = new HorizontalIntensityStatistics(imgData).Gray;

                    //Search for last white or black pixel column
                    crops[j] = Math.Max(
                        Array.LastIndexOf(hHisto.Values, 0),
                        Array.LastIndexOf(hHisto.Values, h * 255)
                    );
                    if (crops[j] < 0) crops[j] = 0;
                }
                finally
                {
                    Working.UnlockBits(imgData);
                }
            }

            int
                cropT = crops[0],
                cropR = crops[1],
                cropB = crops[2],
                cropL = crops[3];


            Result = MangaConverter.MangaConverter.OptimizeContrast(splited);
            Result = ImgUtil.CopyRect(Working, cropL, cropT, Working.Width - cropL - cropR, Working.Height - cropT - cropB);
            
        }

        void Log(String message, params Object[] args)
        {
            tbLog.Text += String.Format(message, args) + "\r\n";
        }
    }
}

using AForge.Imaging;
using AForge.Imaging.Filters;
using MangaConverter.MangaSource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter
{
    public class MangaConverter
    {
        public Logger Log { get; private set; }
        public bool EnableContrastOptimization { get; set; }
        public bool EnableCropBorders { get; set; }
        public bool EnableStraighten { get; set; }
        public MangaOutputFormat OutputFormat { get; private set; }

        public MangaConverter(MangaOutputFormat outputFormat, Logger l = null, bool optimizeContrast = true, bool straighten = true, bool cropBorders = true)
        {
            Log = l ?? new ConsoleLogger();
            EnableContrastOptimization = optimizeContrast;
            OutputFormat = outputFormat;
            EnableCropBorders = cropBorders;
            EnableStraighten = straighten;
        }

        public void ConvertAll(String src, String destDir)
        {
            if (Directory.Exists(src))
                Log.I("Scannig '{0}' for mangas...", Path.GetFullPath(src));
            var all = FindMangaRecursive(src, destDir).ToList();
            if (all.Count == 0)
                Log.E("No manga found in '{0}'", src);
            int cmpt = 0;
            foreach(var m in all){
                cmpt++;
                
                try
                {
                    var mangaName = m.GetName();
                    Log.I("Converting {0}/{1}: {2} ({3})", cmpt, all.Count, mangaName, m.Location);
                    EbookGenerator.Save(
                        GetSplitedPages(m)
                            .Select(Clean)
                            .Select(ResizeForOutput),
                        destDir,
                        mangaName,
                        OutputFormat.Format);
                }
                catch (Exception ex)
                {
                    Log.E(ex.Message);
                    Log.D("\nStackTrace:");
                    Log.D(ex.StackTrace);
                }
                
            }
        }

        public static bool IsClean(Bitmap b)
        {
            b = GrayscaleIfNeeded(b);
            var g = new ImageStatistics(b).Gray;
            int grayTotal = g.Values
                .Skip(10)
                .Take(g.Values.Length - 20)
                .Sum();
            return grayTotal < g.TotalCount * 0.5;
        }

        public Bitmap ResizeForOutput(Bitmap b)
        {
            return OutputFormat.MaxResolution == null
                ? b
                : ImgUtil.Scale(b, OutputFormat.MaxResolution.Value.Width, OutputFormat.MaxResolution.Value.Height);
        }

        public Bitmap Clean(Bitmap src)
        {
            //Skip slow and risky operations if source seams already cleen
            if (!IsClean(src))
            {
                if (EnableStraighten)
                    src = Straighten(src);
                if (EnableCropBorders)
                    src = CropBorders(src);
            }

            if (EnableContrastOptimization)
                src = OptimizeContrast(src);
            
            return src;
        }

        public IEnumerable<String> SafeEnumerateFiles(String path, String searchPattern = "*",
            SearchOption option = SearchOption.TopDirectoryOnly, IEnumerable<String> ignoreList = null)
        {
            String[] files = null;
            try
            {
                files = Directory.GetFiles(path, searchPattern);
            }
            catch(Exception e)
            {
                Log.D(e.Message);
            }

            if (files == null)
                yield break;

            foreach (var f in files)
                yield return f;

            if (option == SearchOption.AllDirectories)
            {
                ignoreList = ignoreList ?? Enumerable.Empty<String>();
                foreach (var f in Directory.EnumerateDirectories(path)
                    .Except(ignoreList, StringComparer.InvariantCultureIgnoreCase)
                    .SelectMany(d => SafeEnumerateFiles(d, searchPattern, option, ignoreList)))
                {
                    yield return f;
                }
            }
        }

        public IEnumerable<IMangaSource> FindMangaRecursive(String src, String dest)
        {
            var ignoreList = new[]{
                @"c:\windows",
                @"c:\$Recycle.Bin",
                @"c:\ProgramData",
                dest,
            };
            bool srcIsFile = File.Exists(src);
            var files = srcIsFile ? new[] { src } : SafeEnumerateFiles(src, "*", SearchOption.AllDirectories, ignoreList);
            var imageMangaDirectories = new HashSet<String>();

            foreach (var f in files)
            {
                switch (Path.GetExtension(f).ToLower())
                {
                    case ".pdf":
                        yield return new PdfMangaSource(f);
                        break;
                    case ".zip":
                    case ".cbz":
                        yield return new ArchiveMangaSource(f);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        var dir = Path.GetDirectoryName(f);
                        if (!imageMangaDirectories.Contains(dir))
                        {
                            yield return new ImagesMangaSource(dir);
                            imageMangaDirectories.Add(dir);
                        }
                        break;
                }
            }
        }

        private IEnumerable<Bitmap> GetSplitedPages(IMangaSource src){
            int cmpt = 0;
            int? pagesCount = src.GetApproximatePagesCount();
            foreach(var p in src.GetPages()){
                Log.V("Processing page {0}/{1}", ++cmpt, pagesCount == null ? "?" : "" + pagesCount);
                if(OutputFormat.SplitDoublePages){
                    foreach(var p2 in SplitPage(p))
                        yield return p2;
                }else{
                    yield return p;
                }
            }
        }

        public static IEnumerable<Bitmap> SplitPage(Bitmap src)
        {
            const double minRatioForSplit = 1;
            double srcRatio = src.Width / (double)src.Height;
            if (srcRatio < minRatioForSplit)
                return new[] { src };
            var r = ImgUtil.HSplit(src).Reverse();
            
            return r;
        }

        public static Bitmap GrayscaleIfNeeded(Bitmap src)
        {
            return src.PixelFormat == PixelFormat.Format8bppIndexed
                ? src 
                : Grayscale.CommonAlgorithms.BT709.Apply(src);
        }

        /// <summary>
        /// Detect and crop page borders
        /// </summary>
        /// <param name="src">8 bpp indexed</param>
        public static Bitmap CropBorders(Bitmap src)
        {
            Bitmap tmp = GrayscaleIfNeeded(src);

            tmp = new OtsuThreshold().Apply(tmp);

            int maxBorderSize = (int)Math.Min(tmp.Height * 0.15, tmp.Width * 0.3);
            var crops = new int[4];

            for (int side = 0; side < 4; side++)
            {
                tmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                var h = tmp.Height - maxBorderSize;
                var imgData = tmp.LockBits(
                    new Rectangle(0, maxBorderSize / 2, maxBorderSize, h),
                    ImageLockMode.ReadOnly,
                    tmp.PixelFormat);
                try
                {
                    var hHisto = new HorizontalIntensityStatistics(imgData).Gray;

                    //Search for last white or black pixel column
                    crops[side] = Math.Max(
                        Array.LastIndexOf(hHisto.Values, 0),
                        Array.LastIndexOf(hHisto.Values, h * 255)
                    );
                    if (crops[side] < 0) crops[side] = 0;
                }
                finally
                {
                    tmp.UnlockBits(imgData);
                }
            }

            int
                cropT = crops[0],
                cropR = crops[1],
                cropB = crops[2],
                cropL = crops[3];

            return ImgUtil.CopyRect(src, cropL, cropT, src.Width - cropL - cropR, src.Height - cropT - cropB);
        }

        public static Bitmap Straighten(Bitmap src)
        {
            var tmp = GrayscaleIfNeeded(src);
            tmp = new FiltersSequence(
                new OtsuThreshold(),
                new Invert(),
                new CannyEdgeDetector(),
                new BlobsFiltering()
                {
                    CoupledSizeFiltering = true,
                    MinWidth = tmp.Height / 6,
                    MinHeight = tmp.Height / 6
                }
            ).Apply(tmp);

            //Detect lines
            var lineTransform = new HoughLineTransformation()
            {
                MinLineIntensity = 100,
                StepsPerDegree = 5,
            };
            lineTransform.ProcessImage(tmp);
            var lines = lineTransform.GetMostIntensiveLines(10);

            //normalise angle and keep only lines close to horizontal or vertical
            int maxRotation = 5;
            var angles = lines
                .Select(l => l.Theta % 90)
                .Select(a => a > 45 ? a - 90 : a)
                .Where(a => a <= maxRotation)
                .ToArray();

            if (angles.Length < 2)
                return src;

            var avg = angles.Average();
            var stdDev = angles.StandardDeviation();

            angles = angles.Where(a => Math.Abs(avg - a) < stdDev).ToArray();

            if (angles.Length < 2 || angles.StandardDeviation() > 1)
            {
                //not enough info to straighten image
                return src;
            }

            var angle = angles.Average();
            if (Math.Abs(angle) < 0.01)
                return src;
            src = AForge.Imaging.Image.Clone(src, PixelFormat.Format24bppRgb);
            return new RotateBilinear(-angle, true).Apply(src);
        }

        public static Bitmap OptimizeContrast(Bitmap src)
        {
            src = GrayscaleIfNeeded(src);
            var statistics = new ImageStatistics(src);
            var filter = new AForge.Imaging.Filters.LevelsLinear();
            var min = Math.Min(100, statistics.Gray.GetRange(0.8).Min);
            var max = Math.Max(150, statistics.Gray.GetRange(0.8).Max - 20);
            filter.Input = new AForge.IntRange(min, max);
            return filter.Apply(src);
        }
    }
}
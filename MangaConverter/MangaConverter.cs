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
        public MangaOutputFormat OutputFormat { get; private set; }

        public MangaConverter(MangaOutputFormat outputFormat, Logger l = null, bool optimizeContrast = true, bool cropBorders = true)
        {
            Log = l ?? new ConsoleLogger();
            EnableContrastOptimization = optimizeContrast;
            OutputFormat = outputFormat;
            EnableCropBorders = cropBorders;
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
                    EbookGenerator.Save(GetSplitedPages(m).Select(Clean), destDir, mangaName, OutputFormat.Format);
                }
                catch (Exception ex)
                {
                    Log.E(ex.Message);
                    Log.D("\nStackTrace:");
                    Log.D(ex.StackTrace);
                }
                
            }
        }

        private Bitmap Clean(Bitmap src)
        {
            if (EnableCropBorders)
                src = CropBorders(src);
            src = ForceGrayScale(src);
            if (EnableContrastOptimization)
                src = OptimizeContrast(src);
            if (OutputFormat.MaxResolution != null)
                src = ImgUtil.Scale(src, OutputFormat.MaxResolution.Value.Width, OutputFormat.MaxResolution.Value.Height);
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

        private static Bitmap ForceGrayScale(Bitmap src)
        {
            var statistics = new ImageStatistics(src);
            return statistics.IsGrayscale
                ? src
                : Grayscale.CommonAlgorithms.BT709.Apply(src);
        }

        /// <summary>
        /// Detect and crop page borders
        /// </summary>
        /// <param name="src">8 bpp indexed</param>
        private static Bitmap CropBorders(Bitmap src)
        {
            Bitmap tmp = src.PixelFormat == PixelFormat.Format8bppIndexed
                ? src 
                : Grayscale.CommonAlgorithms.BT709.Apply(src);

            //Binarize imge
            var gray = new ImageStatistics(tmp).Gray;
            tmp = new Threshold((int)(gray.Mean - gray.StdDev / 2)).Apply(tmp);

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

        public static Bitmap OptimizeContrast(Bitmap src)
        {
            var statistics = new ImageStatistics(src);
            var filter = new AForge.Imaging.Filters.LevelsLinear();
            var min = Math.Min(100, statistics.Gray.GetRange(0.8).Min);
            var max = Math.Max(150, statistics.Gray.GetRange(0.8).Max - 20);
            filter.Input = new AForge.IntRange(min, max);
            return filter.Apply(src);
        }
    }
}
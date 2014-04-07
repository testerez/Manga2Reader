using AForge.Imaging;
using AForge.Imaging.Filters;
using MangaConverter.MangaSource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public MangaOutputFormat OutputFormat { get; private set; }

        public MangaConverter(MangaOutputFormat outputFormat, Logger l = null, bool optimizeContrast = true){
            Log = l ?? new ConsoleLogger();
            EnableContrastOptimization = optimizeContrast;
            OutputFormat = outputFormat;
        }

        public void ConvertAll(String src, String destDir)
        {
            if (Directory.Exists(src))
                Log.I("Scannig '{0}' for mangas...", Path.GetFullPath(src));
            var all = FindMangaRecursive(src).ToList();
            if (all.Count == 0)
                Log.E("No manga found in '{0}'", src);
            int cmpt = 0;
            foreach(var m in all){
                cmpt++;
                
                try
                {
                    var mangaName = m.GetName();
                    Log.I("Converting {0}/{1}: {2}", cmpt, all.Count, mangaName);
                    EbookGenerator.Save(GetSplitedPages(m).Select(Clean), destDir, mangaName, OutputFormat.Format);
                }
                catch (Exception ex)
                {
                    Log.E(ex.Message);
                }
                
            }
        }

        private Bitmap Clean(Bitmap src)
        {
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

        public IEnumerable<IMangaSource> FindMangaRecursive(String src)
        {
            var ignoreList = new[]{
                @"c:\windows",
                @"c:\$Recycle.Bin",
                @"c:\ProgramData",
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
                        if (!imageMangaDirectories.Contains(src))
                        {
                            yield return new ImagesMangaSource(src);
                            imageMangaDirectories.Add(src);
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

        private static IEnumerable<Bitmap> SplitPage(Bitmap src)
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
                : new Grayscale(0.2125, 0.7154, 0.0721).Apply(src);
        }

        private Bitmap OptimizeContrast(Bitmap src)
        {
            var statistics = new ImageStatistics(src);
            var filter = new AForge.Imaging.Filters.LevelsLinear();
            var min = Math.Min(100, statistics.Gray.GetRange(0.8).Min);
            var max = Math.Max(150, statistics.Gray.GetRange(0.8).Max - 20);
            filter.Input = new AForge.IntRange(min, max);
            Log.D("Applying level filter (min: {0}, max: {1})", min, max);
            return filter.Apply(src);
        }
    }
}
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
            var all = FindMangaRecursive(src).ToList();
            if (all.Count == 0)
                Log.E("No manga found in '{0}'", src);
            int cmpt = 0;
            foreach(var m in all){
                cmpt++;
                var mangaName = m.GetName();
                Log.I("Converting {0}/{1}: {2}", cmpt, all.Count, mangaName);
                EbookGenerator.Save(GetSplitedPages(m).Select(Clean), destDir, mangaName, OutputFormat.Format);
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

        public IEnumerable<IMangaSource> FindMangaRecursive(String src)
        {
            if (Directory.Exists(src))
            {
                if (Directory.GetFiles(src, "*.jpg").Any())
                    yield return new ImagesMangaSource(src);
                foreach (var d in Directory.GetFiles(src).Concat(Directory.GetDirectories(src)))
                    foreach(var s in FindMangaRecursive(d))
                        yield return s;
            }
            else if (File.Exists(src))
            {
                switch (Path.GetExtension(src).ToLower())
                {
                    case ".pdf" :
                        yield return new PdfMangaSource(src);
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

        private static Bitmap OptimizeContrast(Bitmap src)
        {
            var statistics = new ImageStatistics(src);
            var filter = new AForge.Imaging.Filters.LevelsLinear();
            filter.Input = new AForge.IntRange(
                statistics.Gray.GetRange(0.8).Min,
                statistics.Gray.GetRange(0.8).Max - 20);
            return filter.Apply(src);
        }
    }
}
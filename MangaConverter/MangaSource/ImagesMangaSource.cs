using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter.MangaSource
{
    public class ImagesMangaSource : IMangaSource
    {
        public String Location { get; private set; }

        public ImagesMangaSource(String path)
        {
            Location = path;
        }

        public string GetName()
        {
            return Path.GetFullPath(Location).Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public IEnumerable<Bitmap> GetPages()
        {
            return Directory.EnumerateFiles(Location, "*.jpg").Select(f => (Bitmap)Bitmap.FromFile(f));
        }

        public int? GetApproximatePagesCount()
        {
            return Directory.EnumerateFiles(Location, "*.jpg").Count();
        }
    }
}

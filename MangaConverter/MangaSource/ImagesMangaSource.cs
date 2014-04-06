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
        private String _path;

        public ImagesMangaSource(String path)
        {
            _path = path;
        }

        public string GetName()
        {
            return _path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public IEnumerable<Bitmap> GetPages()
        {
            return Directory.EnumerateFiles(_path, "*.jpg").Select(f => (Bitmap)Bitmap.FromFile(f));
        }

        public int? GetPagesCount()
        {
            return Directory.EnumerateFiles(_path, "*.jpg").Count();
        }
    }
}

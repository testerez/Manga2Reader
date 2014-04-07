using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter.MangaSource
{
    class ArchiveMangaSource : IMangaSource
    {
        private String _path;

        public ArchiveMangaSource(String path)
        {
            _path = path;
        }

        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(_path);
        }

        public IEnumerable<Bitmap> GetPages()
        {
            return GetImageEntries()
                .Select(e => {
                    using(var s = e.OpenReader()){
                        return (Bitmap)Bitmap.FromStream(s);
                    }
                });
        }

        public int? GetApproximatePagesCount()
        {
            return GetImageEntries().Count();
        }

        private IEnumerable<ZipEntry> GetImageEntries()
        {
            using (var zip = ZipFile.Read(_path))
            {
                return zip.Where(e => Path.GetExtension(e.FileName).ToLower().In(".jpg", ".jpeg"));
            }
        }
    }
}

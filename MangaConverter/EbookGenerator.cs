﻿using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter
{
    public class EbookGenerator
    {
        public enum Format { Cbz }

        public static void Save(IEnumerable<Image> images, String destDir, String name, Format format)
        {
            switch (format)
            {
                case Format.Cbz :
                    SaveAsCbz(images, destDir, name);
                    break;
                default :
                    throw new Exception(String.Format("Format {0} not supported", format));
            }
        }

        public static void SaveAsCbz(IEnumerable<Image> images, String destDir, String name)
        {
            Directory.CreateDirectory(destDir);

            String dest = Path.Combine(destDir, name + ".cbz");
            int page = 1;
            using (var s = new ZipOutputStream(dest))
            {
                s.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                foreach (var img in images)
                {
                    s.PutNextEntry(page.ToString("0000") + ".jpg");
                    ImgUtil.Compress(img, s, ImageFormat.Jpeg, 90);
                    page++;
                }
            }
        }
    }
}
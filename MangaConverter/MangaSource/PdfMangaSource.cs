using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter.MangaSource
{
    class PdfMangaSource : IMangaSource
    {
        private String _path;

        public PdfMangaSource(String path)
        {
            _path = path;
        }

        public IEnumerable<Bitmap> GetPages()
        {
            using (var pdf = new PdfReader(_path))
            {
                for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
                {
                    var img = GetImagesFromPdfDict(pdf.GetPageN(pageNumber), pdf).FirstOrDefault();
                    if(img != null)
                        yield return img;
                }
            }
        }

        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(_path);
        }

        public int? GetApproximatePagesCount()
        {
            using (var pdf = new PdfReader(_path))
            {
                return pdf.NumberOfPages;
            }
        }

        private IEnumerable<Bitmap> GetImagesFromPdfDict(PdfDictionary dict, PdfReader doc)
        {
            PdfDictionary res = (PdfDictionary)(PdfReader.GetPdfObject(dict.Get(PdfName.RESOURCES)));
            PdfDictionary xobj = (PdfDictionary)(PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)));

            if (xobj == null)
                yield break;

            foreach (PdfName name in xobj.Keys)
            {
                PdfObject obj = xobj.Get(name);
                if (!obj.IsIndirect())
                    continue;

                PdfDictionary tg = (PdfDictionary)(PdfReader.GetPdfObject(obj));
                PdfName subtype = (PdfName)(PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)));
                if (PdfName.IMAGE.Equals(subtype))
                {
                    int xrefIdx = ((PRIndirectReference)obj).Number;
                    PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
                    PdfStream str = (PdfStream)(pdfObj);

                    var pdfImage = new PdfImageObject((PRStream)str);
                    yield return (Bitmap)pdfImage.GetDrawingImage();
                }
                else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
                {
                    foreach(var i in GetImagesFromPdfDict(tg, doc))
                        yield return i;
                }
            }
        }
    }
}

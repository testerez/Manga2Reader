using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MangaConverter.MangaSource
{
    public interface IMangaSource
    {
        IEnumerable<Bitmap> GetPages();
        String GetName();
        int? GetPagesCount();
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter
{
    public class MangaOutputFormat
    {
        public static MangaOutputFormat KoboAura = new MangaOutputFormat(
            EbookGenerator.Format.Cbz,
            new Size(758, 1014),
            true,
            "Kobo Aura");

        public static MangaOutputFormat Default = KoboAura;

        public static Dictionary<String, MangaOutputFormat> Presets = new Dictionary<String, MangaOutputFormat>{
            {"kobo_aura", KoboAura}
        };

        public EbookGenerator.Format Format { get; private set; }
        public Size? MaxResolution { get; private set; }
        public bool SplitDoublePages{get; private set;}
        public String Name { get; private set; }

        public MangaOutputFormat(EbookGenerator.Format format, Size? maxResolution = null, bool splitDoublePages = true, String name = null){
            MaxResolution = maxResolution;
            Format = format;
            SplitDoublePages = splitDoublePages;
            Name = name;
        }

        public override string ToString()
        {
            var parts = new List<String>();
            if(!String.IsNullOrEmpty(Name))
                parts.Add(Name);
            parts.Add(Format + " output");
            if (MaxResolution != null)
                parts.Add(String.Format("{0}x{1}", MaxResolution.Value.Width, MaxResolution.Value.Height));
            if (SplitDoublePages)
                parts.Add("Split double pages");
            return parts.Join(", ");
        }
    }
}

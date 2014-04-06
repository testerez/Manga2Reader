using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaConverter;
using NDesk.Options;

namespace Manga2Reader
{
    class Program
    {
        public static void Main(string[] args)
        {
            bool showHelp = false;
            String source = null;
            String destination = null;
            MangaOutputFormat outputFormat = MangaOutputFormat.Default;
            Logger.LogLevel logLevel = Logger.LogLevel.Info;

            //TODO : add params for custom output formats

            var p = new OptionSet() {
                {
                    "s|source=",
                    "Path to the manga(s) to convert. You can point to a directory containing many manga or to a single file.",
                    v => source = v
                },
                {
                    "d|destination:",
                    "Directory to store converted mangas to",
                    v => destination = v
                },
                { 
                    "p|presset=",
                    "Predefined output forma. Avalable options are:\n" + MangaOutputFormat.Presets.Select(kv => kv.Key + ": " + kv.Value).Join("\n"),
                    v => outputFormat = StringToOutputFormat(v)
                },
                {
                    "v",
                    "display verbose messages",
                    v => logLevel = v == null || logLevel > Logger.LogLevel.Verbose ? logLevel : Logger.LogLevel.Verbose
                },
                {
                    "debug",
                    "display debug messages",
                    v => logLevel = v == null || logLevel > Logger.LogLevel.Debug ? logLevel : Logger.LogLevel.Debug
                },
                {
                    "h|help", 
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);

                if (showHelp)
                {
                    ShowHelp(p);
                    return;
                }
                else
                {
                    if (source == null)
                        throw new InvalidOperationException("Missing required option --source");
                    if (destination == null)
                        throw new InvalidOperationException("Missing required option --destination");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'Manga2Reader --help' for more information.");
                return;
            }


            try
            {
                var converter = new MangaConverter.MangaConverter(
                    outputFormat,
                    new ConsoleLogger() {MaxLogLevel = logLevel });
                converter.ConvertAll(source, destination);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        static MangaOutputFormat StringToOutputFormat(String s)
        {
            if (!MangaOutputFormat.Presets.ContainsKey(s))
                throw new Exception(String.Format("Unknown preset '{0}'. Use one of: {1}", s, String.Join(", ", MangaOutputFormat.Presets.Keys)));
            return MangaOutputFormat.Presets[s];
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: Manga2Reader [OPTIONS]");
            Console.WriteLine("Convert mangas for better eReader compatibility");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

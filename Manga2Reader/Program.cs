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
                    "d|destination=",
                    "Directory to store converted mangas to. Default to ./Manga2Reader",
                    v => destination = v
                },
                { 
                    "p|presset=",
                    "Predefined output format.\nPossible values are:\n" +
                        MangaOutputFormat.Presets.Select(kv => String.Format(" - {0}: {1}", kv.Key, kv.Value)).Join("\n"),
                    v => outputFormat = StringToOutputFormat(v)
                },
                {
                    "v",
                    "Display verbose messages",
                    v => logLevel = v == null || logLevel > Logger.LogLevel.Verbose ? logLevel : Logger.LogLevel.Verbose
                },
                {
                    "debug",
                    "Display debug messages",
                    v => logLevel = v == null || logLevel > Logger.LogLevel.Debug ? logLevel : Logger.LogLevel.Debug
                },
                {
                    "h|help", 
                    "Show this message and exit", 
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
                source = extra.FirstOrDefault() ?? ".";
                destination = destination ?? "./Manga2Reader";
                
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
                if (logLevel >= Logger.LogLevel.Debug)
                {
                    Console.WriteLine();
                    Console.WriteLine("StackTrace:");
                    Console.WriteLine(e.StackTrace);
                }
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
            Console.WriteLine();
            Console.WriteLine("Usage: Manga2Reader [source path] [options]");
            Console.WriteLine();
            Console.WriteLine("Detect comic books in source path and convert if for optimal readability on any e-reader");
            Console.WriteLine("[source path] default to ./");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

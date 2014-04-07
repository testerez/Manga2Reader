Manga2Reader
============

`Manga2Reader` is a command line tool that aim to convert any scanned comic book for optimal readability on any e-reader.

Usage
-----
    Manga2Reader [source path] [options]

`[source path]` defaut to `./`, Default destination is `./Manga2Reader`

Available Options
-----------------

- `-d, --destination=VALUE` Directory to store converted mangas to. Default to ./Manga2Reader
- `-p, --presset=VALUE` Predefined output format. Possible values are:

 - `kobo_aura`: Kobo Aura, Cbz output, 758x1014, Split double pages

- `-v` Display verbose messages
- `--debug` Display debug messages
- `-h, --help` Show this message and exit


Actual features
---------------

 - Detect and convert all comic books in source directory
 - Support directory of .jpg files as input
 - Split double pages into single ones
 - Optimize contrast. Usefull when the scan has not been cleaned
 - Resize images
 - Create a .cbz file as output
 
Future features
----------------
 
 - Support more input formats (zip, pdf, cbz...)
 - Support more output formats
 - Better image cleaning (straighten, crop useless borders...)
 - Add a GUI

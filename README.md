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


Current features
---------------

 - Detect and convert all comic books in source directory. Supported formats:
    - zip
    - cbz
    - pdf
    - jpg files
 - Split double pages into single ones
 - Optimize contrast. Usefull when the scan has not been cleaned
 - Resize images to feet e-reader screen resolution
 - Output as zip, cbz or jpg files
 
Future features
----------------
 - Better image cleaning (straighten, crop useless borders...)
 - Add presets for other e-reders (kindle...)
 - Add a GUI

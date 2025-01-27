# PhotoOrg

![GitHub Actions](https://github.com/DerekGn/PhotoOrg/actions/workflows/build.yml/badge.svg)

A simple command line application to move photos and videos to sublfolders based on the original data time metadata tag.

## Usage

```cmd
USAGE:
    PhotoOrg.dll <sourcePath> <targetPath> [OPTIONS]

ARGUMENTS:
    <sourcePath>    Path to search
    <targetPath>    Path to write files

OPTIONS:
                       DEFAULT
    -h, --help                    Prints help information
    -o, --overwrite    True
```

Files are copies to a subfolder that corresponds to the year that the photo was taken. Files that cannot be processed will be moved to a sup folder called unprocessed.

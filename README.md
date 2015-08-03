**EncodingMixToUtf8** is file encoding detection and trasformation to **UTF-8** command line tool.

## Content

* [Why?](#why)
* [Known workarounds](#known-workarounds)
* [How it works](#how-it-works)
* [Command Line Options](#command-line-options)
* [Known issues](#known-issues)
* [Usage examples](#usage-examples)

<a name="why"></a>
## Why?

Visual Studio 2015 RTM ["introduces" compiler bug](https://github.com/dotnet/roslyn/issues/4022) in processing source files encoded with non-UTF-8 encoding. So this is a tool to convert source files to UTF-8 as a quick workaround. I've used it for our repositories. Hope it will be useful for someone else.

<a name="known-workarounds"></a>
## Known workarounds

* Jared Parsons (jaredpar) suggestion:

> You can explicitly specify the code page in the project settings using the CodePage element in the csproj file:

```
<CodePage>1251</CodePage>
```

* Kevin Pilch-Bisson (Pilchie) suggestion:

> You can also run msbuild with the `/p:CodePage=1251` command line option when building on the command line, or set the environment variable `CodePage` to `1251` for VS to pick it up as well.


* KoHHeKT suggestion for manually convert:

> Tools - Options - Environment - Documents - Save Document as Unicode if cannot be saved in Codepage


* You can also use any text editor with support to detect encoding and save as UTF-8. E.g. [Notepad++](https://notepad-plus-plus.org/)

<a name="how-it-works"></a>
## How it works

The tool technically is pretty simple and straightforward except for encoding auto-detect.

For encoding detection I've used package [SimpleHelpers.FileEncoding](https://www.nuget.org/packages/SimpleHelpers.FileEncoding) by [Khalid Salomão](https://github.com/khalidsalomao) for convenience. It wraps [C# port](https://code.google.com/p/ude/) of encoding detection algorithm based on [Mozilla Universal Charset Detector](http://www-archive.mozilla.org/projects/intl/UniversalCharsetDetection.html)

I wish to thanks all the authors for their hard work. It greatly simplifies the task.


<a name="command-line-options"></a>
## Command Line Options

```
  -p, --path                Required. Root directory path to start scanning
                            from

  -m, --search-extension    (Default: .cs;.vb;.settings;.resx) Search file
                            extensions

  -c, --codepage            Restrictions on converting from selected codepage
                            only. Highly recommended to convert only selected
                            codepages instead of everything due to chances of
                            false encoding detection.

  -o, --override            Override encoding detection to correct detection
                            errors for similar encodings (e.g. windows-1251 vs
                            x-mac-cyrillic). Format:
                            detected_code_page=mapped_code_page; e.g.
                            10007=1251; to map x-mac-cyrillic on windows-1251

  -t, --transform           Do text transformation (by default is check only
                            mode)

  -b, --backup              Backup path (modified files go there before
                            processing)

  -l, --log                 (Default: log.txt) Logging file

  --help                    Display this help screen.
```

<a name="known-issues"></a>
## Known issues

* I strongly recommend to run tool in first time **without** `-t, --transform` and without `-c, --codepage` (to get encodings for all source files) and check result log file (by default is `log.txt`) how detection completed. Checking detection before doing transformation helps you to avoid potentially bad conversion in case where detection can go wrong. You can pre convert such files manually or specify `-o, --override` option.

* I also highly recommend using only specified codepage conversion via `-c, --codepage`. You can repeat conversion for other encodings one by one and assure results. At least *carefully* check log file with encodings distribution (in scan only mode) before doing any transformations.

* There are can be a false detection of similar encoding, e.g. **windows-1251** can be detected as **x-mac-cyrillic**. You can use `-o, --override` option to specify replacement encoding in such condition.

* If you do not use any of version control systems specify `Backup path` via `-b, --backup` to avoid text corrupt during conversion. It preserves the original folder structure.

* Any errors (e.g. IO) during the transformation abort conversion potentially leaving files in a partially modified state.

<a name="usage-examples"></a>
## Usage examples

* Check encodings in `w:\Src\Path`

```
> EncodingMixToUtf8.exe -p w:\Src\Path
```

* Check for `windows-1251` encoding only in `w:\Src\Path`. Recommended to do a full scan first to check false detections.

```
> EncodingMixToUtf8.exe -p w:\Src\Path -c 1251
```

* Check for `windows-1251` encoding only with forced overriding `x-mac-cyrillic` to `windows-1251` in `w:\Src\Path`. A log file will contain files with `windows-1251` encoding only.

```
> EncodingMixToUtf8.exe -p w:\Src\Path -c 1251 -o 10007=1251
```

* Transform source files in `w:\Src\Path` from `windows-1251` encoding (with substitution `x-mac-cyrillic` to `windows-1251`) and backing up files to `w:\Src\Backup`

```
> EncodingMixToUtf8.exe -p w:\Src\Path -t -c 1251 -o 10007=1251 -b w:\Src\Backup
```

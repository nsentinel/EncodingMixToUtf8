using CommandLine;
using CommandLine.Text;

namespace EncodingMixToUtf8
{
    /// <summary>
    /// Command line parsed options
    /// </summary>
    internal class Options
    {
        /// <summary>
        /// Root directory path to start scanning from
        /// </summary>
        [Option('p', "path", Required = true, HelpText = "Root directory path to start scanning from")]
        public string PathToScan { get; set; }

        /// <summary>
        /// Gets or sets the search extension for files
        /// </summary>
        [Option('m', "search-extension", DefaultValue = ".cs;.vb;.settings;.resx", HelpText = "Search file extensions")]
        public string SearchExtensions { get; set; }

        /// <summary>
        /// Gets or sets the restrictions to convert from selected code page only.
        /// </summary>
        [Option('c',"codepage", HelpText = "Restrictions on converting from selected codepage only. Highly recommended to convert only selected codepages instead of everything due to chances of false encoding detection.")]
        public string ConvertFromSelectedCodePageOnly { get; set; }

        /// <summary>
        /// Gets or sets the override encoding map.
        /// </summary>
        [Option('o', "override", HelpText = "Override encoding detection to correct detection errors for similar encodings (e.g. windows-1251 vs x-mac-cyrillic). Format: detected_code_page=mapped_code_page; e.g. 10007=1251; to map x-mac-cyrillic on windows-1251")]
        public string OverrideEncodingMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether do text transform or check only
        /// </summary>
        [Option('t', "transform", HelpText = "Do text transformation (by default is check only mode)")]
        public bool Transform { get; set; }

        /// <summary>
        /// Gets or sets the path to backup.
        /// </summary>
        [Option('b', "backup", HelpText = "Backup path (modified files go there before processing)")]
        public string PathToBackup { get; set; }

        /// <summary>
        /// Gets or sets the log filename.
        /// </summary>
        [Option('l', "log", DefaultValue = "log.txt", HelpText = "Logging file")]
        public string LogFilename { get; set; }

        /// <summary>
        /// Gets or sets the last state of the parser.
        /// </summary>
        [ParserState]
        public IParserState LastParserState { get; set; }

        /// <summary>
        /// Gets the usage info
        /// </summary>
        /// <returns>Builded usage help </returns>
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
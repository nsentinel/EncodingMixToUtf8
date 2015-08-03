using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using EncodingMixToUtf8.SimpleHelpers;

namespace EncodingMixToUtf8
{
    internal class Program
    {
        private static void Main()
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(Environment.GetCommandLineArgs(), options)) return;

            char[] separator = {';'};

            string[] exts = options.SearchExtensions.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var encodingMap = new Dictionary<Encoding, List<string>>();
            var unknownEncodingFiles = new List<string>();

            int usingEncoding = string.IsNullOrWhiteSpace(options.ConvertFromSelectedCodePageOnly)
                ? 0
                : int.Parse(options.ConvertFromSelectedCodePageOnly);

            if (usingEncoding != 0) Console.WriteLine($"Processing restricted to codepage {usingEncoding}");

            var overrideMap = new Dictionary<int, Encoding>();
            if (!string.IsNullOrWhiteSpace(options.OverrideEncodingMap))
            {
                foreach (string rule in options.OverrideEncodingMap.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = rule.Split(" =".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    int fromCodePage, toCodePage;
                    if ((parts.Length != 2) || !int.TryParse(parts[0], out fromCodePage) ||
                        !int.TryParse(parts[1], out toCodePage))
                    {
                        Console.WriteLine($"Error parsing override rule: {rule}");
                        continue;
                    }

                    Console.WriteLine($"Overriding codepage {fromCodePage} to {toCodePage}");

                    overrideMap.Add(fromCodePage, Encoding.GetEncoding(toCodePage));
                }
            }

            bool makeBackup = false;
            if (!string.IsNullOrWhiteSpace(options.PathToBackup))
            {
                makeBackup = true;
                Console.WriteLine($"Backing up to {options.PathToBackup}");
            }

            Console.WriteLine(options.Transform ? "Converting files..." : "Scanning files...");

            foreach (string file in Directory.EnumerateFiles(options.PathToScan, "*.*", SearchOption.AllDirectories).
                Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase))))
            {
                Encoding encoding = FileEncoding.DetectFileEncoding(file);

                if (encoding != null)
                {
                    int codePage = encoding.CodePage;
                    Encoding overridedCodePage;

                    if (overrideMap.TryGetValue(codePage, out overridedCodePage))
                    {
                        encoding = overridedCodePage;
                        codePage = overridedCodePage.CodePage;
                    }

                    if ((usingEncoding != 0) && (codePage != usingEncoding)) continue;

                    List<string> files;
                    if (!encodingMap.TryGetValue(encoding, out files))
                    {
                        files = new List<string>();
                        encodingMap.Add(encoding, files);
                    }

                    files.Add(file);

                    if (Equals(encoding, Encoding.UTF8)) continue;

                    if (options.Transform)
                    {
                        if (makeBackup)
                        {
                            string relativePath = GetRelativePath(file, options.PathToScan);
                            string backupPath = Path.Combine(options.PathToBackup, relativePath);

                            Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                            File.Copy(file, backupPath);
                        }

                        string text = File.ReadAllText(file, encoding);
                        File.WriteAllText(file, text, Encoding.UTF8);
                    }
                }
                else
                {
                    unknownEncodingFiles.Add(file);
                }
            }

            var sb = new StringBuilder();

            sb.Append($"Found encodings: {Environment.NewLine}\t");
            sb.AppendLine(
                $"{string.Join(Environment.NewLine+"\t", encodingMap.Select(pair => $"{pair.Key.WebName} [{pair.Key.EncodingName}] <{pair.Key.CodePage}>: {pair.Value.Count}"))}");
            sb.AppendLine($"\tUnknown: {unknownEncodingFiles.Count}");

            sb.AppendLine();
            sb.AppendLine();

            sb.Append(string.Join(Environment.NewLine,
                    encodingMap.Select(
                        pair =>
                            $"{pair.Key.WebName} [{pair.Key.EncodingName}] <{pair.Key.CodePage}>:{Environment.NewLine}\t{string.Join(Environment.NewLine+ "\t", pair.Value)}{Environment.NewLine}")));

            sb.Append(
                $"{Environment.NewLine}UnknownEncoding:{Environment.NewLine}\t{string.Join(Environment.NewLine+ "\t", unknownEncodingFiles)}");

            File.WriteAllText(options.LogFilename, sb.ToString());
        }

        /// <summary>
        /// Gets the relative path to specified file using provided folder as root
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="folder">Root folder.</param>
        /// <returns>Builded relative path</returns>
        private static string GetRelativePath(string filename, string folder)
        {
            Uri pathUri = new Uri(filename);

            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

    }

}

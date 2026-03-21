using System.Text;
using System.Text.RegularExpressions;
using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Parsing;

/// <summary>
/// Parses Paradox-style YAML localization files.
/// </summary>
public sealed partial class ParadoxLocalizationParser
{
    /// <summary>
    /// Pattern to match localization entry: key:version "value"
    /// </summary>
    private static readonly Regex EntryPattern = new(
        """^\s*(?<key>[\w._\[\]]+):(?<version>\d+)?\s*"(?<value>.*)"\s*$""",
        RegexOptions.Compiled);

    /// <summary>
    /// Parses a single localization file.
    /// </summary>
    /// <param name="filePath">Path to the .yml file.</param>
    /// <returns>Parsed localization file.</returns>
    /// <exception cref="ArgumentException">Thrown when file path is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when file format is invalid.</exception>
    public LocalizationFile ParseFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Localization file not found", filePath);

        string content = ReadFileWithBomHandling(filePath);
        string[] lines = [..content
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && line[0] != '#')]; // Игнорируем комментарии

        if (lines.Length == 0)
            throw new InvalidDataException("Empty localization file");
        string language = ParseLanguageTag(lines[0]);
        List<LocalizationEntry> entries = [];

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            int lineNumber = i + 1;

            if (ShouldSkipLine(line))
                continue;

            LocalizationEntry? entry = TryParseEntry(line, lineNumber);
            if (entry is not null)
                entries.Add(entry);
        }

        return new(language, filePath, entries);
    }

    /// <summary>
    /// Parses all localization files in a directory.
    /// </summary>
    /// <param name="directoryPath">Path to the directory.</param>
    /// <param name="pattern">File pattern (default: "*.yml").</param>
    /// <returns>List of parsed localization files.</returns>
    /// <exception cref="ArgumentException">Thrown when directory path is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when directory does not exist.</exception>
    public List<LocalizationFile> ParseDirectory(string directoryPath, string pattern = "*.yml")
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        string[] files = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories);
        List<LocalizationFile> result = [];

        foreach (string file in files)
        {
            try
            {
                LocalizationFile parsed = ParseFile(file);
                result.Add(parsed);
            }
            catch (InvalidDataException e)
            {
                Console.WriteLine($"{file}: {e.Message}");
                // Skip files that don't have valid language tag
                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// Reads file content handling UTF-8 BOM.
    /// </summary>
    private static string ReadFileWithBomHandling(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);

        // Skip UTF-8 BOM if present
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes[3..]);

        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Parses the language tag from the first line (e.g., "l_english:").
    /// </summary>
    private static string ParseLanguageTag(string firstLine)
    {
        string trimmed = firstLine.Trim();

        if (!trimmed.StartsWith('l') || !trimmed.Contains(':'))
            throw new InvalidDataException($"Invalid language tag: {trimmed}");

        int colonIndex = trimmed.IndexOf(':');
        string language = trimmed[1..colonIndex].TrimStart('_');
        return language;
    }

    /// <summary>
    /// Checks if line should be skipped (empty or comment).
    /// </summary>
    private static bool ShouldSkipLine(string line)
    {
        string trimmed = line.Trim();
        return string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#');
    }

    /// <summary>
    /// Tries to parse a single localization entry.
    /// </summary>
    private static LocalizationEntry? TryParseEntry(string line, int lineNumber)
    {
        Match match = EntryPattern.Match(line);

        if (!match.Success)
            return null;

        string key = match.Groups["key"].Value;
        string rawValue = match.Groups["value"].Value;
        string value = UnescapeValue(rawValue);

        int? version = null;
        if (match.Groups["version"].Success)
            version = int.Parse(match.Groups["version"].Value);

        return new(key, version, value, rawValue, lineNumber);
    }

    /// <summary>
    /// Unescapes special characters in the value.
    /// </summary>
    private static string UnescapeValue(string value) =>
        value.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"");
}

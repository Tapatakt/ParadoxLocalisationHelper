using System.Text;
using ParadoxLocalisationHelper.Analysis.Models;
using ParadoxLocalisationHelper.Comparison.Models;
using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Yml;

/// <summary>
/// Writes localization entries to YML files.
/// </summary>
public sealed class LocalizationYmlWriter
{
    /// <summary>
    /// Writes delta entries (added and modified keys) to multiple YML files in a directory.
    /// Files are created based on the structure of the new version.
    /// </summary>
    /// <param name="result">The comparison result.</param>
    /// <param name="outputDirectory">The output directory path.</param>
    /// <param name="language">The language code.</param>
    /// <param name="newVersionFiles">The new version files to use as template.</param>
    /// <returns>List of information about written files.</returns>
    public List<WrittenFileInfo> WriteDeltaYmlFiles(
        ComparisonResult result,
        string outputDirectory,
        string language,
        IEnumerable<LocalizationFile> newVersionFiles)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(outputDirectory);
        ArgumentException.ThrowIfNullOrEmpty(language);
        ArgumentNullException.ThrowIfNull(newVersionFiles);

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        List<WrittenFileInfo> writtenFiles = [];
        Dictionary<string, KeyWithSource> addedByKey = result.Added.ToDictionary(k => k.Key);
        Dictionary<string, Modification> modifiedByKey = result.Modified.ToDictionary(m => m.Key);

        foreach (LocalizationFile newFile in newVersionFiles)
        {
            List<LocalizationEntry> entriesToWrite = [];

            foreach (LocalizationEntry entry in newFile.Entries)
            {
                if (addedByKey.TryGetValue(entry.Key, out KeyWithSource? added))
                {
                    entriesToWrite.Add(new(entry.Key, entry.Version, added.Value, added.Value, entry.LineNumber));
                }
                else if (modifiedByKey.TryGetValue(entry.Key, out Modification? modified))
                {
                    entriesToWrite.Add(new(entry.Key, entry.Version, modified.NewValue, modified.NewValue, entry.LineNumber));
                }
            }

            if (entriesToWrite.Count == 0)
                continue;

            string outputFileName = newFile.FileName.Replace($"l_{newFile.Language}", $"l_{language}");
            string outputPath = Path.Combine(outputDirectory, outputFileName);
            LocalizationFile outputFile = new(language, outputPath, entriesToWrite);
            WriteFile(outputFile, outputPath);

            int charCount = entriesToWrite.Sum(e => e.Value.Length);
            writtenFiles.Add(new(outputPath, entriesToWrite.Count, charCount));
        }

        return writtenFiles;
    }

    /// <summary>
    /// Writes missing keys to multiple YML files in a directory.
    /// Files are created based on the structure of the new version.
    /// </summary>
    /// <param name="result">The missing keys result.</param>
    /// <param name="outputDirectory">The output directory path.</param>
    /// <param name="language">The language code.</param>
    /// <param name="newVersionFiles">The new version files to use as template.</param>
    /// <returns>List of information about written files.</returns>
    public List<WrittenFileInfo> WriteMissingKeysYmlFiles(
        MissingKeysResult result,
        string outputDirectory,
        string language,
        IEnumerable<LocalizationFile> newVersionFiles)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(outputDirectory);
        ArgumentException.ThrowIfNullOrEmpty(language);
        ArgumentNullException.ThrowIfNull(newVersionFiles);

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        List<WrittenFileInfo> writtenFiles = [];
        Dictionary<string, MissingKeyInfo> missingByKey = result.MissingKeys.ToDictionary(k => k.Key);

        foreach (LocalizationFile newFile in newVersionFiles)
        {
            List<LocalizationEntry> entriesToWrite = [];

            foreach (LocalizationEntry entry in newFile.Entries)
            {
                if (missingByKey.TryGetValue(entry.Key, out MissingKeyInfo? missing))
                    entriesToWrite.Add(new(entry.Key, entry.Version, missing.SourceValue, missing.SourceValue, entry.LineNumber));
            }

            if (entriesToWrite.Count == 0)
                continue;

            string outputFileName = newFile.FileName.Replace($"l_{newFile.Language}", $"l_{language}");
            string outputPath = Path.Combine(outputDirectory, outputFileName);
            LocalizationFile outputFile = new(language, outputPath, entriesToWrite);
            WriteFile(outputFile, outputPath);

            int charCount = entriesToWrite.Sum(e => e.Value.Length);
            writtenFiles.Add(new(outputPath, entriesToWrite.Count, charCount));
        }

        return writtenFiles;
    }

    /// <summary>
    /// Writes a collection of localization files to an output directory.
    /// </summary>
    /// <param name="files">The files to write.</param>
    /// <param name="outputDirectory">The output directory.</param>
    public void WriteFiles(IEnumerable<LocalizationFile> files, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentException.ThrowIfNullOrEmpty(outputDirectory);

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        foreach (LocalizationFile file in files)
        {
            string outputPath = Path.Combine(outputDirectory, file.FileName);
            WriteFile(file, outputPath);
        }
    }

    /// <summary>
    /// Writes a single localization file.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <param name="outputPath">The output path.</param>
    public void WriteFile(LocalizationFile file, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        StringBuilder sb = new();
        sb.AppendLine($"l_{file.Language}:");
        sb.AppendLine();

        foreach (LocalizationEntry entry in file.Entries)
        {
            string versionSuffix = entry.Version.HasValue ? $":{entry.Version.Value}" : "";
            string escapedValue = EscapeValue(entry.Value);
            sb.AppendLine($" {entry.Key}{versionSuffix}: \"{escapedValue}\"");
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string EscapeValue(string value) =>
        value.Replace("\\", "\\\\")
             .Replace("\"", "\\\"")
             .Replace("\n", "\\n")
             .Replace("\r", "\\r")
             .Replace("\t", "\\t");
}

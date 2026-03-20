using System.Text;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Storage;

namespace ParadoxLocalisationHelper.Yml;

/// <summary>
/// Writes localization entries to YML files.
/// </summary>
public sealed class LocalizationYmlWriter
{
    /// <summary>
    /// Writes keys from storage to YML files using template structure.
    /// </summary>
    public List<WrittenFileInfo> WriteStorage(
        LocalizationStorage storage,
        string outputDirectory,
        string language)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentException.ThrowIfNullOrEmpty(outputDirectory);
        ArgumentException.ThrowIfNullOrEmpty(language);

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        List<WrittenFileInfo> writtenFiles = [];

        foreach ((string filePath, LocalizationFile file) in storage.Files)
        {
            if (file.Entries.Count == 0)
                continue;

            string outputFileName = Path.GetFileName(filePath).Replace($"l_{file.Language}", $"l_{language}");
            string outputPath = Path.Combine(outputDirectory, outputFileName);
            LocalizationFile outputFile = new(language, outputPath, file.Entries);
            WriteFile(outputFile, outputPath);

            int charCount = file.Entries.Sum(e => e.Value.Length);
            writtenFiles.Add(new(outputPath, file.Entries.Count, charCount));
        }

        return writtenFiles;
    }

    /// <summary>
    /// Writes multiple files to a directory.
    /// </summary>
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
    public void WriteFile(LocalizationFile file, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        StringBuilder sb = new();
        sb.AppendLine($"l_{file.Language}:");
        sb.AppendLine();

        foreach (LocalizationEntry entry in file.Entries)
        {
            string suffix = entry.Version.HasValue ? $":{entry.Version.Value}" : ":";
            string escapedValue = EscapeValue(entry.Value);
            sb.AppendLine($" {entry.Key}{suffix} \"{escapedValue}\"");
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

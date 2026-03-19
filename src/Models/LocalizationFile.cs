using System.Collections.Immutable;

namespace ParadoxLocalisationHelper.Models;

/// <summary>
/// Represents a parsed localization file with all its entries.
/// </summary>
public sealed class LocalizationFile
{
    /// <summary>
    /// The language code (e.g., "english", "russian").
    /// </summary>
    public string Language { get; }

    /// <summary>
    /// The full path to the file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The file name without path.
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// All localization entries in the file.
    /// </summary>
    public ImmutableList<LocalizationEntry> Entries { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationFile"/> class.
    /// </summary>
    /// <param name="language">The language code.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="entries">The list of entries.</param>
    public LocalizationFile(
        string language,
        string filePath,
        IEnumerable<LocalizationEntry> entries)
    {
        ArgumentException.ThrowIfNullOrEmpty(language);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        Language = language;
        FilePath = filePath;
        Entries = [.. entries];
    }
}

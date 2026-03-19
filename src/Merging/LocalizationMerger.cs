using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Storage;

namespace ParadoxLocalisationHelper.Merging;

/// <summary>
/// Merges outdated translation with translation update.
/// </summary>
public sealed class LocalizationMerger
{
    /// <summary>
    /// Merges old translation with new translation update.
    /// Values from update take precedence over old values.
    /// File structure is taken from the new version.
    /// </summary>
    /// <param name="oldTranslation">The outdated translation storage.</param>
    /// <param name="newTranslation">The translation update storage.</param>
    /// <param name="newVersion">The new version storage (for file structure).</param>
    /// <param name="targetLanguage">The target language code.</param>
    /// <returns>List of merged localization files.</returns>
    public List<LocalizationFile> Merge(
        LocalizationStorage oldTranslation,
        LocalizationStorage newTranslation,
        LocalizationStorage newVersion,
        string targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(oldTranslation);
        ArgumentNullException.ThrowIfNull(newTranslation);
        ArgumentNullException.ThrowIfNull(newVersion);
        ArgumentException.ThrowIfNullOrEmpty(targetLanguage);

        List<LocalizationFile> result = [];

        // Use file structure from new version
        foreach (LocalizationFile newVersionFile in newVersion.Files.Values)
        {
            LocalizationFile mergedFile = MergeFile(
                newVersionFile,
                oldTranslation,
                newTranslation,
                targetLanguage);
            result.Add(mergedFile);
        }

        return result;
    }

    private LocalizationFile MergeFile(
        LocalizationFile newVersionFile,
        LocalizationStorage oldTranslation,
        LocalizationStorage newTranslation,
        string targetLanguage)
    {
        List<LocalizationEntry> mergedEntries = [];

        foreach (LocalizationEntry entry in newVersionFile.Entries)
        {
            string key = entry.Key;
            LocalizationEntry? mergedEntry = CreateMergedEntry(
                key,
                entry.Version,
                oldTranslation,
                newTranslation);

            if (mergedEntry is not null)
                mergedEntries.Add(mergedEntry);
        }

        string newFilePath = newVersionFile.FilePath.Replace(
            $"l_{newVersionFile.Language}",
            $"l_{targetLanguage}");

        return new(targetLanguage, newFilePath, mergedEntries);
    }

    private LocalizationEntry? CreateMergedEntry(
        string key,
        int? version,
        LocalizationStorage oldTranslation,
        LocalizationStorage newTranslation)
    {
        // Priority: new translation > old translation
        LocalizationEntry? newEntry = newTranslation.GetEntry(key);
        if (newEntry is not null)
            return new(key, version, newEntry.Value, newEntry.RawValue, newEntry.LineNumber);

        LocalizationEntry? oldEntry = oldTranslation.GetEntry(key);
        if (oldEntry is not null)
            return new(key, version, oldEntry.Value, oldEntry.RawValue, oldEntry.LineNumber);

        return null;
    }
}

using ParadoxLocalisationHelper.Analysis.Models;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Storage;

namespace ParadoxLocalisationHelper.Analysis;

/// <summary>
/// Analyzes localization storages to find missing translation keys and other issues.
/// </summary>
public sealed class MissingKeysAnalyzer
{
    /// <summary>
    /// Finds keys that exist in the source but are missing from the translation.
    /// </summary>
    /// <param name="source">The source localization storage (e.g., English).</param>
    /// <param name="translation">The target translation storage (e.g., Russian).</param>
    /// <returns>Result containing all missing keys and statistics.</returns>
    public static MissingKeysResult FindMissingKeys(
        LocalizationStorage source,
        LocalizationStorage translation)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(translation);

        List<MissingKeyInfo> missing = [];
        int translatedCount = 0;
        int totalCharacters = 0;
        int translatedCharacters = 0;

        foreach (string key in source.GetAllKeys())
        {
            LocalizationEntry entry = source[key]!;
            int valueLength = entry.Value.Length;
            totalCharacters += valueLength;

            if (translation.ContainsKey(key))
            {
                translatedCount++;
                translatedCharacters += valueLength;
                continue;
            }

            string sourceFile = source.GetSourceFile(key) ?? "unknown";
            missing.Add(new(key, entry.Value, sourceFile));
        }

        return new(
            missing,
            source.KeyCount,
            translatedCount,
            totalCharacters,
            translatedCharacters);
    }

    /// <summary>
    /// Performs comprehensive analysis of translation including missing, orphaned and duplicate keys.
    /// </summary>
    /// <param name="source">The source localization storage.</param>
    /// <param name="translation">The target translation storage.</param>
    /// <returns>Comprehensive analysis result.</returns>
    public static TranslationAnalysisResult AnalyzeTranslation(
        LocalizationStorage source,
        LocalizationStorage translation)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(translation);

        List<MissingKeyInfo> missing = [];
        List<OrphanedKeyInfo> orphaned = [];

        // Find missing keys (in source but not in translation)
        foreach (string key in source.GetAllKeys())
        {
            LocalizationEntry entry = source[key]!;

            if (!translation.ContainsKey(key))
            {
                string sourceFile = source.GetSourceFile(key) ?? "unknown";
                missing.Add(new(key, entry.Value, sourceFile));
            }
        }

        // Find orphaned keys and track duplicates in translation
        Dictionary<string, List<string>> translationOccurrences = [];

        foreach (string key in translation.GetAllKeys())
        {
            LocalizationEntry entry = translation[key]!;

            // Check if orphaned (in translation but not in source)
            if (!source.ContainsKey(key))
            {
                string sourceFile = translation.GetSourceFile(key) ?? "unknown";
                orphaned.Add(new(key, entry.Value, sourceFile));
            }

            // Track occurrences for duplicate detection (only in translation)
            string filePath = translation.GetSourceFile(key) ?? "unknown";
            if (!translationOccurrences.TryGetValue(key, out List<string>? list))
            {
                list = [];
                translationOccurrences[key] = list;
            }
            if (!list.Contains(filePath))
                list.Add(filePath);
        }

        // Find duplicates (keys appearing in multiple files within translation)
        List<DuplicateKeyInfo> duplicates = [];
        foreach ((string key, List<string> files) in translationOccurrences)
        {
            if (files.Count > 1)
                duplicates.Add(new(key, files.Count, files));
        }

        return new(
            missing,
            orphaned,
            duplicates,
            source.KeyCount,
            translation.KeyCount);
    }
}

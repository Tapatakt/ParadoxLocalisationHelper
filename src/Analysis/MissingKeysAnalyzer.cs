using ParadoxLocalisationHelper.Analysis.Models;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Storage;

namespace ParadoxLocalisationHelper.Analysis;

/// <summary>
/// Analyzes localization storages to find missing translation keys.
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
            LocalizationEntry entry = source.GetEntry(key)!;
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
}

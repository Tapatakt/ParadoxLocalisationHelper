using System.Collections.Immutable;

namespace ParadoxLocalisationHelper.Analysis.Models;

/// <summary>
/// Result of analyzing missing translation keys.
/// </summary>
public sealed class MissingKeysResult
{
    /// <summary>
    /// List of keys that are missing from the translation.
    /// </summary>
    public ImmutableList<MissingKeyInfo> MissingKeys { get; }

    /// <summary>
    /// Total number of keys in the source.
    /// </summary>
    public int TotalSourceKeys { get; }

    /// <summary>
    /// Number of translated keys.
    /// </summary>
    public int TranslatedKeys { get; }

    /// <summary>
    /// Number of missing keys.
    /// </summary>
    public int MissingCount => MissingKeys.Count;

    /// <summary>
    /// Percentage of translated keys by count (0-100).
    /// </summary>
    public double TranslationPercentageByCount =>
        TotalSourceKeys == 0 ? 0 : (double)TranslatedKeys / TotalSourceKeys * 100;

    /// <summary>
    /// Total number of characters in source values.
    /// </summary>
    public int TotalSourceCharacters { get; }

    /// <summary>
    /// Number of characters in translated values.
    /// </summary>
    public int TranslatedCharacters { get; }

    /// <summary>
    /// Number of characters in missing values.
    /// </summary>
    public int MissingCharacters => MissingKeys.Sum(k => k.SourceValueLength);

    /// <summary>
    /// Percentage of translated characters (0-100).
    /// </summary>
    public double TranslationPercentageByCharacters =>
        TotalSourceCharacters == 0 ? 0 : (double)TranslatedCharacters / TotalSourceCharacters * 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingKeysResult"/> class.
    /// </summary>
    public MissingKeysResult(
        IEnumerable<MissingKeyInfo> missingKeys,
        int totalSourceKeys,
        int translatedKeys,
        int totalSourceCharacters,
        int translatedCharacters)
    {
        MissingKeys = missingKeys.ToImmutableList();
        TotalSourceKeys = totalSourceKeys;
        TranslatedKeys = translatedKeys;
        TotalSourceCharacters = totalSourceCharacters;
        TranslatedCharacters = translatedCharacters;
    }
}

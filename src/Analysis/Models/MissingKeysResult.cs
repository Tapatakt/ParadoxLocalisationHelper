using System.Collections.Immutable;

namespace ParadoxLocalisationHelper.Analysis.Models;

/// <summary>
/// Result of analyzing missing translation keys.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MissingKeysResult"/> class.
/// </remarks>
public sealed class MissingKeysResult(
    IEnumerable<MissingKeyInfo> missingKeys,
    int totalSourceKeys,
    int translatedKeys,
    int totalSourceCharacters,
    int translatedCharacters)
{
    /// <summary>
    /// List of keys that are missing from the translation.
    /// </summary>
    public ImmutableList<MissingKeyInfo> MissingKeys { get; } = missingKeys.ToImmutableList();

    /// <summary>
    /// Total number of keys in the source.
    /// </summary>
    public int TotalSourceKeys { get; } = totalSourceKeys;

    /// <summary>
    /// Number of translated keys.
    /// </summary>
    public int TranslatedKeys { get; } = translatedKeys;

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
    public int TotalSourceCharacters { get; } = totalSourceCharacters;

    /// <summary>
    /// Number of characters in translated values.
    /// </summary>
    public int TranslatedCharacters { get; } = translatedCharacters;

    /// <summary>
    /// Number of characters in missing values.
    /// </summary>
    public int MissingCharacters => MissingKeys.Sum(k => k.SourceValueLength);

    /// <summary>
    /// Percentage of translated characters (0-100).
    /// </summary>
    public double TranslationPercentageByCharacters =>
        TotalSourceCharacters == 0 ? 0 : (double)TranslatedCharacters / TotalSourceCharacters * 100;
}

namespace ParadoxLocalisationHelper.Analysis.Models;

/// <summary>
/// Result of comprehensive translation analysis.
/// </summary>
public sealed class TranslationAnalysisResult
{
    /// <summary>
    /// Keys that exist in source but are missing from translation.
    /// </summary>
    public List<MissingKeyInfo> MissingKeys { get; }

    /// <summary>
    /// Keys that exist in translation but not in source (orphaned).
    /// </summary>
    public List<OrphanedKeyInfo> OrphanedKeys { get; }

    /// <summary>
    /// Keys that appear multiple times in translation (duplicates).
    /// </summary>
    public List<DuplicateKeyInfo> DuplicateKeys { get; }

    /// <summary>
    /// Total keys in source.
    /// </summary>
    public int SourceKeyCount { get; }

    /// <summary>
    /// Total keys in translation.
    /// </summary>
    public int TranslationKeyCount { get; }

    /// <summary>
    /// Number of orphaned keys.
    /// </summary>
    public int OrphanedCount => OrphanedKeys.Count;

    /// <summary>
    /// Number of duplicate keys.
    /// </summary>
    public int DuplicateCount => DuplicateKeys.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationAnalysisResult"/> class.
    /// </summary>
    public TranslationAnalysisResult(
        List<MissingKeyInfo> missingKeys,
        List<OrphanedKeyInfo> orphanedKeys,
        List<DuplicateKeyInfo> duplicateKeys,
        int sourceKeyCount,
        int translationKeyCount)
    {
        MissingKeys = missingKeys;
        OrphanedKeys = orphanedKeys;
        DuplicateKeys = duplicateKeys;
        SourceKeyCount = sourceKeyCount;
        TranslationKeyCount = translationKeyCount;
    }
}

/// <summary>
/// Information about an orphaned key (exists in translation but not in source).
/// </summary>
public sealed class OrphanedKeyInfo
{
    /// <summary>
    /// The localization key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The translation value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The translation file path.
    /// </summary>
    public string SourceFile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedKeyInfo"/> class.
    /// </summary>
    public OrphanedKeyInfo(string key, string value, string sourceFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        Key = key;
        Value = value;
        SourceFile = sourceFile;
    }
}

/// <summary>
/// Information about a duplicate key.
/// </summary>
public sealed class DuplicateKeyInfo
{
    /// <summary>
    /// The localization key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Number of occurrences.
    /// </summary>
    public int OccurrenceCount { get; }

    /// <summary>
    /// Files where the key appears.
    /// </summary>
    public List<string> Files { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyInfo"/> class.
    /// </summary>
    public DuplicateKeyInfo(string key, int occurrenceCount, List<string> files)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentOutOfRangeException.ThrowIfLessThan(occurrenceCount, 2);

        Key = key;
        OccurrenceCount = occurrenceCount;
        Files = files;
    }
}

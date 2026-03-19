namespace ParadoxLocalisationHelper.Analysis.Models;

/// <summary>
/// Information about a missing translation key.
/// </summary>
public sealed class MissingKeyInfo
{
    /// <summary>
    /// The localization key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The source (original) value.
    /// </summary>
    public string SourceValue { get; }

    /// <summary>
    /// The length of the source value in characters.
    /// </summary>
    public int SourceValueLength { get; }

    /// <summary>
    /// The source file path.
    /// </summary>
    public string SourceFile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingKeyInfo"/> class.
    /// </summary>
    public MissingKeyInfo(string key, string sourceValue, string sourceFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        Key = key;
        SourceValue = sourceValue;
        SourceValueLength = sourceValue.Length;
        SourceFile = sourceFile;
    }
}

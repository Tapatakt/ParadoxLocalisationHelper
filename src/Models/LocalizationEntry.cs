namespace ParadoxLocalisationHelper.Models;

/// <summary>
/// Represents a single localization entry with key, value and metadata.
/// </summary>
public sealed class LocalizationEntry
{
    /// <summary>
    /// The localization key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The version suffix (e.g., :0, :1) if present.
    /// </summary>
    public int? Version { get; }

    /// <summary>
    /// The localized value with escape sequences processed.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The raw value as it appears in the file (including quotes).
    /// </summary>
    public string RawValue { get; }

    /// <summary>
    /// The line number in the source file.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationEntry"/> class.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="version">The version number or null.</param>
    /// <param name="value">The processed value.</param>
    /// <param name="rawValue">The raw value from file.</param>
    /// <param name="lineNumber">The line number.</param>
    public LocalizationEntry(
        string key,
        int? version,
        string value,
        string rawValue,
        int lineNumber)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineNumber);

        Key = key;
        Version = version;
        Value = value;
        RawValue = rawValue;
        LineNumber = lineNumber;
    }
}

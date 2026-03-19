namespace ParadoxLocalisationHelper.Comparison.Models;

/// <summary>
/// Represents a localization key with its value and source file.
/// </summary>
public sealed class KeyWithSource
{
    /// <summary>
    /// The localization key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The localized value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The source file path.
    /// </summary>
    public string SourceFile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyWithSource"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="sourceFile">The source file.</param>
    public KeyWithSource(string key, string value, string sourceFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        Key = key;
        Value = value;
        SourceFile = sourceFile;
    }
}

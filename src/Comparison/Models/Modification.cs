namespace ParadoxLocalisationHelper.Comparison.Models;

/// <summary>
/// Represents a modified localization entry between two versions.
/// </summary>
public sealed class Modification
{
    /// <summary>
    /// The localization key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The old value.
    /// </summary>
    public string OldValue { get; }

    /// <summary>
    /// The new value.
    /// </summary>
    public string NewValue { get; }

    /// <summary>
    /// The source file of the old value.
    /// </summary>
    public string OldFile { get; }

    /// <summary>
    /// The source file of the new value.
    /// </summary>
    public string NewFile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Modification"/> class.
    /// </summary>
    public Modification(
        string key,
        string oldValue,
        string newValue,
        string oldFile,
        string newFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
        OldFile = oldFile;
        NewFile = newFile;
    }
}

namespace ParadoxLocalisationHelper.Yml;

/// <summary>
/// Information about a written YML file.
/// </summary>
public sealed class WrittenFileInfo
{
    /// <summary>
    /// The file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The file name.
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// Number of keys in the file.
    /// </summary>
    public int KeyCount { get; }

    /// <summary>
    /// Total number of characters in all values.
    /// </summary>
    public int CharacterCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WrittenFileInfo"/> class.
    /// </summary>
    public WrittenFileInfo(string filePath, int keyCount, int characterCount)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentOutOfRangeException.ThrowIfNegative(keyCount);
        ArgumentOutOfRangeException.ThrowIfNegative(characterCount);

        FilePath = filePath;
        KeyCount = keyCount;
        CharacterCount = characterCount;
    }
}

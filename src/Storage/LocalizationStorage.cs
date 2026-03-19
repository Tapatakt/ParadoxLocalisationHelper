using System.Collections.Immutable;
using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Storage;

/// <summary>
/// Stores and indexes localization entries from multiple files.
/// </summary>
public sealed class LocalizationStorage
{
    private readonly Dictionary<string, LocalizationEntry> _entries = [];
    private readonly Dictionary<string, LocalizationFile> _files = [];

    /// <summary>
    /// Gets all entries indexed by key.
    /// </summary>
    public IReadOnlyDictionary<string, LocalizationEntry> Entries => _entries;

    /// <summary>
    /// Gets all loaded files indexed by file path.
    /// </summary>
    public IReadOnlyDictionary<string, LocalizationFile> Files => _files;

    /// <summary>
    /// Gets the total number of unique keys.
    /// </summary>
    public int KeyCount => _entries.Count;

    /// <summary>
    /// Gets the total number of files.
    /// </summary>
    public int FileCount => _files.Count;

    /// <summary>
    /// Adds a single file to the storage.
    /// </summary>
    /// <param name="file">The localization file to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when file is null.</exception>
    public void AddFile(LocalizationFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        _files[file.FilePath] = file;

        foreach (LocalizationEntry entry in file.Entries)
            _entries[entry.Key] = entry;
    }

    /// <summary>
    /// Adds multiple files to the storage.
    /// </summary>
    /// <param name="files">The files to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when files is null.</exception>
    public void AddFiles(IEnumerable<LocalizationFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        foreach (LocalizationFile file in files)
            AddFile(file);
    }

    /// <summary>
    /// Gets an entry by key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <returns>The entry if found, null otherwise.</returns>
    public LocalizationEntry? GetEntry(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _entries.TryGetValue(key, out LocalizationEntry? entry) ? entry : null;
    }

    /// <summary>
    /// Checks if a key exists in the storage.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if key exists, false otherwise.</returns>
    public bool ContainsKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _entries.ContainsKey(key);
    }

    /// <summary>
    /// Gets all keys from the storage.
    /// </summary>
    public ImmutableHashSet<string> GetAllKeys() => [.. _entries.Keys];

    /// <summary>
    /// Gets the source file for a specific key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <returns>The file path or null if key not found.</returns>
    public string? GetSourceFile(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        foreach (LocalizationFile file in _files.Values)
            if (file.Entries.Any(e => e.Key == key))
                return file.FilePath;

        return null;
    }

    /// <summary>
    /// Clears all entries and files from the storage.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
        _files.Clear();
    }
}

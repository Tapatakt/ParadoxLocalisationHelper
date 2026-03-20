using System.Collections.Immutable;
using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Storage;

/// <summary>
/// Stores and indexes localization entries from multiple files.
/// Supports set operations (Except, Modified, Unchanged, Union).
/// </summary>
public sealed class LocalizationStorage
{
    private readonly Dictionary<string, LocalizationEntry> _entries = [];
    private readonly Dictionary<string, LocalizationFile> _files = [];

    /// <summary>
    /// Gets or sets the name of this storage for identification purposes.
    /// </summary>
    public string Name { get; private set; } = "Unnamed";

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
    /// Gets a value indicating whether the storage is empty.
    /// </summary>
    public bool IsEmpty => _entries.Count == 0;

    /// <summary>
    /// Sets the name of this storage and returns this for fluent API.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>This storage instance.</returns>
    public LocalizationStorage SetName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        return this;
    }

    /// <summary>
    /// Describes this storage to console (files count, keys count, total chars) and returns this.
    /// </summary>
    /// <returns>This storage instance.</returns>
    public LocalizationStorage Describe()
    {
        int totalChars = _entries.Values.Sum(e => e.Value.Length);
        Console.WriteLine($"{Name}: {FileCount} files, {KeyCount} keys, {totalChars} chars");
        return this;
    }

    /// <summary>
    /// Gets statistics for a specific file in this storage.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>Tuple of (keyCount, totalChars) for keys in this storage that belong to the file.</returns>
    public (int KeyCount, int TotalChars) GetFileStats(string filePath)
    {
        int keyCount = 0;
        int totalChars = 0;

        if (_files.TryGetValue(filePath, out LocalizationFile? file))
        {
            foreach (LocalizationEntry entry in file.Entries)
            {
                if (_entries.ContainsKey(entry.Key))
                {
                    keyCount++;
                    totalChars += entry.Value.Length;
                }
            }
        }

        return (keyCount, totalChars);
    }

    /// <summary>
    /// Gets an entry by key.
    /// </summary>
    public LocalizationEntry? this[string key] =>
        _entries.TryGetValue(key, out LocalizationEntry? entry) ? entry : null;

    /// <summary>
    /// Tries to get an entry by key.
    /// </summary>
    public bool TryGetEntry(string key, out LocalizationEntry? entry) =>
        _entries.TryGetValue(key, out entry);

    /// <summary>
    /// Adds a single file to the storage.
    /// </summary>
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
    public void AddFiles(IEnumerable<LocalizationFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        foreach (LocalizationFile file in files)
            AddFile(file);
    }

    /// <summary>
    /// Returns keys that exist in this storage but not in the other.
    /// </summary>
    public LocalizationStorage Except(LocalizationStorage other)
    {
        ArgumentNullException.ThrowIfNull(other);
        LocalizationStorage result = new();
        foreach ((string key, LocalizationEntry entry) in _entries)
            if (!other._entries.ContainsKey(key))
                result.AddEntry(entry, GetSourceFile(key));
        return result;
    }

    /// <summary>
    /// Returns keys that exist in both storages but with different values.
    /// </summary>
    public LocalizationStorage Modified(LocalizationStorage other)
    {
        ArgumentNullException.ThrowIfNull(other);
        LocalizationStorage result = new();
        foreach ((string key, LocalizationEntry entry) in _entries)
            if (other._entries.TryGetValue(key, out LocalizationEntry? otherEntry))
                if (entry.Value != otherEntry.Value)
                    result.AddEntry(entry, GetSourceFile(key));
        return result;
    }

    /// <summary>
    /// Returns keys that exist in both storages with identical values.
    /// </summary>
    public LocalizationStorage Unchanged(LocalizationStorage other)
    {
        ArgumentNullException.ThrowIfNull(other);
        LocalizationStorage result = new();
        foreach ((string key, LocalizationEntry entry) in _entries)
            if (other._entries.TryGetValue(key, out LocalizationEntry? otherEntry))
                if (entry.Value == otherEntry.Value)
                    result.AddEntry(entry, GetSourceFile(key));
        return result;
    }

    /// <summary>
    /// Returns union of keys from both storages (this takes priority for conflicts).
    /// </summary>
    public LocalizationStorage Union(LocalizationStorage other)
    {
        ArgumentNullException.ThrowIfNull(other);
        LocalizationStorage result = new();
        foreach ((string key, LocalizationEntry entry) in _entries)
            result.AddEntry(entry, GetSourceFile(key));
        foreach ((string key, LocalizationEntry entry) in other._entries)
            if (!result._entries.ContainsKey(key))
                result.AddEntry(entry, other.GetSourceFile(key));
        return result;
    }

    /// <summary>
    /// Returns intersection - keys that exist in both storages.
    /// </summary>
    public LocalizationStorage Intersect(LocalizationStorage other)
    {
        ArgumentNullException.ThrowIfNull(other);
        LocalizationStorage result = new();
        foreach ((string key, LocalizationEntry entry) in _entries)
            if (other._entries.ContainsKey(key))
                result.AddEntry(entry, GetSourceFile(key));
        return result;
    }

    /// <summary>
    /// Checks if a key exists in the storage.
    /// </summary>
    public bool ContainsKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _entries.ContainsKey(key);
    }

    /// <summary>
    /// Gets all keys as an immutable set.
    /// </summary>
    public ImmutableHashSet<string> GetAllKeys() => [.. _entries.Keys];

    /// <summary>
    /// Gets the source file for a specific key.
    /// </summary>
    public string GetSourceFile(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        foreach (LocalizationFile file in _files.Values)
            if (file.Entries.Any(e => e.Key == key))
                return file.FilePath;
        return "unknown";
    }

    /// <summary>
    /// Clears all entries and files from the storage.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
        _files.Clear();
    }

    private void AddEntry(LocalizationEntry entry, string sourceFile)
    {
        _entries[entry.Key] = entry;
        if (!_files.TryGetValue(sourceFile, out LocalizationFile? file))
        {
            _files[sourceFile] = new LocalizationFile("l_english", sourceFile, [entry]);
        }
        else
        {
            List<LocalizationEntry> entries = [.. file.Entries, entry];
            _files[sourceFile] = new LocalizationFile(file.Language, sourceFile, entries);
        }
    }
}

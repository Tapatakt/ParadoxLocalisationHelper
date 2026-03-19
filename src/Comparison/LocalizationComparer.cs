using ParadoxLocalisationHelper.Comparison.Models;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Storage;

namespace ParadoxLocalisationHelper.Comparison;

/// <summary>
/// Compares two localization storages and identifies differences.
/// </summary>
public sealed class LocalizationComparer
{
    /// <summary>
    /// Compares two localization storages.
    /// </summary>
    /// <param name="oldStorage">The old/original storage.</param>
    /// <param name="newStorage">The new/modified storage.</param>
    /// <returns>Comparison result with all differences.</returns>
    public ComparisonResult Compare(
        LocalizationStorage oldStorage,
        LocalizationStorage newStorage)
    {
        ArgumentNullException.ThrowIfNull(oldStorage);
        ArgumentNullException.ThrowIfNull(newStorage);

        HashSet<string> oldKeys = [.. oldStorage.GetAllKeys()];
        HashSet<string> newKeys = [.. newStorage.GetAllKeys()];

        List<KeyWithSource> added = FindAddedKeys(oldKeys, newKeys, newStorage);
        List<KeyWithSource> removed = FindRemovedKeys(oldKeys, newKeys, oldStorage);
        List<Modification> modified = FindModifiedKeys(oldKeys, newKeys, oldStorage, newStorage);
        List<string> unchanged = FindUnchangedKeys(oldKeys, newKeys, oldStorage, newStorage);

        return new(added, removed, modified, unchanged);
    }

    private List<KeyWithSource> FindAddedKeys(
        HashSet<string> oldKeys,
        HashSet<string> newKeys,
        LocalizationStorage newStorage)
    {
        List<KeyWithSource> added = [];

        foreach (string key in newKeys)
        {
            if (oldKeys.Contains(key))
                continue;

            LocalizationEntry entry = newStorage.GetEntry(key)!;
            string sourceFile = newStorage.GetSourceFile(key) ?? "unknown";
            added.Add(new(key, entry.Value, sourceFile));
        }

        return added;
    }

    private List<KeyWithSource> FindRemovedKeys(
        HashSet<string> oldKeys,
        HashSet<string> newKeys,
        LocalizationStorage oldStorage)
    {
        List<KeyWithSource> removed = [];

        foreach (string key in oldKeys)
        {
            if (newKeys.Contains(key))
                continue;

            LocalizationEntry entry = oldStorage.GetEntry(key)!;
            string sourceFile = oldStorage.GetSourceFile(key) ?? "unknown";
            removed.Add(new(key, entry.Value, sourceFile));
        }

        return removed;
    }

    private List<Modification> FindModifiedKeys(
        HashSet<string> oldKeys,
        HashSet<string> newKeys,
        LocalizationStorage oldStorage,
        LocalizationStorage newStorage)
    {
        List<Modification> modified = [];

        foreach (string key in oldKeys)
        {
            if (!newKeys.Contains(key))
                continue;

            LocalizationEntry oldEntry = oldStorage.GetEntry(key)!;
            LocalizationEntry newEntry = newStorage.GetEntry(key)!;

            if (oldEntry.Value == newEntry.Value)
                continue;

            string oldFile = oldStorage.GetSourceFile(key) ?? "unknown";
            string newFile = newStorage.GetSourceFile(key) ?? "unknown";
            modified.Add(new(key, oldEntry.Value, newEntry.Value, oldFile, newFile));
        }

        return modified;
    }

    private List<string> FindUnchangedKeys(
        HashSet<string> oldKeys,
        HashSet<string> newKeys,
        LocalizationStorage oldStorage,
        LocalizationStorage newStorage)
    {
        List<string> unchanged = [];

        foreach (string key in oldKeys)
        {
            if (!newKeys.Contains(key))
                continue;

            LocalizationEntry oldEntry = oldStorage.GetEntry(key)!;
            LocalizationEntry newEntry = newStorage.GetEntry(key)!;

            if (oldEntry.Value == newEntry.Value)
                unchanged.Add(key);
        }

        return unchanged;
    }
}

using System.Collections.Immutable;

namespace ParadoxLocalisationHelper.Comparison.Models;

/// <summary>
/// Represents the result of comparing two localization storages.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ComparisonResult"/> class.
/// </remarks>
public sealed class ComparisonResult(
    IEnumerable<KeyWithSource> added,
    IEnumerable<KeyWithSource> removed,
    IEnumerable<Modification> modified,
    IEnumerable<string> unchanged)
{
    /// <summary>
    /// Keys that exist in the new storage but not in the old one.
    /// </summary>
    public ImmutableList<KeyWithSource> Added { get; } = [.. added];

    /// <summary>
    /// Keys that exist in the old storage but not in the new one.
    /// </summary>
    public ImmutableList<KeyWithSource> Removed { get; } = [.. removed];

    /// <summary>
    /// Keys that exist in both but have different values.
    /// </summary>
    public ImmutableList<Modification> Modified { get; } = [.. modified];

    /// <summary>
    /// Keys that exist in both with identical values.
    /// </summary>
    public ImmutableList<string> Unchanged { get; } = [.. unchanged];

    /// <summary>
    /// Total number of changes (added + removed + modified).
    /// </summary>
    public int TotalChanges => Added.Count + Removed.Count + Modified.Count;
}

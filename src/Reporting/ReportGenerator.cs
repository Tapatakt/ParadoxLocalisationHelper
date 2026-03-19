using System.Text;
using System.Text.Json;
using ParadoxLocalisationHelper.Analysis.Models;
using ParadoxLocalisationHelper.Comparison.Models;
using ParadoxLocalisationHelper.Yml;

namespace ParadoxLocalisationHelper.Reporting;

/// <summary>
/// Generates reports in various formats.
/// </summary>
public static class ReportGenerator
{
    /// <summary>
    /// Generates a comparison report.
    /// </summary>
    /// <param name="result">The comparison result.</param>
    /// <param name="format">The output format.</param>
    /// <param name="writtenFiles">Optional information about written YML files.</param>
    /// <returns>Formatted report string.</returns>
    public static string GenerateComparisonReport(
        ComparisonResult result,
        ReportFormat format,
        List<WrittenFileInfo>? writtenFiles = null) =>
        format switch
        {
            ReportFormat.PlainText => GenerateComparisonPlainText(result, writtenFiles),
            ReportFormat.Markdown => GenerateComparisonMarkdown(result, writtenFiles),
            ReportFormat.Json => GenerateComparisonJson(result),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown format")
        };

    /// <summary>
    /// Generates a missing keys report.
    /// </summary>
    /// <param name="result">The missing keys result.</param>
    /// <param name="format">The output format.</param>
    /// <param name="writtenFiles">Optional information about written YML files.</param>
    /// <returns>Formatted report string.</returns>
    public static string GenerateMissingKeysReport(
        MissingKeysResult result,
        ReportFormat format,
        List<WrittenFileInfo>? writtenFiles = null) =>
        format switch
        {
            ReportFormat.PlainText => GenerateMissingPlainText(result, writtenFiles),
            ReportFormat.Markdown => GenerateMissingMarkdown(result, writtenFiles),
            ReportFormat.Json => GenerateMissingJson(result),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown format")
        };

    private static string GenerateComparisonPlainText(ComparisonResult result, List<WrittenFileInfo>? writtenFiles)
    {
        int addedChars = result.Added.Sum(k => k.Value.Length);
        int removedChars = result.Removed.Sum(k => k.Value.Length);
        int modifiedOldChars = result.Modified.Sum(m => m.OldValue.Length);
        int modifiedNewChars = result.Modified.Sum(m => m.NewValue.Length);

        StringBuilder sb = new();
        sb.AppendLine("=== COMPARISON REPORT ===");
        sb.AppendLine();
        sb.AppendLine("By Key Count:");
        sb.AppendLine($"  Added: {result.Added.Count}");
        sb.AppendLine($"  Removed: {result.Removed.Count}");
        sb.AppendLine($"  Modified: {result.Modified.Count}");
        sb.AppendLine($"  Unchanged: {result.Unchanged.Count}");
        sb.AppendLine($"  Total Changes: {result.TotalChanges}");
        sb.AppendLine();
        sb.AppendLine("By Character Count:");
        sb.AppendLine($"  Added: {addedChars} chars");
        sb.AppendLine($"  Removed: {removedChars} chars");
        sb.AppendLine($"  Modified (old): {modifiedOldChars} chars");
        sb.AppendLine($"  Modified (new): {modifiedNewChars} chars");

        if (writtenFiles is not null && writtenFiles.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Generated Files:");
            foreach (WrittenFileInfo file in writtenFiles)
                sb.AppendLine($"  {file.FileName}: {file.KeyCount} keys, {file.CharacterCount} chars");
        }

        return sb.ToString();
    }

    private static string GenerateComparisonMarkdown(ComparisonResult result, List<WrittenFileInfo>? writtenFiles)
    {
        int addedChars = result.Added.Sum(k => k.Value.Length);
        int removedChars = result.Removed.Sum(k => k.Value.Length);
        int modifiedOldChars = result.Modified.Sum(m => m.OldValue.Length);
        int modifiedNewChars = result.Modified.Sum(m => m.NewValue.Length);

        StringBuilder sb = new();
        sb.AppendLine("# Comparison Report");
        sb.AppendLine();
        sb.AppendLine("## By Key Count");
        sb.AppendLine();
        sb.AppendLine("| Metric | Count |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Added | {result.Added.Count} |");
        sb.AppendLine($"| Removed | {result.Removed.Count} |");
        sb.AppendLine($"| Modified | {result.Modified.Count} |");
        sb.AppendLine($"| Unchanged | {result.Unchanged.Count} |");
        sb.AppendLine($"| **Total Changes** | **{result.TotalChanges}** |");
        sb.AppendLine();
        sb.AppendLine("## By Character Count");
        sb.AppendLine();
        sb.AppendLine("| Metric | Characters |");
        sb.AppendLine("|--------|------------|");
        sb.AppendLine($"| Added | {addedChars} |");
        sb.AppendLine($"| Removed | {removedChars} |");
        sb.AppendLine($"| Modified (old) | {modifiedOldChars} |");
        sb.AppendLine($"| Modified (new) | {modifiedNewChars} |");

        if (writtenFiles is not null && writtenFiles.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Generated Files");
            sb.AppendLine();
            sb.AppendLine("| File | Keys | Characters |");
            sb.AppendLine("|------|------|------------|");
            foreach (WrittenFileInfo file in writtenFiles)
                sb.AppendLine($"| {file.FileName} | {file.KeyCount} | {file.CharacterCount} |");
        }

        return sb.ToString();
    }

    private static string GenerateComparisonJson(ComparisonResult result) =>
        JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

    private static string GenerateMissingPlainText(MissingKeysResult result, List<WrittenFileInfo>? writtenFiles)
    {
        StringBuilder sb = new();
        sb.AppendLine("=== MISSING TRANSLATION KEYS ===");
        sb.AppendLine();
        sb.AppendLine("By Key Count:");
        sb.AppendLine($"  Total Source Keys: {result.TotalSourceKeys}");
        sb.AppendLine($"  Translated: {result.TranslatedKeys}");
        sb.AppendLine($"  Missing: {result.MissingCount}");
        sb.AppendLine($"  Progress: {result.TranslationPercentageByCount:F1}%");
        sb.AppendLine();
        sb.AppendLine("By Character Count:");
        sb.AppendLine($"  Total Source Characters: {result.TotalSourceCharacters}");
        sb.AppendLine($"  Translated Characters: {result.TranslatedCharacters}");
        sb.AppendLine($"  Missing Characters: {result.MissingCharacters}");
        sb.AppendLine($"  Progress: {result.TranslationPercentageByCharacters:F1}%");

        if (writtenFiles is not null && writtenFiles.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Generated Files:");
            foreach (WrittenFileInfo file in writtenFiles)
                sb.AppendLine($"  {file.FileName}: {file.KeyCount} keys, {file.CharacterCount} chars");
        }

        return sb.ToString();
    }

    private static string GenerateMissingMarkdown(MissingKeysResult result, List<WrittenFileInfo>? writtenFiles)
    {
        StringBuilder sb = new();
        sb.AppendLine("# Missing Translation Keys");
        sb.AppendLine();
        sb.AppendLine("## Statistics by Key Count");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Total Source Keys | {result.TotalSourceKeys} |");
        sb.AppendLine($"| Translated | {result.TranslatedKeys} |");
        sb.AppendLine($"| Missing | {result.MissingCount} |");
        sb.AppendLine($"| Progress | {result.TranslationPercentageByCount:F1}% |");
        sb.AppendLine();
        sb.AppendLine("## Statistics by Character Count");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Total Source Characters | {result.TotalSourceCharacters} |");
        sb.AppendLine($"| Translated Characters | {result.TranslatedCharacters} |");
        sb.AppendLine($"| Missing Characters | {result.MissingCharacters} |");
        sb.AppendLine($"| Progress | {result.TranslationPercentageByCharacters:F1}% |");

        if (writtenFiles is not null && writtenFiles.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Generated Files");
            sb.AppendLine();
            sb.AppendLine("| File | Keys | Characters |");
            sb.AppendLine("|------|------|------------|");
            foreach (WrittenFileInfo file in writtenFiles)
                sb.AppendLine($"| {file.FileName} | {file.KeyCount} | {file.CharacterCount} |");
        }

        return sb.ToString();
    }

    private static string GenerateMissingJson(MissingKeysResult result) =>
        JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

    private static string EscapeMarkdown(string value) =>
        value.Replace("|", "\\|").Replace("\n", " ").Replace("\r", "");
}

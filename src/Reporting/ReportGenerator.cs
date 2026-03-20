using System.Text;
using ParadoxLocalisationHelper.Analysis.Models;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Storage;

namespace ParadoxLocalisationHelper.Reporting;

/// <summary>
/// Generates translation reports based on loaded localization data.
/// </summary>
public sealed class ReportGenerator
{
    // Input storages
    public required LocalizationStorage OldVersion { get; init; }
    public required LocalizationStorage NewVersion { get; init; }
    public required LocalizationStorage OldTranslation { get; init; }
    public required LocalizationStorage TranslationUpdate { get; init; }

    // Derived storages
    public LocalizationStorage AddedKeys { get; private set; } = new();
    public LocalizationStorage RemovedKeys { get; private set; } = new();
    public LocalizationStorage ModifiedKeys { get; private set; } = new();
    public LocalizationStorage MissingInOldTranslation { get; private set; } = new();
    public LocalizationStorage WorkToDo { get; private set; } = new();
    public LocalizationStorage WorkDone { get; private set; } = new();
    public LocalizationStorage WorkRemaining { get; private set; } = new();
    public LocalizationStorage UnnecessaryKeys { get; private set; } = new();

    // Analysis results
    public (int Count, List<DuplicateKeyInfo> Items) TranslationUpdateDuplicates { get; private set; }
    public (int Count, List<DuplicateKeyInfo> Items) OldTranslationDuplicates { get; private set; }

    /// <summary>
    /// Performs analysis and generates the report.
    /// </summary>
    /// <returns>Path to the generated report file.</returns>
    public string Generate()
    {
        Analyze();
        PrintSummary();
        return GenerateReportFile();
    }

    private void Analyze()
    {
        // Version changes
        AddedKeys = NewVersion.Except(OldVersion).SetName("Added Keys");
        RemovedKeys = OldVersion.Except(NewVersion).SetName("Removed Keys");
        ModifiedKeys = NewVersion.Modified(OldVersion).SetName("Modified Keys");

        // Translation work
        MissingInOldTranslation = OldVersion.Except(OldTranslation).SetName("Missing in Old Translation");
        WorkToDo = AddedKeys.Union(ModifiedKeys).Union(MissingInOldTranslation).SetName("Work To Do");
        WorkDone = WorkToDo.Intersect(TranslationUpdate).SetName("Work Done");
        WorkRemaining = WorkToDo.Except(TranslationUpdate).SetName("Work Remaining");
        UnnecessaryKeys = TranslationUpdate.Except(NewVersion).SetName("Unnecessary Keys");

        // Duplicates analysis
        TranslationUpdateDuplicates = FindDuplicates(TranslationUpdate);
        OldTranslationDuplicates = FindDuplicates(OldTranslation);
    }

    private static (int Count, List<DuplicateKeyInfo> Items) FindDuplicates(LocalizationStorage storage)
    {
        if (storage.IsEmpty)
            return (0, []);
        var result = Analysis.MissingKeysAnalyzer.AnalyzeTranslation(storage, storage);
        return (result.DuplicateCount, result.DuplicateKeys);
    }

    private void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("=== Сводка ===");
        Console.WriteLine();

        if (!AddedKeys.IsEmpty || !RemovedKeys.IsEmpty || !ModifiedKeys.IsEmpty)
        {
            Console.WriteLine("Изменения в версии:");
            Console.WriteLine($"  Добавлено: {AddedKeys.KeyCount}");
            Console.WriteLine($"  Удалено: {RemovedKeys.KeyCount}");
            Console.WriteLine($"  Изменено: {ModifiedKeys.KeyCount}");
            Console.WriteLine();
        }

        if (!WorkToDo.IsEmpty)
        {
            int remaining = WorkRemaining.KeyCount;
            double percent = WorkToDo.KeyCount == 0 ? 100 : (double)WorkDone.KeyCount / WorkToDo.KeyCount * 100;
            Console.WriteLine("Работа по переводу:");
            Console.WriteLine($"  Всего требуется: {WorkToDo.KeyCount}");
            Console.WriteLine($"  Сделано: {WorkDone.KeyCount} ({percent:F1}%)");
            Console.WriteLine($"  Осталось: {remaining}");
            Console.WriteLine();
        }

        if (TranslationUpdateDuplicates.Count > 0)
            Console.WriteLine($"⚠ Дубликаты в обновлении: {TranslationUpdateDuplicates.Count}");
        if (OldTranslationDuplicates.Count > 0)
            Console.WriteLine($"⚠ Дубликаты в устаревшем переводе: {OldTranslationDuplicates.Count}");
        if (!UnnecessaryKeys.IsEmpty)
            Console.WriteLine($"⚠ Лишние ключи в обновлении: {UnnecessaryKeys.KeyCount}");
    }

    private string GenerateReportFile()
    {
        StringBuilder report = new();
        report.AppendLine("# Translation Report");
        report.AppendLine();

        // Main table with all storages
        report.AppendLine("## Overview by File");
        report.AppendLine();
        report.AppendLine(GenerateStorageTable(
            OldVersion, NewVersion, OldTranslation, TranslationUpdate,
            AddedKeys, RemovedKeys, ModifiedKeys,
            MissingInOldTranslation, WorkToDo, WorkDone, WorkRemaining));
        report.AppendLine();



        // Section: Duplicates
        if (TranslationUpdateDuplicates.Count > 0 || OldTranslationDuplicates.Count > 0)
        {
            report.AppendLine("## Duplicate Keys");
            report.AppendLine();

            if (TranslationUpdateDuplicates.Count > 0)
            {
                report.AppendLine($"### In Translation Update ({TranslationUpdateDuplicates.Count})");
                report.AppendLine();
                MarkdownTableBuilder dupTable = new();
                dupTable.AddHeader("Key", "Count", "Files");
                dupTable.AddRowsWithLimit(
                    TranslationUpdateDuplicates.Items,
                    d => new object?[]
                    {
                        $"`{d.Key}`",
                        d.OccurrenceCount,
                        string.Join(", ", d.Files.Select(f => Path.GetFileName(f)))
                    },
                    20,
                    TranslationUpdateDuplicates.Count,
                    "*and {0} more*");
                report.AppendLine(dupTable.Build());
                report.AppendLine();
            }

            if (OldTranslationDuplicates.Count > 0)
            {
                report.AppendLine($"### In Old Translation ({OldTranslationDuplicates.Count})");
                report.AppendLine();
                MarkdownTableBuilder dupTable = new();
                dupTable.AddHeader("Key", "Count", "Files");
                dupTable.AddRowsWithLimit(
                    OldTranslationDuplicates.Items,
                    d => new object?[]
                    {
                        $"`{d.Key}`",
                        d.OccurrenceCount,
                        string.Join(", ", d.Files.Select(f => Path.GetFileName(f)))
                    },
                    20,
                    OldTranslationDuplicates.Count,
                    "*and {0} more*");
                report.AppendLine(dupTable.Build());
                report.AppendLine();
            }
        }

        // Section: Unnecessary Keys
        if (!UnnecessaryKeys.IsEmpty)
        {
            report.AppendLine($"## Unnecessary Keys ({UnnecessaryKeys.KeyCount})");
            report.AppendLine();
            report.AppendLine("Keys present in translation update but not in new version:");
            report.AppendLine();
            MarkdownTableBuilder unnTable = new();
            unnTable.AddHeader("Key", "File");
            foreach (LocalizationFile file in UnnecessaryKeys.Files.Values)
            {
                foreach (LocalizationEntry entry in file.Entries)
                {
                    unnTable.AddRow($"`{entry.Key}`", file.FileName);
                }
            }
            report.AppendLine(unnTable.Build());
            report.AppendLine();
        }

        if (WorkRemaining.IsEmpty && !WorkToDo.IsEmpty)
        {
            report.AppendLine("## 🎉 All work completed!");
            report.AppendLine();
            report.AppendLine("No remaining translation work.");
        }

        string reportPath = $"translation_report_{DateTime.Now:yyyyMMdd_HHmmss}.md";
        File.WriteAllText(reportPath, report.ToString());
        return reportPath;
    }

    /// <summary>
    /// Generates a markdown table showing statistics for multiple storages by file.
    /// </summary>
    private static string GenerateStorageTable(params LocalizationStorage[] storages)
    {
        if (storages.Length == 0)
            return string.Empty;

        // Collect all unique base file names (without language tag)
        HashSet<string> allBaseNames = [];
        foreach (LocalizationStorage storage in storages)
            foreach (LocalizationFile file in storage.Files.Values)
                allBaseNames.Add(GetBaseFileName(file.FileName));

        List<string> baseNames = allBaseNames.OrderBy(f => f).ToList();

        // Build table
        MarkdownTableBuilder table = new();

        // Header: File + storage names
        List<string> headers = ["File", .. storages.Select(s => s.Name)];
        table.AddHeader([.. headers]);

        // Data rows
        foreach (string baseName in baseNames)
        {
            List<string> rowValues = [baseName];
            foreach (LocalizationStorage storage in storages)
            {
                var matchingFiles = storage.Files.Values.Where(f => GetBaseFileName(f.FileName) == baseName).ToList();
                int keyCount = matchingFiles.Sum(f =>
                {
                    (int count, _) = storage.GetFileStats(f.FilePath);
                    return count;
                });
                int totalChars = matchingFiles.Sum(f =>
                {
                    (_, int chars) = storage.GetFileStats(f.FilePath);
                    return chars;
                });
                rowValues.Add(keyCount > 0 ? $"{keyCount}/{totalChars}" : "-");
            }
            table.AddRow([.. rowValues]);
        }

        // Totals row
        List<string> totalRow = ["**Total**"];
        foreach (LocalizationStorage storage in storages)
        {
            int totalKeys = storage.KeyCount;
            int totalChars = storage.Entries.Values.Sum(e => e.Value.Length);
            totalRow.Add($"**{totalKeys}/{totalChars}**");
        }
        table.AddRow([.. totalRow]);

        return table.Build();
    }

    /// <summary>
    /// Gets base file name with language tag replaced by *.
    /// </summary>
    private static string GetBaseFileName(string fileName)
    {
        int langIndex = fileName.LastIndexOf("l_", StringComparison.OrdinalIgnoreCase);
        if (langIndex >= 0)
        {
            int dotIndex = fileName.LastIndexOf('.');
            if (dotIndex > langIndex + 2)
                return fileName[..(langIndex + 2)] + "*" + fileName[dotIndex..];
        }
        return fileName;
    }
}

using System.Text;
using ParadoxLocalisationHelper.Analysis;
using ParadoxLocalisationHelper.Cli;
using ParadoxLocalisationHelper.Comparison;
using ParadoxLocalisationHelper.Configuration;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Merging;
using ParadoxLocalisationHelper.Parsing;
using ParadoxLocalisationHelper.Reporting;
using ParadoxLocalisationHelper.Storage;
using ParadoxLocalisationHelper.Yml;

namespace ParadoxLocalisationHelper;

internal class Program
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");
    private static readonly ConfigurationService ConfigService = new(ConfigPath);
    private static AppState State = new();
    private static ConsoleMenuService Menu = new(State);
    private static readonly ParadoxLocalizationParser Parser = new();

    static void Main(string[] args)
    {
        // Load saved configuration
        State = ConfigService.Load();
        Menu = new ConsoleMenuService(State);

        bool running = true;

        while (running)
        {
            int choice = Menu.ShowMainMenu();

            switch (choice)
            {
                case 0:
                    running = false;
                    break;
                case 1:
                    SetNewVersionPath();
                    break;
                case 2:
                    SetOldVersionPath();
                    break;
                case 3:
                    SetOldTranslationPath();
                    break;
                case 4:
                    SetNewTranslationPath();
                    break;
                case 5:
                    SetSourceLanguage();
                    break;
                case 6:
                    SetTargetLanguage();
                    break;
                case 7:
                    GenerateDeltaReport();
                    break;
                case 8:
                    GenerateMissingKeysReport();
                    break;
                case 9:
                    MergeTranslations();
                    break;
                case 10:
                    GenerateTranslationWorkReport();
                    break;
                default:
                    ConsoleMenuService.ShowError("Invalid option selected.");
                    ConsoleMenuService.WaitForKey();
                    break;
            }
        }
    }

    private static void SetNewVersionPath()
    {
        string path = ConsoleMenuService.ReadFolderPath("new version folder path");
        if (string.IsNullOrEmpty(path))
            return;

        State.NewVersionPath = path;
        ConfigService.Save(State);
        ConsoleMenuService.ShowSuccess($"New version path set to: '{path}'");
        ConsoleMenuService.WaitForKey();
    }

    private static void SetOldVersionPath()
    {
        string path = ConsoleMenuService.ReadFolderPath("old version folder path");
        if (string.IsNullOrEmpty(path))
            return;

        State.OldVersionPath = path;
        ConfigService.Save(State);
        ConsoleMenuService.ShowSuccess($"Old version path set to: '{path}'");
        ConsoleMenuService.WaitForKey();
    }

    private static void SetOldTranslationPath()
    {
        string path = ConsoleMenuService.ReadFolderPath("outdated translation folder path");
        if (string.IsNullOrEmpty(path))
            return;

        State.OldTranslationPath = path;
        ConfigService.Save(State);
        ConsoleMenuService.ShowSuccess($"Outdated translation path set to: '{path}'");
        ConsoleMenuService.WaitForKey();
    }

    private static void SetNewTranslationPath()
    {
        string path = ConsoleMenuService.ReadFolderPath("translation update folder path");
        if (string.IsNullOrEmpty(path))
            return;

        State.NewTranslationPath = path;
        ConfigService.Save(State);
        ConsoleMenuService.ShowSuccess($"Translation update path set to: '{path}'");
        ConsoleMenuService.WaitForKey();
    }

    private static void SetSourceLanguage()
    {
        State.SourceLanguage = ConsoleMenuService.ReadLanguage(State.SourceLanguage);
        ConfigService.Save(State);
        ConsoleMenuService.ShowSuccess($"Source language set to: {State.SourceLanguage}");
        ConsoleMenuService.WaitForKey();
    }

    private static void SetTargetLanguage()
    {
        State.TargetLanguage = ConsoleMenuService.ReadLanguage(State.TargetLanguage);
        ConfigService.Save(State);
        ConsoleMenuService.ShowSuccess($"Target language set to: {State.TargetLanguage}");
        ConsoleMenuService.WaitForKey();
    }

    private static void GenerateDeltaReport()
    {
        if (!ValidatePathsForDelta())
            return;

        try
        {
            ConsoleMenuService.ShowInfo("Loading old version...");
            LocalizationStorage oldStorage = LoadStorage(State.OldVersionPath);

            ConsoleMenuService.ShowInfo("Loading new version...");
            LocalizationStorage newStorage = LoadStorage(State.NewVersionPath);
            List<LocalizationFile> newFiles = Parser.ParseDirectory(State.NewVersionPath);

            ConsoleMenuService.ShowInfo("Comparing versions...");
            LocalizationComparer comparer = new();
            Comparison.Models.ComparisonResult result = comparer.Compare(oldStorage, newStorage);

            // Create output directory for YML files
            string ymlDirectory = $"delta_yml_{DateTime.Now:yyyyMMdd_HHmmss}";
            string ymlPath = Path.Combine(Environment.CurrentDirectory, ymlDirectory);

            // Generate YML files
            ConsoleMenuService.ShowInfo("Writing YML files...");
            LocalizationYmlWriter ymlWriter = new();
            List<WrittenFileInfo> writtenFiles = ymlWriter.WriteDeltaYmlFiles(
                result, ymlPath, State.SourceLanguage, newFiles);

            // Generate report
            string report = ReportGenerator.GenerateComparisonReport(
                result, ReportFormat.Markdown, writtenFiles);
            string reportPath = $"delta_report_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            File.WriteAllText(reportPath, report);

            ConsoleMenuService.ShowSuccess($"Delta report saved to: '{reportPath}'");
            ConsoleMenuService.ShowSuccess($"Delta YML files saved to: '{ymlPath}'");
            ConsoleMenuService.ShowInfo($"Files created: {writtenFiles.Count}");
            ConsoleMenuService.ShowInfo($"Added: {result.Added.Count}, Removed: {result.Removed.Count}, Modified: {result.Modified.Count}");
        }
        catch (Exception ex)
        {
            ConsoleMenuService.ShowError(ex.Message);
        }

        ConsoleMenuService.WaitForKey();
    }

    private static void GenerateMissingKeysReport()
    {
        if (!ValidatePathsForMissingKeys())
            return;

        try
        {
            ConsoleMenuService.ShowInfo("Loading outdated translation...");
            LocalizationStorage oldTranslation = LoadStorage(State.OldTranslationPath);

            ConsoleMenuService.ShowInfo("Loading new version (source language)...");
            LocalizationStorage newVersion = LoadStorage(State.NewVersionPath);
            List<LocalizationFile> newFiles = Parser.ParseDirectory(State.NewVersionPath);

            ConsoleMenuService.ShowInfo("Analyzing missing keys...");
            Analysis.Models.MissingKeysResult result = MissingKeysAnalyzer.FindMissingKeys(newVersion, oldTranslation);

            // Create output directory for YML files
            string ymlDirectory = $"missing_yml_{DateTime.Now:yyyyMMdd_HHmmss}";
            string ymlPath = Path.Combine(Environment.CurrentDirectory, ymlDirectory);

            // Generate YML files
            ConsoleMenuService.ShowInfo("Writing YML files...");
            LocalizationYmlWriter ymlWriter = new();
            List<WrittenFileInfo> writtenFiles = ymlWriter.WriteMissingKeysYmlFiles(
                result, ymlPath, State.SourceLanguage, newFiles);

            // Generate report
            string report = ReportGenerator.GenerateMissingKeysReport(
                result, ReportFormat.Markdown, writtenFiles);
            string reportPath = $"missing_keys_report_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            File.WriteAllText(reportPath, report);

            ConsoleMenuService.ShowSuccess($"Missing keys report saved to: '{reportPath}'");
            ConsoleMenuService.ShowSuccess($"Missing keys YML files saved to: '{ymlPath}'");
            ConsoleMenuService.ShowInfo($"Files created: {writtenFiles.Count}");
            ConsoleMenuService.ShowInfo($"Missing: {result.MissingCharacters} of {result.TotalSourceCharacters} chars ({result.TranslationPercentageByCharacters:F1}% complete)");
        }
        catch (Exception ex)
        {
            ConsoleMenuService.ShowError(ex.Message);
        }

        ConsoleMenuService.WaitForKey();
    }

    private static void MergeTranslations()
    {
        if (!ValidatePathsForMerge())
            return;

        try
        {
            string outputFolderName = ConsoleMenuService.ReadOutputFolderName();
            string outputPath = Path.Combine(Environment.CurrentDirectory, outputFolderName);

            ConsoleMenuService.ShowInfo("Loading outdated translation...");
            LocalizationStorage oldTranslation = LoadStorage(State.OldTranslationPath);

            ConsoleMenuService.ShowInfo("Loading translation update...");
            LocalizationStorage newTranslation = LoadStorage(State.NewTranslationPath);

            ConsoleMenuService.ShowInfo("Loading new version (for file structure)...");
            LocalizationStorage newVersion = LoadStorage(State.NewVersionPath);

            ConsoleMenuService.ShowInfo("Merging translations...");
            LocalizationMerger merger = new();
            List<LocalizationFile> mergedFiles = merger.Merge(
                oldTranslation,
                newTranslation,
                newVersion,
                State.TargetLanguage);

            ConsoleMenuService.ShowInfo("Writing merged files...");
            LocalizationYmlWriter writer = new();
            writer.WriteFiles(mergedFiles, outputPath);

            ConsoleMenuService.ShowSuccess($"Merged translation saved to: '{outputPath}'");
            ConsoleMenuService.ShowInfo($"Files written: {mergedFiles.Count}");
        }
        catch (Exception ex)
        {
            ConsoleMenuService.ShowError(ex.Message);
        }

        ConsoleMenuService.WaitForKey();
    }

    private static bool ValidatePathsForDelta()
    {
        if (!State.HasOldVersion)
        {
            ConsoleMenuService.ShowError("Old version path is not set. Please set it first (option 2).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        if (!State.HasNewVersion)
        {
            ConsoleMenuService.ShowError("New version path is not set. Please set it first (option 1).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        return true;
    }

    private static bool ValidatePathsForMissingKeys()
    {
        if (!State.HasOldTranslation)
        {
            ConsoleMenuService.ShowError("Outdated translation path is not set. Please set it first (option 3).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        if (!State.HasNewVersion)
        {
            ConsoleMenuService.ShowError("New version path is not set. Please set it first (option 1).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        return true;
    }

    private static bool ValidatePathsForMerge()
    {
        if (!State.HasOldTranslation)
        {
            ConsoleMenuService.ShowError("Outdated translation path is not set. Please set it first (option 3).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        if (!State.HasNewTranslation)
        {
            ConsoleMenuService.ShowError("Translation update path is not set. Please set it first (option 4).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        if (!State.HasNewVersion)
        {
            ConsoleMenuService.ShowError("New version path is not set. Please set it first (option 1).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        return true;
    }

    private static void GenerateTranslationWorkReport()
    {
        if (!ValidatePathsForTranslationWork())
            return;

        try
        {
            ConsoleMenuService.ShowInfo("Loading old version...");
            LocalizationStorage oldStorage = LoadStorage(State.OldVersionPath);

            ConsoleMenuService.ShowInfo("Loading new version...");
            LocalizationStorage newStorage = LoadStorage(State.NewVersionPath);
            List<LocalizationFile> newFiles = Parser.ParseDirectory(State.NewVersionPath);

            ConsoleMenuService.ShowInfo("Loading outdated translation...");
            LocalizationStorage oldTranslation = LoadStorage(State.OldTranslationPath);

            ConsoleMenuService.ShowInfo("Analyzing changes and missing keys...");

            // Get modified keys (between old and new version)
            LocalizationComparer comparer = new();
            Comparison.Models.ComparisonResult comparisonResult = comparer.Compare(oldStorage, newStorage);

            // Get missing keys (in translation vs new version)
            Analysis.Models.MissingKeysResult missingResult = MissingKeysAnalyzer.FindMissingKeys(newStorage, oldTranslation);

            // Create combined result for YML generation
            // Combine added keys and missing keys, removing duplicates
            Dictionary<string, Comparison.Models.KeyWithSource> workKeys = [];
            foreach (Comparison.Models.KeyWithSource key in comparisonResult.Added)
                workKeys[key.Key] = key;
            foreach (Analysis.Models.MissingKeyInfo key in missingResult.MissingKeys)
                workKeys[key.Key] = new(key.Key, key.SourceValue, key.SourceFile);

            Comparison.Models.ComparisonResult workResult = new(
                workKeys.Values.ToList(),
                [],
                comparisonResult.Modified,
                []);

            // Create output directory for YML files
            string ymlDirectory = $"translation_work_yml_{DateTime.Now:yyyyMMdd_HHmmss}";
            string ymlPath = Path.Combine(Environment.CurrentDirectory, ymlDirectory);

            // Generate YML files
            ConsoleMenuService.ShowInfo("Writing YML files...");
            LocalizationYmlWriter ymlWriter = new();
            List<WrittenFileInfo> writtenFiles = ymlWriter.WriteDeltaYmlFiles(
                workResult, ymlPath, State.SourceLanguage, newFiles);

            // Calculate detailed breakdown
            HashSet<string> missingKeySet = missingResult.MissingKeys.Select(m => m.Key).ToHashSet();
            HashSet<string> addedKeySet = comparisonResult.Added.Select(a => a.Key).ToHashSet();
            HashSet<string> modifiedKeySet = comparisonResult.Modified.Select(m => m.Key).ToHashSet();

            // Added keys breakdown
            int addedOnlyCount = comparisonResult.Added.Count;
            int addedOnlyChars = comparisonResult.Added.Sum(k => k.Value.Length);
            int addedAndMissingCount = comparisonResult.Added.Count(k => missingKeySet.Contains(k.Key));
            int addedAndMissingChars = comparisonResult.Added.Where(k => missingKeySet.Contains(k.Key)).Sum(k => k.Value.Length);

            // Modified keys breakdown
            int modifiedCount = comparisonResult.Modified.Count;
            int modifiedChars = comparisonResult.Modified.Sum(m => m.NewValue.Length);
            int modifiedAndMissingCount = comparisonResult.Modified.Count(m => missingKeySet.Contains(m.Key));
            int modifiedAndMissingChars = comparisonResult.Modified.Where(m => missingKeySet.Contains(m.Key)).Sum(m => m.NewValue.Length);
            int modifiedAndTranslatedCount = modifiedCount - modifiedAndMissingCount;
            int modifiedAndTranslatedChars = comparisonResult.Modified.Where(m => !missingKeySet.Contains(m.Key)).Sum(m => m.NewValue.Length);

            // Missing breakdown
            int missingTotalCount = missingResult.MissingCount;
            int missingTotalChars = missingResult.MissingCharacters;
            int missingFromAddedCount = missingResult.MissingKeys.Count(m => addedKeySet.Contains(m.Key));
            int missingFromAddedChars = missingResult.MissingKeys.Where(m => addedKeySet.Contains(m.Key)).Sum(m => m.SourceValueLength);
            int missingFromExistingCount = missingResult.MissingKeys.Count(m => !addedKeySet.Contains(m.Key));
            int missingFromExistingChars = missingResult.MissingKeys.Where(m => !addedKeySet.Contains(m.Key)).Sum(m => m.SourceValueLength);

            StringBuilder report = new();
            report.AppendLine("# Translation Work Report");
            report.AppendLine();
            report.AppendLine("This report shows all keys that require translation work.");
            report.AppendLine();
            report.AppendLine("## Overview");
            report.AppendLine();
            report.AppendLine("| Category | Keys | Characters |");
            report.AppendLine("|----------|------|------------|");
            report.AppendLine($"| **Total requiring translation** | **{missingTotalCount}** | **{missingTotalChars}** |");
            report.AppendLine($"| └─ New keys (not translated) | {missingFromAddedCount} | {missingFromAddedChars} |");
            report.AppendLine($"| └─ Existing keys (not translated) | {missingFromExistingCount} | {missingFromExistingChars} |");
            report.AppendLine($"| Modified keys (translated) | {modifiedAndTranslatedCount} | {modifiedAndTranslatedChars} |");
            report.AppendLine();
            report.AppendLine("## Detailed Breakdown");
            report.AppendLine();
            report.AppendLine("### New Keys (Added in new version)");
            report.AppendLine();
            report.AppendLine("| Subcategory | Keys | Characters |");
            report.AppendLine("|-------------|------|------------|");
            report.AppendLine($"| Total new keys | {addedOnlyCount} | {addedOnlyChars} |");
            report.AppendLine($"| └─ Not yet translated | {addedAndMissingCount} | {addedAndMissingChars} |");
            report.AppendLine($"| └─ Already translated | {addedOnlyCount - addedAndMissingCount} | {addedOnlyChars - addedAndMissingChars} |");
            report.AppendLine();
            report.AppendLine("### Modified Keys (Changed between versions)");
            report.AppendLine();
            report.AppendLine("| Subcategory | Keys | Characters |");
            report.AppendLine("|-------------|------|------------|");
            report.AppendLine($"| Total modified keys | {modifiedCount} | {modifiedChars} |");
            report.AppendLine($"| └─ Not yet translated | {modifiedAndMissingCount} | {modifiedAndMissingChars} |");
            report.AppendLine($"| └─ Already translated (need update) | {modifiedAndTranslatedCount} | {modifiedAndTranslatedChars} |");
            report.AppendLine();
            report.AppendLine("### Work Required");
            report.AppendLine();
            report.AppendLine("| Priority | Keys | Characters | Description |");
            report.AppendLine("|----------|------|------------|-------------|");
            report.AppendLine($"| High | {missingTotalCount} | {missingTotalChars} | All untranslated keys (new + existing) |");
            report.AppendLine($"| Medium | {modifiedAndTranslatedCount} | {modifiedAndTranslatedChars} | Translated but modified (need update) |");
            report.AppendLine($"| **Total Work** | **{missingTotalCount + modifiedAndTranslatedCount}** | **{missingTotalChars + modifiedAndTranslatedChars}** | **All items requiring attention** |");
            report.AppendLine();
            report.AppendLine("## Generated Files");
            report.AppendLine();
            report.AppendLine("| File | Keys | Characters |");
            report.AppendLine("|------|------|------------|");
            foreach (WrittenFileInfo file in writtenFiles)
                report.AppendLine($"| {file.FileName} | {file.KeyCount} | {file.CharacterCount} |");

            string reportPath = $"translation_work_report_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            File.WriteAllText(reportPath, report.ToString());

            ConsoleMenuService.ShowSuccess($"Translation work report saved to: '{reportPath}'");
            ConsoleMenuService.ShowSuccess($"YML files saved to: '{ymlPath}'");
            ConsoleMenuService.ShowInfo($"Files created: {writtenFiles.Count}");
            ConsoleMenuService.ShowInfo($"Total requiring translation: {missingTotalCount} keys ({missingTotalChars} chars)");
            ConsoleMenuService.ShowInfo($"Modified and translated: {modifiedAndTranslatedCount} keys ({modifiedAndTranslatedChars} chars)");
        }
        catch (Exception ex)
        {
            ConsoleMenuService.ShowError(ex.Message);
        }

        ConsoleMenuService.WaitForKey();
    }

    private static bool ValidatePathsForTranslationWork()
    {
        if (!State.HasOldVersion)
        {
            ConsoleMenuService.ShowError("Old version path is not set. Please set it first (option 2).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        if (!State.HasNewVersion)
        {
            ConsoleMenuService.ShowError("New version path is not set. Please set it first (option 1).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        if (!State.HasOldTranslation)
        {
            ConsoleMenuService.ShowError("Outdated translation path is not set. Please set it first (option 3).");
            ConsoleMenuService.WaitForKey();
            return false;
        }

        return true;
    }

    private static LocalizationStorage LoadStorage(string path)
    {
        LocalizationStorage storage = new();
        List<LocalizationFile> files = Parser.ParseDirectory(path);
        storage.AddFiles(files);
        return storage;
    }
}

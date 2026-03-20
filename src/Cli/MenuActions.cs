using System.Text;
using ParadoxLocalisationHelper.Analysis;
using ParadoxLocalisationHelper.Analysis.Models;
using ParadoxLocalisationHelper.Configuration;
using ParadoxLocalisationHelper.Merging;
using ParadoxLocalisationHelper.Models;
using ParadoxLocalisationHelper.Parsing;
using ParadoxLocalisationHelper.Reporting;
using ParadoxLocalisationHelper.Storage;
using ParadoxLocalisationHelper.Yml;

namespace ParadoxLocalisationHelper.Cli;

/// <summary>
/// Contains all menu action implementations.
/// </summary>
public sealed class MenuActions
{
    private readonly AppState _state;
    private readonly ConfigurationService _configService;
    private readonly ParadoxLocalizationParser _parser = new();

    public MenuActions(AppState state, ConfigurationService configService)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(configService);
        _state = state;
        _configService = configService;
    }

    public void SetNewVersionPath() =>
        SetPath(v => _state.NewVersionPath = v, "путь к новой версии (оригинал)", "Путь к новой версии установлен");

    public void SetOldVersionPath() =>
        SetPath(v => _state.OldVersionPath = v, "путь к старой версии (оригинал)", "Путь к старой версии установлен");

    public void SetOldTranslationPath() =>
        SetPath(v => _state.OldTranslationPath = v, "путь к устаревшему переводу", "Путь к устаревшему переводу установлен");

    public void SetNewTranslationPath() =>
        SetPath(v => _state.TranslationUpdatePath = v, "путь к обновлению перевода", "Путь к обновлению перевода установлен");

    public void SetSourceLanguage()
    {
        _state.SourceLanguage = ConsoleMenuService.ReadLanguage(_state.SourceLanguage);
        _configService.Save(_state);
        ConsoleMenuService.ShowSuccess($"Язык оригинала установлен: {_state.SourceLanguage}");
    }

    public void SetTargetLanguage()
    {
        _state.TargetLanguage = ConsoleMenuService.ReadLanguage(_state.TargetLanguage);
        _configService.Save(_state);
        ConsoleMenuService.ShowSuccess($"Язык перевода установлен: {_state.TargetLanguage}");
    }

    /// <summary>
    /// Generates comprehensive report based on all available data.
    /// </summary>
    public void GenerateReport()
    {
        ConsoleMenuService.ShowInfo("Загрузка данных...");
        LocalizationStorage oldVersion = LoadStorageOrEmpty(_state.OldVersionPath).SetName("Old Version").Describe();
        LocalizationStorage newVersion = LoadStorageOrEmpty(_state.NewVersionPath).SetName("New Version").Describe();
        LocalizationStorage oldTranslation = LoadStorageOrEmpty(_state.OldTranslationPath).SetName("Old Translation").Describe();
        LocalizationStorage translationUpdate = LoadStorageOrEmpty(_state.TranslationUpdatePath).SetName("Translation Update").Describe();

        ConsoleMenuService.ShowInfo("Анализ и генерация отчёта...");

        ReportGenerator generator = new()
        {
            OldVersion = oldVersion,
            NewVersion = newVersion,
            OldTranslation = oldTranslation,
            TranslationUpdate = translationUpdate
        };

        string reportPath = generator.Generate();
        ConsoleMenuService.ShowSuccess($"Отчёт сохранён: '{reportPath}'");

        // Generate YML for remaining work
        if (!generator.WorkRemaining.IsEmpty)
        {
            string ymlPath = CreateYmlDirectory("remaining_work");
            ConsoleMenuService.ShowInfo("Генерация YML-файлов...");
            new LocalizationYmlWriter().WriteStorage(generator.WorkRemaining, ymlPath, _state.SourceLanguage);
            ConsoleMenuService.ShowSuccess($"YML с оставшейся работой сохранены: '{ymlPath}'");
        }
    }

    /// <summary>
    /// Merges outdated translation with update.
    /// </summary>
    public void MergeTranslations()
    {
        string outputFolderName = ConsoleMenuService.ReadOutputFolderName();
        string outputPath = Path.Combine(Environment.CurrentDirectory, outputFolderName);

        ConsoleMenuService.ShowInfo("Загрузка данных...");
        LocalizationStorage oldTranslation = LoadStorageOrEmpty(_state.OldTranslationPath);
        LocalizationStorage translationUpdate = LoadStorageOrEmpty(_state.TranslationUpdatePath);
        LocalizationStorage newVersion = LoadStorageOrEmpty(_state.NewVersionPath);

        if (newVersion.IsEmpty && oldTranslation.IsEmpty && translationUpdate.IsEmpty)
        {
            ConsoleMenuService.ShowError("Нет данных для слияния. Задайте хотя бы один путь.");
            return;
        }

        ConsoleMenuService.ShowInfo("Слияние...");
        LocalizationMerger merger = new();
        List<LocalizationFile> mergedFiles = merger.Merge(oldTranslation, translationUpdate, newVersion, _state.TargetLanguage);

        ConsoleMenuService.ShowInfo("Запись...");
        LocalizationYmlWriter writer = new();
        writer.WriteFiles(mergedFiles, outputPath);

        ConsoleMenuService.ShowSuccess($"Результат сохранён: '{outputPath}'");
        ConsoleMenuService.ShowInfo($"Файлов: {mergedFiles.Count}");
    }

    private void SetPath(Action<string> setter, string prompt, string successMsg)
    {
        string path = ConsoleMenuService.ReadFolderPath(prompt);
        if (string.IsNullOrEmpty(path))
            return;
        setter(path);
        _configService.Save(_state);
        ConsoleMenuService.ShowSuccess($"{successMsg}: '{path}'");
    }

    private LocalizationStorage LoadStorageOrEmpty(string? path, bool consoleDebug = false)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            return new LocalizationStorage();
        LocalizationStorage storage = new();
        List<LocalizationFile> files = _parser.ParseDirectory(path);

        if (consoleDebug)
        {
            Console.WriteLine($"  [Debug] Loading from: {path}");
            Console.WriteLine($"  [Debug] Found {files.Count} files");
            foreach (LocalizationFile file in files)
            {
                Console.WriteLine($"    - {file.FileName}: {file.Entries.Count} keys");
                foreach (LocalizationEntry entry in file.Entries.Take(5))
                    Console.WriteLine($"      • {entry.Key}");
                if (file.Entries.Count > 5)
                    Console.WriteLine($"      ... and {file.Entries.Count - 5} more");
            }
        }

        storage.AddFiles(files);
        return storage;
    }

    private static string CreateYmlDirectory(string prefix) =>
        Path.Combine(Environment.CurrentDirectory, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}");
}


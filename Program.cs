using ParadoxLocalisationHelper.Cli;
using ParadoxLocalisationHelper.Configuration;
using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper;

internal class Program
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");
    private static readonly ConfigurationService ConfigService = new(ConfigPath);
    private static AppState State = new();

    static void Main(string[] args)
    {
        State = ConfigService.Load();

        ConsoleMenu menu = new(State);
        MenuActions actions = new(State, ConfigService);

        // Settings (1-6)
        menu.AddItem("Установить путь к новой версии (оригинал)", actions.SetNewVersionPath);
        menu.AddItem("Установить путь к старой версии (оригинал)", actions.SetOldVersionPath);
        menu.AddItem("Установить путь к устаревшему переводу", actions.SetOldTranslationPath);
        menu.AddItem("Установить путь к обновлению перевода", actions.SetNewTranslationPath);
        menu.AddItem("Установить язык оригинала", actions.SetSourceLanguage);
        menu.AddItem("Установить язык перевода", actions.SetTargetLanguage);

        // Actions (7-8)
        menu.AddItem(
            "Сгенерировать отчёт",
            actions.GenerateReport,
            "(анализ изменений и прогресса перевода, с YML для оставшейся работы)");

        menu.AddItem(
            "Объединить переводы",
            actions.MergeTranslations,
            "(слияние устаревшего перевода с обновлением)");

        while (menu.ShowAndExecute())
        {
            // Menu handles everything
        }
    }
}

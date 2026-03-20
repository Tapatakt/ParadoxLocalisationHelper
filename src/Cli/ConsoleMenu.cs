using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Cli;

/// <summary>
/// Manages a collection of menu items and handles user interaction.
/// </summary>
public sealed class ConsoleMenu
{
    private readonly List<MenuItem> _items = [];
    private readonly AppState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleMenu"/> class.
    /// </summary>
    public ConsoleMenu(AppState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _state = state;
    }

    /// <summary>
    /// Adds a menu item to the collection.
    /// </summary>
    public void AddItem(string description, Action action, string? helpText = null)
    {
        MenuItem item = new(description, action, helpText);
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    /// <summary>
    /// Displays the menu and processes user selection.
    /// </summary>
    /// <returns>True to continue running, false to exit.</returns>
    public bool ShowAndExecute()
    {
        Console.Clear();
        Console.WriteLine("=== Paradox Localisation Helper ===");
        Console.WriteLine("      Помощник локализации Paradox");
        Console.WriteLine();

        DisplaySettings();
        Console.WriteLine();

        DisplayMenuItems();
        Console.WriteLine();

        Console.WriteLine("  [0] Выход");
        Console.WriteLine();
        Console.Write("Выберите опцию: ");

        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int choice))
            return HandleInvalidChoice();

        if (choice == 0)
            return false;

        if (choice < 1 || choice > _items.Count)
            return HandleInvalidChoice();

        MenuItem selectedItem = _items[choice - 1];

        try
        {
            selectedItem.Action();
        }
        catch (Exception ex)
        {
            ConsoleMenuService.ShowError($"Ошибка выполнения: {ex.Message}\nСтек: {ex.StackTrace}");
            ConsoleMenuService.WaitForKey();
        }

        return true;
    }

    private void DisplaySettings()
    {
        Console.WriteLine("Текущие настройки:"); // Это не пункты меню!
        Console.WriteLine($"  Новая версия:         {FormatPath(_state.NewVersionPath)}");
        Console.WriteLine($"  Старая версия:        {FormatPath(_state.OldVersionPath)}");
        Console.WriteLine($"  Устаревший перевод:   {FormatPath(_state.OldTranslationPath)}");
        Console.WriteLine($"  Обновление перевода:  {FormatPath(_state.TranslationUpdatePath)}");
        Console.WriteLine($"  Язык оригинала:       {_state.SourceLanguage}");
        Console.WriteLine($"  Язык перевода:        {_state.TargetLanguage}");
    }

    private void DisplayMenuItems()
    {
        Console.WriteLine();
        Console.WriteLine("Действия:");

        int optionNumber = 1; // Правильно!
        foreach (MenuItem item in _items)
        {
            string padding = optionNumber < 10 ? "  " : " ";
            Console.WriteLine($"{padding}[{optionNumber}] {item.Description}");

            if (!string.IsNullOrEmpty(item.HelpText))
            {
                string helpPadding = new(' ', optionNumber < 10 ? 7 : 6);
                Console.WriteLine($"{helpPadding}{item.HelpText}");
            }

            optionNumber++;
        }
    }

    private static bool HandleInvalidChoice()
    {
        ConsoleMenuService.ShowError("Неверный выбор.");
        ConsoleMenuService.WaitForKey();
        return true;
    }

    private static string FormatPath(string path) =>
        string.IsNullOrEmpty(path) ? "(не задан)" : $"'{path}'";
}

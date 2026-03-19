using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Cli;

/// <summary>
/// Provides interactive console menu functionality.
/// </summary>
public sealed class ConsoleMenuService
{
    private readonly AppState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleMenuService"/> class.
    /// </summary>
    /// <param name="state">The application state.</param>
    public ConsoleMenuService(AppState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _state = state;
    }

    /// <summary>
    /// Displays the main menu and returns the user's choice.
    /// </summary>
    /// <returns>The selected menu option number, or 0 to exit.</returns>
    public int ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Paradox Localisation Helper ===");
        Console.WriteLine();
        Console.WriteLine("Current Settings:");
        Console.WriteLine($"  [1] New version folder:     {FormatPath(_state.NewVersionPath)}");
        Console.WriteLine($"  [2] Old version folder:     {FormatPath(_state.OldVersionPath)}");
        Console.WriteLine($"  [3] Outdated translation:   {FormatPath(_state.OldTranslationPath)}");
        Console.WriteLine($"  [4] Translation update:     {FormatPath(_state.NewTranslationPath)}");
        Console.WriteLine($"  [5] Source language:        {_state.SourceLanguage}");
        Console.WriteLine($"  [6] Target language:        {_state.TargetLanguage}");
        Console.WriteLine();
        Console.WriteLine("Actions:");
        Console.WriteLine("  [7] Generate delta report (old vs new version) + YML");
        Console.WriteLine("  [8] Generate missing keys report (outdated translation vs new version)");
        Console.WriteLine("  [9] Merge outdated translation with update");
        Console.WriteLine(" [10] Generate translation work report (missing + modified keys)");
        Console.WriteLine();
        Console.WriteLine("  [0] Exit");
        Console.WriteLine();
        Console.Write("Select option: ");

        string? input = Console.ReadLine();
        if (int.TryParse(input, out int choice))
            return choice;

        return -1;
    }

    /// <summary>
    /// Prompts the user to enter a folder path.
    /// </summary>
    /// <param name="prompt">The prompt message.</param>
    /// <returns>The validated folder path, or empty string if cancelled.</returns>
    public static string ReadFolderPath(string prompt)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"Enter {prompt} (or 'cancel' to go back):");
            Console.Write("> ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Path cannot be empty. Try again.");
                continue;
            }

            if (input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (!Directory.Exists(input))
            {
                Console.WriteLine($"Directory not found: '{input}'. Try again.");
                continue;
            }

            return input;
        }
    }

    /// <summary>
    /// Prompts the user to enter a language code.
    /// </summary>
    /// <param name="currentValue">The current language value.</param>
    /// <returns>The new language code.</returns>
    public static string ReadLanguage(string currentValue)
    {
        Console.WriteLine();
        Console.WriteLine($"Enter language code (current: {currentValue}):");
        Console.Write("> ");
        string? input = Console.ReadLine()?.Trim();

        return string.IsNullOrEmpty(input) ? currentValue : input.ToLower();
    }

    /// <summary>
    /// Prompts the user to enter an output folder name.
    /// </summary>
    /// <returns>The output folder path.</returns>
    public static string ReadOutputFolderName()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Enter name for the output folder:");
            Console.Write("> ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Name cannot be empty. Try again.");
                continue;
            }

            if (input.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Console.WriteLine("Name contains invalid characters. Try again.");
                continue;
            }

            return input;
        }
    }

    /// <summary>
    /// Waits for user to press any key.
    /// </summary>
    public static void WaitForKey()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }

    /// <summary>
    /// Shows an error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static void ShowError(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Shows a success message.
    /// </summary>
    /// <param name="message">The success message.</param>
    public static void ShowSuccess(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Shows an informational message.
    /// </summary>
    /// <param name="message">The message.</param>
    public static void ShowInfo(string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
    }

    private static string FormatPath(string path) =>
        string.IsNullOrEmpty(path) ? "(not set)" : $"'{path}'";
}

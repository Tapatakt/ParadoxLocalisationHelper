namespace ParadoxLocalisationHelper.Cli;

/// <summary>
/// Provides static helper methods for console UI interactions.
/// </summary>
public static class ConsoleMenuService
{
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
            Console.WriteLine($"Введите {prompt} (или 'cancel'/'отмена' для возврата):");
            Console.Write("> ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Путь не может быть пустым. Попробуйте ещё раз.");
                continue;
            }

            if (input.Equals("cancel", StringComparison.OrdinalIgnoreCase) || input.Equals("отмена", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (!Directory.Exists(input))
            {
                Console.WriteLine($"Папка не найдена: '{input}'. Попробуйте ещё раз.");
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
        Console.WriteLine($"Введите код языка (текущий: {currentValue}):");
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
            Console.WriteLine("Введите имя для выходной папки:");
            Console.Write("> ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Имя не может быть пустым. Попробуйте ещё раз.");
                continue;
            }

            if (input.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Console.WriteLine("Имя содержит недопустимые символы. Попробуйте ещё раз.");
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
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
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
        Console.WriteLine($"Ошибка: {message}");
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
}

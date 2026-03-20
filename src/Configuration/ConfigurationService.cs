using System.Text.Json;
using ParadoxLocalisationHelper.Models;

namespace ParadoxLocalisationHelper.Configuration;

/// <summary>
/// Manages application configuration persistence.
/// </summary>
public sealed class ConfigurationService
{
    private readonly string _configPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="configPath">Path to the configuration file.</param>
    public ConfigurationService(string configPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(configPath);
        _configPath = configPath;
    }

    /// <summary>
    /// Loads application state from configuration file.
    /// </summary>
    /// <returns>Loaded AppState or new instance if file doesn't exist.</returns>
    public AppState Load()
    {
        if (!File.Exists(_configPath))
            return new();

        try
        {
            string json = File.ReadAllText(_configPath);
            AppStateDto? dto = JsonSerializer.Deserialize<AppStateDto>(json);
            return dto is null ? new() : FromDto(dto);
        }
        catch (JsonException)
        {
            return new();
        }
    }

    /// <summary>
    /// Saves application state to configuration file.
    /// </summary>
    /// <param name="state">The state to save.</param>
    public void Save(AppState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        AppStateDto dto = ToDto(state);
        string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }

    private static AppStateDto ToDto(AppState state) =>
        new()
        {
            NewVersionPath = state.NewVersionPath,
            OldVersionPath = state.OldVersionPath,
            OldTranslationPath = state.OldTranslationPath,
            NewTranslationPath = state.TranslationUpdatePath,
            SourceLanguage = state.SourceLanguage,
            TargetLanguage = state.TargetLanguage
        };

    private static AppState FromDto(AppStateDto dto) =>
        new()
        {
            NewVersionPath = dto.NewVersionPath ?? "",
            OldVersionPath = dto.OldVersionPath ?? "",
            OldTranslationPath = dto.OldTranslationPath ?? "",
            TranslationUpdatePath = dto.NewTranslationPath ?? "",
            SourceLanguage = dto.SourceLanguage ?? "english",
            TargetLanguage = dto.TargetLanguage ?? "russian"
        };

    private sealed class AppStateDto
    {
        public string? NewVersionPath { get; set; }
        public string? OldVersionPath { get; set; }
        public string? OldTranslationPath { get; set; }
        public string? NewTranslationPath { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
    }
}

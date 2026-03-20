namespace ParadoxLocalisationHelper.Models;

/// <summary>
/// Holds the application configuration and user-selected paths.
/// </summary>
public sealed class AppState
{
    /// <summary>
    /// Path to the folder containing the new version (source language).
    /// </summary>
    public string NewVersionPath { get; set; } = string.Empty;

    /// <summary>
    /// Path to the folder containing the old version (source language).
    /// </summary>
    public string OldVersionPath { get; set; } = string.Empty;

    /// <summary>
    /// Path to the folder containing the outdated translation.
    /// </summary>
    public string OldTranslationPath { get; set; } = string.Empty;

    /// <summary>
    /// Path to the folder containing the translation update.
    /// </summary>
    public string TranslationUpdatePath { get; set; } = string.Empty;

    /// <summary>
    /// The source/original language code (e.g., "english").
    /// </summary>
    public string SourceLanguage { get; set; } = "english";

    /// <summary>
    /// The target translation language code (e.g., "russian").
    /// </summary>
    public string TargetLanguage { get; set; } = "russian";

    /// <summary>
    /// Gets a value indicating whether new version path is set.
    /// </summary>
    public bool HasNewVersion => !string.IsNullOrEmpty(NewVersionPath);

    /// <summary>
    /// Gets a value indicating whether old version path is set.
    /// </summary>
    public bool HasOldVersion => !string.IsNullOrEmpty(OldVersionPath);

    /// <summary>
    /// Gets a value indicating whether old translation path is set.
    /// </summary>
    public bool HasOldTranslation => !string.IsNullOrEmpty(OldTranslationPath);

    /// <summary>
    /// Gets a value indicating whether new translation path is set.
    /// </summary>
    public bool HasNewTranslation => !string.IsNullOrEmpty(TranslationUpdatePath);
}

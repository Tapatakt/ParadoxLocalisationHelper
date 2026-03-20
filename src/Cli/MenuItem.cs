namespace ParadoxLocalisationHelper.Cli;

/// <summary>
/// Represents a single menu item with description and action.
/// </summary>
public sealed class MenuItem
{
    /// <summary>
    /// Gets the display description of the menu item.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the action to execute when the item is selected.
    /// </summary>
    public Action Action { get; }

    /// <summary>
    /// Gets the detailed help text for this menu item.
    /// </summary>
    public string? HelpText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItem"/> class.
    /// </summary>
    public MenuItem(string description, Action action, string? helpText = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(description);
        ArgumentNullException.ThrowIfNull(action);
        Description = description;
        Action = action;
        HelpText = helpText;
    }
}

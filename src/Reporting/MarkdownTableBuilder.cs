using System.Text;

namespace ParadoxLocalisationHelper.Reporting;

/// <summary>
/// Builds Markdown tables with a fluent API.
/// </summary>
public sealed class MarkdownTableBuilder
{
    private readonly StringBuilder _sb;
    private readonly List<string> _headers = [];
    private readonly List<string[]> _rows = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownTableBuilder"/> class.
    /// </summary>
    public MarkdownTableBuilder()
    {
        _sb = new StringBuilder();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownTableBuilder"/> class
    /// with an existing StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    public MarkdownTableBuilder(StringBuilder sb)
    {
        _sb = sb;
    }

    /// <summary>
    /// Adds a header row with column names.
    /// </summary>
    /// <param name="columns">The column headers.</param>
    /// <returns>The builder for chaining.</returns>
    public MarkdownTableBuilder AddHeader(params string[] columns)
    {
        _headers.Clear();
        _headers.AddRange(columns);
        return this;
    }

    /// <summary>
    /// Adds a data row.
    /// </summary>
    /// <param name="values">The row values.</param>
    /// <returns>The builder for chaining.</returns>
    public MarkdownTableBuilder AddRow(params object?[] values)
    {
        string[] stringValues = values.Select(v => FormatValue(v)).ToArray();
        _rows.Add(stringValues);
        return this;
    }

    /// <summary>
    /// Adds multiple rows from a collection.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items to add.</param>
    /// <param name="rowSelector">Function to convert item to row values.</param>
    /// <returns>The builder for chaining.</returns>
    public MarkdownTableBuilder AddRows<T>(IEnumerable<T> items, Func<T, object?[]> rowSelector)
    {
        foreach (T item in items)
            AddRow(rowSelector(item));
        return this;
    }

    /// <summary>
    /// Adds rows with a limit and optional "more" indicator.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items to add.</param>
    /// <param name="rowSelector">Function to convert item to row values.</param>
    /// <param name="limit">Maximum number of rows to add.</param>
    /// <param name="totalCount">Total count for "more" indicator.</param>
    /// <param name="moreText">Text template for "more" row (use {0} for remaining count).</param>
    /// <returns>The builder for chaining.</returns>
    public MarkdownTableBuilder AddRowsWithLimit<T>(
        IEnumerable<T> items,
        Func<T, object?[]> rowSelector,
        int limit,
        int totalCount,
        string? moreText = null)
    {
        List<T> limitedItems = items.Take(limit).ToList();
        foreach (T item in limitedItems)
            AddRow(rowSelector(item));

        if (totalCount > limit)
        {
            int remaining = totalCount - limit;
            string text = moreText ?? $"... (и ещё {remaining})";
            string[] emptyRow = new string[_headers.Count];
            Array.Fill(emptyRow, "");
            emptyRow[0] = "...";
            emptyRow[1] = string.Format(text, remaining);
            _rows.Add(emptyRow);
        }

        return this;
    }

    /// <summary>
    /// Adds a simple key-value row (2 columns).
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>The builder for chaining.</returns>
    public MarkdownTableBuilder AddKeyValue(string key, object? value)
    {
        if (_headers.Count == 0)
            AddHeader("Параметр", "Значение");
        AddRow(key, value);
        return this;
    }

    /// <summary>
    /// Builds and returns the table as a string.
    /// </summary>
    /// <returns>The Markdown table.</returns>
    public string Build()
    {
        if (_headers.Count == 0)
            return string.Empty;

        // Header row
        _sb.AppendLine("| " + string.Join(" | ", _headers) + " |");

        // Separator row
        string[] separators = _headers.Select(h => new string('-', Math.Max(3, h.Length))).ToArray();
        _sb.AppendLine("| " + string.Join(" | ", separators) + " |");

        // Data rows
        foreach (string[] row in _rows)
        {
            string[] paddedRow = new string[_headers.Count];
            for (int i = 0; i < _headers.Count; i++)
                paddedRow[i] = i < row.Length ? row[i] : "";
            _sb.AppendLine("| " + string.Join(" | ", paddedRow) + " |");
        }

        return _sb.ToString();
    }

    /// <summary>
    /// Appends the table to a StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    public void AppendTo(StringBuilder sb)
    {
        Build();
        sb.Append(_sb.ToString());
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
            return "";
        if (value is string s)
            return s;
        if (value is int i)
            return i.ToString();
        if (value is long l)
            return l.ToString();
        if (value is double d)
            return d.ToString("F1");
        if (value is float f)
            return f.ToString("F1");
        if (value is bool b)
            return b ? "Да" : "Нет";
        return value.ToString() ?? "";
    }
}

/// <summary>
/// Extension methods for StringBuilder to simplify table creation.
/// </summary>
public static class MarkdownTableExtensions
{
    /// <summary>
    /// Starts a new table section with a header.
    /// </summary>
    public static MarkdownTableBuilder StartTable(this StringBuilder sb)
        => new(sb);

    /// <summary>
    /// Appends a simple key-value table.
    /// </summary>
    public static StringBuilder AppendKeyValueTable(
        this StringBuilder sb,
        IEnumerable<KeyValuePair<string, object?>> rows)
    {
        MarkdownTableBuilder table = new(sb);
        table.AddHeader("Параметр", "Значение");
        foreach (KeyValuePair<string, object?> row in rows)
            table.AddRow(row.Key, row.Value);
        sb.AppendLine(table.Build());
        return sb;
    }

    /// <summary>
    /// Appends a key-value table from a list of tuples.
    /// </summary>
    public static StringBuilder AppendKeyValueTable(
        this StringBuilder sb,
        params (string Key, object? Value)[] rows)
    {
        MarkdownTableBuilder table = new(sb);
        table.AddHeader("Параметр", "Значение");
        foreach ((string Key, object? Value) in rows)
            table.AddRow(Key, Value);
        sb.AppendLine(table.Build());
        return sb;
    }
}

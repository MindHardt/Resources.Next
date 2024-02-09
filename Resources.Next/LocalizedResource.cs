using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace Resources.Next;

/// <summary>
/// Wraps a localized resource.
/// </summary>
public class LocalizedResource(IReadOnlyDictionary<string, string> localizations)
{
    private readonly FrozenDictionary<string, string> _localizations = localizations.ContainsKey(string.Empty)
        ? localizations.ToFrozenDictionary()
        : throw new InvalidOperationException("No localization for default value found");

    /// <summary>
    /// A default fallback value of this <see cref="LocalizedResource"/>.
    /// </summary>
    public string Default => _localizations[string.Empty];

    /// <summary>
    /// Gets value of this resource with provided <paramref name="culture"/>
    /// or throws <see cref="KeyNotFoundException"/> if none is found.
    /// </summary>
    public string GetRequired(string culture) => _localizations[culture];
    /// <inheritdoc cref="GetRequired(string)"/>
    public string GetRequired(CultureInfo? culture) =>
        GetRequired((culture ?? CultureInfo.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets value of this resource with provided <paramref name="culture"/>
    /// or <see langword="null" /> if none is found.
    /// </summary>
    public string? GetOrNull(string culture) => _localizations.GetValueOrDefault(culture);
    /// <inheritdoc cref="GetOrNull(string)"/>
    public string? GetOrNull(CultureInfo? culture) =>
        GetOrNull((culture ?? CultureInfo.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets value of this resource with provided <paramref name="culture"/>
    /// or <see cref="Default"/> fallback value.
    /// </summary>
    public string GetOrDefault(string culture) => _localizations.GetValueOrDefault(culture) ?? Default;
    /// <inheritdoc cref="GetOrDefault(string)"/>
    public string GetOrDefault(CultureInfo? culture) =>
        GetOrDefault((culture ?? CultureInfo.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets <see cref="Default"/> fallback value for display.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => Default;
}